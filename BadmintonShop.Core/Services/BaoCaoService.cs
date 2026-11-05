using System.Data;
using System.Globalization;
using System.Numerics;
using BadmintonShop.Core.Models;

namespace BadmintonShop.Core.Services;

// SV4: kết quả chụp một lần, dùng chung cho bảng, biểu đồ, Excel và PDF.
public sealed record KetQuaBaoCao(DateTime TuNgay,DateTime DenNgay,string LoaiBan,decimal TongDoanhThu,
    IReadOnlyDictionary<string,DataTable> Bang);

public static class BaoCaoService
{
    // Chi là tổng tiền phiếu nhập trong kỳ, không phải giá vốn hàng đã bán.
    // Chỉ lập thu–chi toàn cửa hàng; không trừ chi nhập chung khỏi thu riêng một kênh bán.
    public static KetQuaBaoCao Tao(DuLieuBaoCao source,DateTime tu,DateTime den,string? loai)
    {
        var ketQua=Tao(source.BanHang,tu,den,loai);
        if(loai!=null)return ketQua;
        var bang=new Dictionary<string,DataTable>(ketQua.Bang);
        var thu=source.BanHang.AsEnumerable()
            .Where(r=>r.Field<DateTime>("NgayLap").Date>=tu.Date && r.Field<DateTime>("NgayLap").Date<=den.Date
                && r.Field<string>("TrangThai")=="Hoàn thành")
            .GroupBy(r=>r.Field<int>("MaHD")).Select(g=>g.First())
            .GroupBy(r=>(r.Field<DateTime>("NgayLap").Year,(r.Field<DateTime>("NgayLap").Month-1)/3+1))
            .ToDictionary(g=>g.Key,g=>g.Sum(r=>r.Field<decimal>("TongTien")));
        var chi=source.NhapHang.AsEnumerable()
            .Where(r=>r.Field<DateTime>("NgayNhap").Date>=tu.Date && r.Field<DateTime>("NgayNhap").Date<=den.Date)
            .GroupBy(r=>r.Field<int>("MaPN")).Select(g=>g.First())
            .GroupBy(r=>(r.Field<DateTime>("NgayNhap").Year,(r.Field<DateTime>("NgayNhap").Month-1)/3+1))
            .ToDictionary(g=>g.Key,g=>g.Sum(r=>r.Field<decimal>("TongTien")));
        for(int nam=tu.Year;nam<=den.Year;nam++)
        {
            var dt=Bang("Mã",typeof(int),"Chỉ tiêu",typeof(string),"Q1",typeof(decimal),"Q2",typeof(decimal),
                "Q3",typeof(decimal),"Q4",typeof(decimal),"Tổng",typeof(decimal));
            var t=Enumerable.Range(1,4).Select(q=>thu.GetValueOrDefault((nam,q))).ToArray();
            var c=Enumerable.Range(1,4).Select(q=>chi.GetValueOrDefault((nam,q))).ToArray();
            dt.Rows.Add(1,"Thu bán hàng",t[0],t[1],t[2],t[3],t.Sum());
            dt.Rows.Add(2,"Chi nhập hàng",c[0],c[1],c[2],c[3],c.Sum());
            dt.Rows.Add(3,"Chênh lệch thu–chi",t[0]-c[0],t[1]-c[1],t[2]-c[2],t[3]-c[3],t.Sum()-c.Sum());
            bang.Add($"Thu–chi {nam}",dt);
        }
        return ketQua with { Bang=bang };
    }

    // SV4: phân bổ đồng nguyên theo phần dư lớn nhất, tổng luôn bằng net hóa đơn.
    public static Dictionary<int,decimal> PhanBo(decimal net,IReadOnlyDictionary<int,decimal> trongSo)
    {
        QuyTacBanHang.KiemTraTien(net);
        var tong=trongSo.Values.Sum();
        if(trongSo.Values.Any(x=>x<0 || decimal.Truncate(x)!=x) || (tong==0 && net!=0)) throw new InvalidOperationException("Chi tiết không khớp tổng hóa đơn.");
        var total=new BigInteger(tong);var n=new BigInteger(net);
        var ketQua=trongSo.ToDictionary(x=>x.Key,x=>tong==0?0:(decimal)(n*new BigInteger(x.Value)/total));
        var du=(int)(net-ketQua.Values.Sum());
        foreach(var x in trongSo.OrderByDescending(x=>tong==0?BigInteger.Zero:n*new BigInteger(x.Value)%total).ThenBy(x=>x.Key).Take(du)) ketQua[x.Key]++;
        return ketQua;
    }

