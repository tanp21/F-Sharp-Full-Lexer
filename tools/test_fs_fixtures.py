#!/usr/bin/env python3
# ruff: noqa: E402, I001
from __future__ import annotations

import argparse
import difflib
import sys
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parents[1]
SRC_ROOT = REPO_ROOT / "src"
if str(SRC_ROOT) not in sys.path:
    sys.path.insert(0, str(SRC_ROOT))

from fsharp_full_lexer.compare import (  # noqa: E402
    compare_file,
    ensure_generated_tables,
    python_tokens,
)


DEFAULT_FIXTURE_DIRS = (
    REPO_ROOT / "tests" / "fixtures",
    REPO_ROOT / "tests" / "fixtures" / "manual_tests",
)
DEFAULT_OUTPUT_DIR = REPO_ROOT / "tests" / "output" / "lexer-diff"


def display_path(path: Path) -> str:
    try:
        return str(path.relative_to(REPO_ROOT))
    except ValueError:
        return str(path)


def discover_fs_files(dirs: list[Path]) -> list[Path]:
    seen: set[Path] = set()
    files: list[Path] = []
    for directory in dirs:
        for path in sorted(directory.glob("*.fs")):
            resolved = path.resolve()
            if resolved not in seen:
                seen.add(resolved)
                files.append(path)
    return files


def compare_with_cached_official(path: Path, defines: list[str], output_dir: Path) -> bool:
    output_dir.mkdir(parents=True, exist_ok=True)
    official_path = output_dir / f"{path.name}.official.tokens"
    python_path = output_dir / f"{path.name}.python.tokens"
    diff_path = output_dir / f"{path.name}.diff"

    if not official_path.exists():
        print(
            f"{display_path(path)}: missing cached official tokens at "
            f"{display_path(official_path)}"
        )
        return False

    official = official_path.read_text(encoding="utf-8").rstrip("\n")
    ours = python_tokens(path, defines).rstrip("\n")
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
        print(f"{display_path(path)}: differs, see {display_path(diff_path)}")
        return False

    if diff_path.exists():
        diff_path.unlink()
    print(f"{display_path(path)}: ok")
    return True


def main() -> int:
    parser = argparse.ArgumentParser(
        description="Compare every .fs fixture in tests/fixtures and manual_tests."
    )
    parser.add_argument(
        "dirs",
        nargs="*",
        type=Path,
        default=list(DEFAULT_FIXTURE_DIRS),
        help=(
            "Fixture directories to scan non-recursively. Defaults to tests/fixtures "
            "and tests/fixtures/manual_tests."
        ),
    )
    parser.add_argument("-D", "--define", action="append", default=[])
    parser.add_argument(
        "-o",
        "--output-dir",
        type=Path,
        default=DEFAULT_OUTPUT_DIR,
        help="Directory for official/python token files and diffs.",
    )
    parser.add_argument(
        "--cached-official",
        action="store_true",
        help=(
            "Compare against existing *.official.tokens files instead of running "
            "dotnet OfficialTokenizer."
        ),
    )
    args = parser.parse_args()

    dirs = [path if path.is_absolute() else REPO_ROOT / path for path in args.dirs]
    output_dir = args.output_dir if args.output_dir.is_absolute() else REPO_ROOT / args.output_dir
    files = discover_fs_files(dirs)
    if not files:
        print("No .fs fixture files found.", file=sys.stderr)
        return 1

    ensure_generated_tables()

    print(f"Testing {len(files)} .fs fixture files:")
    for path in files:
        print(f"  {display_path(path)}")

    ok = True
    for path in files:
        if args.cached_official:
            result = compare_with_cached_official(path, args.define, output_dir)
        else:
            result = compare_file(path, args.define, output_dir)
        ok = result and ok

    return 0 if ok else 1


if __name__ == "__main__":
    raise SystemExit(main())
