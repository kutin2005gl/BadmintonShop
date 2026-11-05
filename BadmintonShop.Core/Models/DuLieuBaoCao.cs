using System.Data;

namespace BadmintonShop.Core.Models;

// Hai tập riêng trong cùng giao dịch đọc: chi nhập không bị nhân theo dòng bán.
public sealed record DuLieuBaoCao(DataTable BanHang, DataTable NhapHang);
