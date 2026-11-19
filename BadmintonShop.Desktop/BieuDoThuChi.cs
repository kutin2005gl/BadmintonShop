using System.Data;
using System.Drawing.Drawing2D;

namespace BadmintonShop.Desktop;

internal sealed class BieuDoThuChi : Panel
{
    // TV1: khoi tao bieu do thu chi tu bang du lieu
    public BieuDoThuChi(DataTable bang)
    {
        Dock = DockStyle.Fill;
        BackColor = Color.White;
        var scroll = new Panel { AutoScroll = true, Dock = DockStyle.Fill };
        var cot = new CacCot(bang);
        scroll.Controls.Add(cot);
        scroll.SizeChanged += (_, _) =>
        {
            var rong = Math.Max(scroll.ClientSize.Width, 88 + 65 * bang.Rows.Count);
            var cao = scroll.ClientSize.Height - (rong > scroll.ClientSize.Width ? SystemInformation.HorizontalScrollBarHeight : 0);
            cot.Size = new Size(rong, Math.Max(140, cao));
        };
        Controls.Add(scroll);
        Controls.Add(new Label
        {
            Text = "Thu và chi theo kỳ (đồng)",
            Dock = DockStyle.Top, Height = 34, Padding = new Padding(12, 6, 0, 0),
            Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = ShopTheme.Ink
        });
    }

    private sealed class CacCot(DataTable bang) : Control
    {
        private static readonly Color MauChi = Color.FromArgb(186, 91, 32);
        private readonly (string Ky, decimal Thu, decimal Chi)[] ky = bang.AsEnumerable()
            .Select(r => (r.Field<string>("Kỳ")!, r.Field<decimal>("Thu"), r.Field<decimal>("Chi"))).ToArray();

        // TV1: ve cac cot thu chi theo ky
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using var thu = new SolidBrush(ShopTheme.Green);
            using var chi = new SolidBrush(MauChi);
            using var text = new SolidBrush(ShopTheme.Ink);
            using var line = new Pen(ShopTheme.Line);
            using var canGiua = new StringFormat { Alignment = StringAlignment.Center };
            using var canPhai = new StringFormat { Alignment = StringAlignment.Far };
            g.FillRectangle(thu, 12, 7, 11, 11);
            g.DrawString("Thu", Font, text, 27, 4);
            g.FillRectangle(chi, 74, 7, 11, 11);
            g.DrawString("Chi", Font, text, 89, 4);

            const int dinh = 38;
            var day = Height - 30;
            var max = ky.Max(x => Math.Max(x.Thu, x.Chi));
            var nhanMax = max.ToString("N0");
            var trai = Math.Max(55, g.MeasureString(nhanMax, Font).Width + 8);
            var phai = Width - 12;
            for (int i = 0; i <= 4; i++)
            {
                var y = dinh + (day - dinh) * i / 4f;
                g.DrawLine(line, trai, y, phai, y);
                var tien = max * (4 - i) / 4;
                if (max > 0 || i == 4)
                    g.DrawString(tien.ToString("N0"), Font, text, trai - 5, y - Font.Height / 2f, canPhai);
            }
            var khoangKy = (phai - trai) / ky.Length;
            var rongCot = Math.Min(27, khoangKy / 3);
            for (int i = 0; i < ky.Length; i++)
            {
                var x = trai + khoangKy * (i + .5f);
                if (x + khoangKy < e.ClipRectangle.Left || x - khoangKy > e.ClipRectangle.Right) continue;
                VeCot(thu, x - rongCot - 1, ky[i].Thu);
                VeCot(chi, x + 1, ky[i].Chi);
                g.DrawString(ky[i].Ky, Font, text, x, day + 5, canGiua);
            }

            void VeCot(Brush mau, float x, decimal tien)
            {
                // Dấu màu tại trục cho biết chuỗi có giá trị 0 ở kỳ này.
                var cao = max == 0 ? 2 : Math.Max(2, (float)(tien / max) * (day - dinh));
                g.FillRectangle(mau, x, day - cao, rongCot, cao);
            }
        }

        // TV1: bat dem ve va ve lai khi doi kich thuoc
        protected override void OnHandleCreated(EventArgs e)
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            base.OnHandleCreated(e);
        }
    }
}
