using System.Data;
using BadmintonShop.Core.Services;

namespace BadmintonShop.Desktop;

public sealed class FrmLocNangCao : Form
{
    public Func<DataRow,bool>? KetQua { get; private set; }
    public string MoTa { get; private set; }="";
    public FrmLocNangCao(DataTable source)
    {
        Text="Tìm kiếm nâng cao";Size=new Size(900,290);MinimumSize=Size;StartPosition=FormStartPosition.CenterParent;
        string[] fields=["TongTien","NgayLap","TrangThai","KhachHang","DiaChiKhachHang","MaHD"];
        string[] captions=["Tổng tiền (đồng)","Ngày lập","Trạng thái","Khách hàng","Địa chỉ khách hàng","Mã hóa đơn"];
        var panel=new FlowLayoutPanel { Dock=DockStyle.Fill,Padding=new Padding(12),FlowDirection=FlowDirection.TopDown,WrapContents=false };
        Controls.Add(panel);var lines=new List<(ComboBox Field,ComboBox Op,TextBox Value,CheckBox Not)>();
        for(int i=0;i<2;i++)
        {
            var line=new FlowLayoutPanel { Width=850,Height=40 };
            var field=Sv4Ui.Chon(captions);field.Width=180;var op=Sv4Ui.Chon(LocNangCao.ToanTu);op.Width=70;op.SelectedItem=i==0?">=":"<=";
            var value=new TextBox { Width=300,PlaceholderText="Số đồng, văn bản hoặc dd/MM/yyyy" };var not=new CheckBox { Text="NOT",AutoSize=true };
            line.Controls.AddRange([Sv4Ui.Nhan($"({i+1})"),not,field,op,value]);panel.Controls.Add(line);lines.Add((field,op,value,not));
        }
        var join=Sv4Ui.Chon("AND","OR");var useSecond=new CheckBox { Text="Dùng điều kiện (2)",AutoSize=true,Checked=true };
        var actions=new FlowLayoutPanel { Width=850,Height=45 };actions.Controls.AddRange([useSecond,join]);panel.Controls.Add(actions);
        var description=new Label { AutoSize=true,Text="(NOT tùy chọn (điều kiện 1)) AND/OR (NOT tùy chọn (điều kiện 2)). Văn bản: =, !=; ngày: dd/MM/yyyy." };panel.Controls.Add(description);
        actions.Controls.Add(Sv4Ui.Nut("Áp dụng",()=>
        {
            try
            {
                DieuKienLoc Condition(int i)=>new(fields[lines[i].Field.SelectedIndex],lines[i].Op.Text,lines[i].Value.Text,lines[i].Not.Checked);
                KetQua=LocNangCao.Tao(source,fields.ToHashSet(),Condition(0),useSecond.Checked?Condition(1):null,join.Text);
                string Describe(int i)=>$"{(lines[i].Not.Checked?"NOT ":"")}({lines[i].Field.Text} {lines[i].Op.Text} {lines[i].Value.Text})";
                MoTa=Describe(0)+(useSecond.Checked?$" {join.Text} {Describe(1)}":"");DialogResult=DialogResult.OK;
            }
            catch(ArgumentException ex) { MessageBox.Show(this,ex.Message,"Bộ lọc"); }
        }));
        ShopTheme.Apply(this);
    }
}
