using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.IO;
using System.Windows.Forms;

namespace biggerprint
{
    public partial class Form1 : Form
    {
        Document document;
        public Form1()
        {
            document = new Document();
            InitializeComponent();
        }

        Pen gridPen = new Pen(Brushes.Black, 1);

        private void button1_Click(object sender, EventArgs e)
        {
            PrintDialog dialog = new PrintDialog();
            dialog.Document = GetPrintDocument(document);
            if(dialog.ShowDialog() == DialogResult.OK)
                dialog.Document.Print();
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
                var pageSize = ev.Graphics.VisibleClipBounds.Size;
                // unfreedom pageSize
                pageSize = new SizeF(pageSize.Width * (float)Document.scaleX, pageSize.Height * (float)Document.scaleY);

                ev.HasMorePages = true;
                if (pageIndexX == 0 && pageIndexY == 0)
                {
                    pageCountX = doc.PagesRequiredX(pageSize);
                    pageCountY = doc.PagesRequiredY(pageSize);
                }
                doc.Render(ev.Graphics, pageIndexX * pageSize.Width, pageIndexY * pageSize.Height);

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

            var dialog = new PrintPreviewDialog();
            dialog.Document = GetPrintDocument(document);
            dialog.ShowDialog();
        }
    }
}
