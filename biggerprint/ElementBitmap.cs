using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace biggerprint
{
    public class ElementBitmap: Element
    {
        public Bitmap bitmap;
        public float width, height;
        public ElementBitmap(Bitmap bmap)
        {
            bitmap = bmap;
            width = bmap.Width / 10.0f;
            height = bmap.Height / 10.0f;
        }

        public override AABB GetBounds()
        {
            return new AABB(0, 0, width, height);
        }

        public override void Render(Graphics g)
        {
            var t = g.Transform.Clone();
            g.MultiplyTransform(transform);
            g.DrawImage((Image)bitmap, GetBounds().RectangleF);

            g.Transform = t;
        }
    }
}