    // SV4: chỉ Hoàn thành ghi doanh thu/số bán; số đơn theo trạng thái đếm tất cả, kể cả Đã hủy.
    public static KetQuaBaoCao Tao(DataTable source,DateTime tu,DateTime den,string? loai)
    {
        if(tu.Date>den.Date) throw new ArgumentException("Ngày bắt đầu phải trước ngày kết thúc.");
        var hoaDon=source.AsEnumerable()
            .Where(x=>x.Field<DateTime>("NgayLap").Date>=tu.Date && x.Field<DateTime>("NgayLap").Date<=den.Date
                && (loai==null || x.Field<string>("LoaiBan")==loai))
            .GroupBy(x=>x.Field<int>("MaHD")).ToList();
        var hoanThanh=hoaDon.Where(x=>x.First().Field<string>("TrangThai")=="Hoàn thành").ToList();
        var tong=hoanThanh.Sum(x=>x.First().Field<decimal>("TongTien"));
        var bang=new Dictionary<string,DataTable>();
        foreach(var (ten,format) in new[]{("Theo ngày","yyyy-MM-dd"),("Theo tháng","yyyy-MM"),("Theo năm","yyyy")})
        {
            var dt=Bang("Kỳ",typeof(string),"Doanh thu",typeof(decimal));
            foreach(var g in hoanThanh.GroupBy(x=>x.First().Field<DateTime>("NgayLap").ToString(format,CultureInfo.InvariantCulture)).OrderBy(x=>x.Key))
                dt.Rows.Add(g.Key,g.Sum(x=>x.First().Field<decimal>("TongTien")));
            bang.Add(ten,dt);
        }
        var quyTong=Bang("Kỳ",typeof(string),"Doanh thu",typeof(decimal));
        foreach(var g in hoanThanh.GroupBy(h=>
            { var ngay=h.First().Field<DateTime>("NgayLap");return (Nam:ngay.Year,Quy:(ngay.Month-1)/3+1); })
            .OrderBy(g=>g.Key.Nam).ThenBy(g=>g.Key.Quy))
            quyTong.Rows.Add($"{g.Key.Nam}-Q{g.Key.Quy}",g.Sum(h=>h.First().Field<decimal>("TongTien")));
        bang.Add("Theo quý",quyTong);
        var banChay=Bang("Mã SP",typeof(int),"Sản phẩm",typeof(string),"Số lượng",typeof(long));
        foreach(var g in hoanThanh.SelectMany(x=>x).Where(x=>!x.IsNull("MaSP")).GroupBy(x=>x.Field<int>("MaSP"))
            .Select(g=>new {Ma=g.Key,Ten=g.First().Field<string>("TenSP"),SL=g.Sum(x=>(long)x.Field<int>("SoLuong"))}).OrderByDescending(x=>x.SL).ThenBy(x=>x.Ma))
            banChay.Rows.Add(g.Ma,g.Ten,g.SL);
        bang.Add("Bán chạy",banChay);
        var dmTong=new Dictionary<int,decimal>();var tenDM=new Dictionary<int,string>();
        var doanhThuDong=new List<(int MaSP,string TenSP,int Nam,int Quy,decimal Tien)>();
        foreach(var hd in hoanThanh)
        {
            // Phân bổ một lần theo sản phẩm; mọi chiều cộng lại từ cùng số tiền dòng.
            var weights=hd.Where(x=>!x.IsNull("MaSP")).GroupBy(x=>x.Field<int>("MaSP"))
                .ToDictionary(x=>x.Key,x=>x.Sum(r=>r.Field<decimal>("ThanhTien")));
            var phanBo=PhanBo(hd.First().Field<decimal>("TongTien"),weights);
            foreach(var sp in hd.Where(x=>!x.IsNull("MaSP")).GroupBy(x=>x.Field<int>("MaSP")))
            {
                var row=sp.First();var ngay=row.Field<DateTime>("NgayLap");var tien=phanBo[sp.Key];
                doanhThuDong.Add((sp.Key,row.Field<string>("TenSP")!,ngay.Year,(ngay.Month-1)/3+1,tien));
                var maDM=row.Field<int>("MaDM");tenDM[maDM]=row.Field<string>("TenDanhMuc")!;
                dmTong[maDM]=dmTong.GetValueOrDefault(maDM)+tien;
            }
        }
        var danhMuc=Bang("Mã DM",typeof(int),"Danh mục",typeof(string),"Doanh thu",typeof(decimal));
        foreach(var x in dmTong.OrderBy(x=>x.Key)) danhMuc.Rows.Add(x.Key,tenDM[x.Key],x.Value);
        bang.Add("Theo danh mục",danhMuc);
        var trangThai=Bang("Trạng thái",typeof(string),"Số đơn",typeof(int));
        foreach(var tt in QuyTacBanHang.TrangThaiDon) trangThai.Rows.Add(tt,hoaDon.Count(x=>x.First().Field<string>("TrangThai")==tt));
        bang.Add("Trạng thái đơn",trangThai);
        foreach(var (maCot,tenCot,tenBang,nhomRong) in new[]
            {("MaNV","TenNhanVien","Nhân viên × năm","Chưa gán nhân viên"),
             ("MaKH","TenKhachHang","Khách hàng × năm","Khách vãng lai")})
        {
            var dt=Bang("Mã",typeof(int),"Tên",typeof(string));
            for(int nam=tu.Year;nam<=den.Year;nam++)dt.Columns.Add(nam.ToString(CultureInfo.InvariantCulture),typeof(decimal));
            dt.Columns.Add("Tổng",typeof(decimal));
            var tongNam=new decimal[den.Year-tu.Year+1];
            foreach(var nhom in hoanThanh.GroupBy(h=>h.First().Field<int?>(maCot)).OrderBy(g=>g.Key))
            {
                var row=dt.NewRow();row[0]=nhom.Key.HasValue?nhom.Key.Value:DBNull.Value;
                row[1]=nhom.Key.HasValue?nhom.First().First().Field<string>(tenCot)??$"Mã {nhom.Key}":nhomRong;
                var theoNam=nhom.GroupBy(h=>h.First().Field<DateTime>("NgayLap").Year)
                    .ToDictionary(g=>g.Key,g=>g.Sum(h=>h.First().Field<decimal>("TongTien")));
                for(int nam=tu.Year;nam<=den.Year;nam++)
                { var tien=theoNam.GetValueOrDefault(nam);row[nam-tu.Year+2]=tien;tongNam[nam-tu.Year]+=tien; }
                row["Tổng"]=theoNam.Values.Sum();dt.Rows.Add(row);
            }
            var total=dt.NewRow();total[0]=DBNull.Value;total[1]="Tất cả";
            for(int i=0;i<tongNam.Length;i++)total[i+2]=tongNam[i];
            total["Tổng"]=tongNam.Sum();dt.Rows.Add(total);bang.Add(tenBang,dt);
        }
        // Mỗi năm một bảng; nếu chọn A–B chưa trọn năm thì chỉ tính phần giao với A–B.
        // Giữ sản phẩm xuất hiện trong tập đã lọc, kể cả chỉ có đơn chưa hoàn thành.
        var sanPham=hoaDon.SelectMany(x=>x).Where(x=>!x.IsNull("MaSP"))
            .GroupBy(x=>x.Field<int>("MaSP")).OrderBy(x=>x.Key)
            .Select(x=>(Ma:x.Key,Ten:x.First().Field<string>("TenSP"))).ToList();
        var theoQuy=doanhThuDong.GroupBy(x=>(x.MaSP,x.Nam,x.Quy)).ToDictionary(x=>x.Key,x=>x.Sum(r=>r.Tien));
        for(int nam=tu.Year;nam<=den.Year;nam++)
        {
            var dt=Bang("Mã SP",typeof(int),"Sản phẩm",typeof(string),"Q1",typeof(decimal),"Q2",typeof(decimal),
                "Q3",typeof(decimal),"Q4",typeof(decimal),"Tổng",typeof(decimal));
            var tongQuy=new decimal[4];
            foreach(var sp in sanPham)
            {
                var quy=Enumerable.Range(1,4).Select(q=>theoQuy.GetValueOrDefault((sp.Ma,nam,q))).ToArray();
                dt.Rows.Add(sp.Ma,sp.Ten,quy[0],quy[1],quy[2],quy[3],quy.Sum());
                for(int q=0;q<4;q++)tongQuy[q]+=quy[q];
            }
            dt.Rows.Add(DBNull.Value,"Tất cả",tongQuy[0],tongQuy[1],tongQuy[2],tongQuy[3],tongQuy.Sum());
            bang.Add($"Sản phẩm × quý {nam}",dt);
        }
        return new KetQuaBaoCao(tu.Date,den.Date,loai??"Tất cả",tong,bang);
    }

    // SV4: tạo bảng có kiểu số để file xuất và lưới giữ nguyên kiểu dữ liệu.
    private static DataTable Bang(params object[] cot)
    {
        var dt=new DataTable();for(int i=0;i<cot.Length;i+=2) dt.Columns.Add((string)cot[i],(Type)cot[i+1]);return dt;
    }
}
