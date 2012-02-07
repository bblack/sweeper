using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace sweeper
{
	public partial class BmpBoardControl : UserControl
	{
		public static Color[] NumberSpaceColors 
		{
			get
			{
				return new Color[] {
				Color.White,
				Color.Blue,
				Color.Green,
				Color.Red,
				Color.Purple,
				Color.Black,
				Color.Maroon,
				Color.Turquoise,
				Color.Gray };
			}
		}

		private model.Board board;
		private Bitmap m_bmp;

		public class BoardChangedEventArgs : EventArgs
		{
			private model.Board oldboard;
			private model.Board newboard;

			public model.Board OldBoard { get { return this.oldboard; } }
			public model.Board NewBoard { get { return this.newboard; } }

			public BoardChangedEventArgs(model.Board oldboard, model.Board newboard)
			{
				this.oldboard = oldboard;
				this.newboard = newboard;
			}
		}
		public event EventHandler<BoardChangedEventArgs> BoardChanged;

		public model.Board Board
		{
			get { return board; }
			set
			{
				model.Board oldboard = board;
				if (board != null)
				{
					board.SpaceRevealed -= board_SpaceRevealed;
					board.SpaceFlagToggled -= board_SpaceRevealed;
					board.GameOver -= board_GameOver;
					board.Success -= board_Success;
				}
				board = value;
				if (board != null)
				{
					board.SpaceRevealed += new EventHandler<EventArgs>(board_SpaceRevealed);
					board.SpaceFlagToggled += new EventHandler<EventArgs>(board_SpaceRevealed);
					board.GameOver += new EventHandler<EventArgs>(board_GameOver);
					board.Success += new EventHandler<EventArgs>(board_Success);
				}
				Invalidate();
				if (this.BoardChanged != null)
					this.BoardChanged(this, new BoardChangedEventArgs(oldboard, board));
			}
		}

		void board_Success(object sender, EventArgs e)
		{
			if (MessageBox.Show("Success! Play another?", "Success", MessageBoxButtons.YesNo) == DialogResult.Yes)
			{
				this.Board = new model.Board();
			}
		}

		void board_GameOver(object sender, EventArgs e)
		{
			if (MessageBox.Show("Failure. Play another?", "Failure", MessageBoxButtons.YesNo) == DialogResult.Yes)
			{
				this.Board = new model.Board();
			}
		}

		void board_SpaceRevealed(object sender, EventArgs e)
		{
			this.Invalidate();
		}

		public BmpBoardControl()
		{
			InitializeComponent();
			SetStyle(
				ControlStyles.SupportsTransparentBackColor |
				ControlStyles.AllPaintingInWmPaint |
				ControlStyles.Opaque |
				ControlStyles.UserPaint,
				true
			);

			this.SizeChanged += new EventHandler(BmpBoardControl_SizeChanged);
			this.MouseClick += new MouseEventHandler(BmpBoardControl_MouseClick);
			this.MouseDoubleClick += new MouseEventHandler(BmpBoardControl_MouseClick);
		}

		void BmpBoardControl_MouseClick(object sender, MouseEventArgs e)
		{
			if (!this.ClientRectangle.Contains(e.Location) || board == null) return;

			int rownum = (int)((double)e.Y / this.ClientSize.Height * board.HEIGHT);
			int colnum = (int)((double)e.X / this.ClientSize.Width * board.WIDTH);

			if (e.Button == MouseButtons.Left)
			{
				board.spaces[rownum, colnum].Reveal();
			}
			else if (e.Button == MouseButtons.Right)
			{
				board.ToggleFlagOnSpace(rownum, colnum);
			}
		}

		void BmpBoardControl_SizeChanged(object sender, EventArgs e)
		{
			this.m_bmp = null;
			this.Invalidate();
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			if (m_bmp == null)
				m_bmp = new Bitmap(this.ClientSize.Width, this.ClientSize.Height);
			Graphics g = Graphics.FromImage(m_bmp);

			g.Clear(this.BackColor);
			if (board != null)
			{

				for (int row = 0; row < board.HEIGHT; row++)
				{
					for (int col = 0; col < board.WIDTH; col++)
					{
						RectangleF spacerectf = new RectangleF(
							(float)col / board.WIDTH * this.ClientSize.Width,
							(float)row / board.HEIGHT * this.ClientSize.Height,
							(float)this.ClientSize.Width / board.WIDTH,
							(float)this.ClientSize.Height / board.HEIGHT
						);

						Brush bgbrush;
						if (!board.spaces[row, col].IsRevealed && board.spaces[row, col].flagged)
							bgbrush = Brushes.Goldenrod;
						else if (!board.spaces[row, col].IsRevealed)
							bgbrush = Brushes.SteelBlue;
						else if (board.spaces[row, col].IsBomb)
							bgbrush = Brushes.Crimson;
						else
							bgbrush = Brushes.Transparent;
						g.FillRectangle(bgbrush, spacerectf);
						if ((row + col) % 2 == 0)
							g.FillRectangle(new SolidBrush(Color.FromArgb(0x10, Color.Black)), spacerectf);

						Brush topleftBrush = new SolidBrush(Color.FromArgb(96, board.spaces[row, col].IsRevealed ? Color.Black : Color.White));
						Brush bottomrightBrush = new SolidBrush(Color.FromArgb(96, board.spaces[row, col].IsRevealed ? Color.White : Color.Black));

						/* 3D effect:
						e.Graphics.FillPolygon(bottomrightBrush, new PointF[]{
							new PointF(spacerectf.X, spacerectf.Y + spacerectf.Height),
							new PointF(spacerectf.X + 2, spacerectf.Y + spacerectf.Height - 2),
							new PointF(spacerectf.X + spacerectf.Width - 2, spacerectf.Y + spacerectf.Height - 2),
							new PointF(spacerectf.X + spacerectf.Width - 2, spacerectf.Y + 2),
							new PointF(spacerectf.X + spacerectf.Width, spacerectf.Y),
							new PointF(spacerectf.X + spacerectf.Width, spacerectf.Y + spacerectf.Height)});
						e.Graphics.FillPolygon(topleftBrush, new PointF[]{
							spacerectf.Location,
							new PointF(spacerectf.X + spacerectf.Width, spacerectf.Y),
							new PointF(spacerectf.X + spacerectf.Width - 2, spacerectf.Y + 2),
							new PointF(spacerectf.X + 2, spacerectf.Y + 2),
							new PointF(spacerectf.X + 2, spacerectf.Y + spacerectf.Height -2),
							new PointF(spacerectf.X, spacerectf.Y + spacerectf.Height)});
						 */

						String caption;
						Brush captionBrush = Brushes.Black;
						if (board.IsGameOver && board.spaces[row, col].IsBomb)
						{
							caption = "";
							g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
							g.FillEllipse(Brushes.Black,
								spacerectf.X + spacerectf.Width * 1 / 4,
								spacerectf.Y + spacerectf.Height * 1 / 4,
								spacerectf.Width * 2 / 4,
								spacerectf.Height * 2 / 4);
							g.FillRectangle(Brushes.Black,
								spacerectf.X + spacerectf.Width * 7 / 16,
								spacerectf.Y + spacerectf.Height * 1 / 8,
								spacerectf.Width * 2 / 16,
								spacerectf.Height * 6 / 8);
							g.FillRectangle(Brushes.Black,
								spacerectf.X + spacerectf.Width * 1 / 8,
								spacerectf.Y + spacerectf.Height * 7 / 16,
								spacerectf.Width * 6 / 8,
								spacerectf.Height * 2 / 16);
							g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
						}
						else if (!board.spaces[row, col].IsRevealed)
							caption = "";
						else if (board.spaces[row, col].IsBomb)
							caption = "B";
						else if (board.spaces[row, col].AdjacentBombCount() > 0)
						{
							int numBombs = board.spaces[row, col].AdjacentBombCount();
							caption = numBombs.ToString();
							captionBrush = new SolidBrush(NumberSpaceColors[numBombs]);
						}
						else // Revealed, non-bomb, non-bomb-adjacent space
							caption = "";

						SizeF captionSize;
						float fontSize = 5f;
						float fontSizeStep = 1f;
						Font font = new Font(FontFamily.GenericSansSerif, fontSize, FontStyle.Bold);
						do
						{
							fontSize = fontSize + fontSizeStep;
							font = new Font(font.FontFamily, fontSize, font.Style);
							captionSize = g.MeasureString(caption, font);
						} while (captionSize.Height != 0 && captionSize.Width != 0 && captionSize.Width < spacerectf.Width && captionSize.Height < spacerectf.Height);
						fontSize = fontSize - fontSizeStep;
						captionSize = g.MeasureString(caption, font);
						PointF captionPosition = new PointF(spacerectf.X + spacerectf.Width / 2 - captionSize.Width / 2, spacerectf.Y + spacerectf.Height / 2 - captionSize.Height / 2);
						g.DrawString(caption, font, captionBrush, captionPosition);
					}
				}
			}

			g.Dispose();
			e.Graphics.DrawImageUnscaled(m_bmp, 0, 0);
		}
	}
}
