using System.Data;
using BadmintonShop.Core.DataAccess;
using BadmintonShop.Core.Services;
using BadmintonShop.Desktop;
using Microsoft.Data.SqlClient;

internal static partial class Program
{
    private static void AdvancedSearchIntegrationChecks()
    {
        PhienDangNhap.BatDau(1,nv,"search-check","Demo",VaiTro.Admin);
        Test("DB-A01 SQL-backed advanced dialog / boundaries / OR cities / NOT canceled / cached list",()=>
        {
            int[] amounts=[999999,1000000,2000000,2000001];var ids=new List<int>();
            for(int i=0;i<amounts.Length;i++)
            {
                int kh=Convert.ToInt32(DbHelper.ExecuteScalar("INSERT KhachHang(HoTen,Email,MatKhau,DiaChi) VALUES(@name,@email,'test-only',@city);SELECT CAST(SCOPE_IDENTITY() AS int)",
                    new SqlParameter("@name","Lọc "+i),new SqlParameter("@email",$"advanced{i}@example.invalid"),new SqlParameter("@city",i==0?"TP.HCM":i==1?"Hà Nội":"Đà Nẵng")));
                int hd=Convert.ToInt32(DbHelper.ExecuteScalar("INSERT HoaDon(NgayLap,MaKH,TrangThai) VALUES('20261101',@kh,N'Chờ xác nhận');SELECT CAST(SCOPE_IDENTITY() AS int)",new SqlParameter("@kh",kh)));
                DbHelper.ExecuteNonQuery("INSERT ChiTietHoaDon(MaHD,MaSP,SoLuong,DonGia) VALUES(@hd,@sp,1,@price)",new SqlParameter("@hd",hd),new SqlParameter("@sp",sp1),new SqlParameter("@price",amounts[i]));
                DbHelper.ExecuteNonQuery("UPDATE HoaDon SET TrangThai=@state WHERE MaHD=@hd",new SqlParameter("@hd",hd),new SqlParameter("@state",i==2?"Đã hủy":"Hoàn thành"));ids.Add(hd);
            }
            using var form=new FrmDonHang();form.Show();WaitUi(form);
            var dates=Controls(form).OfType<DateTimePicker>().ToArray();dates[0].Value=new DateTime(2026,11,1);dates[1].Value=new DateTime(2026,11,1);
            Button Button(string text)=>Controls(form).OfType<Button>().Single(b=>b.Text==text);
            Button("Tải / lọc").PerformClick();WaitUi(form);
            var grid=Controls(form).OfType<DataGridView>().Single(g=>g.Columns.Contains("MaHD"));Eq(4,grid.Rows.Count);
            // Warm each row's detail to count subsequent filtering and selection together.
            foreach(DataGridViewRow row in grid.Rows) { grid.CurrentCell=row.Cells["TongTien"];WaitUi(form); }
            using var counter=new SqlCounter();
            void Apply(int field1,string op1,string val1,int field2,string op2,string val2,string join,bool not=false,bool second=true)
            {
                Exception? error=null;
                using var timer=new System.Windows.Forms.Timer { Interval=100 };
                timer.Tick+=(_,_)=>
                {
                    var dialog=Application.OpenForms.OfType<FrmLocNangCao>().FirstOrDefault();if(dialog==null)return;timer.Stop();
                    try
                    {
                        var combos=Controls(dialog).OfType<ComboBox>().ToArray();var inputs=Controls(dialog).OfType<TextBox>().ToArray();var checks=Controls(dialog).OfType<CheckBox>().ToArray();
                        combos[0].SelectedIndex=field1;combos[1].SelectedItem=op1;inputs[0].Text=val1;checks[0].Checked=not;
                        combos[2].SelectedIndex=field2;combos[3].SelectedItem=op2;inputs[1].Text=val2;combos[4].SelectedItem=join;checks[2].Checked=second;
                        Controls(dialog).OfType<Button>().Single(b=>b.Text=="Áp dụng").PerformClick();
                    }
                    catch(Exception ex) { error=ex;dialog.Close(); }
                };
                timer.Start();Button("Tìm nâng cao").PerformClick();WaitUi(form);if(error!=null)throw error;
            }
            string Ids()=>string.Join(",",grid.Rows.Cast<DataGridViewRow>().Select(r=>Convert.ToInt32(r.Cells["MaHD"].Value)).Order());
            Apply(0,">=","1000000",0,"<=","2000000","AND");Eq(string.Join(",",ids.Skip(1).Take(2)),Ids());
            Apply(4,"=","TP.HCM",4,"=","Hà Nội","OR");Eq(string.Join(",",ids.Take(2)),Ids());
            Apply(2,"=","Đã hủy",2,"=","Đã hủy","AND",not:true,second:false);Eq(string.Join(",",new[]{ids[0],ids[1],ids[3]}),Ids());
            Eq(0,counter.Count);Ghi("SQL advanced dialog AND/OR/NOT + selection after detail warm-up=0");
            using var bitmap=new Bitmap(form.Width,form.Height);form.DrawToBitmap(bitmap,form.ClientRectangle);bitmap.Save(Path.Combine(output,"advanced-search.png"));form.Close();
        });
        Test("DB-C04 exact expiry queries SQL again / cached snapshot before expiry",()=>
        {
            var now=DateTimeOffset.UtcNow;var cache=new BoNhoPhien<int,DataTable>(TimeSpan.FromMinutes(10),()=>now);using var counter=new SqlCounter();
            DataTable Read()=>cache.Lay(0,"demo",()=>new SanPhamRepository().LayDanhSachHienThi());
            Read();Eq(1,counter.Count);now=now.AddMinutes(9);Read();Eq(1,counter.Count);now=now.AddMinutes(1);Read();Eq(2,counter.Count);
            Ghi("SQL expiry: initial=1; 9 minutes extra=0; exactly 10 minutes extra=1 (injected clock)");
        });
        PhienDangNhap.DangXuat();
    }
}
