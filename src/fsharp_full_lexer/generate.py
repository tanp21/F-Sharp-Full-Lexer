from __future__ import annotations

import argparse
from pathlib import Path

from fslexpy.generator import generate_text

ROOT = Path(__file__).resolve().parents[2]
OFFICIAL = ROOT / "fsharp" / "src" / "Compiler"
GENERATED = ROOT / "src" / "fsharp_full_lexer" / "generated"


TARGETS = [
    (OFFICIAL / "lex.fsl", GENERATED / "lexer_tables.py", "fsharp_lexer_tables"),
    (OFFICIAL / "pplex.fsl", GENERATED / "pplexer_tables.py", "fsharp_pplexer_tables"),
]


def render_target(input_path: Path, module_name: str) -> str:
    return generate_text(
        input_path.read_text(encoding="utf-8-sig"),
        filename=str(input_path),
        unicode=True,
        module_name=module_name,
    )


def generate_all(*, check: bool = False) -> int:
    GENERATED.mkdir(parents=True, exist_ok=True)
    failures = 0
    for input_path, output_path, module_name in TARGETS:
        rendered = render_target(input_path, module_name)
        if check:
            current = output_path.read_text(encoding="utf-8") if output_path.exists() else ""
            if current != rendered:
                print(f"{output_path} is not up to date")
                failures += 1
        else:
            output_path.write_text(rendered, encoding="utf-8")
            print(f"generated {output_path}")
    return failures


def main() -> None:
    parser = argparse.ArgumentParser(prog="fsharp-lexer-generate")
    parser.add_argument("--check", action="store_true")
    args = parser.parse_args()
    raise SystemExit(generate_all(check=args.check))


if __name__ == "__main__":
    main()
