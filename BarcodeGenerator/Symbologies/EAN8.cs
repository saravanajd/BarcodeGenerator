using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BarcodeGenerator
{
    class EAN8 : BarcodeCommon, IBarCode
    {

        private string[] _aOddParity = { "0001101", "0011001", "0010011", "0111101", "0100011", "0110001", "0101111", "0111011", "0110111", "0001011" };
        private string[] _aEvenParity = { "1110010", "1100110", "1101100", "1000010", "1011100", "1001110", "1010000", "1000100", "1001000", "1110100" };

        private string QuiteZone = "0000000000";
        private string LeadTail = "101";
        private string Separator = "01010";

        #region Constractor

        public EAN8(string data)
        {
            this.data = data;
        }

        public EAN8(string data, float width, float height, Color foreColor, Color backColor, float scale)
        {
            this.data = data;
            this.width = width;
            this.height = height;
            this.scale = scale;
            this.foreColor = foreColor;
            this.backColor = backColor;

        }
        #endregion

        #region Funcions

        private int CalculateChecksumDigit()
        {
            //  EAN-13 check sum being calculated in reverse order for compatibility with UPC-A barcodes.
            var degits = data.Take(7).Select(x => x - '0').Reverse();
            var even = degits.Where((x, i) => i % 2 == 0).Sum() * 3;
            var odd = degits.Where((x, i) => i % 2 != 0).Sum();

            int checkDegit = (10 - ((even + odd) % 10)) % 10;

            return checkDegit;
        }

        private Bitmap GenerateImage()
        {
            if (!new Regex(@"^\d+$").IsMatch(data))
            {
                throw new Exception("GENERATE_IMAGE-1: EAN-8 allowed numeric values only");
            }
            if (base.data.Length < 7)
            {
                throw new Exception("GENERATE_IMAGE-2: EAN-8 format required min 7 char.");
            }
            if (base.data.Length > 8)
            {
                throw new Exception("GENERATE_IMAGE-3: EAN-8 format required max 8 char.");
            }

            int checkDegit = this.CalculateChecksumDigit();
            if (this.data.Length == 8)
            {
                if ((int)(this.data[7] - '0') != checkDegit)
                    throw new Exception("GENERATE_IMAGE-4: Invalid check degit");
            }
            else
            {
                this.data = string.Concat(this.data, checkDegit);
            }
            // validation

            float tempWidth = (this.Width * this.Scale) * 100;
            float tempHeight = (this.Height * this.Scale) * 100;

            Bitmap bmp = new Bitmap((int)tempWidth, (int)tempHeight);

            Graphics g = Graphics.FromImage(bmp);
            this.DrawUpcaBarcode(g, new Point(0, 0));
            g.Dispose();
            return bmp;
        }

        public string DigitToPatterns(string digits, string[] patterns)
        {
            return string.Join("", digits.Select(x => patterns[(x - '0')]));
        }

        private void DrawUpcaBarcode(Graphics g, Point pt)
        {
            float width = this.Width * this.Scale;
            float height = this.Height * this.Scale;

            // A UPCE Barcode should be a total of 71 modules wide.
            float lineWidth = width / 87f;

            // Save the GraphicsState.
            System.Drawing.Drawing2D.GraphicsState gs = g.Save();

            // Set the PageUnit to Inch because all of our measurements are in inches.
            g.PageUnit = System.Drawing.GraphicsUnit.Inch;

            // Set the PageScale to 1, so an inch will represent a true inch.
            g.PageScale = 1;

            System.Drawing.SolidBrush brush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);

            float xPosition = 0;

            System.Text.StringBuilder strbUPC = new System.Text.StringBuilder();

            float xStart = pt.X;
            float yStart = pt.Y;
            float xEnd = 0;

            System.Drawing.Font font = new System.Drawing.Font("Arial", this.FontSize * this.Scale);

            string strEAN = string.Concat(QuiteZone, LeadTail, DigitToPatterns(data.Substring(0, data.Length / 2), _aOddParity),
                     Separator, DigitToPatterns(data.Substring(data.Length / 2), _aEvenParity), LeadTail, QuiteZone);

            string sTempUPC = strEAN;

            float fTextHeight = g.MeasureString(sTempUPC, font).Height;

            // Draw the barcode lines.
            for (int i = 0; i < strEAN.Length; i++)
            {
                if (sTempUPC.Substring(i, 1) == "1")
                {
                    if (xStart == pt.X)
                        xStart = xPosition;

                    // Save room for the UPC number below the bar code.
                    if ((i > 12 && i < 41) | (i > 45 && i < 74))
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

            xPosition = xStart - g.MeasureString(string.Empty, font).Width;
            float yPosition = yStart + (height - fTextHeight);
            // Draw Product Type.
            g.DrawString(string.Empty, font, brush, new System.Drawing.PointF(xPosition, yPosition));

            // Each digit is 7 modules wide, therefore the MFG_Number is 4 digits wide so
            //    4 * 7 = 28, then add 3 for the LeadTrailer Info and another 7 for good measure,
            //    that is where the 38 comes from.
            xPosition += g.MeasureString(string.Empty, font).Width + 31 * lineWidth -
                            g.MeasureString(data.Substring(0, data.Length / 2), font).Width;

            // Draw MFG Number.
            g.DrawString(data.Substring(0, data.Length / 2), font, brush, new System.Drawing.PointF(xPosition, yPosition));

            // Add the width of the MFG Number and 5 modules for the separator.
            xPosition += g.MeasureString(data.Substring(0, data.Length / 2), font).Width +
                         5 * lineWidth;

            // Draw Product ID.
            g.DrawString(data.Substring(data.Length / 2), font, brush, new System.Drawing.PointF(xPosition, yPosition));

            // Each digit is 7 modules wide, therefore the Product Id is 5 digits wide so
            //    5 * 7 = 35, then add 3 for the LeadTrailer Info, + 8 more just for spacing
            //  that is where the 46 comes from.
            //    xPosition += 46 * lineWidth;

            // Draw Check Digit.
            //   g.DrawString(this.ChecksumDigit, font, brush, new System.Drawing.PointF(xPosition, yPosition));

            // Restore the GraphicsState.
            g.Restore(gs);
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
