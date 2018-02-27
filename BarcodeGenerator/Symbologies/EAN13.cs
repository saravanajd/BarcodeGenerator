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
    class EAN13 : BarcodeCommon, IBarCode
    {
        #region Properties

        // Left Hand Digits.
        private string[] _aOddLeft = { "0001101", "0011001", "0010011", "0111101",
                                          "0100011", "0110001", "0101111", "0111011",
                                          "0110111", "0001011" };

        private string[] _aEvenLeft = { "0100111", "0110011", "0011011", "0100001",
                                           "0011101", "0111001", "0000101", "0010001",
                                           "0001001", "0010111" };


        private string[] countryCodePattern = { "OOOOOO","OOEOEE","OOEEOE","OOEEEO","OEOOEE",
                                            "OEEOOE","OEEEOO","OEOEOE","OEOEEO","OEEOEO"};

        // Right Hand Digits.
        private string[] _aRight = { "1110010", "1100110", "1101100", "1000010",
                                        "1011100", "1001110", "1010000", "1000100",
                                        "1001000", "1110100" };

        private string QuiteZone = "0000000000";
        private string LeadTail = "101";
        private string Separator = "01010";

        private string CountryCode = "00";
        private string ManufacturerCode = "";
        private string ProductCode = "";
        private string ChecksumDigit = "";

        #endregion

        #region Constractor

        public EAN13(string data)
        {
            this.data = data;
        }

        public EAN13(string data, float width, float height, Color foreColor, Color backColor, float scale)
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

        private Bitmap GenerateImage()
        {
            if (!new Regex(@"^\d+$").IsMatch(data))
            {
                throw new Exception("GENERATE_IMAGE-1: EAN-13 allowed numeric values only");
            }
            if (base.data.Length < 12)
            {
                throw new Exception("GENERATE_IMAGE-2: EAN-13 format required min 12 char.");
            }
            if (base.data.Length > 13)
            {
                throw new Exception("GENERATE_IMAGE-3: EAN-13 format required max 13 char.");
            }

            CountryCode = this.data.Substring(0, 2);
            ManufacturerCode = this.data.Substring(2, 5);
            ProductCode = this.data.Substring(7, 5);
            int checkDegit = this.CalculateChecksumDigit();
            if (this.data.Length == 13)
            {
                // (char - '0') one way to convert int
                if ((int)(this.data[12] - '0') != checkDegit)
                    throw new Exception("GENERATE_IMAGE-4: Invalid check degit");
            }
            else
            {
                this.data = string.Concat(this.data, checkDegit);
            }
            ChecksumDigit = this.data.Substring(12);
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

            System.Drawing.SolidBrush brush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);

            float xPosition = 0;


            float xStart = pt.X;
            float yStart = pt.Y;
            float xEnd = 0;

            System.Drawing.Font font = new System.Drawing.Font("Arial", this.FontSize * this.Scale);

            System.Text.StringBuilder sbUPC = new System.Text.StringBuilder();
            // Build the UPC Code.
            sbUPC.AppendFormat("{0}{1}{2}{3}{4}{1}{0}",
                                    this.QuiteZone, this.LeadTail,
                                    DigitToPatternsLeft(this.data.Substring(0, 7)),
                                    this.Separator,
                                    DigitToPatterns(this.data.Substring(7), this._aRight));

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
                    if ((i > 12 && i < 55) || (i > 59 && i < 101))
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

            xPosition = xStart - g.MeasureString(this.CountryCode.Substring(0,1) , font).Width;
            float yPosition = yStart + (height - fTextHeight);
            // Draw Product Type.
            g.DrawString(this.CountryCode.Substring(0, 1), font, brush, new System.Drawing.PointF(xPosition, yPosition));

            // Each digit is 7 modules wide, therefore the MFG_Number is 5 digits wide so
            //    5 * 7 = 35, then add 3 for the LeadTrailer Info and another 7 for good measure,
            //    that is where the 45 comes from.
            xPosition += g.MeasureString(this.CountryCode.Substring(0, 1), font).Width + 45 * lineWidth -
                            g.MeasureString(string.Concat(this.CountryCode[1], this.ManufacturerCode), font).Width;

            // Draw MFG Number.
            g.DrawString(string.Concat(this.CountryCode[1], this.ManufacturerCode), font, brush, new System.Drawing.PointF(xPosition, yPosition));

            // Add the width of the MFG Number and 5 modules for the separator.
            xPosition += g.MeasureString(string.Concat(this.CountryCode[1], this.ManufacturerCode), font).Width +
                         5 * lineWidth;

            // Draw Product ID.
            g.DrawString(string.Concat(this.ProductCode,this.ChecksumDigit), font, brush, new System.Drawing.PointF(xPosition, yPosition));

            g.Restore(gs);

        }

        private string DigitToPatternsLeft(string digits)
        {
            string pattern = this.countryCodePattern[(CountryCode[0] - '0')];
            digits = digits.Substring(1);
            StringBuilder sbTemp = new StringBuilder();
            for (int i = 0; i < digits.Length; i++)
            {
                if(pattern[i] == 'E')
                {
                    sbTemp.Append(this._aEvenLeft[(digits[i] - '0')]);
                }
                else if(pattern[i] == 'O')
                {
                    sbTemp.Append(this._aOddLeft[(digits[i] - '0')]);
                }
            }

            return Convert.ToString(sbTemp);

        }

        public string DigitToPatterns(string digits, string[] patterns)
        {
            var str = string.Join("", digits.Select(x => patterns[(x - '0')]));
            return string.Join("", digits.Select(x => patterns[(x - '0')]));
        }

        private int CalculateChecksumDigit()
        {
            //  EAN-13 check sum being calculated in reverse order for compatibility with UPC-A barcodes.
            var degits = data.Take(12).Select(x => x - '0').Reverse();
            var even = degits.Where((x, i) => i % 2 == 0).Sum() * 3;
            var odd = degits.Where((x, i) => i % 2 != 0).Sum();

            int checkDegit = (10 - ((even + odd) % 10)) % 10;

            return checkDegit;
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
