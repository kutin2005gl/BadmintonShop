"""Independent, read-only XLSX/PDF cell reconciliation, including split year pivots."""
import json
import sys
from pathlib import Path
from decimal import Decimal

import pdfplumber
from openpyxl import load_workbook

folder = Path(sys.argv[1])
normalize = lambda value: "".join(str(value or "").split())

for prefix in (sys.argv[2:] or ("demo-report", "wide-pivots", "ui-report")):
    expected = json.loads((folder / f"{prefix}-expected.json").read_text(encoding="utf-8-sig"))
    book = load_workbook(folder / f"{prefix}.xlsx", data_only=True)
    with pdfplumber.open(folder / f"{prefix}.pdf") as pdf:
        coverage = {table["Name"]: set() for table in expected}
        for table in expected:
            sheet = book[table["Name"]]
            for row_index, row in enumerate(table["Rows"], 7):
                for col_index, value in enumerate(row, 1):
                    actual = sheet.cell(row_index, col_index).value
                    assert actual == value or (actual in (None, "") and value in (None, "")), (prefix, table["Name"], row_index, col_index, actual, value)
        for page in pdf.pages:
            title = normalize(page.extract_text().splitlines()[0])
            table = next(table for table in expected if title.startswith(normalize("BADMINTONSHOP—" + table["Name"])))
            extracted = page.extract_tables()
            assert len(extracted) == 1, (prefix, title, extracted)
            headers, *rows = extracted[0]
            assert len(rows) == len(table["Rows"]), (prefix, title, len(rows))
            for col, header in enumerate(headers):
                field = "Tổng" if normalize(header) == normalize("Tổng toàn kỳ") else header
                index = next(i for i, candidate in enumerate(table["Columns"]) if normalize(candidate) == normalize(field))
                coverage[table["Name"]].add(index)
                for row, source in zip(rows, table["Rows"]):
                    value = source[index]
                    if isinstance(value, (int, float)):
                        # PDF uses Vietnamese whole-dong formatting for both IDs and money.
                        assert Decimal(row[col].replace(".", "").replace(",", ".")) == Decimal(str(value)), (prefix, title, header, row[col], value)
                    else:
                        assert normalize(row[col]) == normalize(value), (prefix, title, header, row[col], value)
            assert all(20 <= char["x0"] <= char["x1"] <= page.width - 20 for char in page.chars), (prefix, title, "horizontal overflow")
        for table in expected:
            assert coverage[table["Name"]] == set(range(len(table["Columns"]))), (prefix, table["Name"], "missing columns")
        print(f"PASS {prefix}: {len(expected)} XLSX tables and {len(pdf.pages)} PDF pages; every displayed cell matches; all columns covered; horizontal bounds valid")
