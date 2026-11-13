using System.Data;
using System.Diagnostics;
using BadmintonShop.Core.DataAccess;
using BadmintonShop.Core.Services;
using BadmintonShop.Desktop;

internal static partial class Program
{
    // Provider diagnostics include commands issued directly on transactions and adapters,
    // not just DbHelper.ExecuteQuery. No connection string or parameter values are logged.
    private sealed class SqlCounter : IObserver<DiagnosticListener>,IObserver<KeyValuePair<string,object?>>,IDisposable
    {
        private readonly List<IDisposable> subscriptions=[];
        private int count;
        public int Count=>Volatile.Read(ref count);
        public SqlCounter()=>subscriptions.Add(DiagnosticListener.AllListeners.Subscribe(this));
        public void OnNext(DiagnosticListener listener)
        {
            if(listener.Name.Contains("SqlClient"))subscriptions.Add(listener.Subscribe(this,key=>key.EndsWith("WriteCommandBefore",StringComparison.Ordinal)));
        }
        public void OnNext(KeyValuePair<string,object?> value)=>Interlocked.Increment(ref count);
        public void OnCompleted() { }
        public void OnError(Exception error) { }
        public void Dispose() { foreach(var subscription in subscriptions)subscription.Dispose(); }
    }
    private static void WaitUi(Form form)
    {
        var timer=Stopwatch.StartNew();
        do
        {
            Application.DoEvents();Thread.Sleep(20);
            var labels=Controls(form).OfType<Label>().ToArray();
            var error=labels.FirstOrDefault(l=>l.Text.StartsWith("Lỗi:"));
            if(error!=null)throw new Exception(error.Text);
            if(timer.ElapsedMilliseconds>150 && !labels.Any(l=>l.Text.StartsWith("Đang tải")))return;
        }while(timer.Elapsed<TimeSpan.FromSeconds(15));
        throw new TimeoutException("UI load did not settle");
    }
    private static void DesktopCacheChecks()
    {
        PhienDangNhap.BatDau(1,nv,"demo-check","Demo",VaiTro.Admin);
        Test("DB-C01 actual product form / ten keywords + image selection / refresh / committed change",()=>
        {
            using var counter=new SqlCounter();using var form=new FrmSanPham();form.Show();WaitUi(form);
            int initial=counter.Count;True(initial>0);
            var search=Controls(form).OfType<TextBox>().Single();var grid=Controls(form).OfType<DataGridView>().Single();
            Button Button(string text)=>Controls(form).OfType<Button>().Single(b=>b.Text==text);
            foreach(var keyword in new[]{"Vợt","Cầu","thử","SV4","1","2","3","không có","", "Vợt"})
            {
                search.Text=keyword;Button("Lọc").PerformClick();WaitUi(form);
                foreach(DataGridViewRow row in grid.Rows) { grid.CurrentCell=row.Cells["TenSP"];Application.DoEvents(); }
                WaitUi(form);
            }
            Eq(initial,counter.Count);Ghi($"SQL product initial={initial}; ten keywords + selection extra={counter.Count-initial}");
            Button("Tải lại").PerformClick();WaitUi(form);Eq(initial+1,counter.Count);
            var product=new SanPhamRepository().LayTheoMa(sp1)!;product.TenSP="Vợt đổi sau lưu";new SanPhamRepository().Luu(product);
            int before=counter.Count;search.Text="Vợt đổi sau lưu";Button("Lọc").PerformClick();WaitUi(form);
            Eq(before+1,counter.Count);Eq(1,grid.Rows.Count);Eq(product.TenSP,grid.Rows[0].Cells["TenSP"].Value.ToString());
            Ghi("SQL product refresh=1; filter after committed edit reload=1");form.Close();
        });
        Test("DB-C02 report form / same filters zero SQL / explicit refresh / mutation invalidation",()=>
        {
            using var counter=new SqlCounter();using var form=new FrmThongKe();form.Show();WaitUi(form);
            int initial=counter.Count;True(initial>=2);
            var report=(KetQuaBaoCao)typeof(FrmThongKe).GetField("ketQua",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)!.GetValue(form)!;
            var grids=Controls(form).OfType<DataGridView>().ToArray();Eq(report.Bang.Count,grids.Length);
            foreach(var grid in grids)
            {
                var data=(System.Data.DataTable)grid.DataSource!;True(report.Bang.Values.Contains(data));
                for(int r=0;r<data.Rows.Count;r++)for(int c=0;c<data.Columns.Count;c++)Eq(data.Rows[r][c],grid.Rows[r].Cells[c].Value);
            }
            foreach(var chart in Controls(form).OfType<BieuDoCot>())
            {
                var d=chart.DuLieu!;if(report.Bang.Values.Contains(d))continue;
                // CO-06: biểu đồ "Sản phẩm × quý" vẽ Q1–Q4 của dòng đang chọn; phải khớp đúng một dòng của bảng báo cáo.
                True(d.Columns[1].ColumnName.StartsWith("Doanh thu — ",StringComparison.Ordinal) && d.Rows.Count==4);
                var q=Enumerable.Range(0,4).Select(i=>d.Rows[i].Field<decimal>(1)).ToArray();
                True(report.Bang.Where(b=>b.Key.StartsWith("Sản phẩm × quý ",StringComparison.Ordinal)).SelectMany(b=>b.Value.AsEnumerable())
                    .Any(r=>Enumerable.Range(1,4).Select(i=>r.Field<decimal>($"Q{i}")).SequenceEqual(q)));
            }
            XuatTaiLieu.Excel(Path.Combine(output,"ui-report.xlsx"),report);XuatTaiLieu.BaoCaoPdf(Path.Combine(output,"ui-report.pdf"),report);
            ExportExpected("ui-report",report);Eq(initial,counter.Count);
            Button Button(string text)=>Controls(form).OfType<Button>().Single(b=>b.Text==text);
            Button("Xem báo cáo").PerformClick();WaitUi(form);Eq(initial,counter.Count);
            Button("Làm mới").PerformClick();WaitUi(form);Eq(initial+2,counter.Count);
            DbHelper.ExecuteNonQuery("UPDATE PhieuNhap SET NgayNhap=NgayNhap WHERE MaPN=-1");
            int before=counter.Count;Button("Xem báo cáo").PerformClick();WaitUi(form);Eq(before+2,counter.Count);
            Ghi($"SQL report initial={initial}; repeated scope=0; refresh=2; after mutation=2");form.Close();
        });
        PhienDangNhap.DangXuat();
    }
    private static void MainSubChecks()
    {
        PhienDangNhap.BatDau(1,nv,"demo-check","Demo",VaiTro.Admin);
        Test("DB-M01 invoice and receipt every visible cell / matching detail / empty list",()=>
        {
            foreach(Form form in new Form[]{new FrmDonHang(),new FrmDanhSachPhieuNhap()})
            using(form)
            {
                form.Show();WaitUi(form);
                bool invoice=form is FrmDonHang;
                var grids=Controls(form).OfType<DataGridView>().ToArray();
                var main=grids.Single(g=>g.Columns.Contains(invoice?"MaHD":"MaPN"));var sub=grids.Single(g=>g!=main);
                if(invoice)
                {
                    var dates=Controls(form).OfType<DateTimePicker>().ToArray();
                    dates[0].Value=new DateTime(2025,1,1);dates[1].Value=new DateTime(2026,12,31);
                    Controls(form).OfType<Button>().Single(b=>b.Text=="Tải / lọc").PerformClick();WaitUi(form);
                }
                True(main.Rows.Count>=2);
                foreach(var row in main.Rows.Cast<DataGridViewRow>().Take(3))
                foreach(var column in main.Columns.Cast<DataGridViewColumn>().Where(c=>c.Visible))
                {
                    main.CurrentCell=row.Cells[column.Index];WaitUi(form);
                    int id=Convert.ToInt32(row.Cells[invoice?"MaHD":"MaPN"].Value);
                    var expected=invoice?new HoaDonRepository().LayChiTietHienThi(id):new PhieuNhapRepository().LayChiTietHienThi(id);
                    var actual=(System.Data.DataTable)sub.DataSource!;
                    Eq(expected.Rows.Count,actual.Rows.Count);
                    for(int i=0;i<expected.Rows.Count;i++)
                    { Eq(expected.Rows[i]["MaSP"],actual.Rows[i]["MaSP"]);Eq(expected.Rows[i]["ThanhTien"],actual.Rows[i]["ThanhTien"]); }
                }
                main.DataSource=((System.Data.DataTable)main.DataSource!).Clone();WaitUi(form);True(sub.DataSource==null);
                form.Close();
            }
        });
        PhienDangNhap.DangXuat();
    }
}
