using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BarcodeGenerator
{
    internal class UPCA : BarcodeCommon, IBarCode
    {
        #region Properties

        // Left Hand Digits.
        private string[] _aLeft = { "0001101", "0011001", "0010011", "0111101",
                                     "0100011", "0110001", "0101111", "0111011",
                                     "0110111", "0001011" };

        // Right Hand Digits.
        private string[] _aRight = { "1110010", "1100110", "1101100", "1000010",
                                      "1011100", "1001110", "1010000", "1000100",
                                      "1001000", "1110100" };

        private string QuiteZone = "0000000000";
        private string LeadTail = "101";
        private string Separator = "01010";

        private string ManufacturerCode = "";
        private string NumberSystem = "";
        private string ProductCode = "";
        private string ChecksumDigit = "";
        #endregion

        #region Constroctor
        public UPCA(string data)
        {
            this.data = data;
        }

        public UPCA(string data, float width, float height, Color foreColor, Color backColor, float scale)
        {
            this.data = data;
            this.width = width;
            this.height = height;
            this.scale = scale;
            this.foreColor = foreColor;
            this.backColor = backColor;

        }
        #endregion

        #region Functions

        private int CalculateChecksumDigit()
        {

            var degits = data.Take(11).Select(x => x - '0');
            var even = degits.Where((x, i) => i % 2 == 0).Sum() * 3;
            var odd = degits.Where((x, i) => i % 2 != 0).Sum();

            int checkDegit = 10 - ((even + odd) % 10);

            return checkDegit;
        }
        private Bitmap GenerateImage()
        {
            if (!new Regex(@"^\d+$").IsMatch(data))
            {
                throw new Exception("GENERATE_IMAGE-1: UPC-A allowed numeric values only");
            }
            if (base.data.Length < 11)
            {
                throw new Exception("GENERATE_IMAGE-2: UPC-A format required min 11 char.");
            }
            if (base.data.Length > 12)
            {
                throw new Exception("GENERATE_IMAGE-3: UPC-A format required max 12 char.");
            }

            /// <summary>
            /// System Description 
            /// 0 - Regular UPC codes 
            /// 1 - Reserved 
            /// 2 - Weightitems marked at the store 
            /// 3 - National Drug/Health-related code 
            /// 4 - No format restrictions, in-store use on non-food items 
            /// 5 - Coupons 
            /// 6 - Reserved 
            /// 7 - Regular UPC codes 
            /// 8 - Reserved 
            /// 9 - Reserved 
            /// </summary>
            /// <value></value>
            /// 

            NumberSystem = this.data.Substring(0, 1);
            ManufacturerCode = this.data.Substring(1, 5);
            ProductCode = this.data.Substring(6, 5);
            if (NumberSystem == "1" && NumberSystem == "6"
                                    && Convert.ToInt32(NumberSystem) > 7)
            {
                throw new Exception("GENERATE_IMAGE-4: Invalid format, Number system value is reserved type");
            }
            int checkDegit = this.CalculateChecksumDigit();
            if (this.data.Length == 12)
            {
                // (char - '0') one way to convert int
                if ((int)(this.data[11] - '0') != checkDegit)
                    throw new Exception("GENERATE_IMAGE-5: Invalid check degit");
            }
            else
            {
                this.data = string.Concat(this.data, checkDegit);
            }
            ChecksumDigit = this.data.Substring(11, 1);
            // validation

            float tempWidth = (this.Width * this.Scale) * 100;
            float tempHeight = (this.Height * this.Scale) * 100;

            Bitmap bmp = new Bitmap((int)tempWidth, (int)tempHeight);

            Graphics g = Graphics.FromImage(bmp);
            this.DrawUpcaBarcode(g, new Point(0, 0));
            g.Dispose();
            return bmp;
        }
        private void DrawUpcaBarcode(Graphics g, Point pt)
        {
            float width = this.Width * this.Scale;
            float height = this.Height * this.Scale;

            // A UPC-A excluding 2 or 5 digit supplement information 
            //	should be a total of 113 modules wide. Supplement information is typically 
            //	used for periodicals and books.
            float lineWidth = width / 113f;

            // Save the GraphicsState.
            System.Drawing.Drawing2D.GraphicsState gs = g.Save();

            // Set the PageUnit to Inch because all of our measurements are in inches.
            g.PageUnit = System.Drawing.GraphicsUnit.Inch;

            // Set the PageScale to 1, so an inch will represent a true inch.
            g.PageScale = 1;


            float xPosition = 0;

            float xStart = pt.X;
            float yStart = pt.Y;
            float xEnd = 0;

            System.Drawing.SolidBrush brush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
            System.Drawing.Font font = new System.Drawing.Font("Arial", this.FontSize * this.Scale);

            System.Text.StringBuilder sbUPC = new System.Text.StringBuilder();
            // Build the UPC Code.
            sbUPC.AppendFormat("{0}{1}{2}{3}{4}{5}{6}{1}{0}",
                                    this.QuiteZone, this.LeadTail,
                                    DigitToPatterns(this.NumberSystem, this._aLeft),
                                    DigitToPatterns(this.ManufacturerCode, this._aLeft),
                                    this.Separator,
                                    DigitToPatterns(this.ProductCode, this._aRight),
                                    DigitToPatterns(this.ChecksumDigit, this._aRight));

            string sTempUPC = sbUPC.ToString();

            float fTextHeight = g.MeasureString(sTempUPC, font).Height;

            // Draw the barcode lines.
            for (int i = 0; i < sbUPC.Length; i++)
            {
                if (sTempUPC.Substring(i, 1) == "1")
                {
                    if (xStart == pt.X)
                        xStart = xPosition;

                    // Save room for the UPC number below the bar code.
                    if ((i > 19 && i < 56) || (i > 59 && i < 95))
                        // Draw space for the number
                        g.FillRectangle(brush, xPosition, yStart, lineWidth, height - fTextHeight);
                    else
                        // Draw a full line.
                        g.FillRectangle(brush, xPosition, yStart, lineWidth, height);
                }

                xPosition += lineWidth;
                xEnd = xPosition;
            }

            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            // Draw the upc numbers below the line.

            xPosition = xStart - g.MeasureString(this.NumberSystem, font).Width;
            float yPosition = yStart + (height - fTextHeight);
            // Draw Product Type.
            g.DrawString(this.NumberSystem, font, brush, new System.Drawing.PointF(xPosition, yPosition));

            // Each digit is 7 modules wide, therefore the MFG_Number is 5 digits wide so
            //    5 * 7 = 35, then add 3 for the LeadTrailer Info and another 7 for good measure,
            //    that is where the 45 comes from.
            xPosition += g.MeasureString(this.NumberSystem, font).Width + 45 * lineWidth -
                            g.MeasureString(this.ManufacturerCode, font).Width;

            // Draw MFG Number.
            g.DrawString(this.ManufacturerCode, font, brush, new System.Drawing.PointF(xPosition, yPosition));

            // Add the width of the MFG Number and 5 modules for the separator.
            xPosition += g.MeasureString(this.ManufacturerCode, font).Width +
                         5 * lineWidth;

            // Draw Product ID.
            g.DrawString(this.ProductCode, font, brush, new System.Drawing.PointF(xPosition, yPosition));

            // Each digit is 7 modules wide, therefore the Product Id is 5 digits wide so
            //    5 * 7 = 35, then add 3 for the LeadTrailer Info, + 8 more just for spacing
            //  that is where the 46 comes from.
            xPosition += 46 * lineWidth;

            // Draw Check Digit.
            g.DrawString(this.ChecksumDigit, font, brush, new System.Drawing.PointF(xPosition, yPosition));

            // Restore the GraphicsState.
            g.Restore(gs);

        }

        public  string DigitToPatterns(string degits,string[] patterns)
        {
            return string.Join("", degits.Select(x => patterns[(x - '0')]));
        }
        #endregion

        #region IBarCode Member
        public Image Image
        {
            get { return this.GenerateImage(); }
        }
        #endregion

    }
}
