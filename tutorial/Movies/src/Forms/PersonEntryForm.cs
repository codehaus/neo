using System;
using System.ComponentModel;
using System.Windows.Forms;
using Movies.Model;
using Neo.Core;


namespace Movies.Forms
{
	/// <summary>
	/// Summary description for PersonEntryForm.
	/// </summary>
	public class PersonEntryForm : Form
	{
		private Container components = null;
		public TextBox nameTextBox;
		private Label label1;
		private Button cancelButton;
		private Button okButton;
		private ObjectContext context;

		public PersonEntryForm(ObjectContext aContext)
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
			this.nameTextBox = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.cancelButton = new System.Windows.Forms.Button();
			this.okButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// nameTextBox
			// 
			this.nameTextBox.Location = new System.Drawing.Point(57, 13);
			this.nameTextBox.Name = "nameTextBox";
			this.nameTextBox.Size = new System.Drawing.Size(224, 20);
			this.nameTextBox.TabIndex = 12;
			this.nameTextBox.Text = "(name)";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(9, 13);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(104, 24);
			this.label1.TabIndex = 13;
			this.label1.Text = "Name:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// cancelButton
			// 
			this.cancelButton.Location = new System.Drawing.Point(208, 48);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.TabIndex = 11;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// okButton
			// 
			this.okButton.Location = new System.Drawing.Point(128, 48);
			this.okButton.Name = "okButton";
			this.okButton.TabIndex = 10;
			this.okButton.Text = "OK";
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// PersonEntryForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(288, 77);
			this.Controls.Add(this.nameTextBox);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.okButton);
			this.Name = "PersonEntryForm";
			this.Text = "Person Entry";
			this.ResumeLayout(false);

		}
		#endregion

		public void cancelButton_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		public void okButton_Click(object sender, EventArgs e)
		{
			Person newPerson = new PersonFactory(context).CreateObject();
			newPerson.Name = nameTextBox.Text;
			this.Close();
		}

	}
}
