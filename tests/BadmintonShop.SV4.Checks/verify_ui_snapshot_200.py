"""Read-only reconciliation for the UI run on ...021438_b10b8080.

Oracle is hand-derived from ReportProgressFixture plus one production-login UI order of 100.
It does not call BaoCaoService or obtain expected values from either export.
Period: 2026-01-01 through 2026-11-06 inclusive. The December purchase is outside it.
"""
import json
import subprocess
import sys
from pathlib import Path
from openpyxl import load_workbook

folder = Path(sys.argv[1])
tables = []

def table(name, columns, rows):
    tables.append(dict(Name=name, Columns=columns, Rows=rows))

for name, rows in (
    ('Theo ngày', [['2026-01-01', 70], ['2026-07-01', 30], ['2026-11-06', 100]]),
    ('Theo tháng', [['2026-01', 70], ['2026-07', 30], ['2026-11', 100]]),
    ('Theo năm', [['2026', 200]]),
    ('Theo quý', [['2026-Q1', 70], ['2026-Q3', 30], ['2026-Q4', 100]]),
):
    table(name, ['Kỳ', 'Doanh thu'], rows)
table('Bán chạy', ['Mã SP', 'Sản phẩm', 'Số lượng'], [[1, 'Vợt thử', 2], [2, 'Cầu thử', 1]])
table('Theo danh mục', ['Mã DM', 'Danh mục', 'Doanh thu'], [[1, 'SV4 TEST Vợt', 170], [2, 'SV4 TEST Cầu', 30]])
table('Trạng thái đơn', ['Trạng thái', 'Số đơn'], [
    ['Chờ xác nhận', 1], ['Đã xác nhận', 0], ['Đang giao', 0], ['Hoàn thành', 3], ['Đã hủy', 1]])
table('Nhân viên × năm', ['Mã', 'Tên', '2026', 'Tổng'], [
    [None, 'Chưa gán nhân viên', 130, 130], [1, 'SV4 TEST', 70, 70], [None, 'Tất cả', 200, 200]])
table('Khách hàng × năm', ['Mã', 'Tên', '2026', 'Tổng'], [
    [None, 'Khách vãng lai', 30, 30], [1, 'Khách 1', 70, 70], [3, 'Khách nghiệm thu', 100, 100], [None, 'Tất cả', 200, 200]])
table('Sản phẩm × quý 2026', ['Mã SP', 'Sản phẩm', 'Q1', 'Q2', 'Q3', 'Q4', 'Tổng'], [
    [1, 'Vợt thử', 70, 0, 0, 100, 170], [2, 'Cầu thử', 0, 0, 30, 0, 30],
    [3, 'Sản phẩm 3', 0, 0, 0, 0, 0], [None, 'Tất cả', 70, 0, 30, 100, 200]])
table('Thu–chi 2026', ['Mã', 'Chỉ tiêu', 'Q1', 'Q2', 'Q3', 'Q4', 'Tổng'], [
    [1, 'Thu bán hàng', 70, 0, 30, 100, 200], [2, 'Chi nhập hàng', 0, 0, 0, 0, 0],
    [3, 'Chênh lệch thu–chi', 70, 0, 30, 100, 200]])

book = load_workbook(folder / 'ui-snapshot-200.xlsx', data_only=True)
assert book.sheetnames == [t['Name'] for t in tables], 'Missing/extra/reordered sheets'
for expected in tables:
    sheet = book[expected['Name']]
    assert [cell.value for cell in sheet[6]] == expected['Columns'], sheet.title
    assert sheet.max_row == 6 + len(expected['Rows']), (sheet.title, 'extra/missing rows')
    assert sheet.cell(3, 2).value == 200, (sheet.title, 'snapshot total')
(folder / 'ui-snapshot-200-expected.json').write_text(json.dumps(tables, ensure_ascii=False, indent=2), encoding='utf-8')
subprocess.run([sys.executable, str(Path(__file__).with_name('verify_progress_exports.py')), str(folder), 'ui-snapshot-200'], check=True)
print('PASS frozen independent oracle: original 2026 revenue 70+30 plus UI order 100 = 200; 11 exact schemas/row counts; December purchases excluded')
