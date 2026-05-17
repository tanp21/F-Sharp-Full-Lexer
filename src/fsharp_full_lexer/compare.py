from __future__ import annotations

import argparse
import difflib
import subprocess
import sys
from pathlib import Path

from .api import tokenize
from .official_format import format_official_tokens


def official_command(path: Path, defines: list[str]) -> list[str]:
    command = ["dotnet", "run", "-c", "Proto", "--project", "tools/OfficialTokenizer", "--"]
    for define in defines:
        command.extend(["--define", define])
    command.append(str(path))
    return command


def python_tokens(path: Path, defines: list[str]) -> str:
    source = path.read_text(encoding="utf-8-sig")
    tokens = tokenize(source, defines=defines, skip_trivia=False, use_lex_filter=False)
    return format_official_tokens(tokens)


def compare_file(path: Path, defines: list[str], output_dir: Path) -> bool:
    output_dir.mkdir(parents=True, exist_ok=True)

    official = subprocess.run(
        official_command(path, defines),
        check=True,
        text=True,
        stdout=subprocess.PIPE,
    ).stdout.rstrip("\n")
    ours = python_tokens(path, defines).rstrip("\n")

    official_path = output_dir / f"{path.name}.official.tokens"
    python_path = output_dir / f"{path.name}.python.tokens"
    diff_path = output_dir / f"{path.name}.diff"

    official_path.write_text(official + "\n", encoding="utf-8")
    python_path.write_text(ours + "\n", encoding="utf-8")

    diff = "\n".join(
        difflib.unified_diff(
            official.splitlines(),
            ours.splitlines(),
            fromfile=str(official_path),
            tofile=str(python_path),
            lineterm="",
        )
    )

    if diff:
        diff_path.write_text(diff + "\n", encoding="utf-8")
        print(f"{path}: differs, see {diff_path}")
        return False

    if diff_path.exists():
        diff_path.unlink()
    print(f"{path}: ok")
    return True


def main() -> None:
    parser = argparse.ArgumentParser(prog="fsharp-lexer-diff")
    parser.add_argument("paths", nargs="+", type=Path)
    parser.add_argument("-D", "--define", action="append", default=[])
    parser.add_argument("-o", "--output-dir", type=Path, default=Path("tests/output/lexer-diff"))
    args = parser.parse_args()

    ok = True
    for path in args.paths:
        ok = compare_file(path, args.define, args.output_dir) and ok

    if not ok:
        sys.exit(1)


if __name__ == "__main__":
    main()
