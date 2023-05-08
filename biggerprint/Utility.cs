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
        public static SizeF Unfreedom(SizeF s)
        {
            return new SizeF(s.Width * 0.254f, s.Height * 0.254f);
        }
    }
}
