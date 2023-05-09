using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SimpleDXF;

namespace biggerprint
{
    public class Document
    {

        public Document() { }

        SimpleDXF.Document dxfdoc = null;
        public Document(SimpleDXF.Document doc)
        {
            dxfdoc = doc;
        }

        public SizeF pageSize;

        const double gridsize = 50;
        public float left = 0, top = 0;
        public float width = 150, height = 150;
        public float boundsPadding = 5.0f; // 5mm


        Pen crossHatchPen = new Pen(Brushes.LightGray, 0.2f); // 0.2mm lines
        Pen pagePen = new Pen(Brushes.Red, 0.2f); // 0.4mm lines


        public void CalculateBounds()
        {
            float minx = float.MaxValue, miny = float.MaxValue;
            float maxx = float.MinValue, maxy = float.MinValue;

            foreach (var a in dxfdoc.Arcs)
            {
                float start = Utility.DegToRad((float)a.StartAngle);
                float stop = Utility.DegToRad((float)a.EndAngle);

                var rect = new RectangleF((float)a.Center.X + (float)a.Radius, (float)a.Center.Y + (float)a.Radius, (float)a.Radius * 2.0f, (float)a.Radius * 2.0f);
                var bounds = Utility.GetArcBounds(rect, start, stop-start);

                minx = Math.Min(minx, bounds.Left);
                miny = Math.Min(miny, bounds.Top);
                maxx = Math.Max(maxx, bounds.Right);
                maxy = Math.Max(maxy, bounds.Bottom);
            }
            foreach (var a in dxfdoc.Lines)
            {
                minx = Math.Min(minx, (float)(a.P1.X));
                miny = Math.Min(miny, -(float)(a.P1.Y));
                maxx = Math.Max(maxx, (float)(a.P1.X));
                maxy = Math.Max(maxy, -(float)(a.P1.Y));

                minx = Math.Min(minx, (float)(a.P2.X));
                miny = Math.Min(miny, -(float)(a.P2.Y));
                maxx = Math.Max(maxx, (float)(a.P2.X));
                maxy = Math.Max(maxy, -(float)(a.P2.Y));
            }
            foreach (var p in dxfdoc.Polylines)
            {
                for (int i = 0; i < p.Vertexes.Count; i++)
                {
                    var a = p.Vertexes[i];
                    var b = p.Vertexes[(i + 1) % p.Vertexes.Count];
                    PointF A = new PointF((float)a.Position.X, -(float)a.Position.Y);
                    PointF B = new PointF((float)b.Position.X, -(float)b.Position.Y);

                    if (i < p.Vertexes.Count - 1 || p.Closed)
                    {
                        float bulge = (float)p.Vertexes[i].Bulge;
                        var bounds = Utility.GetBulgedLineBounds(A, B, bulge);
                        minx = Math.Min(minx, bounds.Left);
                        miny = Math.Min(miny, bounds.Top);
                        maxx = Math.Max(maxx, bounds.Right);
                        maxy = Math.Max(maxy, bounds.Bottom);
                    }
                }
            }

            minx -= boundsPadding;
            miny -= boundsPadding;
            maxx += boundsPadding;
            maxy += boundsPadding;


            left = minx;
            top = miny;
            width = maxx - minx;
            height = maxy - miny;
        }


        public int PagesRequiredX(SizeF pageSize)
        {
            return (int)Math.Ceiling((double)(width / pageSize.Width));
        }

        public int PagesRequiredY(SizeF pageSize)
        {
            return (int)Math.Ceiling((double)(height / pageSize.Height));
        }

        public int PagesRequired(SizeF pageSize)
        {
            return PagesRequiredX(pageSize) * PagesRequiredY(pageSize);
        }

        public void Render(Graphics g, Matrix transform = null, bool showCrossHatch = false, bool showPages = false)
        {
            if (transform != null)
                g.MultiplyTransform(transform);

            if (showCrossHatch)
            {
                g.SetClip(new RectangleF(left, top, width, height));
                for (double x = left; x < left + width; x += gridsize)
                {
                    for (double y = top; y < top + height; y += gridsize)
                    {
                        g.DrawLine(crossHatchPen, (float)x, (float)y, (float)(x + gridsize), (float)(y + gridsize));
                        g.DrawLine(crossHatchPen, (float)(x + gridsize), (float)y, (float)x, (float)(y + gridsize));
                    }
                }
                g.ResetClip();
            }

            if (showPages)
            {
                for (double x = left; x < left + width; x += pageSize.Width)
                {
                    for (double y = top; y < top + height; y += pageSize.Height)
                    {
                        g.DrawRectangle(pagePen, (float)x, (float)y, pageSize.Width, pageSize.Height);
                    }
                }
            }

            foreach (var a in dxfdoc.Arcs)
            {
                var rect = new RectangleF((float)(a.Center.X - a.Radius), -(float)(a.Center.Y - a.Radius), (float)a.Radius * 2.0f, -(float)a.Radius * 2.0f);
                g.DrawArc(Pens.Black, rect, (float)a.StartAngle, (float)(a.EndAngle - a.StartAngle));
            }

            foreach (var a in dxfdoc.Lines)
            {
                g.DrawLine(Pens.Black, (float)a.P1.X, -(float)a.P1.Y, (float)a.P2.X, -(float)a.P2.Y);
            }

            foreach (var p in dxfdoc.Polylines)
            {
                GraphicsPath path = new GraphicsPath();
                for (int i = 0; i < p.Vertexes.Count; i++)
                {
                    var a = p.Vertexes[i];
                    var b = p.Vertexes[(i + 1) % p.Vertexes.Count];
                    PointF A = new PointF((float)a.Position.X, -(float)a.Position.Y);
                    PointF B = new PointF((float)b.Position.X, -(float)b.Position.Y);

                    if (i < p.Vertexes.Count - 1 || p.Closed)
                    {
                        float bulge = (float)p.Vertexes[i].Bulge;

                        if (i == 2)
                            continue;

                        Utility.DrawBulgedLine(g, Pens.Black, A, B, bulge);

                        var rect = Utility.GetBulgedLineBounds(A, B, bulge);
                        g.DrawRectangle(Pens.Green, rect.X, rect.Y, rect.Width, rect.Height);
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
                mat.Translate(-pageIndexX * pageSize.Width - left, -pageIndexY * pageSize.Height - top);
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
}
