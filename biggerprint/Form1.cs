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

        Document document;
        public Form1()
        {
            document = new Document();
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


            p_view.MouseWheel += new MouseEventHandler(this.panel1_MouseWheel);

            (p_view as Control).KeyDown += Form1_KeyDown;
            (p_view as Control).KeyUp += Form1_KeyUp;
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

        public PrintDocument GetPrintDocument(Document doc)
        {
            PrintDocument pdoc = new PrintDocument();

            int pageIndexX = 0;
            int pageIndexY = 0;
            int pageCountX = 0;
            int pageCountY = 0;
            pdoc.PrintPage += (object s, PrintPageEventArgs ev) =>
            {
                var pageSize = document.pageSize;

                ev.HasMorePages = true;
                if (pageIndexX == 0 && pageIndexY == 0)
                {
                    pageCountX = doc.PagesRequiredX(pageSize);
                    pageCountY = doc.PagesRequiredY(pageSize);
                }

                Matrix mat = new Matrix();
                var scale = Utility.Unfreedom(new SizeF(1, 1));
                mat.Scale(1.0f / scale.Width, 1.0f / scale.Height); // unfreedom
                mat.Translate(-pageIndexX * pageSize.Width - document.left, -pageIndexY * pageSize.Height - document.top);
                doc.Render(ev.Graphics, mat, true, false);

                pageIndexX++;
                if (pageIndexX == pageCountX)
                {
                    pageIndexX = 0;
                    pageIndexY++;
                    if (pageIndexY == pageCountY)
                        ev.HasMorePages = false;
                }
            };
            return pdoc;
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            SimpleDXF.Document doc = new SimpleDXF.Document("end.dxf");
            doc.Read();
            document = new Document(doc);
            document.CalculateBounds();
            var pdoc = GetPrintDocument(document);
            document.pageSize = Utility.Unfreedom(pdoc.DefaultPageSettings.PrintableArea.Size);
            document.pageSize = new SizeF(document.pageSize.Width * Document.scaleX, document.pageSize.Height * Document.scaleY);
            HomeView();
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
            view.Reset();
            view.Translate(document.left + document.width / 2, document.top + document.height / 2);
            view.Translate((float)p_view.Width / 2, (float)p_view.Height / 2);
            float s = Math.Min(p_view.Width / document.width, p_view.Height / document.height) * 0.75f;
            view.Scale(s, s);
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
            var dialog = new PrintPreviewDialog();
            dialog.Document = GetPrintDocument(document);
            dialog.ShowDialog();
        }

        private void toolStripSplitButton1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PrintDialog dialog = new PrintDialog();
            dialog.Document = GetPrintDocument(document);
            if (dialog.ShowDialog() == DialogResult.OK)
                dialog.Document.Print();
        }

        private void p_view_Paint(object sender, PaintEventArgs e)
        {
            document.Render(e.Graphics, view, showCrossHatching.Checked, showPages.Checked);
        }
    }
}
