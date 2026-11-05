"""Validate/package the visually reviewed copies; use bundled Python."""
import hashlib
import json
import shutil
import zipfile
from pathlib import Path
import pdfplumber
from docx import Document

root = Path(__file__).resolve().parents[2]
folder = root / 'docs/bao-cao/ban-ra-soat-dot-5'
evidence = root / 'docs/bao-cao/minh-chung-dot-5'
items = [('bao-cao-7-nhom', 'bao-cao-ky-thuat-7-nhom.md', 2),
         ('huong-dan-ban-giao', 'huong-dan-ban-giao-dot-4.md', 1),
         ('kich-ban-bao-ve', 'kich-ban-bao-ve-dot-3.md', 2)]
sha = lambda path: hashlib.sha256(path.read_bytes()).hexdigest()
records = []
for stem, source, count in items:
    docx = folder / (stem + '.docx')
    pdf = folder / (stem + '.pdf')
    shutil.copyfile(folder / (stem + '-pages') / (stem + '.pdf'), pdf)
    document = Document(docx)
    assert not document.styles.element.xpath('.//w:pBdr'), (stem, 'style border')
    assert not document.element.xpath('.//w:pBdr'), (stem, 'paragraph border')
    with pdfplumber.open(pdf) as rendered:
        assert len(rendered.pages) == count, (stem, 'page count')
        for index, page in enumerate(rendered.pages, 1):
            text = page.extract_text()
            assert text and f'Trang {index}' in text, (stem, 'footer/text')
            assert (folder / (stem + '-pages') / f'page-{index}.png').is_file()
            assert all(24 <= c['x0'] <= c['x1'] <= page.width - 24 for c in page.chars), (stem, 'horizontal overflow')
    records.append(dict(output=stem, source=source, source_sha256=sha(root/'docs/bao-cao'/source),
                        docx_sha256=sha(docx), pdf_sha256=sha(pdf), pages=count,
                        renderer='Installed Word object model + bundled render_docx.py rasterize + Poppler'))
(folder/'source-hashes.json').write_text(json.dumps(records, ensure_ascii=False, indent=2), encoding='utf-8')
(evidence/'document-page-qa.txt').write_text(
    'Final review by Codex on 05/11/2026: all 5 final page PNGs inspected individually.\n'
    'Report 1-2; handover 1; defense 1-2: Vietnamese text, headings, numbering, question table, page footers readable; no clipped/overlapping content.\n'
    'Earlier blue title border and nearly empty trailing pages corrected before final render.\n'
    'Programmatic checks: page counts, PNG coverage, footer numbering, horizontal text bounds and absence of paragraph/style borders passed.\n'
    'These are technical review copies, not member acceptance or final scope approval.\n', encoding='utf-8')
archive = folder.with_suffix('.zip')
with zipfile.ZipFile(archive, 'w', compression=zipfile.ZIP_DEFLATED) as bundle:
    for path in sorted(folder.rglob('*')):
        if path.is_file() and not (path.suffix == '.pdf' and path.parent.name.endswith('-pages')):
            bundle.write(path, path.relative_to(folder))
    for _, source, _ in items:
        bundle.write(root/'docs/bao-cao'/source, 'nguon/'+source)
    for name in ['ui-snapshot-300.xlsx', 'ui-snapshot-300.pdf', 'export-results.txt', 'document-page-qa.txt']:
        bundle.write(evidence/name, 'minh-chung/'+name)
print(f'PASS package: 3 DOCX + 3 PDF + 5 page PNGs + source hashes; {archive}')
