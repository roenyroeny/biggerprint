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
        public static float mod(float x, float m)
        {
            return (x % m + m) % m;
        }
        public static float DegToRad(float deg)
        {
            return deg / 57.2957795f;
        }
        public static float RadToDeg(float rad)
        {
            return rad * 57.2957795f;
        }

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

        public static void DrawBulgedLine(Graphics g, Pen p, PointF A, PointF B, float bulge)
        {
            if (Math.Abs(bulge) < 0.01f)
            {
                g.DrawLine(Pens.Black, A, B);
            }
            else
            {
                // http://www.lee-mac.com/bulgeconversion.html

                // bulge = tan(angle/4)
                // angle = 4 * atan(bulge)

                float dx = (A.X - B.X) / 2.0f;
                float dy = (A.Y - B.Y) / 2.0f;

                float cx = (A.X + B.X) / 2.0f;
                float cy = (A.Y + B.Y) / 2.0f;

                float Dx = cx + dy * (float)bulge;
                float Dy = cy - dx * (float)bulge;

                {
                    // thanks chatgpt
                    // Define the three points.
                    PointF pointA = A;
                    PointF pointB = B;
                    PointF pointC = new PointF(Dx, Dy);

                    // Calculate the center and radius of the circle that passes through the three points.
                    float _a = pointB.X - pointA.X;
                    float _b = pointB.Y - pointA.Y;
                    float c = pointC.X - pointA.X;
                    float d = pointC.Y - pointA.Y;
                    float e = _a * (pointA.X + pointB.X) + _b * (pointA.Y + pointB.Y);
                    float f = c * (pointA.X + pointC.X) + d * (pointA.Y + pointC.Y);
                    float _g = 2.0f * (_a * (pointC.Y - pointB.Y) - _b * (pointC.X - pointB.X));
                    float centerX = (d * e - _b * f) / _g;
                    float centerY = (_a * f - c * e) / _g;
                    float radius = (float)Math.Sqrt(Math.Pow(pointA.X - centerX, 2) + Math.Pow(pointA.Y - centerY, 2));

                    // Calculate the start and end angles of the arc.
                    float startAngle = (float)Math.Atan2(pointA.Y - centerY, pointA.X - centerX);
                    float endAngle = (float)Math.Atan2(pointB.Y - centerY, pointB.X - centerX);
                    float sweepAngle = endAngle - startAngle;

                    g.DrawRectangle(Pens.Green, centerX, centerY, 5, 5);

                    // Draw the arc.
                    g.DrawArc(p, centerX - radius, centerY - radius, radius * 2, radius * 2, RadToDeg(startAngle), RadToDeg(sweepAngle));
                }
            }
        }

        public static RectangleF GetArcBounds(RectangleF rect, float startAngle, float sweepAngle)
        {
            float start = startAngle;
            float stop = startAngle + sweepAngle;
            start = mod(start, (float)Math.PI * 2.0f);
            stop = mod(stop, (float)Math.PI * 2.0f);

            float centerX = rect.Left + rect.Width / 2;
            float centerY = rect.Top + rect.Height / 2;

            float minx = float.MaxValue, miny = float.MaxValue;
            float maxx = float.MinValue, maxy = float.MinValue;

            float radiusX = rect.Width / 2;
            float radiusY = rect.Height / 2;

            minx = Math.Min(minx, (float)(centerX + Math.Cos(start) * radiusX));
            maxx = Math.Max(maxx, (float)(centerX + Math.Cos(start) * radiusX));

            miny = Math.Min(miny, (float)(centerY + Math.Sin(start) * radiusY));
            maxy = Math.Max(maxy, (float)(centerY + Math.Sin(start) * radiusY));

            minx = Math.Min(minx, (float)(centerX + Math.Cos(stop) * radiusX));
            maxx = Math.Max(maxx, (float)(centerX + Math.Cos(stop) * radiusX));

            miny = Math.Min(miny, (float)(centerY + Math.Sin(stop) * radiusY));
            maxy = Math.Max(maxy, (float)(centerY + Math.Sin(stop) * radiusY));


            for (float v = 0; v <= (float)Math.PI * 2.0f; v += (float)Math.PI / 2.0f)
            {
                if (v >= start && v <= stop)
                {
                    minx = Math.Min(minx, (float)(centerX + Math.Cos(v) * radiusX));
                    maxx = Math.Max(maxx, (float)(centerX + Math.Cos(v) * radiusX));
                    miny = Math.Min(miny, (float)(centerY + Math.Sin(v) * radiusY));
                    maxy = Math.Max(maxy, (float)(centerY + Math.Sin(v) * radiusY));
                }
            }
            return new RectangleF(minx, miny, maxx - minx, maxy - miny);
        }


        public static RectangleF GetBulgedLineBounds(PointF A, PointF B, float bulge)
        {
            if (Math.Abs(bulge) < 0.01f)
            {
                float minx = Math.Min(A.X, B.X), miny = Math.Min(A.Y, B.Y);
                float maxx = Math.Max(A.X, B.X), maxy = Math.Max(A.Y, B.Y);

                return new RectangleF(minx, miny, maxx - minx, maxy - miny);
            }
            else
            {
                // http://www.lee-mac.com/bulgeconversion.html

                // bulge = tan(angle/4)
                // angle = 4 * atan(bulge)

                float dx = (A.X - B.X) / 2.0f;
                float dy = (A.Y - B.Y) / 2.0f;

                float cx = (A.X + B.X) / 2.0f;
                float cy = (A.Y + B.Y) / 2.0f;

                float Dx = cx + dy * (float)bulge;
                float Dy = cy - dx * (float)bulge;


                // thanks chatgpt
                // Define the three points.
                PointF pointA = A;
                PointF pointB = B;
                PointF pointC = new PointF(Dx, Dy);

                // Calculate the center and radius of the circle that passes through the three points.
                float _a = pointB.X - pointA.X;
                float _b = pointB.Y - pointA.Y;
                float c = pointC.X - pointA.X;
                float d = pointC.Y - pointA.Y;
                float e = _a * (pointA.X + pointB.X) + _b * (pointA.Y + pointB.Y);
                float f = c * (pointA.X + pointC.X) + d * (pointA.Y + pointC.Y);
                float _g = 2.0f * (_a * (pointC.Y - pointB.Y) - _b * (pointC.X - pointB.X));
                float centerX = (d * e - _b * f) / _g;
                float centerY = (_a * f - c * e) / _g;
                float radius = (float)Math.Sqrt(Math.Pow(pointA.X - centerX, 2) + Math.Pow(pointA.Y - centerY, 2));

                // Calculate the start and end angles of the arc.
                float startAngle = (float)Math.Atan2(pointA.Y - centerY, pointA.X - centerX);
                float endAngle = (float)Math.Atan2(pointB.Y - centerY, pointB.X - centerX);
                if (startAngle < 0)
                    startAngle += (float)Math.PI * 2.0f;
                if (endAngle < 0)
                    endAngle += (float)Math.PI * 2.0f;
                float sweepAngle = endAngle - startAngle;
                if (sweepAngle < 0)
                    sweepAngle += (float)Math.PI * 2.0f;


                // Draw the arc.
                return GetArcBounds(new RectangleF(centerX - radius, centerY - radius, radius * 2, radius * 2), startAngle, sweepAngle);
            }
        }
    }
}
