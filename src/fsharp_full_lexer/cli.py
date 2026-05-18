from __future__ import annotations

import argparse
import json
from pathlib import Path

from .api import tokenize
from .official_format import format_official_tokens


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("path")
    parser.add_argument("-D", "--define", action="append", default=[])
    parser.add_argument("--raw", action="store_true")
    parser.add_argument("--trivia", action="store_true")
    parser.add_argument("--json", action="store_true", help="Emit the older JSON debug format.")
    args = parser.parse_args()

    source = Path(args.path).read_text(encoding="utf-8-sig")
    tokens = tokenize(
        source,
        defines=args.define,
        skip_trivia=not args.trivia,
        use_lex_filter=not args.raw,
    )

    if args.json:
        print(
            json.dumps(
                [
                    {
                        "kind": tok.kind.name,
                        "value": tok.value,
                        "text": tok.text,
                        "line": tok.range.start.line if tok.range else None,
                        "column": tok.range.start.column if tok.range else None,
                    }
                    for tok in tokens
                ],
                default=str,
                indent=2,
            )
        )
        return

    output = format_official_tokens(tokens, ident_only=True)
    if output:
        print(output)


if __name__ == "__main__":
    main()
