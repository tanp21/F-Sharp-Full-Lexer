# F# Identifier Lexer

- Course: Principal of Programming Language (CS320).
- Students' name and ID:
    - Lê Tiến Đạt: 23125028
    - Lê Đức Tùng Dương: 23125081
    - Phạm Đức Duy: 23125032
    - Phạm Nguyễn Anh Tài: 23125016.

We propose a Python implementation of an FsLex-style lexer generator, focusing explicitly on lexing **F# Identifiers**.

**Scope Update:** We have adopted the original F# Lexer and shifted our focus from a "lexer for all" approach to a more specialized implementation that only targets F# identifiers. This allows for more precise alignment with F# identifier specifications.

## Quick Start

For a quick evaluation of the lexer's output on a sample identifier file, simply run:

```bash
uv run fsharp-lexer tests/fixtures/manual_tests/ident.fs
```

This will run our Python lexer implementation and output the recognized identifier tokens directly to the console.

## Codebase Structure

Our codebase is structured to isolate our Python lexer implementation from the official C#-based F# tooling, making it easier to generate tables and test them.

```text
.
├── fsharp/                       # Vendored/upstream F# compiler source used by the runner
├── src/
│   ├── fsharp_full_lexer/        # Python F# lexer package and CLI entry points (Our core identifier lexer)
│   │   └── generated/            # Generated lexer table modules from official F# specs
│   └── fslexpy/                  # FsLex-style scanner/parser and table generator
├── tests/                        # pytest suite for unit testing our lexer's accuracy
│   ├── fixtures/                 # Small F# fixture files for edge cases
│   │   └── manual_tests/         # Larger stress fixtures (like ident.fs) used for parity checks
│   └── output/
│       └── lexer-diff/           # Output directory for both official and Python token outputs, and their diffs
└── tools/
    └── OfficialTokenizer/        # F# runner for the official FSharp.Compiler.Service tokenizer
```

The handwritten compatibility layer currently lives mostly in
`src/fsharp_full_lexer/lexer.py`, while formatting for official token comparison
is in `src/fsharp_full_lexer/official_format.py`.

## Comparison Guide (Testing & Parity)

### How to initialize the code and run the comparison

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

1. **Check your .NET installation:**
   ```bash
   dotnet --info
   ```

2. **Build the official F# compiler bootstrap tools:**
   *(This is required once before using the official tokenizer runner)*
   ```bash
   dotnet publish fsharp/proto.proj /restore /p:Configuration=Proto /p:DotNetBuild=false /p:DotNetBuildSourceOnly=false /p:IgnoreMibc=true /p:NoOptimizationData=true
   ```

3. **Run the official tokenizer on a test file:**
   ```bash
   dotnet run -c Proto --project tools/OfficialTokenizer -- tests/fixtures/manual_tests/ident.fs
   ```

4. **Build the FsLexYacc project:**
   ```bash
   dotnet build FsLexYacc/FsLexYacc.sln
   ```

5. **Build the FSharp Compiler Service:**
   ```bash
   dotnet build fsharp/FSharp.Compiler.Service.slnx
   ```

6. **Run our Python lexer unit tests:**
   ```bash
   uv run pytest
   ```

7. **Run the lexer diff tool for output comparison:**
   ```bash
   uv run fsharp-lexer-diff tests/fixtures/manual_tests/ident.fs
   ```

8. **Regenerate and check official F# lexer tables:**
   ```bash
   uv run fsharp-lexer-generate --check
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

### Examples

We strictly test our code (lexer) *before* comparing the output with the original official compiler's lexer. We run our test suite using `pytest`:

```bash
UV_CACHE_DIR=/tmp/uv-cache uv run pytest
```

When you are ready to evaluate exact token parity against the official F# lexer specs, use the comparison helper tool `fsharp-lexer-diff`. This command regenerates the official tables, evaluates the file using our python lexer, and then uses the local official F# compiler bootstrap tool to check the same file, outputting differences if any exist.

Run the official tokenizer comparison on a manual test file like `ident.fs`:

```bash
uv run fsharp-lexer-diff tests/fixtures/manual_tests/ident.fs
```

This writes the output for our lexer and the official lexer to the following path:
`tests/output/lexer-diff/`

The generated files for `ident.fs` will be:
```text
tests/output/lexer-diff/ident.fs.official.tokens
tests/output/lexer-diff/ident.fs.python.tokens
tests/output/lexer-diff/ident.fs.diff
```

The output format within these `.tokens` files looks like this:

```text
line:start_col:end_col<TAB>TOKEN_NAME<TAB>lexeme<TAB>tag<TAB>full_matched_length
```

For instance, an identifier declaration in `tests/fixtures/manual_tests/ident.fs`:
```fsharp
let simple = 1
```

Might produce output resembling (depending on tokens included):
```text
8:4:10	IDENT	simple	tag	6
```

The command exits with status `1` if our output and the official output differ, letting us perfectly tune identifier edge cases (e.g. `` `backtick identifiers` ``, unicode variables, and weird connectors) against the spec.


### Coverage

`src/fslexpy/` contains a Python clone of the core FsLexYacc pipeline:

- `.fsl` scanner/parser
- FsLex-style AST
- regex AST to NFA
- NFA to DFA
- generated Python transition/action tables
- table interpreter with `LexBuffer`

`src/fsharp_full_lexer/generated/` contains generated table modules for the
official F# `lex.fsl` and `pplex.fsl`.

Since we are focusing strictly on Identifiers, the remaining work is ensuring that our F# token parity correctly captures every possible identifier variation matching the F# language specification without worrying about unrelated language grammar syntax.

## References:

We adapt the code from two main sources:

- [FsLexYacc](https://github.com/fsprojects/FsLexYacc.git): The original F# Lexer. 
- [dotnet/fsharp](https://github.com/dotnet/fsharp.git): The F# compiler, F# core library, and F# editor tools. 

Generator reference:

- `FsLexYacc/src/FsLex.Core/fslexast.fs`
- `FsLexYacc/src/FsLex.Core/fslexdriver.fs`
- `FsLexYacc/src/FsLex.Core/fslexlex.fsl`
- `FsLexYacc/src/FsLex.Core/fslexpars.fsy`
- `FsLexYacc/src/FsLexYacc.Runtime/Lexing.fs`

F# lexer rule source:

- `fsharp/src/Compiler/lex.fsl`
- `fsharp/src/Compiler/pplex.fsl`