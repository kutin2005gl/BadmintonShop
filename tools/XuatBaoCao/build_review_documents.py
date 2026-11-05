"""Build review copies from the three maintained Markdown sources. Use bundled Python."""
import re
import sys
from pathlib import Path
from docx import Document
from docx.shared import Cm, Pt, RGBColor
from docx.oxml import OxmlElement
from docx.oxml.ns import qn
from docx.enum.table import WD_TABLE_ALIGNMENT, WD_CELL_VERTICAL_ALIGNMENT

root = Path(__file__).resolve().parents[2]
destination = Path(sys.argv[1]).resolve()
destination.mkdir(parents=True, exist_ok=True)
sources = [
    ('bao-cao-ky-thuat-7-nhom.md', 'bao-cao-7-nhom'),
    ('huong-dan-ban-giao-dot-4.md', 'huong-dan-ban-giao'),
    ('kich-ban-bao-ve-dot-3.md', 'kich-ban-bao-ve'),
]

def inline(paragraph, text):
    # Preserve evidence labels and source paths without creating external transmissions.
    text = re.sub(r'\[([^\]]+)\]\(([^)]+)\)', r'\1 (\2)', text)
    for token in re.split(r'(\*\*.*?\*\*|`[^`]+`)', text):
        run = paragraph.add_run(token[2:-2] if token.startswith('**') else token[1:-1] if token.startswith('`') else token)
        run.bold = token.startswith('**')
        if token.startswith('`'):
            run.font.name = 'Consolas'
            run.font.size = Pt(9)

for filename, stem in sources:
    source = root / 'docs/bao-cao' / filename
    doc = Document()
    section = doc.sections[0]
    section.page_width, section.page_height = Cm(21), Cm(29.7)
    section.top_margin = section.bottom_margin = Cm(1.8)
    section.left_margin = section.right_margin = Cm(2)
    section.header_distance = section.footer_distance = Cm(.8)
    for name in ['Normal', 'Title', 'Subtitle', 'Heading 1', 'Heading 2', 'Heading 3', 'List Bullet', 'List Number']:
        style = doc.styles[name]
        style.font.name = 'Calibri'
        style.font.color.rgb = RGBColor(0, 0, 0)
        style.paragraph_format.space_after = Pt(4)
    doc.styles['Normal'].font.size = Pt(10.5)
    doc.styles['Normal'].paragraph_format.line_spacing = 1.02
    doc.styles['Title'].font.size = Pt(22)
    doc.styles['Title'].paragraph_format.keep_with_next = True
    doc.styles['Subtitle'].font.size = Pt(10.5)
    doc.styles['Subtitle'].font.italic = False
    for name, size in [('Heading 1', 14), ('Heading 2', 12), ('Heading 3', 11)]:
        doc.styles[name].font.size = Pt(size)
        doc.styles[name].paragraph_format.space_before = Pt(10)
        doc.styles[name].paragraph_format.keep_with_next = True
    section.header.paragraphs[0].text = 'BadmintonShop  |  Bản kỹ thuật chờ nhóm duyệt'
    section.header.paragraphs[0].runs[0].font.size = Pt(9)
    footer = section.footer.paragraphs[0]
    footer.add_run('05/11/2026  |  Trang ')
    field = OxmlElement('w:fldSimple'); field.set(qn('w:instr'), 'PAGE'); footer._p.append(field)
    doc.core_properties.author = 'Nhóm BadmintonShop'
    doc.core_properties.subject = 'Bản kỹ thuật để rà soát, chưa nghiệm thu thay TV1–TV4'
    lines = source.read_text(encoding='utf-8-sig').splitlines()
    i = 0
    while i < len(lines):
        line = lines[i].strip()
        i += 1
        if not line:
            continue
        if line.startswith('|'):
            rows = [line]
            while i < len(lines) and lines[i].strip().startswith('|'):
                rows.append(lines[i].strip()); i += 1
            rows = [r for r in rows if not re.fullmatch(r'[| :\-]+', r)]
            cells = [[c.strip() for c in r.strip('|').split('|')] for r in rows]
            table = doc.add_table(rows=0, cols=len(cells[0]))
            table.alignment = WD_TABLE_ALIGNMENT.CENTER
            table.autofit = False
            widths = [Cm(5), Cm(12)] if len(cells[0]) == 2 else [Cm(17 / len(cells[0]))] * len(cells[0])
            for row_number, values in enumerate(cells):
                row = table.add_row()
                for index, (cell, value) in enumerate(zip(row.cells, values)):
                    cell.width = widths[index]
                    cell.vertical_alignment = WD_CELL_VERTICAL_ALIGNMENT.CENTER
                    inline(cell.paragraphs[0], value)
                    for run in cell.paragraphs[0].runs:
                        run.font.size = Pt(10)
                        if row_number == 0: run.bold = True
                    properties = cell._tc.get_or_add_tcPr()
                    margins = OxmlElement('w:tcMar')
                    for edge in ['top', 'left', 'bottom', 'right']:
                        item = OxmlElement('w:' + edge); item.set(qn('w:w'), '100'); item.set(qn('w:type'), 'dxa'); margins.append(item)
                    properties.append(margins)
                    borders = OxmlElement('w:tcBorders')
                    for edge in ['top', 'left', 'bottom', 'right']:
                        item = OxmlElement('w:' + edge); item.set(qn('w:val'), 'single'); item.set(qn('w:sz'), '4'); item.set(qn('w:color'), 'D9D9D9'); borders.append(item)
                    properties.append(borders)
                    if row_number == 0:
                        shading = OxmlElement('w:shd'); shading.set(qn('w:fill'), 'E7EDF2'); properties.append(shading)
                if row_number == 0:
                    repeat = OxmlElement('w:tblHeader'); row._tr.get_or_add_trPr().append(repeat)
            doc.add_paragraph()
        elif line.startswith('#'):
            hashes, title = line.split(' ', 1)
            # Heading text uses words and spaces; punctuation stays in body/source text.
            title = re.sub(r'[^\w\s]', ' ', title, flags=re.UNICODE)
            title = re.sub(r'\s+', ' ', title).strip()
            doc.add_paragraph(title, 'Title' if len(hashes) == 1 else f'Heading {min(3, len(hashes)-1)}')
            if len(hashes) == 1:
                doc.add_paragraph('Bản rà soát 05/11/2026. Chờ nhóm duyệt nội dung và phạm vi.', 'Subtitle')
        else:
            list_match = re.match(r'^(?:- |\d+\. )(.*)$', line)
            paragraph = doc.add_paragraph(style='List Bullet' if line.startswith('- ') else 'Normal')
            if list_match and not line.startswith('- '):
                paragraph.paragraph_format.left_indent = Cm(.6)
                paragraph.paragraph_format.first_line_indent = Cm(-.6)
            inline(paragraph, list_match.group(1) if line.startswith('- ') else line)
    doc.add_paragraph('Nguồn biên tập: docs/bao-cao/' + filename)
    # The bundled blank template may carry a blue rule in Title/Subtitle styles.
    for tree in (doc.styles.element, doc.element, section.header._element, section.footer._element):
        for border in tree.xpath('.//w:pBdr'):
            border.getparent().remove(border)
    path = destination / (stem + '.docx')
    doc.save(path)
    print(path)
