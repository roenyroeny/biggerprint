using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace biggerprint
{

	class Operation
	{
		public Element element;

		

	};

	class OperationDrag : Operation
	{

	};

	public partial class Form1 : Form
	{
		Matrix view = new Matrix();
		int mouseX, mouseY;
		bool mouseMoved = false;

		CheckBox showPages;
		CheckBox showCrossHatching;

		Document document = null;

		Element selectedElement = null;

		Operation operation = null;

		// dragging
		Element dragElement = null;

		// scaling
		PointF firstPoint, secondPoint;

		public bool Valid
		{
			get
			{
				return Bounds.X != float.PositiveInfinity && Bounds.Y != float.PositiveInfinity;
			}
		}

		public Form1()
		{
			InitializeComponent();

			// no idea why this cant be done through the WYSIWYG editor
			{
				showPages = new CheckBox();
				showPages.Text = "pages";
				statusStrip1.Items.Add(new ToolStripControlHost(showPages));
				showPages.CheckedChanged += CheckedChanged;
				showPages.Checked = true;
			}
			{
				showCrossHatching = new CheckBox();
				showCrossHatching.Text = "cross-hatching";
				statusStrip1.Items.Add(new ToolStripControlHost(showCrossHatching));
				showCrossHatching.CheckedChanged += CheckedChanged;
			}

			DoubleBuffered = true;

			p_view.GetType().GetMethod("SetStyle",
				System.Reflection.BindingFlags.Instance |
				System.Reflection.BindingFlags.NonPublic).Invoke(p_view,
			new object[]
			{
				System.Windows.Forms.ControlStyles.UserPaint |
				System.Windows.Forms.ControlStyles.AllPaintingInWmPaint |
				System.Windows.Forms.ControlStyles.DoubleBuffer, true
			});

			UpdateOrientationUI();

			p_view.MouseWheel += new MouseEventHandler(this.panel1_MouseWheel);

			p_view.KeyDown += Form1_KeyDown;
			p_view.KeyUp += Form1_KeyUp;
		}

		private void CheckedChanged(object sender, EventArgs e)
		{
			p_view.Invalidate();
		}

		PointF ToView(PointF p)
		{
			var ps = new PointF[] { p };
			view.TransformPoints(ps);
			return ps[0];
		}

		PointF ToView(double x, double y)
		{
			var ps = new PointF[] { new PointF((float)x, (float)y) };
			view.TransformPoints(ps);
			return ps[0];
		}

		PointF FromView(PointF p)
		{
			var v = view.Clone();
			v.Invert();
			var ps = new PointF[] { p };
			v.TransformPoints(ps);
			return ps[0];
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			p_view.AllowDrop = true;
			AllowDrop = true;
		}

		private void panel1_MouseWheel(object sender, MouseEventArgs e)
		{
			var p = FromView(new PointF(mouseX, mouseY));

			float s2 = view.Elements[0];
			view.Translate(p.X, p.Y);
			float t = -e.Delta / 120.0f;
			float s = 1.0f - t * 0.125f;

			view.Scale(s, s, MatrixOrder.Prepend);

			view.Translate(-p.X, -p.Y);
			p_view.Invalidate();
		}

		public Element PickElement(PointF point)
		{
			if (document == null)
				return null;
			foreach (Element e in document.elements)
			{
				if (e.Contains(point.X, point.Y))
					return e;
			}

			return null;
		}

		private void Form1_KeyDown(object sender, KeyEventArgs e)
		{
			if (ModifierKeys == Keys.Control && e.KeyCode == Keys.F)
			{
				// textBox1.Focus();
				// textBox1.SelectAll();
			}

			if (e.KeyCode == Keys.Home)
				HomeView();
		}

		void HomeView()
		{
			if (document == null || !document.IsValid)
				return;
			var bounds = document.Bounds;
			float s = Math.Min(p_view.Width / bounds.width, p_view.Height / bounds.height) * 0.75f;
			view.Reset();
			view.Translate((float)p_view.Width / 2, (float)p_view.Height / 2);
			view.Scale(s, s);
			view.Translate(-bounds.centerX, -bounds.centerY);
			p_view.Invalidate();
		}

		private void Form1_KeyUp(object sender, KeyEventArgs e)
		{
		}

		private void p_view_MouseMove(object sender, MouseEventArgs e)
		{
			mouseMoved = true;
			var np = FromView(new PointF((float)e.X, (float)e.Y));
			// delta in view
			var v2 = view.Clone();
			v2.Invert();
			var p = new PointF((float)(e.X - mouseX), (float)(e.Y - mouseY));
			var ps = new PointF[] { p };
			v2.TransformVectors(ps);
			var delta = ps[0];


			if (e.Button == MouseButtons.Left)
			{
				if (dragElement != null)
				{
					dragElement.transform.Translate(delta.X, delta.Y);
				}
				else
				{
					// strafe
					view.Translate(delta.X, delta.Y);
				}
				p_view.Invalidate();
			}

			p_view.Invalidate();

			mouseX = e.X;
			mouseY = e.Y;
		}

		private void button2_Click(object sender, EventArgs e)
		{
			if (document == null)
				return;
			var dialog = new PrintPreviewDialog();
			dialog.Document = document.GetPrintDocument();
			dialog.ShowDialog();
		}

		private void toolStripSplitButton1_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void printToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (document == null)
				return;
			PrintDialog dialog = new PrintDialog();
			dialog.Document = document.GetPrintDocument();
			if (dialog.ShowDialog() == DialogResult.OK)
				dialog.Document.Print();
		}

		private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (document == null)
				return;
			PrintPreviewDialog dialog = new PrintPreviewDialog();
			dialog.Document = document.GetPrintDocument();
			if (dialog.ShowDialog() == DialogResult.OK)
				dialog.Document.Print();
		}

		private void p_view_Paint(object sender, PaintEventArgs e)
		{
			if (document == null)
				return;

			e.Graphics.Transform = view;
			document.Render(e.Graphics, true, showCrossHatching.Checked, showPages.Checked);

			var np = FromView(new PointF((float)mouseX, (float)mouseY));

			var p2 = FromView(firstPoint);

			e.Graphics.DrawLine(Pens.Black, np, p2);

			// if (selectedElement != null && selectedElement.Contains(np.X, np.Y))
			//     e.Graphics.FillRectangle(Brushes.Red, 0, 0, 100, 100);

			if (selectedElement != null)
			{
				selectedElement.RenderHilight(e.Graphics);
			}
		}

		private void newToolStripMenuItem_Click(object sender, EventArgs e)
		{
			document = null;
		}

		private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			new Settings().ShowDialog();
			if (document == null)
				return;

			HomeView();
		}

		private void loadToolStripMenuItem_Click(object sender, EventArgs e)
		{
			openFileDialog1.CheckFileExists = true;
			if (openFileDialog1.ShowDialog() == DialogResult.OK)
			{
				ImportNewDocument(openFileDialog1.FileName);
			}
		}

		public void ImportNewDocument(string file)
		{
			if (document == null)
				document = new Document();

			var element = Import(file);
			if (element != null)
				document.elements.Add(element);

			HomeView();
		}

		public void UpdateOrientationUI()
		{
			portraitToolStripMenuItem.Checked = Settings.pageOrientation == Settings.PageOrientation.Portrait;
			landscapeToolStripMenuItem.Checked = Settings.pageOrientation == Settings.PageOrientation.Landscape;
			autoToolStripMenuItem.Checked = Settings.pageOrientation == Settings.PageOrientation.Auto;
			switch (Settings.pageOrientation)
			{
				case Settings.PageOrientation.Portrait:
					toolStripDropDownButton1.Text = portraitToolStripMenuItem.Text;
					break;
				case Settings.PageOrientation.Landscape:
					toolStripDropDownButton1.Text = landscapeToolStripMenuItem.Text;
					break;
				case Settings.PageOrientation.Auto:
					toolStripDropDownButton1.Text = autoToolStripMenuItem.Text;
					break;
			}
			p_view.Invalidate();
		}

		private void portraitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Settings.pageOrientation = Settings.PageOrientation.Portrait;
			UpdateOrientationUI();
		}

		private void landscapeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Settings.pageOrientation = Settings.PageOrientation.Landscape;
			UpdateOrientationUI();
		}

		private void autoToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Settings.pageOrientation = Settings.PageOrientation.Auto;
			UpdateOrientationUI();
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (!Settings.SaveSettings())
				MessageBox.Show("Failed to save settings.", "Error", MessageBoxButtons.OK);
		}

		private void githubToolStripMenuItem_Click(object sender, EventArgs e)
		{
			System.Diagnostics.Process.Start($"https://github.com/roenyroeny/biggerprint");
		}

		private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			new AboutBox1().ShowDialog();
		}

		private void p_view_MouseDown(object sender, MouseEventArgs e)
		{
			mouseMoved = false;
			var np = FromView(new PointF((float)mouseX, (float)mouseY));

			if (selectedElement != null && selectedElement.Contains(np.X, np.Y))
			{
				if (e.Button == MouseButtons.Left)
					operation = new OperationDrag { element = selectedElement };

				if (e.Button == MouseButtons.Right)
					contextMenuStrip1.Show(PointToScreen(e.Location));
			}
		}

		private void p_view_MouseUp(object sender, MouseEventArgs e)
		{
			var np = FromView(new PointF((float)mouseX, (float)mouseY));

			if (mouseMoved)
				return;

			selectedElement = PickElement(np);
			p_view.Invalidate();

			dragElement = null;
		}

		private void p_view_Click(object sender, EventArgs e)
		{

		}

		private void removeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (selectedElement != null)
				RemoveElement(selectedElement);
		}

		public void RemoveElement(Element element)
		{
			if (selectedElement == null)
				return;

			document.elements.Remove(element);

			selectedElement = null;
			p_view.Invalidate();
		}

		private void resetScaleToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (selectedElement == null)
				return;

			selectedElement.transform = new Matrix();
			p_view.Invalidate();
		}

		private void p_view_DragEnter(object sender, DragEventArgs e)
		{
			string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);


			MessageBox.Show(files[0]);
		}

		private void p_view_DragDrop(object sender, DragEventArgs e)
		{

		}

		private void p_view_DragOver(object sender, DragEventArgs e)
		{

		}

		private void scaleToolStripMenuItem_Click(object sender, EventArgs e)
		{
			firstPoint = new PointF(mouseX, mouseY);
		}

		private void p_view_DragLeave(object sender, EventArgs e)
		{

		}

		Element Import(string file)
		{
			var ext = Path.GetExtension(file);
			switch (ext)
			{
				case ".dxf":
					SimpleDXF.Document doc = new SimpleDXF.Document(file);
					doc.Read();
					return new ElementDXF(doc);
				case ".jpg":
				case ".png":
					var bmap = (Bitmap)Image.FromFile(file);
					if (bmap == null)
						return null;
					return new ElementBitmap(bmap);
			}
			return null;
		}
	}
}
