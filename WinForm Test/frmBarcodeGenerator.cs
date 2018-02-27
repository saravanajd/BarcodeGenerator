using System;
using System.Windows.Forms;
using BarcodeGenerator;
using System.Drawing.Imaging;

namespace WinForm_Test
{
    public partial class frmBarcodeGenerator : Form
    {
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        public frmBarcodeGenerator()
        {
            InitializeComponent();
        }

        private void frmBarcodeGenerator_Load(object sender, EventArgs e)
        {
            cmbBarcodeType.DataSource = Enum.GetValues(typeof(SymbologyType));
        }

        private void flowLayoutPanel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            try
            {
                Barcode barcode = new Barcode();
                barcode.Data = txtBarcode.Text.Trim();
                if (cbIncludeDimentions.Checked)
                {
                    barcode.Width = float.Parse(txtWidth.Text);
                    barcode.Height = float.Parse(txtHeight.Text);
                }
                barcode.SymbologyType = (SymbologyType)cmbBarcodeType.SelectedItem;
                pbBarcode.Image = barcode.DrawBarcode();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "BarcodeGenerator", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
