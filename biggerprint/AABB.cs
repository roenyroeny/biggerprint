using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleDXF;

namespace biggerprint
{
    public class AABB
    {
        public float minx, miny, maxx, maxy;

        public float width { get { return maxx - minx; } }
        public float centerX { get { return (maxx + minx) * 0.5f; } }
        public float height { get { return maxy - miny; } }
        public float centerY { get { return (maxy + miny) * 0.5f; } }
        public RectangleF RectangleF { get { return new RectangleF(minx, miny, maxx - minx, maxy - miny); } }
        // public Rectangle Rectangle { get { return new Rectangle((int)minx, (int)miny, (int)maxx - (int)minx, (int)maxy - (int)miny); } }

        public AABB(float minx, float miny, float maxx, float maxy)
        {
            this.minx = minx;
            this.miny = miny;
            this.maxx = maxx;
            this.maxy = maxy;
        }

        public static AABB Empty()
        {
            return new AABB(float.MaxValue, float.MaxValue, float.MinValue, float.MinValue);
        }

        public static AABB Add(AABB a, AABB b)
        {
            float minx = Math.Min(a.minx, b.minx);
            float miny = Math.Min(a.miny, b.miny);
            float maxx = Math.Max(a.maxx, b.maxx);
            float maxy = Math.Max(a.maxy, b.maxy);
            return new AABB(minx, miny, maxx, maxy);
        }

        public static AABB Add(AABB a, float x, float y)
        {
            float minx = Math.Min(a.minx, x);
            float miny = Math.Min(a.miny, y);
            float maxx = Math.Max(a.maxx, x);
            float maxy = Math.Max(a.maxy, y);
            return new AABB(minx, miny, maxx, maxy);
        }

        public bool Contains(float x, float y)
        {
            return x >= minx && y >= miny && x <= maxx && y <= maxy;
        }
    }
}
