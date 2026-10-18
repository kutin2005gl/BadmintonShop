using System.Data;
using System.Globalization;
using System.Numerics;

namespace BadmintonShop.Core.Services;

// SV4: kết quả chụp một lần, dùng chung cho bảng, biểu đồ, Excel và PDF.
public sealed record KetQuaBaoCao(DateTime TuNgay,DateTime DenNgay,string LoaiBan,decimal TongDoanhThu,
    IReadOnlyDictionary<string,DataTable> Bang);

public static class BaoCaoService
{
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
        var hoaDon=source.AsEnumerable().GroupBy(x=>x.Field<int>("MaHD")).ToList();
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
        var banChay=Bang("Mã SP",typeof(int),"Sản phẩm",typeof(string),"Số lượng",typeof(long));
        foreach(var g in hoanThanh.SelectMany(x=>x).Where(x=>!x.IsNull("MaSP")).GroupBy(x=>x.Field<int>("MaSP"))
            .Select(g=>new {Ma=g.Key,Ten=g.First().Field<string>("TenSP"),SL=g.Sum(x=>(long)x.Field<int>("SoLuong"))}).OrderByDescending(x=>x.SL).ThenBy(x=>x.Ma))
            banChay.Rows.Add(g.Ma,g.Ten,g.SL);
        bang.Add("Bán chạy",banChay);
        var dmTong=new Dictionary<int,decimal>();var tenDM=new Dictionary<int,string>();
        foreach(var hd in hoanThanh)
        {
            var weights=hd.Where(x=>!x.IsNull("MaDM")).GroupBy(x=>x.Field<int>("MaDM"))
                .ToDictionary(x=>x.Key,x=>x.Sum(r=>r.Field<decimal>("ThanhTien")));
            foreach(var row in hd.Where(x=>!x.IsNull("MaDM"))) tenDM[row.Field<int>("MaDM")]=row.Field<string>("TenDanhMuc")!;
            foreach(var (ma,tien) in PhanBo(hd.First().Field<decimal>("TongTien"),weights)) dmTong[ma]=dmTong.GetValueOrDefault(ma)+tien;
        }
        var danhMuc=Bang("Mã DM",typeof(int),"Danh mục",typeof(string),"Doanh thu",typeof(decimal));
        foreach(var x in dmTong.OrderBy(x=>x.Key)) danhMuc.Rows.Add(x.Key,tenDM[x.Key],x.Value);
        bang.Add("Theo danh mục",danhMuc);
        var trangThai=Bang("Trạng thái",typeof(string),"Số đơn",typeof(int));
        foreach(var tt in QuyTacBanHang.TrangThaiDon) trangThai.Rows.Add(tt,hoaDon.Count(x=>x.First().Field<string>("TrangThai")==tt));
        bang.Add("Trạng thái đơn",trangThai);
        return new KetQuaBaoCao(tu.Date,den.Date,loai??"Tất cả",tong,bang);
    }

    // SV4: tạo bảng có kiểu số để file xuất và lưới giữ nguyên kiểu dữ liệu.
    private static DataTable Bang(params object[] cot)
    {
        var dt=new DataTable();for(int i=0;i<cot.Length;i+=2) dt.Columns.Add((string)cot[i],(Type)cot[i+1]);return dt;
    }
}
