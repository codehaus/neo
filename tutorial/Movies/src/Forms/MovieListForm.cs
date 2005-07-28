using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using Movies.Model;
using Neo.Core;


namespace Movies.Forms
{
	/// <summary>
	/// Window listing movies in textual form
	/// </summary>
	public class MovieListForm : Form
	{
		private Container components = null;
		private RichTextBox outputBox;
		private ObjectContext context;

		public MovieListForm(ObjectContext aContext)
		{
			InitializeComponent();
			context = aContext;
			WriteAllMovies();
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
			this.outputBox = new System.Windows.Forms.RichTextBox();
			this.SuspendLayout();
			// 
			// outputBox
			// 
			this.outputBox.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.outputBox.Location = new System.Drawing.Point(0, 2);
			this.outputBox.Name = "outputBox";
			this.outputBox.Size = new System.Drawing.Size(360, 352);
			this.outputBox.TabIndex = 0;
			this.outputBox.Text = "richTextBox1";
			// 
			// MovieListForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(360, 357);
			this.Controls.Add(this.outputBox);
			this.Name = "MovieListForm";
			this.Text = "Movie List";
			this.ResumeLayout(false);

		}
		#endregion


		private void WriteAllMovies()
		{
			FetchSpecification fetchSpec = new FetchSpecification();
			fetchSpec.Spans = new string[]{ "Director" };
			MovieList movieList = new MovieFactory(context).Find(fetchSpec);

			StringBuilder builder = new StringBuilder();
			builder.AppendFormat("Found {0} movie(s):\n\n", movieList.Count);
			foreach(Movie m in movieList)
			{
				builder.Append(m.Title);
				if(m.Year > 0)
					builder.AppendFormat(" ({0})", m.Year);
				builder.Append("\n");
				if(m.Director != null)
					builder.AppendFormat("Directed by {0}\n", m.Director.Name);
				builder.Append("\n");
			}
			outputBox.Text = builder.ToString();
		}

	}
}
