using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace biggerprint
{
    public abstract class Element
    {
        public Matrix transform = new Matrix();
        public abstract void Render(Graphics g);

        public void RenderHilight(Graphics g)
        {

            var xform = g.Transform.Clone();
            g.MultiplyTransform(transform);
            var borderpen = new Pen(Brushes.Black, 0.1f);

            var aabb = GetBounds();
            g.DrawRectangles(borderpen, new RectangleF[] { aabb.RectangleF });
            g.FillRectangle(new SolidBrush(Color.FromArgb(16, 255, 0, 0)), aabb.RectangleF);
            g.Transform = xform;
        }

        public abstract AABB GetBounds();

        public bool Contains(float x, float y)
        {
            var t = transform.Clone();
            t.Invert();
            var points = new PointF[] { new PointF(x, y) };
            t.TransformPoints(points);

            return GetBounds().Contains(points[0].X, points[0].Y);
        }

        public AABB GetTransformedBounds()
        {
            var bounds = GetBounds();
            var points = new PointF[] {
                new PointF(bounds.minx, bounds.miny),
                new PointF(bounds.maxx, bounds.miny),
                new PointF(bounds.minx, bounds.maxy),
                new PointF(bounds.maxx, bounds.maxy)
            };
            transform.TransformPoints(points);
            float minx = Math.Min(Math.Min(points[0].X, points[1].X), Math.Min(points[2].X, points[3].X));
            float maxx = Math.Max(Math.Max(points[0].X, points[1].X), Math.Max(points[2].X, points[3].X));
            float miny = Math.Min(Math.Min(points[0].Y, points[1].Y), Math.Min(points[2].Y, points[3].Y));
            float maxy = Math.Max(Math.Max(points[0].Y, points[1].Y), Math.Max(points[2].Y, points[3].Y));

            return new AABB(minx, miny, maxx, maxy);
        }
    }
}
