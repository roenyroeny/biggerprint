using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace biggerprint
{
    public class Document
    {
        public Document() { }

        public SizeF pageSize;

        const double gridsize = 50;
        public AABB bounds = AABB.Empty();
        public float boundsPadding = 5.0f; // 5mm

        Pen crossHatchPen = new Pen(Brushes.LightGray, 0.2f); // 0.2mm lines
        Pen pagePen = new Pen(Brushes.Red, 0.2f); // 0.4mm lines

        public List<Element> elements = new List<Element>();

        public void CalculateBounds()
        {
            bounds = AABB.Empty();
            bool first = true;
            foreach (var e in elements)
            {
                var b = e.GetBounds();
                if (first)
                    bounds = b;
                else
                    bounds = AABB.Add(bounds, b);
                first = false;
            }

            bounds.minx -= boundsPadding;
            bounds.miny -= boundsPadding;
            bounds.maxx += boundsPadding;
            bounds.maxy += boundsPadding;
        }


        public int PagesRequiredX(SizeF pageSize)
        {
            return (int)Math.Ceiling((double)(bounds.width / pageSize.Width));
        }

        public int PagesRequiredY(SizeF pageSize)
        {
            return (int)Math.Ceiling((double)(bounds.height / pageSize.Height));
        }

        public int PagesRequired(SizeF pageSize)
        {
            return PagesRequiredX(pageSize) * PagesRequiredY(pageSize);
        }

        public void Render(Graphics g, Matrix transform = null, bool showCrossHatch = false, bool showPages = false)
        {
            if (transform != null)
                g.MultiplyTransform(transform);

            foreach (var e in elements)
                e.Render(g);

            if (showCrossHatch)
            {
                g.SetClip(bounds.RectangleF);
                for (double y = bounds.miny; y < bounds.maxy; y += gridsize)
                {
                    for (double x = bounds.minx; x < bounds.maxx; x += gridsize)
                    {
                        g.DrawLine(crossHatchPen, (float)x, (float)y, (float)(x + gridsize), (float)(y + gridsize));
                        g.DrawLine(crossHatchPen, (float)(x + gridsize), (float)y, (float)x, (float)(y + gridsize));
                    }
                }
                g.ResetClip();
            }

            if (showPages)
            {
                for (double y = bounds.miny; y < bounds.maxy; y += pageSize.Height)
                {
                    for (double x = bounds.minx; x < bounds.maxx; x += pageSize.Width)
                    {
                        g.DrawRectangle(pagePen, (float)x, (float)y, pageSize.Width, pageSize.Height);
                    }
                }
            }
        }

        public PrintDocument GetPrintDocument()
        {
            PrintDocument pdoc = new PrintDocument();

            int pageIndexX = 0;
            int pageIndexY = 0;
            int pageCountX = 0;
            int pageCountY = 0;
            pdoc.PrintPage += (object s, PrintPageEventArgs ev) =>
            {
                ev.HasMorePages = true;
                if (pageIndexX == 0 && pageIndexY == 0)
                {
                    pageCountX = PagesRequiredX(pageSize);
                    pageCountY = PagesRequiredY(pageSize);
                }

                Matrix mat = new Matrix();
                var scale = Utility.Unfreedom(new SizeF(1, 1));
                mat.Scale(1.0f / scale.Width, 1.0f / scale.Height); // unfreedom
                mat.Translate(-pageIndexX * pageSize.Width -bounds.minx, -pageIndexY * pageSize.Height - bounds.miny);
                Render(ev.Graphics, mat, true, false);

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
    }

    public abstract class Element
    {
        public abstract void Render(Graphics g);

        public abstract AABB GetBounds();
    }
}
