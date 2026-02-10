namespace Crypto.Futures.FrontEnd
{
    public partial class FrmMain : Form
    {
        public FrmMain()
        {
            InitializeComponent();
        }

        private void MnuArbitrageBot_Click(object sender, EventArgs e)
        {
            var frm = new FrmFundingRate();
            frm.MdiParent = this;
            frm.WindowState = FormWindowState.Maximized;
            frm.Show(); 

        }

    }
}
