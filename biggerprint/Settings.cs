﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using SimpleDXF;
using static biggerprint.Settings;

namespace biggerprint
{
    public partial class Settings : Form
    {
        const float callibrationRectSize = 100; // mm

        static float sizeX = callibrationRectSize, sizeY = callibrationRectSize;
        public static float paddingSize = 5.0f; // mm padding around page to help alignment
        public static SizeF pageSize;

        // maximum physical size printable per page. might not match what the printer reports.
        // but is callibrated so that after printing it will match in the real world
        public static SizeF pageSizeCallibrated
        {
            get
            {
                return new SizeF(pageSize.Width * scaleX, pageSize.Height * scaleY);
            }
        }
        
        public static SizeF pageSizeWithoutPaddingCallibrated
        {
            get
            {
                return new SizeF(pageSizeCallibrated.Width - paddingSize * 2, pageSizeCallibrated.Height - paddingSize * 2);
            }
        }

        public enum PageOrientation
        {
            Portrait, 
            Landscape,
            Auto
        }
        public static PageOrientation pageOrientation = PageOrientation.Auto;

        public static SizeF GetCallibratedPageSize(PageOrientation pageOrientation)
        {
            switch(pageOrientation)
            {
                case PageOrientation.Portrait:
                    return new SizeF(pageSize.Width * scaleX, pageSize.Height * scaleY);
                case PageOrientation.Landscape:
                    return new SizeF(pageSize.Height * scaleY, pageSize.Width * scaleX);
                default: throw null;
            }
        }


        public static float scaleX { get { return sizeX / callibrationRectSize; } }
        public static float scaleY { get { return sizeY / callibrationRectSize; } }

        public Settings()
        {
            InitializeComponent();
            numericUpDown1.Value = (decimal)sizeX;
            numericUpDown2.Value = (decimal)sizeY;
        }

        public static string Path { get { return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/biggerprint"; } }
        public static string Filename { get { return Path + "/settings.xml"; } }

        public static bool LoadSettings()
        {
            try
            {
                var doc = XDocument.Load(Filename);
                var root = doc.Element("settings");
                sizeX = float.Parse(root.Attribute("sizeX").Value);
                sizeY = float.Parse(root.Attribute("sizeY").Value);
                pageOrientation = (PageOrientation)int.Parse(root.Attribute("orientation").Value);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static bool SaveSettings()
        {
            try
            {
                System.IO.Directory.CreateDirectory(Path);
                XElement e = new XElement("settings");
                e.SetAttributeValue("sizeX", sizeX);
                e.SetAttributeValue("sizeY", sizeY);
                e.SetAttributeValue("orientation", (int)pageOrientation);
                e.Save(Filename);
            }
            catch
            {
                return false;
            }
            return true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            PrintDialog dialog = new PrintDialog();
            dialog.Document = GetPrintDocument();
            if (dialog.ShowDialog() == DialogResult.OK)
                dialog.Document.Print();
        }

        public PrintDocument GetPrintDocument()
        {
            PrintDocument pdoc = new PrintDocument();

            pdoc.PrintPage += (object s, PrintPageEventArgs ev) =>
            {
                float x = (float)(ev.PageBounds.X + ev.PageBounds.Width / 2);
                float y = (float)(ev.PageBounds.Y + ev.PageBounds.Height / 2);
                float width = Utility.Freedom(callibrationRectSize);
                float height = Utility.Freedom(callibrationRectSize);
                ev.Graphics.DrawRectangle(Pens.Black, x - width / 2, y - height / 2, width, height);
                float offset = Utility.Freedom(5);
                ev.Graphics.DrawString("Measure me!", Font, Brushes.DarkGray, x - width / 2 + offset, y - height / 2 + offset);
                ev.HasMorePages = false;
            };
            return pdoc;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            sizeX = (float)numericUpDown1.Value;
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            sizeY = (float)numericUpDown1.Value;
        }

        private void Settings_Load(object sender, EventArgs e)
        {

        }

        private void Settings_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveSettings();
        }
    }
}
