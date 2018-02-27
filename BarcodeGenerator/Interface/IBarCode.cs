using System.Drawing;

namespace BarcodeGenerator
{
    interface IBarCode
    {
        string Data { get; }

        Image Image { get; }

        float Height { get; }
        float Width { get; }
        string FontName { get; }
        float FontSize { get; }
        float Scale { get; }
        Color ForeColor { get; }
        Color BackColor { get; }
    }
}
