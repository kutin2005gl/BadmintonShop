"""Use installed Word for PDF conversion, then the bundled render_docx.py rasterizer.

Explicit Windows fallback when LibreOffice is unavailable. Never modify managed runtime files.
"""
import importlib.util
import os
import subprocess
import sys
from pathlib import Path

canonical, poppler, folder = map(Path, sys.argv[1:4])
spec = importlib.util.spec_from_file_location('bundled_docx_renderer', canonical)
renderer = importlib.util.module_from_spec(spec)
spec.loader.exec_module(renderer)
os.environ['PATH'] = str(poppler) + os.pathsep + os.environ.get('PATH', '')

def word_convert(doc_path, user_profile, convert_tmp_dir, stem, verbose):
    pdf = Path(convert_tmp_dir) / (stem + '.pdf')
    command = ['powershell.exe', '-NoProfile', '-NonInteractive', '-File', str(Path(__file__).with_name('render_with_word.ps1')),
               '-DocumentPath', str(Path(doc_path).resolve()), '-PdfPath', str(pdf)]
    result = subprocess.run(command, text=True, capture_output=True, timeout=120)
    details = result.stdout + result.stderr
    if result.returncode or not pdf.is_file():
        return '', details
    return str(pdf), details

renderer.convert_to_pdf = word_convert
for docx in sorted(folder.glob('*.docx')):
    out = folder / (docx.stem + '-pages')
    # Remove only this renderer's old page images so a shorter revision has no stale last page.
    for previous in out.glob('page-*.png'):
        previous.unlink()
    paths = renderer.rasterize(str(docx), str(out), 130, True, True)
    print(f'PASS Word -> bundled render_docx.py -> Poppler: {docx.name}: {len(paths)} pages', flush=True)
