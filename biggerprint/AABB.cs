using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace biggerprint
{
    public  class AABB
    {
        public float minx, miny, maxx, maxy;

        public float width { get { return maxx- minx; } }
        public float height{ get { return maxy - miny; } }
        public RectangleF RectangleF { get { return new RectangleF(minx, miny, maxx - minx, maxy - miny); } }

        public AABB(float minx, float miny, float maxx, float maxy)
        {
            this.minx = minx;
            this.miny = miny;
            this.maxx = maxx;
            this.maxy = maxy;
        }

        public static AABB Empty()
        {
            return new AABB ( float.MaxValue, float.MaxValue, float.MinValue, float.MinValue );
        }

        public static AABB Add(AABB a, AABB b)
        {
            float minx = Math.Min(a.minx, b.minx);
            float miny = Math.Min(a.miny, b.miny);
            float maxx = Math.Max(a.maxx, b.maxx);
            float maxy = Math.Max(a.maxy, b.maxy);
            return new AABB(minx, miny, maxx, maxy);
        }

        public static AABB Add(AABB a,float x, float y)
        {
            float minx = Math.Min(a.minx, x);
            float miny = Math.Min(a.miny, y);
            float maxx = Math.Max(a.maxx, x);
            float maxy = Math.Max(a.maxy, y);
            return new AABB(minx, miny, maxx, maxy);
        }
    }
}
