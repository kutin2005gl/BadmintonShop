
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BadmintonShop.Core.DataAccess;
using BadmintonShop.Core.Models;

namespace BadmintonShop.Desktop
{
    public partial class FrmQuanLyDanhGia : Form
    {
        private readonly DanhGiaRepository _repository = new();

        private readonly DataGridView dgvDanhGia = new();
        private readonly ComboBox cboTrangThai = new();
        private readonly Button btnLamMoi = new();
        private readonly Button btnDuyet = new();
        private readonly Button btnTuChoi = new();
        private readonly Label lblThongKe = new();

        public FrmQuanLyDanhGia()
        {
            InitializeComponent();
            TaoGiaoDien();
            TaiDanhSach();
        }

        private void TaoGiaoDien()
        {
            Text = "Quản lý đánh giá sản phẩm";
            MinimumSize = new Size(800, 500);
            BackColor = Color.White;

            var panelChinh = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(20),
                BackColor = Color.White
            };

            panelChinh.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 55));
            panelChinh.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 60));
            panelChinh.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100));
            panelChinh.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 65));

            var lblTieuDe = new Label
            {
                Text = "QUẢN LÝ ĐÁNH GIÁ SẢN PHẨM",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(35, 100, 75),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            panelChinh.Controls.Add(lblTieuDe, 0, 0);

            var panelLoc = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoScroll = true
            };

            panelLoc.Controls.Add(new Label
            {
                Text = "Trạng thái:",
                AutoSize = true,
                Margin = new Padding(0, 12, 10, 0)
            });

            cboTrangThai.DropDownStyle =
                ComboBoxStyle.DropDownList;

            cboTrangThai.Width = 180;
            cboTrangThai.Height = 35;

            cboTrangThai.Items.AddRange(new object[]
            {
                "Tất cả",
                "Chờ duyệt",
                "Đã duyệt",
                "Từ chối"
            });

            cboTrangThai.SelectedIndex = 0;
            cboTrangThai.SelectedIndexChanged +=
                (_, _) => TaiDanhSach();

            panelLoc.Controls.Add(cboTrangThai);

            btnLamMoi.Text = "Làm mới";
            btnLamMoi.Width = 110;
            btnLamMoi.Height = 35;
            btnLamMoi.Margin = new Padding(15, 0, 0, 0);
            btnLamMoi.Click += (_, _) => TaiDanhSach();

            panelLoc.Controls.Add(btnLamMoi);

            lblThongKe.AutoSize = true;
            lblThongKe.Margin = new Padding(20, 12, 0, 0);

            panelLoc.Controls.Add(lblThongKe);

            panelChinh.Controls.Add(panelLoc, 0, 1);

            dgvDanhGia.Dock = DockStyle.Fill;
            dgvDanhGia.ReadOnly = true;
            dgvDanhGia.AllowUserToAddRows = false;
            dgvDanhGia.AllowUserToDeleteRows = false;
            dgvDanhGia.MultiSelect = false;
            dgvDanhGia.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            dgvDanhGia.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;

            dgvDanhGia.BackgroundColor = Color.White;
            dgvDanhGia.RowHeadersVisible = false;
            dgvDanhGia.AutoGenerateColumns = false;

            dgvDanhGia.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "MaDG",
                HeaderText = "Mã ĐG",
                FillWeight = 45
            });

            dgvDanhGia.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "MaSP",
                HeaderText = "Mã SP",
                FillWeight = 45
            });

            dgvDanhGia.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "MaKH",
                HeaderText = "Mã KH",
                FillWeight = 45
            });

            dgvDanhGia.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SoSao",
                HeaderText = "Số sao",
                FillWeight = 50
            });

            dgvDanhGia.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "NoiDung",
                HeaderText = "Nội dung",
                FillWeight = 200
            });

            dgvDanhGia.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "NgayDanhGia",
                HeaderText = "Ngày đánh giá",
                FillWeight = 110,
                DefaultCellStyle =
                    new DataGridViewCellStyle
                    {
                        Format = "dd/MM/yyyy HH:mm"
                    }
            });

            dgvDanhGia.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "TrangThai",
                HeaderText = "Trạng thái",
                FillWeight = 85
            });

            panelChinh.Controls.Add(dgvDanhGia, 0, 2);

            var panelNut = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false
            };

            btnDuyet.Text = "Duyệt đánh giá";
            btnDuyet.Width = 160;
            btnDuyet.Height = 40;
            btnDuyet.BackColor = Color.SeaGreen;
            btnDuyet.ForeColor = Color.White;
            btnDuyet.FlatStyle = FlatStyle.Flat;

            btnDuyet.Click += (_, _) =>
                CapNhatTrangThai("Đã duyệt");

            btnTuChoi.Text = "Từ chối đánh giá";
            btnTuChoi.Width = 160;
            btnTuChoi.Height = 40;
            btnTuChoi.BackColor = Color.Firebrick;
            btnTuChoi.ForeColor = Color.White;
            btnTuChoi.FlatStyle = FlatStyle.Flat;

            btnTuChoi.Click += (_, _) =>
                CapNhatTrangThai("Từ chối");

            panelNut.Controls.Add(btnDuyet);
            panelNut.Controls.Add(btnTuChoi);

            panelChinh.Controls.Add(panelNut, 0, 3);

            Controls.Add(panelChinh);
        }

        private void TaiDanhSach()
        {
            try
            {
                var danhSach = _repository.LayTatCa();

                string trangThai =
                    cboTrangThai.SelectedItem?.ToString()
                    ?? "Tất cả";

                if (trangThai != "Tất cả")
                {
                    danhSach = danhSach
                        .Where(x => x.TrangThai == trangThai)
                        .ToList();
                }

                dgvDanhGia.DataSource = null;
                dgvDanhGia.DataSource = danhSach;

                lblThongKe.Text =
                    $"Tổng: {danhSach.Count} đánh giá";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không thể tải danh sách đánh giá:\n"
                    + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void CapNhatTrangThai(string trangThaiMoi)
        {
            if (dgvDanhGia.CurrentRow?.DataBoundItem
                is not DanhGia danhGia)
            {
                MessageBox.Show(
                    "Vui lòng chọn một đánh giá.",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                return;
            }

            if (danhGia.TrangThai != "Chờ duyệt")
            {
                MessageBox.Show(
                    "Chỉ có thể xử lý đánh giá đang chờ duyệt.",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            string hanhDong = trangThaiMoi == "Đã duyệt"
                ? "duyệt"
                : "từ chối";

            var xacNhan = MessageBox.Show(
                $"Bạn có chắc muốn {hanhDong} đánh giá " +
                $"#{danhGia.MaDG}?",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (xacNhan != DialogResult.Yes)
                return;

            try
            {
                _repository.CapNhatTrangThai(
                    danhGia.MaDG,
                    trangThaiMoi
                );

                MessageBox.Show(
                    "Cập nhật trạng thái thành công!",
                    "Thành công",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                TaiDanhSach();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không thể cập nhật đánh giá:\n"
                    + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}
