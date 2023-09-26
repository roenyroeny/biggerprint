using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlTypes;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Net.Sockets;

namespace biggerprint
{
    public class Document
    {
        const double gridsize = 50;
        public float boundsPadding = 5.0f; // 5mm
        public AABB pagesBounds
        {
            get
            {
                var bounds = Bounds;
                float w = PagesRequiredX * pageSizeWithoutPaddingCallibrated.Width;
                float h = PagesRequiredY * pageSizeWithoutPaddingCallibrated.Height;
                float cx = (bounds.minx + bounds.maxx) / 2.0f;
                float cy = (bounds.miny + bounds.maxy) / 2.0f;
                return new AABB(cx - w / 2.0f, cy - h / 2.0f, cx + w / 2.0f, cy + h / 2.0f);
            }
        }

        public bool IsValid { get { return elements.Count > 0; } }

        Pen crossHatchPen = new Pen(Brushes.Gray, 0.2f); // 0.2mm lines
        Pen pagePen = new Pen(Color.FromArgb(128, 255, 64, 64), 0.2f); // 0.4mm lines

        public List<Element> elements = new List<Element>();

        public Document()
        {
            pagePen.DashOffset = 0.25f;
            pagePen.DashStyle = DashStyle.Dash;
        }

        public AABB Bounds
        {
            get
            {
                var bounds = AABB.Empty();
                bool first = true;
                foreach (var e in elements)
                {
                    var b = e.GetTransformedBounds();
                    if (first)
                        bounds = b;
                    else
                        bounds = AABB.Add(bounds, b);
                    first = false;
                }

                bounds.minx -= boundsPadding;
                bounds.miny -= boundsPadding;
                bounds.maxx += boundsPadding;
                bounds.maxy += boundsPadding;
                return bounds;
            }
        }

        public Settings.PageOrientation pageOrientation
        {
            get
            {
                if (Settings.pageOrientation == Settings.PageOrientation.Auto)
                {
                    var bounds = Bounds;
                    // try portrait
                    int pcount = (int)Math.Ceiling((double)(bounds.width / Settings.pageSizeWithoutPaddingCallibrated.Width)) *
                        (int)Math.Ceiling((double)(bounds.height / Settings.pageSizeWithoutPaddingCallibrated.Height));

                    int lcount = (int)Math.Ceiling((double)(bounds.width / Settings.pageSizeWithoutPaddingCallibrated.Height)) *
                        (int)Math.Ceiling((double)(bounds.height / Settings.pageSizeWithoutPaddingCallibrated.Width));

                    return pcount < lcount ? Settings.PageOrientation.Portrait : Settings.PageOrientation.Landscape;

                }
                else
                    return Settings.pageOrientation;
            }
        }

        public SizeF pageSizeCallibrated
        {
            get
            {
                if (pageOrientation == Settings.PageOrientation.Portrait)
                    return Settings.pageSizeCallibrated;
                else
                    return new SizeF(Settings.pageSizeCallibrated.Height, Settings.pageSizeCallibrated.Width);
            }
        }
        public SizeF pageSizeWithoutPaddingCallibrated
        {
            get
            {
                if (pageOrientation == Settings.PageOrientation.Portrait)
                    return Settings.pageSizeWithoutPaddingCallibrated;
                else
                    return new SizeF(Settings.pageSizeWithoutPaddingCallibrated.Height, Settings.pageSizeWithoutPaddingCallibrated.Width);
            }
        }

        public int PagesRequiredX { get { return (int)Math.Ceiling((double)(Bounds.width / pageSizeWithoutPaddingCallibrated.Width)); } }

        public int PagesRequiredY { get { return (int)Math.Ceiling((double)(Bounds.height / pageSizeWithoutPaddingCallibrated.Height)); } }

        public int PagesRequired { get { return PagesRequiredX * PagesRequiredY; } }

