using System;
using System.ComponentModel;
using System.Windows.Forms;
using Neo.Core;


namespace Movies.Forms
{
	/// <summary>
	/// Main window, i.e. the MDI frame.
	/// </summary>
	public class MainWindowForm : Form
	{
		private Container components = null;
		private MainMenu mainMenu1;
		private MenuItem menuItem1;
		private MenuItem exitMenuItem;
		private MenuItem menuItem3;
		private MenuItem saveMenuItem;
		private MenuItem menuItem4;
		private MenuItem listMoviesMenuItem;
		private MenuItem menuItem2;
		private MenuItem newMovieMenuItem;
		private System.Windows.Forms.MenuItem newPersonMenuItem;
		private ObjectContext context;

		public MainWindowForm(ObjectContext aContext)
		{
			InitializeComponent();
			context = aContext;
		}

		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.mainMenu1 = new System.Windows.Forms.MainMenu();
			this.menuItem1 = new System.Windows.Forms.MenuItem();
			this.newMovieMenuItem = new System.Windows.Forms.MenuItem();
			this.newPersonMenuItem = new System.Windows.Forms.MenuItem();
			this.menuItem2 = new System.Windows.Forms.MenuItem();
			this.saveMenuItem = new System.Windows.Forms.MenuItem();
			this.menuItem3 = new System.Windows.Forms.MenuItem();
			this.exitMenuItem = new System.Windows.Forms.MenuItem();
			this.menuItem4 = new System.Windows.Forms.MenuItem();
			this.listMoviesMenuItem = new System.Windows.Forms.MenuItem();
			// 
			// mainMenu1
			// 
			this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.menuItem1,
																					  this.menuItem4});
			// 
			// menuItem1
			// 
			this.menuItem1.Index = 0;
			this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.newPersonMenuItem,
																					  this.newMovieMenuItem,
																					  this.menuItem2,
																					  this.saveMenuItem,
																					  this.menuItem3,
																					  this.exitMenuItem});
			this.menuItem1.Text = "File";
			// 
			// newMovieMenuItem
			// 
			this.newMovieMenuItem.Index = 1;
			this.newMovieMenuItem.Text = "New Movie...";
			this.newMovieMenuItem.Click += new System.EventHandler(this.newMovieMenuItem_Click);
			// 
			// newPersonMenuItem
			// 
			this.newPersonMenuItem.Index = 0;
			this.newPersonMenuItem.Text = "New Person...";
			this.newPersonMenuItem.Click += new System.EventHandler(this.newPersonMenuItem_Click);
			// 
			// menuItem2
			// 
			this.menuItem2.Index = 2;
			this.menuItem2.Text = "-";
			// 
			// saveMenuItem
			// 
			this.saveMenuItem.Index = 3;
			this.saveMenuItem.Shortcut = System.Windows.Forms.Shortcut.CtrlS;
			this.saveMenuItem.Text = "&Save";
			this.saveMenuItem.Click += new System.EventHandler(this.saveMenuItem_Click);
			// 
			// menuItem3
			// 
			this.menuItem3.Index = 4;
			this.menuItem3.Text = "-";
			// 
			// exitMenuItem
			// 
			this.exitMenuItem.Index = 5;
			this.exitMenuItem.Text = "E&xit";
			this.exitMenuItem.Click += new System.EventHandler(this.exitMenuItem_Click);
			// 
			// menuItem4
			// 
			this.menuItem4.Index = 1;
			this.menuItem4.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.listMoviesMenuItem});
			this.menuItem4.Text = "View";
			// 
			// listMoviesMenuItem
			// 
			this.listMoviesMenuItem.Index = 0;
			this.listMoviesMenuItem.Text = "List Movies";
			this.listMoviesMenuItem.Click += new System.EventHandler(this.listMoviesMenuItem_Click);
			// 
			// MainWindowForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(768, 609);
			this.IsMdiContainer = true;
			this.Menu = this.mainMenu1;
			this.Name = "MainWindowForm";
			this.Text = "Movies";

		}
		#endregion

		
		private void newPersonMenuItem_Click(object sender, System.EventArgs e)
		{
			PersonEntryForm form = new PersonEntryForm(context);
			form.MdiParent = this;
			form.Show();
		}

		private void newMovieMenuItem_Click(object sender, EventArgs e)
		{
			MovieEntryForm form = new MovieEntryForm(context);
			form.MdiParent = this;
			form.Show();
		}

		private void saveMenuItem_Click(object sender, EventArgs e)
		{
			context.SaveChanges();
		}

		private void exitMenuItem_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void listMoviesMenuItem_Click(object sender, EventArgs e)
		{
			MovieListForm form = new MovieListForm(context);
			form.MdiParent = this;
			form.Show();
		}

	
	}

}
