using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Text.RegularExpressions;

namespace BarcodeGenerator
{
    internal class UPCE : BarcodeCommon, IBarCode
    {
        #region Properties
        private string[] _aOddParity = { "0001101", "0011001", "0010011", "0111101", "0100011", "0110001",
                                      "0101111", "0111011", "0110111", "0001011" };
        private string[] _aEvenParity = { "0100111", "0110011", "0011011", "0100001", "0011101", "0111001",
                                     "0000101", "0010001", "0001001", "0010111" };

        private string[] numberSystem_0 = { "EEEOOO", "EEOEOO", "EEOOEO", "EEOOOE", "EOEEOO", "EOOEEO",
                                            "EOOOEE", "EOEOEO", "EOEOOE", "EOOEOE" };
        private string[] numberSystem_1 = { "OOOEEE", "OOEOEE", "OOEEOE", "OOEEEO", "OEOOEE", "OEEOOE",
                                            "OEEEOO", "OEOEOE", "OEOEEO", "OEEOEO" };

        private string QuiteZone = "000000000";
        private string LeadTail = "101";
        private string Separator = "010101";

        #endregion

        #region Constroctor
        public UPCE(string data, float width, float height, Color foreColor, Color backColor, float scale)
        {
            this.data = data;
            this.width = width;
            this.height = height;
            this.scale = scale;
            this.foreColor = foreColor;
            this.backColor = backColor;

        }
        public UPCE(string data)
        {
            this.data = data;
        }
        #endregion

        #region Functions

        private int CalculateChecksumDigit()
        {
            string UPCAString = string.Empty;

            // Convert to UPC-A Number
            int N = int.Parse(data.Substring(6, 1));

            string EncodedValue = string.Empty;
            switch (N)
            {
                case 0:
                case 1:
                case 2:
                    EncodedValue = string.Concat(data.Substring(0, 3), data[6], "0000", data.Substring(3, 3));
                    break;
                case 3:
                    EncodedValue = string.Concat(data.Substring(0, 4), "00000", data.Substring(4, 2));
                    break;
                case 4:
                    EncodedValue = string.Concat(data.Substring(0, 5), "00000", data[5]);
                    break;
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                    EncodedValue = string.Concat(this.data.Substring(0, 6), "0000", data[6]);
                    break;
                default:
                    throw new Exception("Encode_UPCA-1: Invalid UPC-E number");
            }
            int oddNumber = 0;
            int evenNumber = 0;
            for (int i = 1; i <= 11; i++)
            {
                int temp = 0;
                if (int.TryParse(EncodedValue.Substring(i - 1, 1), out temp))
                {
                    if ((i % 2) != 0)
                    {
                        oddNumber += temp;
                    }
                    else
                    {
                        evenNumber += temp;
                    }
                }
            }
            int checkDegit = 10 - (((oddNumber * 3) + (evenNumber)) % 10);
            return checkDegit;
        }
        private Bitmap GenerateImage()
        {
            if (!new Regex(@"^\d+$").IsMatch(data))
            {
                throw new Exception("GENERATE_IMAGE-1: UPC-E allowed numeric values only");
            }
            if (base.data.Length < 7)
            {
                throw new Exception("GENERATE_IMAGE-2: UPC-E format required min 7 char.");
            }
            if (base.data.Length > 8)
            {
                throw new Exception("GENERATE_IMAGE-2: UPC-E format required max 8 char.");
            }
            if (base.data.Substring(0, 1) != "0" && base.data.Substring(0, 1) != "1")
            {
                throw new Exception("GENERATE_IMAGE-3: Invalid number system");
            }
            int checkDegit = this.CalculateChecksumDigit();
            if (this.data.Length == 8)
            {
                if (this.data[7] != checkDegit)
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
        private void DrawUpcaBarcode(Graphics g, Point pt)
        {
            float width = this.Width * this.Scale;
            float height = this.Height * this.Scale;

            // A UPCE Barcode should be a total of 71 modules wide.
            float lineWidth = width / 71f;

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


            string pattern = (data[0] == '0') ? numberSystem_0[int.Parse(data.Substring(7, 1))]
                                : numberSystem_1[int.Parse(data.Substring(7, 1))];

            string strUPC = QuiteZone + LeadTail;
            string ManufacturerCode = this.data.Substring(1, 6);
            for (int i = 0; i < pattern.Length; i++)
            {
                int n = int.Parse(Convert.ToString(ManufacturerCode[i]));
                if (pattern[i] == 'O')
                {
                    strUPC += _aOddParity[n];
                }
                else if (pattern[i] == 'E')
                {
                    strUPC += _aEvenParity[n];
                }
            }
            strUPC += Separator;
            strUPC += QuiteZone;

            string sTempUPC = strUPC;

            float fTextHeight = g.MeasureString(sTempUPC, font).Height;

            // Draw the barcode lines.
            for (int i = 0; i < strUPC.Length; i++)
            {
                if (sTempUPC.Substring(i, 1) == "1")
                {
                    if (xStart == pt.X)
                        xStart = xPosition;

                    // Save room for the UPC number below the bar code.
                    if ((i > 12 && i < 55))
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
            xPosition = xStart - g.MeasureString(this.data.Substring(0, 1), font).Width;
            float yPosition = yStart + (height - fTextHeight);
            // Draw Product Type.
            g.DrawString(this.data.Substring(0, 1), font, brush, new System.Drawing.PointF(xPosition, yPosition));

            // Each digit is 7 modules wide, therefore the MFG_Number is 5 digits wide so
            //    5 * 7 = 35, then add 3 for the LeadTrailer Info and another 7 for good measure,
            //    that is where the 45 comes from.
            xPosition += g.MeasureString(this.data.Substring(0, 1), font).Width + 38 * lineWidth -
                            g.MeasureString(ManufacturerCode, font).Width;

            // Draw MFG Number.
            g.DrawString(ManufacturerCode, font, brush, new System.Drawing.PointF(xPosition, yPosition));

            // Add the width of the MFG Number and 5 modules for the separator.
            xPosition += g.MeasureString(ManufacturerCode, font).Width +
                         13 * lineWidth;

            // Draw Product ID.
            //g.DrawString(this.ProductCode, font, brush, new System.Drawing.PointF(xPosition, yPosition));

            // Each digit is 7 modules wide, therefore the Product Id is 5 digits wide so
            //    6 * 7 = 42, then add 3 for the LeadTrailer Info and add 5 for seprator, + 8 more just for spacing
            //  that is where the 46 comes from.
            //xPosition += 58 * lineWidth;

            // Draw Check Digit.
            g.DrawString(this.data.Substring(7, 1), font, brush, new System.Drawing.PointF(xPosition, yPosition));

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