        public AABB GetPageWithoutPadding(int x, int y)
        {
            return new AABB(
                pagesBounds.minx + pageSizeWithoutPaddingCallibrated.Width * x,
                pagesBounds.miny + pageSizeWithoutPaddingCallibrated.Height * y,
                pagesBounds.minx + pageSizeWithoutPaddingCallibrated.Width * (x + 1),
                pagesBounds.miny + pageSizeWithoutPaddingCallibrated.Height * (y + 1)
                );
        }

        public AABB GetPage(int x, int y)
        {
            var page = GetPageWithoutPadding(x, y);
            return new AABB(
                page.minx - Settings.paddingSize,
                page.miny - Settings.paddingSize,
                page.maxx + Settings.paddingSize,
                page.maxy + Settings.paddingSize
                );
        }

        public void Render(Graphics g, bool preview = false, bool showCrossHatch = false, bool showPages = false)
        {
            var pagebounds = pagesBounds;
            if (preview)
                g.FillRectangle(Brushes.LightGray, pagebounds.RectangleF);

            foreach (var e in elements)
                e.Render(g);

            if (showCrossHatch)
            {
                g.SetClip(pagesBounds.RectangleF);

                for (double y = pagebounds.miny; y < pagebounds.maxy - pagesBounds.width * 0.01f; y += gridsize)
                {
                    for (double x = pagebounds.minx; x < pagebounds.maxx - pagesBounds.height * 0.01f; x += gridsize)
                    {
                        g.DrawLine(crossHatchPen, (float)x, (float)y, (float)(x + gridsize), (float)(y + gridsize));
                        g.DrawLine(crossHatchPen, (float)(x + gridsize), (float)y, (float)x, (float)(y + gridsize));
                    }
                }
                g.ResetClip();
            }

            if (showPages)
            {
                for (int y = 0; y < PagesRequiredY; y++)
                {
                    for (int x = 0; x < PagesRequiredX; x++)
                    {
                        // var pagepad = GetPageWithoutPadding(x, y);
                        // g.DrawRectangles(pagePen, new[] { pagepad.RectangleF });

                        var page = GetPage(x, y);
                        g.DrawRectangles(pagePen, new[] { page.RectangleF });
#if false
                        page.minx += 5.0f;
                        page.miny += 5.0f;
                        page.maxx -= 5.0f;
                        page.maxy -= 5.0f;
                        g.DrawRectangles(pagePen, new[] { page.RectangleF });
#endif
                    }
                }
            }
        }

        public PrintDocument GetPrintDocument()
        {
            PrintDocument pdoc = new PrintDocument();
            if (pageOrientation == Settings.PageOrientation.Landscape)
                pdoc.DefaultPageSettings.Landscape = true;

            int pageIndexX = 0;
            int pageIndexY = 0;
            int pageCountX = PagesRequiredX;
            int pageCountY = PagesRequiredY;
            pdoc.PrintPage += (object s, PrintPageEventArgs ev) =>
            {
                ev.HasMorePages = true;

                Matrix mat = new Matrix();
                var freedomScale = Utility.Unfreedom(new SizeF(1, 1));

                AABB page = GetPage(pageIndexX, pageIndexY);
                if (pageOrientation == Settings.PageOrientation.Portrait)
                    mat.Scale((float)ev.PageSettings.PrintableArea.Width / page.width, (float)ev.PageSettings.PrintableArea.Height / page.height);
                else
                    mat.Scale((float)ev.PageSettings.PrintableArea.Height / page.width, (float)ev.PageSettings.PrintableArea.Width / page.height);
                mat.Translate(-page.minx, -page.miny);

                ev.Graphics.Transform = mat;
                Render(ev.Graphics, false, true, false);

                pageIndexX++;
                if (pageIndexX == pageCountX)
                {
                    pageIndexX = 0;
                    pageIndexY++;
                    if (pageIndexY == pageCountY)
                        ev.HasMorePages = false;
                }
            };
            return pdoc;
        }
    }
}
