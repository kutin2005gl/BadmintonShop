using BadmintonShop.Core.DataAccess;
using BadmintonShop.Core.Models;
using BadmintonShop.Core.Services;

namespace BadmintonShop.Web.Services;

// Only public catalog data lives in this application singleton, never customer orders/profiles.
public sealed class DanhMucCongKhai
{
    private readonly BoNhoPhien<int,(List<SanPham> Products,List<DanhMuc> Categories)> cache=
        new(BoNhoPhien<int,int>.ThoiHanMacDinh);
    public (List<SanPham> Products,List<DanhMuc> Categories) Lay(bool refresh=false)=>cache.Lay(0,
        $"{DbHelper.ConnectionString}|{DbHelper.DauHieuDuLieu}",
        ()=> (new SanPhamRepository().LayTatCa(),new DanhMucRepository().LayTatCa()),refresh);
}
