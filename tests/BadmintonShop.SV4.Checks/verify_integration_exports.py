"""Read integration evidence independently with openpyxl/pypdf; never connects to SQL."""
import json
import sys
from decimal import Decimal
from pathlib import Path
from openpyxl import load_workbook
from pypdf import PdfReader

folder = Path(sys.argv[1])
expected = Decimal((folder / "final-expected-revenue.txt").read_text(encoding="utf-8-sig"))
book = load_workbook(folder / "final-integration-report.xlsx", data_only=True)
assert len(book.sheetnames) == 6
for title, column in [("Theo ngày", 2), ("Theo tháng", 2), ("Theo năm", 2), ("Theo danh mục", 3)]:
    sheet = book[title]
    assert Decimal(str(sheet.cell(3, 2).value)) == expected
    total = sum(Decimal(str(sheet.cell(row, column).value)) for row in range(7, sheet.max_row + 1))
    assert total == expected, (title, total, expected)
report = PdfReader(folder / "final-integration-report.pdf")
assert len(report.pages) == 6
text = " ".join((page.extract_text() or "") for page in report.pages)
assert "Hoàn thành" in text and "phần dư lớn nhất" in text
money = lambda amount: f"{int(amount):,}".replace(",", ".")
assert money(expected) in text or f"{int(expected):,}" in text
invoice = " ".join(" ".join((page.extract_text() or "").split()) for page in PdfReader(folder / "promotion-web-invoice.pdf").pages)
values = json.loads((folder / "historical-invoice-expected.json").read_text(encoding="utf-8-sig"))
for label, key in [("Tiền hàng trước giảm:", "Gross"), ("Giảm sản phẩm:", "ProductDiscount"), ("Giảm hóa đơn / voucher:", "InvoiceDiscount"), ("Tổng thanh toán:", "Net")]:
    assert f"{label} {money(values[key])} đồng" in invoice, (label, invoice)
print(f"PASS independent integration exports: 6 sheets, 6 PDF pages; completed revenue={expected}; historical invoice gross/product/invoice/net discounts match.")

# SV4 final integration: reconcile each persisted filter against its independently recorded SQL result.
manifest = folder / "filtered-expected.tsv"
if manifest.exists():
    rows = manifest.read_text(encoding="utf-8-sig").splitlines()
    assert len(rows) == 12
    for row in rows:
        prefix, total_text = row.split("\t")
        total = Decimal(total_text)
        filtered = load_workbook(folder / f"{prefix}.xlsx", data_only=True)
        for title, column in [("Theo ngày", 2), ("Theo tháng", 2), ("Theo năm", 2), ("Theo danh mục", 3)]:
            sheet = filtered[title]
            assert Decimal(str(sheet.cell(3, 2).value)) == total, prefix
            assert sum(Decimal(str(sheet.cell(r, column).value)) for r in range(7, sheet.max_row + 1)) == total, prefix
        pdf = PdfReader(folder / f"{prefix}.pdf")
        assert len(pdf.pages) == 6
        text = " ".join(page.extract_text() or "" for page in pdf.pages)
        assert money(total) in text or f"{int(total):,}" in text, prefix
    print("PASS independent filtered exports: all 12 SQL/table/XLSX/PDF combinations match.")

native_sql = folder / "native-sql.txt"
if native_sql.exists():
    # Actual window exports are checked separately from the automated fixture's totals.
    sql_lines = native_sql.read_text(encoding="utf-8-sig").splitlines()
    marker = sql_lines.index("CompletedRevenue_20260101_20261026")
    native_total = Decimal(sql_lines[marker + 2])
    native_book = load_workbook(folder / "native-report.xlsx", data_only=True)
    assert len(native_book.sheetnames) == 6
    for title, column in [("Theo ngày", 2), ("Theo tháng", 2), ("Theo năm", 2), ("Theo danh mục", 3)]:
        sheet = native_book[title]
        assert "01/01/2026" in sheet.cell(2, 1).value and "26/10/2026" in sheet.cell(2, 1).value
        assert Decimal(str(sheet.cell(3, 2).value)) == native_total
        assert sum(Decimal(str(sheet.cell(r, column).value)) for r in range(7, sheet.max_row + 1)) == native_total
    native_pdf = PdfReader(folder / "native-report.pdf")
    assert len(native_pdf.pages) == 6
    assert money(native_total) in " ".join(p.extract_text() or "" for p in native_pdf.pages)
    invoice = " ".join(" ".join((p.extract_text() or "").split()) for p in PdfReader(folder / "native-invoice-42.pdf").pages)
    for label, amount in [("Tiền hàng trước giảm:", 1000000), ("Giảm sản phẩm:", 50000), ("Giảm hóa đơn / voucher:", 142500), ("Tổng thanh toán:", 807500), ("Tiền thừa:", 192500)]:
        assert f"{label} {money(amount)} đồng" in invoice, label
    assert "42|Hoàn thành|142500|807500|1000000" in sql_lines
    print(f"PASS native UI exports: SQL/table/Excel/PDF total={native_total}; saved POS42 discounts/cash/change match.")
