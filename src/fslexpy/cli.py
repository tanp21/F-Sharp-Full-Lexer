from __future__ import annotations

import argparse
from pathlib import Path

from .generator import generate


def main() -> None:
    parser = argparse.ArgumentParser(prog="fslexpy")
    parser.add_argument("input")
    parser.add_argument("-o", "--output", required=True)
    parser.add_argument("--module", default="generated_lexer")
    parser.add_argument("--ascii", action="store_true")
    parser.add_argument("-i", "--case-insensitive", action="store_true")
    args = parser.parse_args()

    generate(
        Path(args.input),
        Path(args.output),
        unicode=not args.ascii,
        case_insensitive=args.case_insensitive,
        module_name=args.module,
    )


if __name__ == "__main__":
    main()
