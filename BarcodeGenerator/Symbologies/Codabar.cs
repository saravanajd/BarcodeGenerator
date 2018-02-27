using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BarcodeGenerator
{
    class Codabar : BarcodeCommon, IBarCode
    {

        private System.Collections.Hashtable Codabar_Code = new System.Collections.Hashtable()
        {
            {'0', "101010011" },{'1', "101011001"},{'2', "101001011"},{'3', "110010101"},{'4', "101101001"},
            { '5', "110101001"},{'6', "100101011"},{'7', "100101101"},{'8', "100110101"},{'9', "110100101"},
            { 'A', "1011001001"},{'B', "1010010011"},{'C', "1001001011"},{'D', "1010011001"},
            { 'a', "1011001001"},{'b', "1010010011"},{'c', "1001001011"},{'d', "1010011001"},
            { '-', "101001101"},{'$', "101100101"},{':', "1101011011"},{'/', "1101101011"},{'.', "1101101101"},{'+', "101100110011"},
        };



        #region Constractor

        public Codabar(string data)
        {
            this.data = data;
        }

        public Codabar(string data, float width, float height, Color foreColor, Color backColor, float scale)
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

        private Image GenerateImage()
        {
            if (base.data.Length < 2)
            {
                throw new Exception("GENERATE_IMAGE-1: Codabar format required min 2 char.");
            }
            if (!("ABCD".Contains(this.data.ToUpper().Trim()[0])))
            {
                throw new Exception("GENERATE_IMAGE-2: Invalide start charecter, Codabar starts with any one of this charecter 'A','B','C','D'");
            }
            if (!("ABCD".Contains(this.data.ToUpper().Trim()[data.Length - 1])))
            {
                throw new Exception("GENERATE_IMAGE-3: Invalide end charecter, Codabar ends with any one of this charecter 'A','B','C','D'");
            }
            if (!new Regex(@"^[0-9\-\+\$\.\:\/]+$").IsMatch(this.data.Substring(1,data.Length - 2)))
            {
                throw new Exception("GENERATE_IMAGE-4: Codabar data contains invalid charecter");
            }//Validation



            float tempWidth = (this.Width * this.Scale) * 100;
            float tempHeight = (this.Height * this.Scale) * 100;

            Bitmap bmp = new Bitmap((int)tempWidth, (int)tempHeight);

            Graphics g = Graphics.FromImage(bmp);
            this.DrawUpcaBarcode(g, new Point(0, 0));
            return Label_Generic(bmp);
            g.Dispose();
            return bmp;
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


            string strCodabar = string.Join("", this.data.Select(x => string.Concat(Codabar_Code[x], "0")));

            // To remove the extra 0 
            strCodabar = strCodabar.Substring(0, strCodabar.Length - 1);

            string sTempUPC = strCodabar;

            float fTextHeight = g.MeasureString(sTempUPC, font).Height;

            // Draw the barcode lines.
            for (int i = 0; i < strCodabar.Length; i++)
            {
                if (sTempUPC.Substring(i, 1) == "1")
                {
                    if (xStart == pt.X)
                        xStart = xPosition;

                    // Save room for the UPC number below the bar code.
                  //  if ((i > 12 && i < 41) | (i > 45 && i < 74))
                        // Draw space for the number
                        g.FillRectangle(brush, xPosition, yStart, lineWidth, height - fTextHeight);
                    //else
                    //    // Draw a full line.
                    //    g.FillRectangle(brush, xPosition, yStart, lineWidth, height);
                }

                xPosition += lineWidth;
                xEnd = xPosition;
            }

            //xPosition = xStart - g.MeasureString(string.Empty, font).Width;
            //float yPosition = yStart + (height - fTextHeight);
            //// Draw Product Type.
            //g.DrawString(string.Empty, font, brush, new System.Drawing.PointF(xPosition, yPosition));

            //// Each digit is 7 modules wide, therefore the MFG_Number is 4 digits wide so
            ////    4 * 7 = 28, then add 3 for the LeadTrailer Info and another 7 for good measure,
            ////    that is where the 38 comes from.
            //xPosition += g.MeasureString(string.Empty, font).Width + 31 * lineWidth -
            //                g.MeasureString(data.Substring(0), font).Width;

            //// Draw MFG Number.
            //g.DrawString(data.Substring(0), font, brush, new System.Drawing.PointF(xPosition, yPosition));

            ////// Add the width of the MFG Number and 5 modules for the separator.
            ////xPosition += g.MeasureString(data.Substring(0, data.Length / 2), font).Width +
            ////             5 * lineWidth;

            ////// Draw Product ID.
            ////g.DrawString(data.Substring(data.Length / 2), font, brush, new System.Drawing.PointF(xPosition, yPosition));

            ////// Each digit is 7 modules wide, therefore the Product Id is 5 digits wide so
            //////    5 * 7 = 35, then add 3 for the LeadTrailer Info, + 8 more just for spacing
            //////  that is where the 46 comes from.
            //////    xPosition += 46 * lineWidth;

            ////// Draw Check Digit.
            //////   g.DrawString(this.ChecksumDigit, font, brush, new System.Drawing.PointF(xPosition, yPosition));

            ////// Restore the GraphicsState.
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
