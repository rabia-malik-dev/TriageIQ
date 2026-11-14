#!/usr/bin/env python3
"""
Build private study guide PDFs from Markdown in study/src/ into study-guides/.

Both study/ and study-guides/ are gitignored (M9). Copy templates from study-templates/src first.
"""
from __future__ import annotations

import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
SRC = ROOT / "study" / "src"
OUT = ROOT / "study-guides"
TEMPLATE = ROOT / "study-templates" / "src"


def main() -> int:
    try:
        from reportlab.lib.pagesizes import letter
        from reportlab.pdfgen import canvas
    except ImportError:
        print("Install reportlab: pip install reportlab", file=sys.stderr)
        return 2

    if not SRC.exists():
        if TEMPLATE.exists():
            print(f"study/src not found. Copy {TEMPLATE} -> {SRC} (private).", file=sys.stderr)
        else:
            print("study/src not found. Create Markdown sources per docs/SPEC.md section 12.", file=sys.stderr)
        return 2

    OUT.mkdir(parents=True, exist_ok=True)
    for md_path in sorted(SRC.glob("*.md")):
        pdf_path = OUT / f"{md_path.stem}.pdf"
        text = md_path.read_text(encoding="utf-8")
        c = canvas.Canvas(str(pdf_path), pagesize=letter)
        c.setTitle(md_path.stem)
        y = 750
        c.setFont("Helvetica", 11)
        for line in text.splitlines():
            if y < 60:
                c.showPage()
                c.setFont("Helvetica", 11)
                y = 750
            c.drawString(50, y, line[:110])
            y -= 14
        c.save()
        print(f"Wrote {pdf_path}")

    return 0


if __name__ == "__main__":
    raise SystemExit(main())