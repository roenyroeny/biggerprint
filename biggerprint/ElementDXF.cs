using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace biggerprint
{
    public class ElementDXF : Element
    {
        SimpleDXF.Document dxfdoc = null;

        public ElementDXF(SimpleDXF.Document _dxfdoc)
        {
            dxfdoc = _dxfdoc;
        }

        public override AABB GetBounds()
        {
            AABB bounds = AABB.Empty();

            foreach (var a in dxfdoc.Arcs)
            {
                float start = Utility.DegToRad((float)a.StartAngle);
                float stop = Utility.DegToRad((float)a.EndAngle);

                var rect = new RectangleF((float)a.Center.X + (float)a.Radius, (float)a.Center.Y + (float)a.Radius, (float)a.Radius * 2.0f, (float)a.Radius * 2.0f);
                bounds = AABB.Add(bounds, Utility.GetArcBounds(rect, start, stop - start));
            }
            foreach (var a in dxfdoc.Lines)
            {
                bounds = AABB.Add(bounds, (float)a.P1.X, (float)-a.P1.Y);
                bounds = AABB.Add(bounds, (float)a.P2.X, (float)-a.P2.Y);
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
                        bounds = AABB.Add(bounds, Utility.GetBulgedLineBounds(A, B, bulge));
                    }
                }
            }

            return bounds;
        }

        public override void Render(Graphics g)
        {
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

                        Utility.DrawBulgedLine(g, Pens.Black, A, B, bulge);
                    }
                }
            }
        }
    }
}
