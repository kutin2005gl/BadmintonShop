using BadmintonShop.Core.DataAccess;

namespace BadmintonShop.Desktop;

/// <summary>Form tam de thu ket noi Core + CSDL. SV4 se thay bang FrmMain.</summary>
public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();
        Text = "BadmintonShop - thu ket noi CSDL";

        var luoi = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
        Controls.Add(luoi);

        if (!DbHelper.ThuKetNoi(out var loi))
        {
            MessageBox.Show("Khong ket noi duoc CSDL:\n" + loi, "Loi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        luoi.DataSource = new NhaCungCapRepository().LayTatCa();
    }
}
