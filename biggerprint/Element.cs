using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
	}
}
