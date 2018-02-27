using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace BarcodeGenerator
{
    abstract class BarcodeCommon
    {

        protected string data = "";
        protected string label = "";

        protected float height = 1.2f;
        protected float width = 2.5f;

        protected string fontName = "Arial";
        protected float fontSize = 17.0f;
        protected float scale = 1f;
        protected Color foreColor = Color.Black;
        protected Color backColor = Color.White;


        protected List<string> errors = new List<string>();

        #region Properties

        public List<string> Errors { get { return this.errors; } }
        public string Data
        {
            get { return this.data; }
        }

        public float Height
        {
            get { return height; }

        }
        public float Width
        {
            get { return width; }
        }
        public string FontName
        {
            get { return fontName; }
        }
        public float FontSize
        {
            get { return fontSize; }
        }
        public float Scale
        {
            get { return scale; }
        }

        public Color ForeColor
        {
            get { return foreColor; }
        }

        public Color BackColor
        {
            get { return backColor; }
        }

        public void Error(string error)
        {
            this.Errors.Add(error);
            throw new Exception(error);
        }
        #endregion

        #region Functions

        protected Image Label_Generic(Image img)
        {
            try
            {
                System.Drawing.Font font = new System.Drawing.Font("Arial", this.FontSize * this.Scale);

                using (Graphics g = Graphics.FromImage(img))
                {
                    g.DrawImage(img, (float)0, (float)0);

                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

                    StringFormat f = new StringFormat();
                    f.Alignment = StringAlignment.Near;
                    f.LineAlignment = StringAlignment.Near;
                    int LabelX = 0;
                    int LabelY = 0;
                    LabelPositions LabelPosition = LabelPositions.BOTTOMCENTER;
                    switch (LabelPosition)
                    {
                        case LabelPositions.BOTTOMCENTER:
                            LabelX = img.Width / 2;
                            LabelY = img.Height - (font.Height);
                            f.Alignment = StringAlignment.Center;
                            break;
                        case LabelPositions.BOTTOMLEFT:
                            LabelX = 0;
                            LabelY = img.Height - (font.Height);
                            f.Alignment = StringAlignment.Near;
                            break;
                        case LabelPositions.BOTTOMRIGHT:
                            LabelX = img.Width;
                            LabelY = img.Height - (font.Height);
                            f.Alignment = StringAlignment.Far;
                            break;
                        case LabelPositions.TOPCENTER:
                            LabelX = img.Width / 2;
                            LabelY = 0;
                            f.Alignment = StringAlignment.Center;
                            break;
                        case LabelPositions.TOPLEFT:
                            LabelX = img.Width;
                            LabelY = 0;
                            f.Alignment = StringAlignment.Near;
                            break;
                        case LabelPositions.TOPRIGHT:
                            LabelX = img.Width;
                            LabelY = 0;
                            f.Alignment = StringAlignment.Far;
                            break;
                    }//switch

                    //color a background color box at the bottom of the barcode to hold the string of data
                    g.FillRectangle(new SolidBrush(BackColor), new RectangleF((float)0, (float)LabelY, (float)img.Width, (float)font.Height));

                    //draw datastring under the barcode image
                    g.DrawString(string.IsNullOrEmpty(label) ? data : label, font, new SolidBrush(ForeColor), new RectangleF((float)0, (float)LabelY, (float)img.Width, (float)font.Height), f);

                    g.Save();
                }//using
                return img;
            }//try
            catch (Exception ex)
            {
                throw new Exception("ELABEL_GENERIC-1: " + ex.Message);
            }//catch
        }

        #endregion




    }
}
