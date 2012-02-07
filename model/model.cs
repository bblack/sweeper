using System;
using System.Collections.Generic;
using System.Text;

namespace model
{
	public class Board
	{
		private DateTime? dtFirstSpaceRevealed = new DateTime?();
		private DateTime? dtGameOver = new DateTime?();
		private bool gameover = false;

		public Space[,] spaces;
		public readonly int WIDTH = 20;
		public readonly int HEIGHT = 20;
		public readonly int BOMBS = 80;

		public bool IsGameOver
		{
			get { return this.gameover; }
		}

		public TimeSpan? Elapsed
		{
			get
			{
				if (!dtFirstSpaceRevealed.HasValue)
					return new TimeSpan(0);
				else if (!dtGameOver.HasValue)
					return TimeSpan.FromTicks(DateTime.UtcNow.Ticks - dtFirstSpaceRevealed.Value.Ticks);
				else
					return TimeSpan.FromTicks(dtGameOver.Value.Ticks - dtFirstSpaceRevealed.Value.Ticks);
			}
		}

		public event EventHandler<EventArgs> SpaceRevealed;
		public event EventHandler<EventArgs> SpaceFlagToggled;
		public event EventHandler<EventArgs> GameOver;
		public event EventHandler<EventArgs> Success;

		public Board()
		{
			spaces = new Space[HEIGHT, WIDTH];
			for (int y = 0; y < HEIGHT; y++)
			{
				for (int x = 0; x < WIDTH; x++)
				{
					spaces[y, x] = new Space(this, y, x);
					spaces[y, x].Revealed += new EventHandler<EventArgs>(spaceRevealedHandler);
					spaces[y, x].FlagToggled += new EventHandler<EventArgs>(Board_FlagToggled);
				}
			}

			for (int i = 0; i < BOMBS; i++)
			{
				Random r = new Random();
				Space s;
				do
				{
					s = spaces[r.Next(0, HEIGHT), r.Next(0, WIDTH)];
				} while (s.IsBomb);
				s.isbomb = true;
			}
		}

		void Board_FlagToggled(object sender, EventArgs e)
		{
			if (this.SpaceFlagToggled != null)
				this.SpaceFlagToggled(this, new EventArgs());
		}

		void spaceRevealedHandler(object sender, EventArgs e)
		{
			if (!dtFirstSpaceRevealed.HasValue)
				dtFirstSpaceRevealed = DateTime.UtcNow;

			if (this.SpaceRevealed != null)
				this.SpaceRevealed(this, e);

			if (((Space)sender).IsBomb)
			{
				EndGame(false);
			}
			else if (CountUncovered() == this.HEIGHT * this.WIDTH - this.BOMBS)
			{
				EndGame(true);	
			}
		}

		private void EndGame(bool success)
		{
			this.gameover = true;
			if (!this.dtGameOver.HasValue)
				this.dtGameOver = DateTime.UtcNow;
			if (success)
			{
				if (this.Success != null)
					this.Success(this, new EventArgs());
			}
			else
			{
				if (this.GameOver != null)
					this.GameOver(this, new EventArgs());
			}
		}

		private int CountUncovered()
		{
			int retval = 0;

			for (int y = 0; y < HEIGHT; y++)
			{
				for (int x = 0; x < WIDTH; x++)
				{
					if (spaces[y, x].IsRevealed)
						retval++;
				}
			}

			return retval;
		}

		public int AdjacentBombCount(Space s)
		{
			return AdjacentSpaces(s).FindAll(delegate(Space sp) { return sp.IsBomb; }).Count;
		}

		public List<Space> AdjacentSpaces(Space s)
		{
			List<Space> retval =  new List<Space>();
			for (int x = -1; x <= 1; x++)
			{
				for (int y = -1; y <= 1; y++)
				{
					if (x == 0 && y == 0) continue;
					if (s.colnum + x < 0 || s.colnum + x >= WIDTH ||
						s.rownum + y < 0 || s.rownum + y >= HEIGHT) continue;
					
					retval.Add(this.spaces[s.rownum + y, s.colnum + x]);
				}
			}
			return retval;
		}

		public int BombsMinusFlags()
		{
			int flagcount = 0;

			for (int y = 0; y < HEIGHT; y++)
			{
				for (int x = 0; x < WIDTH; x++)
				{
					if (spaces[y, x].flagged)
						flagcount++;
				}
			}

			return BOMBS - flagcount;
		}

		public void ToggleFlagOnSpace(int rownum, int colnum)
		{
			if (!this.gameover)
				this.spaces[rownum, colnum].ToggleFlag();
		}
	}

	public class Space
	{
		private bool isrevealed = false;
		internal bool isbomb = false;
		private Board b;

		public readonly int rownum;
		public readonly int colnum;
		public bool flagged;

		public event EventHandler<EventArgs> Revealed;
		public event EventHandler<EventArgs> FlagToggled;

		public bool IsRevealed { get { return isrevealed; } }
		public bool IsBomb { get { return isbomb; } }

		public Space(Board b, int rownum, int colnum)
		{
			this.b = b;
			this.rownum = rownum;
			this.colnum = colnum;
		}

		public void Reveal()
		{
			if (this.isrevealed || b.IsGameOver) return;

			this.isrevealed = true;
			if (this.Revealed != null)
				this.Revealed(this, new EventArgs());

			if (this.AdjacentBombCount() == 0)
			{
				foreach (Space s in this.AdjacentSpaces())
				{
					s.Reveal();
				}
			}
		}

		public List<Space> AdjacentSpaces()
		{
			return this.b.AdjacentSpaces(this);
		}

		public int AdjacentBombCount()
		{
			return this.b.AdjacentBombCount(this);
		}

		public void ToggleFlag()
		{
			flagged = !flagged;
			if (this.FlagToggled != null)
				this.FlagToggled(this, new EventArgs());
		}
	}
}
