using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.IO;
using System.Windows.Forms;

namespace biggerprint
{
    public partial class Form1 : Form
    {
        Matrix view = new Matrix();
        int mouseX, mouseY;

        CheckBox showPages;
        CheckBox showCrossHatching;

        Document document = null;

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
        }

        private void panel1_MouseWheel(object sender, MouseEventArgs e)
        {
            /*
            if (ModifierKeys == Keys.Control)
            {
                if (selectedEdge != null)
                {
                    selectedEdge.Distance += e.Delta / 12;
                    if (selectedEdge.Distance < 0)
                        selectedEdge.Distance = 0;
                }
                if (selectedNode != null)
                {
                    selectedNode.Circumference += e.Delta / 12;
                    if (selectedNode.Circumference < 0)
                        selectedNode.Circumference = 0;
                }
            }
            else*/
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
            float s = Math.Min(p_view.Width / document.bounds.width, p_view.Height / document.bounds.height) * 0.75f;
            view.Reset();
            view.Translate((float)p_view.Width / 2, (float)p_view.Height / 2);
            view.Scale(s, s);
            view.Translate(-(document.bounds.minx + document.bounds.width / 2), -(document.bounds.miny + document.bounds.height / 2));
            p_view.Invalidate();
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
        }

        private void p_view_MouseMove(object sender, MouseEventArgs e)
        {
            var np = FromView(new PointF((float)e.X, (float)e.Y));
            /*
            if (ModifierKeys == Keys.Control)
            {
                if (dragNode != null && !dragNode.locked)
                {
                    dragNode.X = np.X;
                    dragNode.Y = np.Y;
                    selectNode(null);
                }
            }
            else*/
            {
                //if (selectedEdge == null && selectedNode == null)
                {
                    // strafe
                    if (e.Button == MouseButtons.Left)
                    {
                        var v2 = view.Clone();
                        v2.Invert();
                        var p = new PointF((float)(e.X - mouseX), (float)(e.Y - mouseY));
                        var ps = new PointF[] { p };
                        v2.TransformVectors(ps);
                        view.Translate(ps[0].X, ps[0].Y);
                        p_view.Invalidate();
                    }
                }
            }
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
            document.Render(e.Graphics, view, true, showCrossHatching.Checked, showPages.Checked);
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
            document.CalculateBounds();
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
            document = new Document();

            var element = Import(file);
            if (element != null)
                document.elements.Add(element);

            document.CalculateBounds();
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
