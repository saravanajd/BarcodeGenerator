using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeGenerator
{
    public class Barcode
    {
        #region Variables

        private string data = "";

        private SymbologyType _SymbologyType = SymbologyType.UNSPECIFIED;

        IBarCode ibarcode = new Blank();

        private Image _Image = null;
        private float _Height = 1.2f;
        private float _Width = 2.5f;
        //private float _Height = 1.02f;
        //private float _Width = 2.5f;

        private string _FontName = "Arial";
        private float _FontSize = 17.0f;
        private float _Scale = 0.8f;
        private Color _ForeColor = Color.Black;
        private Color _BackColor = Color.White;


        private float _fMinimumAllowableScale = .8f;
        private float _fMaximumAllowableScale = 2.0f;
        #endregion

        #region Properties
        public string Data
        {
            get { return data; }
            set { data = value; }
        }

        public SymbologyType SymbologyType
        {
            get { return _SymbologyType; }
            set { _SymbologyType = value; }
        }

        public Image Image
        {
            get { return _Image; }
        }

        public float Height
        {
            get { return _Height; }
            set { _Height = value / 100; }
        }

        public float Width
        {
            get { return _Width; }
            set
            {
                _Width = value / 100;
            }
        }

        public string FontName
        {
            set { _FontName = value; }
        }

        public float FontSize
        {
            set { _FontSize = value; }
        }

        public float Scale
        {

            set
            {
                if (value < this._fMinimumAllowableScale || value > this._fMaximumAllowableScale)
                    throw new Exception("Scale value out of allowable range.  Value must be between " +
                                       this._fMinimumAllowableScale.ToString() + " and " +
                                       this._fMaximumAllowableScale.ToString());
                _Scale = value;
            }
        }

        public Color ForeColor
        {
            set { _ForeColor = value; }
        }

        public Color BackColor
        {
            set { _BackColor = value; }
        }

        public static Version Version
        {
            get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version; }
        }
        #endregion

        #region Constroctor

        public Barcode()
        {

        }
        public Barcode(string data)
        {
            this.data = data;
        }

        public Barcode(string data, SymbologyType iType)
        {
            this.data = data;
            this.SymbologyType = iType;
        }

        #endregion

        #region Functions

        #region DraBarcode
        public Image DrawBarcode(SymbologyType symbologyType, string data, float width, float height)
        {
            this.Width = width;
            this.Height = height;
            return DrawBarcode(SymbologyType, data);
        }

        public Image DrawBarcode(SymbologyType symbologyType, string data, Color foreColor, Color backColor, float width, float height)
        {
            this.Width = width;
            this.Height = height;
            return DrawBarcode(symbologyType, data, foreColor, backColor);
        }

        public Image DrawBarcode(SymbologyType symbologyType, string data, Color foreColor, Color backColor)
        {
            this.ForeColor = foreColor;
            this.BackColor = backColor;
            return DrawBarcode(symbologyType, data);
        }

        public Image DrawBarcode(SymbologyType symbologyType, string data)
        {
            this.SymbologyType = symbologyType;
            return DrawBarcode();
        }

        public Image DrawBarcode()
        {
            if (string.IsNullOrEmpty(this.data))
                throw new Exception("BarcodeGenerator: Input data can not be empty.");
            if (this.SymbologyType == SymbologyType.UNSPECIFIED)
                throw new Exception("BarcodeGenerator: Symbology Type can not be unspecified.");


            switch (this._SymbologyType)
            {
                case SymbologyType.Codabar:
                    ibarcode = new Codabar(this.data, this._Width, this.Height, this._ForeColor, this._BackColor, this._Scale);
                    break;
                case SymbologyType.UPCA:
                    ibarcode = new UPCA(this.data, this._Width, this.Height, this._ForeColor, this._BackColor, this._Scale);
                    break;
                case SymbologyType.UPCE:
                    ibarcode = new UPCE(this.data, this._Width, this.Height, this._ForeColor, this._BackColor, this._Scale);
                    break;
                case SymbologyType.EAN8:
                    ibarcode = new EAN8(this.data, this._Width, this.Height, this._ForeColor, this._BackColor, this._Scale);
                    break;
                case SymbologyType.EAN13:
                    ibarcode = new EAN13(this.data, this._Width, this.Height, this._ForeColor, this._BackColor, this._Scale);
                    break;
                default:
                    break;
            }

            this.data = ibarcode.Data;


            return ibarcode.Image;
        }

        #endregion

        #endregion
    }
}
