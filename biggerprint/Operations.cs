using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace biggerprint
{
    public abstract class Operation
    {
        public Element element;

        public abstract void OnMouseMove(int prevMouseX, int prevMouseY, int mouseX, int mouseY);
        public abstract void OnMouseDown();
        public abstract void OnMouseUp();
        public abstract void Render(Graphics g);
    };

    public class OperationDrag : Operation
    {
        public override void OnMouseMove(int prevMouseX, int prevMouseY, int mouseX, int mouseY)
        {
            var pp = Main.instance.FromView(new PointF((float)prevMouseX, (float)prevMouseY));
            var np = Main.instance.FromView(new PointF((float)mouseX, (float)mouseY));

            element.transform.Translate((float)(np.X - pp.X), (float)(np.Y - pp.Y), MatrixOrder.Append);
        }
        public override void Render(Graphics g)
        {

        }

        public override void OnMouseDown()
        {

        }

        public override void OnMouseUp()
        {
            Main.instance.operation = null;
        }
    };
    public class OperationScale : Operation
    {
        public PointF firstPoint, secondPoint;

        public override void OnMouseMove(int prevMouseX, int prevMouseY, int mouseX, int mouseY)
        {
            var np = Main.instance.FromView(new PointF((float)mouseX, (float)mouseY));

            secondPoint = np;
        }

        public override void Render(Graphics g)
        {
            var p1 = Main.instance.ToView(firstPoint);
            var p2 = Main.instance.ToView(secondPoint);
            g.DrawLine(Pens.Black, p1, p2);
        }

        public override void OnMouseDown()
        {
        }

        public override void OnMouseUp()
        {
            var view = element.transform.Clone();
            view.Invert();

            var points = new PointF[] { firstPoint, secondPoint };
            float w = secondPoint.X - firstPoint.X;
            float h = secondPoint.Y - firstPoint.Y;

            float distance = (float)Math.Sqrt((double)(w * w + h * h));

            var dialog = new ScaleDialog();
            dialog.distance = distance;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                float desiredDistance = dialog.distance;
                distance /= desiredDistance;
                element.transform.Scale(1.0f / distance, 1.0f / distance);
            }
            Main.instance.operation = null;
        }
    };
}
