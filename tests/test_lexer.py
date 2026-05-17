from pathlib import Path

import pytest

from fsharp_full_lexer import TokenKind, tokenize
from fsharp_full_lexer.official_format import format_official_tokens

FIXTURE_DIR = Path(__file__).parent / "fixtures"
TOP_LEVEL_FSHARP_FIXTURES = sorted(
    path.name
    for path in FIXTURE_DIR.iterdir()
    if path.is_file() and path.suffix in {".fs", ".fsi", ".fsx"}
)


def kinds(source: str, **kwargs):
    return [tok.kind for tok in tokenize(source, **kwargs)]


def raw(source: str, **kwargs):
    return tokenize(source, use_lex_filter=False, **kwargs)


def test_keywords_identifiers_and_basic_punctuation():
    tokens = raw("module M\nlet x = 1")
    assert [t.kind for t in tokens] == [
        TokenKind.MODULE,
        TokenKind.IDENT,
        TokenKind.LET,
        TokenKind.IDENT,
        TokenKind.EQUALS,
        TokenKind.INT32,
        TokenKind.EOF,
    ]
    assert tokens[1].value == "M"
    assert tokens[5].value == (1, False)


def test_numeric_suffixes():
    tokens = raw("1y 255uy 32768s 1L 1.5 1.0f 12M 10I")
    assert [t.kind for t in tokens[:-1]] == [
        TokenKind.INT8,
        TokenKind.UINT8,
        TokenKind.INT16,
        TokenKind.INT64,
        TokenKind.IEEE64,
        TokenKind.IEEE32,
        TokenKind.DECIMAL,
        TokenKind.BIGNUM,
    ]


def test_string_forms():
    tokens = raw('"a\\n" @"a""b" """abc"""')
    assert [t.kind for t in tokens[:-1]] == [TokenKind.STRING, TokenKind.STRING, TokenKind.STRING]
    assert tokens[0].value == ("a\n", "Regular")
    assert tokens[1].value == ('a"b', "Verbatim")
    assert tokens[2].value == ("abc", "TripleQuote")


def test_comments_are_trivia_by_default():
    tokens = raw("let x = 1 // comment\n(* block *)\nx")
    assert [t.kind for t in tokens] == [
        TokenKind.LET,
        TokenKind.IDENT,
        TokenKind.EQUALS,
        TokenKind.INT32,
        TokenKind.IDENT,
        TokenKind.EOF,
    ]


def test_preprocessor_skips_inactive_code():
    tokens = raw("#if DEBUG\nlet x = 1\n#else\nlet x = 2\n#endif", defines=set())
    assert [(t.kind, t.value) for t in tokens] == [
        (TokenKind.LET, False),
        (TokenKind.IDENT, "x"),
        (TokenKind.EQUALS, None),
        (TokenKind.INT32, (2, False)),
        (TokenKind.EOF, None),
    ]


def test_raw_trivia_mode_keeps_comments_and_whitespace():
    tokens = tokenize("x // y", skip_trivia=False, use_lex_filter=False)
    assert [t.kind for t in tokens] == [
        TokenKind.IDENT,
        TokenKind.WHITESPACE,
        TokenKind.LINE_COMMENT,
        TokenKind.LINE_COMMENT,
        TokenKind.LINE_COMMENT,
        TokenKind.EOF,
    ]
    assert [t.text for t in tokens[2:-1]] == ["//", " ", "y"]


def test_official_text_format_matches_fsharp_tokenizer_shape():
    tokens = tokenize("let x = 1", skip_trivia=False, use_lex_filter=False)

    assert format_official_tokens(tokens) == "\n".join(
        [
            "1:1:3\tLET\tlet\t166\t3",
            "1:4:4\tWHITESPACE\t \t6\t1",
            "1:5:5\tIDENT\tx\t197\t1",
            "1:6:6\tWHITESPACE\t \t6\t1",
            "1:7:7\tEQUALS\t=\t71\t1",
            "1:8:8\tWHITESPACE\t \t6\t1",
            "1:9:9\tINT32\t1\t182\t1",
        ]
    )


def test_official_text_format_skips_newline_whitespace():
    tokens = tokenize("let x = 1\n\nlet y = 2", skip_trivia=False, use_lex_filter=False)
    lines = format_official_tokens(tokens).splitlines()

    assert "1:10:0\tWHITESPACE\t\\n\t6\t1" not in lines
    assert lines[0] == "1:1:3\tLET\tlet\t166\t3"
    assert lines[7] == "3:1:3\tLET\tlet\t166\t3"


def test_lexfilter_inserts_layout_tokens():
    tokens = tokenize("let x =\n  1\nlet y = 2")
    assert TokenKind.OLET in [t.kind for t in tokens]
    assert TokenKind.OBLOCKBEGIN in [t.kind for t in tokens]
    assert TokenKind.OBLOCKEND in [t.kind for t in tokens]


@pytest.mark.parametrize(
    ("fixture", "expected_raw_kinds"),
    [
        (
            "core_module.fs",
            {
                TokenKind.NAMESPACE,
                TokenKind.MODULE,
                TokenKind.TYPE,
                TokenKind.MEMBER,
                TokenKind.MUTABLE,
                TokenKind.LARROW,
            },
        ),
        (
            "domain_model.fs",
            {
                TokenKind.MODULE,
                TokenKind.TYPE,
                TokenKind.PRIVATE,
                TokenKind.MATCH,
                TokenKind.WITH,
                TokenKind.BAR,
                TokenKind.INTERP_STRING_BEGIN_END,
            },
        ),
        (
            "strings_comments.fsx",
            {
                TokenKind.MODULE,
                TokenKind.LET,
                TokenKind.STRING,
                TokenKind.INTERP_STRING_BEGIN_END,
            },
        ),
        (
            "preprocessor.fs",
            {
                TokenKind.MODULE,
                TokenKind.LET,
                TokenKind.STRING,
                TokenKind.TRUE,
            },
        ),
        (
            "script_workflow.fsx",
            {
                TokenKind.OPEN,
                TokenKind.LET,
                TokenKind.LBRACK_BAR,
                TokenKind.BAR_RBRACK,
                TokenKind.FUN,
            },
        ),
    ],
)
def test_complete_fsharp_fixtures_raw_tokenize(fixture, expected_raw_kinds):
    source = (FIXTURE_DIR / fixture).read_text()
    tokens = tokenize(source, use_lex_filter=False, defines={"RELEASE"})
    token_kinds = {tok.kind for tok in tokens}

    assert tokens[-1].kind == TokenKind.EOF
    assert TokenKind.LEX_FAILURE not in token_kinds
    assert expected_raw_kinds <= token_kinds


@pytest.mark.parametrize("fixture", TOP_LEVEL_FSHARP_FIXTURES)
def test_complete_fsharp_fixtures_filtered_tokenize(fixture):
    source = (FIXTURE_DIR / fixture).read_text()
    tokens = tokenize(source, defines={"RELEASE"})
    token_kinds = {tok.kind for tok in tokens}

    assert tokens[-1].kind == TokenKind.EOF
    assert TokenKind.LEX_FAILURE not in token_kinds
    assert TokenKind.OBLOCKBEGIN in token_kinds
    assert TokenKind.OBLOCKEND in token_kinds


def test_preprocessor_fixture_selects_release_branch():
    source = (FIXTURE_DIR / "preprocessor.fs").read_text()
    tokens = tokenize(source, use_lex_filter=False, defines={"RELEASE"})
    strings = [tok.value[0] for tok in tokens if tok.kind == TokenKind.STRING]

    assert strings == ["release"]
