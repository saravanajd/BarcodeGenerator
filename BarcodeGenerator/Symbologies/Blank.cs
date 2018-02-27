using System;

namespace BarcodeGenerator
{
    class Blank : BarcodeCommon, IBarCode
    {
        public string Encoded_Value
        {
            get { throw new NotImplementedException(); }
        }
        public System.Drawing.Image Image
        {
            get { throw new NotImplementedException(); }
        }

    }
}
