"""SV4: independent readers for generated artifacts; pip install openpyxl pypdf."""
import argparse
from decimal import Decimal
from pathlib import Path

from openpyxl import load_workbook
from pypdf import PdfReader

parser = argparse.ArgumentParser()
parser.add_argument("directory", type=Path)
parser.add_argument("--expected-revenue", type=Decimal, required=True)
args = parser.parse_args()
wb = load_workbook(args.directory / "database-report.xlsx", data_only=True)
assert len(wb.sheetnames) == 6
for title, amount_column in [("Theo ngày", 2), ("Theo tháng", 2), ("Theo năm", 2), ("Theo danh mục", 3)]:
    sheet = wb[title]
    assert Decimal(str(sheet.cell(3, 2).value)) == args.expected_revenue
    values = [sheet.cell(row, amount_column).value for row in range(7, sheet.max_row + 1)]
    assert all(isinstance(value, (int, float)) for value in values)
    assert sum(Decimal(str(value)) for value in values) == args.expected_revenue
pdf = PdfReader(args.directory / "database-report.pdf")
assert len(pdf.pages) == 6
text = "\n".join(page.extract_text() or "" for page in pdf.pages)
assert "Hoàn thành" in text and "Doanh thu" in text and "phần dư lớn nhất" in text
assert f"{int(args.expected_revenue):,}" in text or f"{int(args.expected_revenue):,}".replace(",", ".") in text
assert "Tiền thừa: 20 đồng" in "\n".join(page.extract_text() or "" for page in PdfReader(args.directory / "database-invoice.pdf").pages)
discount_text = "\n".join(page.extract_text() or "" for page in PdfReader(args.directory / "discount-invoice.pdf").pages)
assert "7,5" in discount_text and "205 đồng" in discount_text
assert len(PdfReader(args.directory / "invoice.pdf").pages) > 1
print(f"PASS independent readers: 6 sheets, 6 report pages, Unicode, fractional discount, revenue={args.expected_revenue}, persisted cash/change")
