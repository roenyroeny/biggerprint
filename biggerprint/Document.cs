using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

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
        public float boundsPadding = 10.0f; // 10mm

        // error scale. 1mm isnt exactly 1mm in reallife after printing
        public const float scaleX = 0.9649f;
        public const float scaleY = 0.9674f;

        Pen crossHatchPen = new Pen(Brushes.LightGray, 0.2f); // 0.2mm lines
        Pen pagePen = new Pen(Brushes.Red, 0.2f); // 0.4mm lines


        public void CalculateBounds()
        {
            float minx = float.MaxValue, miny = float.MaxValue;
            float maxx = float.MinValue, maxy = float.MinValue;

            foreach (var a in dxfdoc.Arcs)
            {
                float start = (float)a.StartAngle / 57.2957795f;
                float stop = (float)a.EndAngle / 57.2957795f;
                start %= ((float)Math.PI * 2.0f);
                stop %= ((float)Math.PI * 2.0f);


                minx = Math.Min(minx, (float)(a.Center.X + Math.Cos(start) * a.Radius));
                miny = Math.Min(miny, -(float)(a.Center.Y + Math.Sin(start) * a.Radius));
                minx = Math.Min(minx, (float)(a.Center.X + Math.Cos(stop) * a.Radius));
                miny = Math.Min(miny, -(float)(a.Center.Y + Math.Sin(stop) * a.Radius));

                for (float v = 0; v < (float)Math.PI * 2.0f; v += (float)Math.PI / 2.0f)
                {
                    if (v > start && v < stop)
                    {
                        minx = Math.Min(minx, (float)(a.Center.X + Math.Cos(v) * a.Radius));
                        miny = Math.Min(miny, -(float)(a.Center.Y + Math.Sin(v) * a.Radius));
                        maxx = Math.Max(maxx, (float)(a.Center.X + Math.Cos(v) * a.Radius));
                        maxy = Math.Max(maxy, -(float)(a.Center.Y + Math.Sin(v) * a.Radius));
                    }
                }
            }
            foreach (var a in dxfdoc.Lines)
            {
                minx = Math.Min(minx, (float)(a.P1.X));
                miny = Math.Min(miny, -(float)(a.P1.Y));
                maxx = Math.Max(maxx, (float)(a.P1.X));
                maxy = Math.Max(maxy,- (float)(a.P1.Y));

                minx = Math.Min(minx, (float)(a.P2.X));
                miny = Math.Min(miny, -(float)(a.P2.Y));
                maxx = Math.Max(maxx, (float)(a.P2.X));
                maxy = Math.Max(maxy, -(float)(a.P2.Y));
            }
            foreach (var a in dxfdoc.Polylines)
            {
                List<PointF> points = new List<PointF>();
                foreach (var p in a.Vertexes)
                {
                    minx = Math.Min(minx, (float)(p.Position.X));
                    miny = Math.Min(miny, -(float)(p.Position.Y));
                    maxx = Math.Max(maxx, (float)(p.Position.X));
                    maxy = Math.Max(maxy, -(float)(p.Position.Y));
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
            // g.ScaleTransform(1.0f / (float)scaleX, 1.0f / (float)scaleY); // do this externally
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

            if (showPages && pageSize.Width != 0 && pageSize.Height != 0)
                for (double x = left; x < left + width; x += pageSize.Width)
                {
                    for (double y = top; y < top + height; y += pageSize.Height)
                    {
                        g.DrawRectangle(pagePen, (float)x, (float)y, pageSize.Width, pageSize.Height);
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

            foreach (var a in dxfdoc.Polylines)
            {
                List<PointF> points = new List<PointF>();
                foreach (var p in a.Vertexes)
                    points.Add(new PointF((float)p.Position.X, -(float)p.Position.Y));
                g.DrawPolygon(Pens.Black, points.ToArray());
            }
        }
    }
}
