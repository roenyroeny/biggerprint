using System;
using System.Collections.Generic;
using System.Drawing;
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


        const double gridsize = 50;
        public float left = 0, top = 0;
        public float width = 150, height = 150;

        public void CalculateBounds()
        {
            float minx = float.MaxValue, miny = float.MaxValue;
            float maxx = float.MinValue, maxy = float.MinValue;

            /*
            foreach (var a in dxfdoc.Arcs)
            {
                minx = Math.Min(minx, (float)(a.Center.X - a.Radius));
                miny = Math.Min(miny, (float)(a.Center.Y - a.Radius));
                maxx = Math.Max(maxx, (float)(a.Center.X + a.Radius));
                maxy = Math.Max(maxy, (float)(a.Center.Y + a.Radius));
            }*/
            foreach (var a in dxfdoc.Lines)
            {
                minx = Math.Min(minx, (float)(a.P1.X));
                miny = Math.Min(miny, (float)(a.P1.Y));
                maxx = Math.Max(maxx, (float)(a.P1.X));
                maxy = Math.Max(maxy, (float)(a.P1.Y));

                minx = Math.Min(minx, (float)(a.P2.X));
                miny = Math.Min(miny, (float)(a.P2.Y));
                maxx = Math.Max(maxx, (float)(a.P2.X));
                maxy = Math.Max(maxy, (float)(a.P2.Y));
            }
            foreach (var a in dxfdoc.Polylines)
            {
                // List<PointF> points = new List<PointF>();
                // foreach (var p in a.Vertexes)
                //     points.Add(new PointF((float)p.Position.X, (float)p.Position.Y));
                // g.DrawPolygon(Pens.Black, points.ToArray());
            }
            left = minx;
            top = miny;
            width = maxx - minx;
            height = maxy - miny;
        }

        // magic 10thou to 1mm scale used to unfreedom everything
        public const double scaleX = 0.254 * 0.9649;
        public const double scaleY = 0.254 * 0.9674;

        Pen crossHatchPen = new Pen(Brushes.Gray, 0.2f); // 0.2mm lines

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

        public void Render(Graphics g, float offsetx = 0, float offsety = 0)
        {
            g.ScaleTransform(1.0f / (float)scaleX, 1.0f / (float)scaleY);
            g.TranslateTransform(-offsetx, -offsety);

            for (double x = 0; x < width; x += gridsize)
            {
                for (double y = 0; y < height; y += gridsize)
                {

                    g.DrawLine(crossHatchPen, (float)x, (float)y, (float)(x + gridsize), (float)(y + gridsize));
                    g.DrawLine(crossHatchPen, (float)(x + gridsize), (float)y, (float)x, (float)(y + gridsize));

                }
            }
            g.TranslateTransform(-left, -top);

            // g.DrawEllipse(Pens.Red, 0, 0, width, height);

            foreach (var a in dxfdoc.Arcs)
            {
                var rect = new RectangleF((float)(a.Center.X - a.Radius), (float)(a.Center.Y - a.Radius), (float)a.Radius * 2.0f, (float)a.Radius * 2.0f);
                g.DrawArc(Pens.Black, rect, (float)a.StartAngle, (float)a.EndAngle);
            }
            foreach (var a in dxfdoc.Lines)
            {
                g.DrawLine(Pens.Black, (float)a.P1.X, (float)a.P1.Y, (float)a.P2.X, (float)a.P2.Y);
            }
            foreach (var a in dxfdoc.Polylines)
            {
                List<PointF> points = new List<PointF>();
                foreach (var p in a.Vertexes)
                    points.Add(new PointF((float)p.Position.X, (float)p.Position.Y));
                g.DrawPolygon(Pens.Black, points.ToArray());
            }

        }
    }
}
