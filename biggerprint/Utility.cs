using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace biggerprint
{
    internal class Utility
    {
        public static float Freedom(float s)
        {
            return s / 0.254f;
        }

        public static float Unfreedom(float s)
        {
            return s * 0.254f;
        }
        public static SizeF Freedom(SizeF s)
        {
            return new SizeF(Freedom(s.Width), Freedom(s.Height));
        }

        public static SizeF Unfreedom(SizeF s)
        {
            return new SizeF(Unfreedom(s.Width), Unfreedom(s.Height));
        }
    }
}
