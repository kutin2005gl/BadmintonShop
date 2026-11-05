using System.Data;
using BadmintonShop.Core.Services;

internal static partial class Program
{
    private static void AdvancedSearchChecks()
    {
        Test("A01 six operators / inclusive million boundaries / AND OR NOT / city groups",()=>
        {
            var table=new DataTable();table.Columns.Add("Id",typeof(int));table.Columns.Add("Total",typeof(decimal));table.Columns.Add("City");table.Columns.Add("State");table.Columns.Add("Date",typeof(DateTime));
            table.Rows.Add(1,999999m,"TP.HCM","Hoàn thành",new DateTime(2026,1,1));
            table.Rows.Add(2,1000000m,"Hà Nội","Hoàn thành",new DateTime(2026,1,2));
            table.Rows.Add(3,2000000m,"Đà Nẵng","Đã hủy",new DateTime(2026,1,3));
            table.Rows.Add(4,2000001m,DBNull.Value,"Hoàn thành",new DateTime(2026,1,4));
            var allowed=table.Columns.Cast<DataColumn>().Select(c=>c.ColumnName).ToHashSet();
            string Ids(DieuKienLoc a,DieuKienLoc? b=null,string join="AND")=>string.Join(",",table.AsEnumerable().Where(LocNangCao.Tao(table,allowed,a,b,join)).Select(r=>r["Id"]));
            Eq("2,3",Ids(new("Total",">=","1000000"),new("Total","<=","2000000")));
            Eq("1,2",Ids(new("City","=","TP.HCM"),new("City","=","Hà Nội"),"OR"));
            Eq("1,2,4",Ids(new("State","=","Đã hủy",true)));
            Eq("3,4",Ids(new("Total",">","1000000")));Eq("1",Ids(new("Total","<","1000000")));
            Eq("2",Ids(new("Total","=","1000000")));Eq("1,3,4",Ids(new("Total","!=","1000000")));
            Eq("2,3,4",Ids(new("Date",">=","02/01/2026")));
            foreach(var condition in new[]{new DieuKienLoc("Total","=",""),new("Total","=","abc"),new("Date","=","31/02/2026"),new("City",">","a"),new("Unknown","=","1"),new("Total","==","1")})
                Reject(()=>LocNangCao.Tao(table,allowed,condition));
            Reject(()=>LocNangCao.Tao(table,allowed,new("Total",">=","2000000"),new("Total","<=","1000000")));
        });
    }
}
