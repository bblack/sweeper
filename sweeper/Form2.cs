using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace sweeper
{
	public partial class Form2 : Form
	{
		public Form2()
		{
			InitializeComponent();
			this.bmpBoardControl1.BoardChanged += new EventHandler<BmpBoardControl.BoardChangedEventArgs>(bmpBoardControl1_BoardChanged);
			this.bmpBoardControl1.Board = new model.Board();
			timer1.Tick += new EventHandler(timer1_Tick);
			timer1.Interval = 100;
			timer1.Start();
		}

		void bmpBoardControl1_BoardChanged(object sender, BmpBoardControl.BoardChangedEventArgs e)
		{
			if (e.OldBoard != null)
			{
				// Remove old event handler bindings
				e.OldBoard.SpaceFlagToggled -= NewBoard_SpaceFlagToggled;
			}

			e.NewBoard.SpaceFlagToggled += new EventHandler<EventArgs>(NewBoard_SpaceFlagToggled);

			this.toolStripStatusLabel1.Text = String.Format("{0}", e.NewBoard.BombsMinusFlags());
		}

		void timer1_Tick(object sender, EventArgs e)
		{
			toolStripStatusLabel2.Text = (bmpBoardControl1.Board == null) ? "--" : String.Format("{0:F1}", bmpBoardControl1.Board.Elapsed.Value.TotalSeconds);
		}

		void NewBoard_SpaceFlagToggled(object sender, EventArgs e)
		{
			this.toolStripStatusLabel1.Text = String.Format("{0}" , (sender as model.Board).BombsMinusFlags());
		}

		public Form2(model.Board board) : this()
		{
			this.bmpBoardControl1.Board = board;
		}

		public void SetBoard(model.Board b)
		{
			this.bmpBoardControl1.Board = b;
		}
	}
}