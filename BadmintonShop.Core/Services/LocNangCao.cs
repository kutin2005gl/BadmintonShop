using System.Data;
using System.Globalization;

namespace BadmintonShop.Core.Services;

public sealed record DieuKienLoc(string Truong,string ToanTu,string GiaTri,bool PhuDinh=false);

public static class LocNangCao
{
    // TV1: Khai bao toan tu tim chuoi trong van ban.
    public const string Chua = "chứa";
    public static readonly string[] ToanTu=[">","<",">=","<=","=","!=",Chua];
    public static Func<DataRow,bool> Tao(DataTable source,IReadOnlySet<string> allowed,
        DieuKienLoc first,DieuKienLoc? second=null,string join="AND")
    {
        if(join is not ("AND" or "OR"))throw new ArgumentException("Chỉ dùng AND hoặc OR.");
        (Func<DataRow,bool> Predicate,object Value) Compile(DieuKienLoc condition)
        {
            if(!allowed.Contains(condition.Truong) || !source.Columns.Contains(condition.Truong))throw new ArgumentException("Trường lọc không hợp lệ.");
            if(!ToanTu.Contains(condition.ToanTu))throw new ArgumentException("Toán tử không hợp lệ.");
            var type=source.Columns[condition.Truong]!.DataType;var text=condition.GiaTri.Trim();
            // TV1: Chan toan tu chua tren so va ngay truoc khi doc gia tri.
            if(condition.ToanTu==Chua && type!=typeof(string))throw new ArgumentException("Toán tử chứa chỉ dùng cho trường văn bản.");
            if(text.Length==0)throw new ArgumentException("Hãy nhập giá trị lọc.");
            object value;
            if(type==typeof(string))
            {
                if(condition.ToanTu is not ("=" or "!=" or Chua))throw new ArgumentException("Văn bản chỉ dùng =, != hoặc chứa.");
                value=text;
            }
            else if(type==typeof(DateTime))
            {
                if(!DateTime.TryParseExact(text,"dd/MM/yyyy",CultureInfo.InvariantCulture,DateTimeStyles.None,out var date))throw new ArgumentException("Ngày phải đúng dd/MM/yyyy.");
                value=date;
            }
            else if(type==typeof(decimal) || type==typeof(int) || type==typeof(long))
            {
                if(!decimal.TryParse(text,NumberStyles.Number,CultureInfo.GetCultureInfo("vi-VN"),out var number) || number!=decimal.Truncate(number))throw new ArgumentException("Giá trị phải là số nguyên (đồng / mã).");
                value=number;
            }
            else throw new ArgumentException("Kiểu trường không hỗ trợ.");
            bool Predicate(DataRow row)
            {
                // Missing values do not match comparisons; NOT includes the missing group.
                bool matches=false;
                if(!row.IsNull(condition.Truong))
                {
                    int cmp=type==typeof(string)?StringComparer.CurrentCultureIgnoreCase.Compare(row[condition.Truong],value):
                        type==typeof(DateTime)?((DateTime)row[condition.Truong]).Date.CompareTo((DateTime)value):Convert.ToDecimal(row[condition.Truong]).CompareTo((decimal)value);
                    // TV1: Tim chuoi khong phan biet hoa thuong tren o co gia tri.
                    matches=condition.ToanTu switch { Chua=>((string)row[condition.Truong]).Contains((string)value,StringComparison.CurrentCultureIgnoreCase),">"=>cmp>0,"<"=>cmp<0,">="=>cmp>=0,"<="=>cmp<=0,"="=>cmp==0,"!="=>cmp!=0,_=>false };
                }
                return condition.PhuDinh?!matches:matches;
            }
            return (Predicate,value);
        }
        var a=Compile(first);if(second==null)return a.Predicate;var b=Compile(second);
        if(join=="AND" && first.Truong==second.Truong && !first.PhuDinh && !second.PhuDinh)
        {
            object? lower=first.ToanTu is ">" or ">="?a.Value:second.ToanTu is ">" or ">="?b.Value:null;
            object? upper=first.ToanTu is "<" or "<="?a.Value:second.ToanTu is "<" or "<="?b.Value:null;
            if(lower is IComparable comparable && upper!=null && comparable.CompareTo(upper)>0)throw new ArgumentException("Cận dưới phải nhỏ hơn hoặc bằng cận trên.");
        }
        return join=="AND"?row=>a.Predicate(row)&&b.Predicate(row):row=>a.Predicate(row)||b.Predicate(row);
    }
}
