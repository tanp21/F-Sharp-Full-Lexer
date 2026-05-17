# F# Full Lexer

Python implementation of an FsLex-style lexer generator plus generated F# lexer
tables from the official F# compiler lexer specs.

Generator reference:

- `FsLexYacc/src/FsLex.Core/fslexast.fs`
- `FsLexYacc/src/FsLex.Core/fslexdriver.fs`
- `FsLexYacc/src/FsLex.Core/fslexlex.fsl`
- `FsLexYacc/src/FsLex.Core/fslexpars.fsy`
- `FsLexYacc/src/FsLexYacc.Runtime/Lexing.fs`

F# lexer rule source:

- `fsharp/src/Compiler/lex.fsl`
- `fsharp/src/Compiler/pplex.fsl`

Run tests:

```bash
UV_CACHE_DIR=/tmp/uv-cache uv run pytest
```

## Nix + direnv

This repo includes a Nix flake and `.envrc` for a reproducible dev shell:

```bash
direnv allow
```

The shell provides:

- Python 3.13
- `uv`
- `direnv`/`nix-direnv`
- .NET SDK 6, 8, and 10 combined in one `dotnet`
- repo-local caches at `.nix-cache/nuget` and `.nix-cache/uv`

Useful checks inside the shell:

```bash
dotnet --info
dotnet publish fsharp/proto.proj /restore /p:Configuration=Proto /p:DotNetBuild=false /p:DotNetBuildSourceOnly=false /p:IgnoreMibc=true /p:NoOptimizationData=true
dotnet run -c Proto --project tools/OfficialTokenizer -- tests/fixtures/core_module.fs
dotnet build FsLexYacc/FsLexYacc.sln
dotnet build fsharp/FSharp.Compiler.Service.slnx
uv run pytest
uv run fsharp-lexer-generate --check
uv run fsharp-lexer-diff tests/fixtures/core_module.fs
```

The .NET 6 SDK is explicitly permitted in `flake.nix` because
`FsLexYacc/global.json` pins the 6.0 SDK line.

Build the local official F# compiler bootstrap tools once before using the
official tokenizer runner:

```bash
dotnet publish fsharp/proto.proj /restore \
  /p:Configuration=Proto \
  /p:DotNetBuild=false \
  /p:DotNetBuildSourceOnly=false \
  /p:IgnoreMibc=true \
  /p:NoOptimizationData=true
```

Run the official FSharp.Compiler.Service tokenizer from `fsharp/` on a file:

```bash
dotnet run -c Proto --project tools/OfficialTokenizer -- tests/fixtures/core_module.fs
```

The output format is:

```text
line:start_col:end_col<TAB>TOKEN_NAME<TAB>lexeme<TAB>tag<TAB>full_matched_length
```

You can pass conditional defines like this:

```bash
dotnet run -c Proto --project tools/OfficialTokenizer -- --define RELEASE tests/fixtures/preprocessor.fs
```

For a side-by-side check against the Python lexer, write both outputs to files:

```bash
dotnet run -c Proto --project tools/OfficialTokenizer -- tests/fixtures/core_module.fs > /tmp/official.tokens
uv run fsharp-lexer --trivia --raw tests/fixtures/core_module.fs > /tmp/python.tokens
```

The official tokenizer API is line-oriented and emits editor-facing trivia
tokens. Use `--trivia --raw` on the Python side when you want the closest
comparison surface.

Or let the helper run both sides and write generated comparison files:

```bash
uv run fsharp-lexer-diff tests/fixtures/core_module.fs
uv run fsharp-lexer-diff --define RELEASE tests/fixtures/preprocessor.fs
```

This writes:

```text
tests/output/lexer-diff/<file>.official.tokens
tests/output/lexer-diff/<file>.python.tokens
tests/output/lexer-diff/<file>.diff
```

The command exits with status `1` if the outputs differ.

Regenerate official F# lexer tables:

```bash
UV_CACHE_DIR=/tmp/uv-cache uv run fsharp-lexer-generate
UV_CACHE_DIR=/tmp/uv-cache uv run fsharp-lexer-generate --check
```

Generate a lexer table module from any `.fsl` file:

```bash
UV_CACHE_DIR=/tmp/uv-cache uv run fslexpy path/to/Lexer.fsl -o generated_lexer.py
```

Tokenize a file:

```bash
UV_CACHE_DIR=/tmp/uv-cache uv run fsharp-lexer path/to/file.fs
```

By default `fsharp-lexer` emits the same tab-separated shape as
`tools/OfficialTokenizer`:

```text
line:start_col:end_col<TAB>TOKEN_NAME<TAB>lexeme<TAB>tag<TAB>full_matched_length
```

Use `--json` for the older debug JSON output.

## Coverage

`src/fslexpy/` contains a Python clone of the core FsLexYacc pipeline:

- `.fsl` scanner/parser
- FsLex-style AST
- regex AST to NFA
- NFA to DFA
- generated Python transition/action tables
- table interpreter with `LexBuffer`

`src/fsharp_full_lexer/generated/` contains generated table modules for the
official F# `lex.fsl` and `pplex.fsl`.

The remaining work for exact F# token parity is semantic action binding: the
generated tables preserve action IDs and original F# action source text, but the
arbitrary F# action blocks still need full Python ports before the generated
tables can replace every hand-written token action.
