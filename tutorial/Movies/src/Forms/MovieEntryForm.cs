using System;
using System.ComponentModel;
using System.Windows.Forms;
using Movies.Model;
using Neo.Core;


namespace Movies.Forms
{
	public class MovieEntryForm : Form
	{
		private Container components = null;
		private Label label1;
		private Label label2;
		public TextBox titleTextBox;
		public TextBox yearTextBox;
		private Button okButton;
		private Button cancelButton;
		public ComboBox directorComboBox;
		private Label label3;
		private ObjectContext context;

		public MovieEntryForm(ObjectContext aContext)
		{
			InitializeComponent();
			context = aContext;
			LoadDirectorComboBox();
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
			this.titleTextBox = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.yearTextBox = new System.Windows.Forms.TextBox();
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.directorComboBox = new System.Windows.Forms.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// titleTextBox
			// 
			this.titleTextBox.Location = new System.Drawing.Point(56, 16);
			this.titleTextBox.Name = "titleTextBox";
			this.titleTextBox.Size = new System.Drawing.Size(152, 20);
			this.titleTextBox.TabIndex = 0;
			this.titleTextBox.Text = "(new movie)";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(8, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(40, 24);
			this.label1.TabIndex = 1;
			this.label1.Text = "Title:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(8, 48);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(40, 24);
			this.label2.TabIndex = 3;
			this.label2.Text = "Year:";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// yearTextBox
			// 
			this.yearTextBox.Location = new System.Drawing.Point(56, 48);
			this.yearTextBox.Name = "yearTextBox";
			this.yearTextBox.Size = new System.Drawing.Size(48, 20);
			this.yearTextBox.TabIndex = 2;
			this.yearTextBox.Text = "";
			// 
			// okButton
			// 
			this.okButton.Location = new System.Drawing.Point(56, 120);
			this.okButton.Name = "okButton";
			this.okButton.TabIndex = 4;
			this.okButton.Text = "OK";
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.Location = new System.Drawing.Point(136, 120);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.TabIndex = 5;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// directorComboBox
			// 
			this.directorComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.directorComboBox.Location = new System.Drawing.Point(56, 80);
			this.directorComboBox.Name = "directorComboBox";
			this.directorComboBox.Size = new System.Drawing.Size(152, 21);
			this.directorComboBox.TabIndex = 6;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(8, 80);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(48, 24);
			this.label3.TabIndex = 7;
			this.label3.Text = "Director:";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// MovieEntryForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(216, 149);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.directorComboBox);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.yearTextBox);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.titleTextBox);
			this.Name = "MovieEntryForm";
			this.Text = "Movie Entry";
			this.ResumeLayout(false);

		}
		#endregion

		private void LoadDirectorComboBox()
		{
			directorComboBox.Items.Clear();
			directorComboBox.DisplayMember = "Name";
			PersonList persons = new PersonFactory(context).FindAllObjects();
			foreach(Person p in persons)
				directorComboBox.Items.Add(p);
		}
		
		public void cancelButton_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		public void okButton_Click(object sender, EventArgs e)
		{
			Movie newMovie = new MovieFactory(context).CreateObject();
			newMovie.Title = titleTextBox.Text;
			if((yearTextBox != null) && (yearTextBox.Text != ""))
				newMovie.Year = Int32.Parse(yearTextBox.Text);
			if(directorComboBox.SelectedItem != null)
				newMovie.Director = (Person)directorComboBox.SelectedItem;
			this.Close();
		}

	}
}
