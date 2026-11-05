"""Create two UI fixtures from the actual Windows SaveFileDialog output, stdlib only."""
from pathlib import Path
import zipfile
import xml.etree.ElementTree as E

p = Path(__file__).resolve().parents[2] / 'docs/bao-cao/minh-chung-dot-3'
ns = '{http://schemas.openxmlformats.org/spreadsheetml/2006/main}'
with zipfile.ZipFile(p / 'ui-template.xlsx') as z:
    files = {i.filename: z.read(i.filename) for i in z.infolist()}
sheet = E.fromstring(files['xl/worksheets/sheet1.xml'])
row = E.SubElement(sheet.find(ns + 'sheetData'), ns + 'row', {'r': '2'})
for i, value in enumerate(['', 'Vợt nghiệm thu UI 1601', '1', 'UI', '123000', 'Cây', 'Đang bán']):
    cell = E.SubElement(row, ns + 'c', {'r': chr(65+i)+'2', 't': 'inlineStr'})
    E.SubElement(E.SubElement(cell, ns + 'is'), ns + 't').text = value
for name in ['valid', 'invalid']:
    if name == 'invalid':
        list(row)[4].find(ns + 'is/' + ns + 't').text = 'sai số'
    files['xl/worksheets/sheet1.xml'] = E.tostring(sheet)
    with zipfile.ZipFile(p / f'ui-{name}.xlsx', 'w', zipfile.ZIP_DEFLATED) as z:
        for key, value in files.items():
            z.writestr(key, value)
