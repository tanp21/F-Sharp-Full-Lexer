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


def test_string_text_uses_official_identifier_and_integer_chunks():
    tokens = tokenize(
        '"0x1.8p1 1_2e3_4 0x_FF_FF',
        skip_trivia=False,
        use_lex_filter=False,
    )
    assert [token.text for token in tokens[:-1]] == [
        '"',
        "0x1",
        ".",
        "8",
        "p1",
        " ",
        "1_2",
        "e3_4",
        " ",
        "0",
        "x_FF_FF",
    ]


def test_interpolated_strings_keep_escaped_braces_as_text():
    tokens = tokenize('$"{{literal}} and {42}"', skip_trivia=False, use_lex_filter=False)
    assert [(token.kind, token.text) for token in tokens[:-1]] == [
        (TokenKind.STRING_TEXT, '$"'),
        (TokenKind.STRING_TEXT, "{{"),
        (TokenKind.STRING_TEXT, "literal"),
        (TokenKind.STRING_TEXT, "}}"),
        (TokenKind.STRING_TEXT, " "),
        (TokenKind.STRING_TEXT, "and"),
        (TokenKind.STRING_TEXT, " "),
        (TokenKind.INTERP_STRING_BEGIN_PART, "{"),
        (TokenKind.INT32, "42"),
        (TokenKind.STRING_TEXT, "}"),
        (TokenKind.INTERP_STRING_END, '"'),
    ]


def test_interpolation_expression_keeps_pattern_match_tokens():
    tokens = tokenize(
        '$"{match x with | 1 -> "one" | _ -> "other"}"',
        skip_trivia=False,
        use_lex_filter=False,
    )
    assert [token.kind for token in tokens[:-1]] == [
        TokenKind.STRING_TEXT,
        TokenKind.INTERP_STRING_BEGIN_PART,
        TokenKind.MATCH,
        TokenKind.WHITESPACE,
        TokenKind.IDENT,
        TokenKind.WHITESPACE,
        TokenKind.WITH,
        TokenKind.WHITESPACE,
        TokenKind.BAR,
        TokenKind.WHITESPACE,
        TokenKind.INT32,
        TokenKind.WHITESPACE,
        TokenKind.RARROW,
        TokenKind.WHITESPACE,
        TokenKind.STRING_TEXT,
        TokenKind.STRING_TEXT,
        TokenKind.STRING,
        TokenKind.WHITESPACE,
        TokenKind.BAR,
        TokenKind.WHITESPACE,
        TokenKind.UNDERSCORE,
        TokenKind.WHITESPACE,
        TokenKind.RARROW,
        TokenKind.WHITESPACE,
        TokenKind.STRING_TEXT,
        TokenKind.STRING_TEXT,
        TokenKind.STRING,
        TokenKind.STRING_TEXT,
        TokenKind.INTERP_STRING_END,
    ]


def test_unit_static_member_operator_is_not_block_comment():
    tokens = tokenize(
        "static member (*): 'T * float -> 'U",
        skip_trivia=False,
        use_lex_filter=False,
    )
    assert [token.kind for token in tokens[:-1]] == [
        TokenKind.STATIC,
        TokenKind.WHITESPACE,
        TokenKind.MEMBER,
        TokenKind.WHITESPACE,
        TokenKind.LPAREN_STAR_RPAREN,
        TokenKind.COLON,
        TokenKind.WHITESPACE,
        TokenKind.QUOTE,
        TokenKind.IDENT,
        TokenKind.WHITESPACE,
        TokenKind.STAR,
        TokenKind.WHITESPACE,
        TokenKind.IDENT,
        TokenKind.WHITESPACE,
        TokenKind.RARROW,
        TokenKind.WHITESPACE,
        TokenKind.QUOTE,
        TokenKind.IDENT,
    ]


def test_symbolic_operator_maximal_munch_matches_official_rules():
    tokens = raw("let ( ** ) x y = x ** y\nlet (.!) r = !r\nlet ( *. ) a b = a *. b")
    assert [token.kind for token in tokens] == [
        TokenKind.LET,
        TokenKind.LPAREN,
        TokenKind.INFIX_STAR_STAR_OP,
        TokenKind.RPAREN,
        TokenKind.IDENT,
        TokenKind.IDENT,
        TokenKind.EQUALS,
        TokenKind.IDENT,
        TokenKind.INFIX_STAR_STAR_OP,
        TokenKind.IDENT,
        TokenKind.LET,
        TokenKind.LPAREN,
        TokenKind.DOT,
        TokenKind.PREFIX_OP,
        TokenKind.RPAREN,
        TokenKind.IDENT,
        TokenKind.EQUALS,
        TokenKind.PREFIX_OP,
        TokenKind.IDENT,
        TokenKind.LET,
        TokenKind.LPAREN,
        TokenKind.INFIX_STAR_DIV_MOD_OP,
        TokenKind.RPAREN,
        TokenKind.IDENT,
        TokenKind.IDENT,
        TokenKind.EQUALS,
        TokenKind.IDENT,
        TokenKind.INFIX_STAR_DIV_MOD_OP,
        TokenKind.IDENT,
        TokenKind.EOF,
    ]


def test_ident_bang_uses_official_keyword_fallback():
    tokens = raw("let! x = y\nuse! r = y\ndo! y\ntry! y\nyield! y\nreturn! y")
    assert [token.kind for token in tokens] == [
        TokenKind.BINDER,
        TokenKind.IDENT,
        TokenKind.EQUALS,
        TokenKind.IDENT,
        TokenKind.BINDER,
        TokenKind.IDENT,
        TokenKind.EQUALS,
        TokenKind.IDENT,
        TokenKind.DO_BANG,
        TokenKind.IDENT,
        TokenKind.IDENT,
        TokenKind.IDENT,
        TokenKind.YIELD_BANG,
        TokenKind.IDENT,
        TokenKind.YIELD_BANG,
        TokenKind.IDENT,
        TokenKind.EOF,
    ]
    assert tokens[10].text == "try!"


def test_preprocessor_defined_call_is_single_identifier_chunk():
    source = "#if defined(DEBUG) || defined(TEST)\n#endif"
    lines = format_official_tokens(
        tokenize(source, skip_trivia=False, use_lex_filter=False)
    ).splitlines()

    assert "1:1:3\tHASH_IF\t#if\t0\t35" in lines
    assert "1:4:4\tWHITESPACE\t \t6\t35" in lines
    assert "1:5:18\tIDENT\tdefined(DEBUG)\t197\t35" in lines
    assert "1:19:35\tWHITESPACE\t || defined(TEST)\t6\t35" in lines


def test_line_comment_keeps_block_doc_marker_as_word():
    tokens = tokenize("// // vs (**), global::", skip_trivia=False, use_lex_filter=False)
    assert [token.text for token in tokens[:-1]] == [
        "//",
        " ",
        "//",
        " ",
        "vs",
        " ",
        "(**),",
        " ",
        "global::",
    ]


def test_hash_help_emits_hash_only_like_unknown_directive_name():
    tokens = tokenize("#help", skip_trivia=False, use_lex_filter=False)
    assert [(token.kind, token.text) for token in tokens[:-1]] == [(TokenKind.HASH, "#")]


def test_inactive_code_splits_quoted_text_like_words():
    source = "#if false\n#warning \"Experimental path active\"\n#endif"
    lines = format_official_tokens(
        tokenize(source, skip_trivia=False, use_lex_filter=False)
    ).splitlines()

    assert '2:10:22\tINACTIVECODE\t"Experimental\t8\t13' in lines
    assert "2:23:23\tINACTIVECODE\t \t8\t1" in lines
    assert "2:24:27\tINACTIVECODE\tpath\t8\t4" in lines
    assert "2:29:35\tINACTIVECODE\tactive\"\t8\t7" in lines

    source = '#if false\n#r "System.Core"\n#endif'
    lines = format_official_tokens(
        tokenize(source, skip_trivia=False, use_lex_filter=False)
    ).splitlines()
    assert '2:4:16\tINACTIVECODE\t"System.Core"\t8\t13' in lines


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


def test_official_text_format_uses_utf16_columns_and_lengths():
    tokens = tokenize('"🌍"', skip_trivia=False, use_lex_filter=False)

    assert format_official_tokens(tokens) == "\n".join(
        [
            '1:1:1\tSTRING_TEXT\t"\t10\t1',
            "1:2:3\tSTRING_TEXT\t🌍\t10\t2",
            '1:4:4\tSTRING\t"\t205\t1',
        ]
    )


def test_raw_inactive_code_keeps_non_conditional_directives_as_inactive_code():
    source = '#if DEBUG\n#r "System.Core"\n#nowarn "9" "41"\nmodule LexerTest.Debug\n#endif'
    tokens = tokenize(source, skip_trivia=False, use_lex_filter=False)
    lines = format_official_tokens(tokens).splitlines()

    assert "2:1:2\tINACTIVECODE\t#r\t8\t2" in lines
    assert '3:1:7\tINACTIVECODE\t#nowarn\t8\t7' in lines
    assert "4:8:22\tINACTIVECODE\tLexerTest.Debug\t8\t15" in lines
    assert all("WARN_DIRECTIVE" not in line for line in lines)


def test_raw_block_comment_skips_newline_comment_chunks():
    tokens = tokenize("(* a\n   b *)", skip_trivia=False, use_lex_filter=False)
    lines = format_official_tokens(tokens).splitlines()

    assert "1:5:0\tCOMMENT\t\\n\t5\t1" not in lines
    assert "2:1:3\tCOMMENT\t   \t5\t3" in lines


def test_raw_numeric_edge_cases_match_official_shapes():
    tokens = tokenize("1.23e-4 [1..3]", skip_trivia=False, use_lex_filter=False)

    assert [token.kind for token in tokens if token.kind != TokenKind.WHITESPACE][:-1] == [
        TokenKind.IEEE64,
        TokenKind.LBRACK,
        TokenKind.INT32_DOT_DOT,
        TokenKind.INT32,
        TokenKind.RBRACK,
    ]
    assert tokens[0].text == "1.23e-4"


def test_raw_single_backtick_and_unicode_operator_recovery_match_official():
    source = "let `∂/∂x` f x = f x\nlet (§) x = x * 2\nlet y = 3"
    lines = format_official_tokens(
        tokenize(source, skip_trivia=False, use_lex_filter=False)
    ).splitlines()

    assert "1:5:5\tIDENT\t`\t197\t1" in lines
    assert all("1:6" not in line for line in lines)
    assert "2:1:3\tLET\tlet\t166\t3" in lines
    assert "2:5:5\tLPAREN\t(\t94\t1" in lines
    assert all("2:10" not in line for line in lines)
    assert "3:5:5\tIDENT\ty\t197\t1" in lines


def test_raw_operator_compatibility_cases():
    tokens = tokenize(
        "(=>) (>>|) (-->) (|>>) 1 <<< 2 >>> 3 &&& 4 ||| 5 <| 6 << 7",
        skip_trivia=False,
        use_lex_filter=False,
    )

    lines = format_official_tokens(tokens).splitlines()
    assert "1:2:3\tINFIX_COMPARE_OP\t=>\t194\t2" in lines
    assert "1:7:7\tGREATER\t>\t165\t3" in lines
    assert "1:8:8\tGREATER\t>\t165\t3" in lines
    assert "1:9:9\tINFIX_BAR_OP\t|\t192\t3" in lines
    assert "1:13:15\tPLUS_MINUS_OP\t-->\t188\t3" in lines
    assert "1:19:21\tINFIX_BAR_OP\t|>>\t192\t3" in lines
    assert "1:26:28\tINFIX_COMPARE_OP\t<<<\t194\t3" in lines
    assert "1:32:32\tGREATER\t>\t165\t3" in lines
    assert "1:33:33\tGREATER\t>\t165\t3" in lines
    assert "1:34:34\tGREATER\t>\t165\t3" in lines
    assert "1:38:40\tINFIX_AMP_OP\t&&&\t189\t3" in lines
    assert "1:44:46\tINFIX_BAR_OP\t|||\t192\t3" in lines
    assert "1:50:51\tINFIX_COMPARE_OP\t<|\t194\t2" in lines
    assert "1:55:56\tINFIX_COMPARE_OP\t<<\t194\t2" in lines


def test_raw_verbatim_and_unicode_escape_string_chunks_match_official():
    source = '@"He said ""Hi""" "\\u0041\\U00000043"'
    tokens = tokenize(source, skip_trivia=False, use_lex_filter=False)
    lines = format_official_tokens(tokens).splitlines()

    assert '1:11:12\tSTRING_TEXT\t""\t10\t2' in lines
    assert '1:15:16\tSTRING_TEXT\t""\t10\t2' in lines
    assert "1:20:25\tSTRING_TEXT\t\\\\u0041\t10\t6" in lines
    assert "1:26:35\tSTRING_TEXT\t\\\\U00000043\t10\t10" in lines


def test_raw_interpolation_expr_continues_across_inner_string():
    source = '$"Nested: {sprintf "%d" (1 + 1)}"'
    tokens = tokenize(source, skip_trivia=False, use_lex_filter=False)
    lines = format_official_tokens(tokens).splitlines()

    assert "1:12:18\tIDENT\tsprintf\t197\t7" in lines
    assert '1:20:20\tSTRING_TEXT\t"\t10\t1' in lines
    assert "1:21:21\tSTRING_TEXT\t%\t10\t1" in lines
    assert "1:22:22\tSTRING_TEXT\td\t10\t1" in lines
    assert '1:23:23\tSTRING\t"\t205\t1' in lines
    assert "1:25:25\tLPAREN\t(\t94\t1" in lines
    assert "1:28:28\tPLUS_MINUS_OP\t+\t188\t1" in lines
    assert "1:32:32\tSTRING_TEXT\t}\t10\t1" in lines
    assert '1:33:33\tINTERP_STRING_END\t"\t201\t1' in lines


def test_raw_invalid_backslash_recovery_does_not_open_next_string():
    source = 'let x = 1 \\ "unterminated"\nlet y = $"ok {1}"'
    lines = format_official_tokens(
        tokenize(source, skip_trivia=False, use_lex_filter=False)
    ).splitlines()

    assert all("unterminated" not in line for line in lines)
    assert "2:5:5\tIDENT\ty\t197\t1" in lines
    assert '2:9:10\tSTRING_TEXT\t$"\t10\t2' in lines
    assert "2:14:14\tINTERP_STRING_BEGIN_PART\t{\t203\t1" in lines


def test_raw_interpolation_format_and_escaped_brace_chunks_match_official():
    source = '$"{123.456:0.00}" $"{{escaped}}"'
    lines = format_official_tokens(
        tokenize(source, skip_trivia=False, use_lex_filter=False)
    ).splitlines()

    assert "1:4:10\tIEEE64\t123.456\t173\t7" in lines
    assert "1:12:15\tIEEE64\t0.00\t173\t4" in lines
    assert "1:21:22\tSTRING_TEXT\t{{\t10\t2" in lines
    assert "1:23:29\tSTRING_TEXT\tescaped\t10\t7" in lines
    assert "1:30:31\tSTRING_TEXT\t}}\t10\t2" in lines


def test_raw_additional_longest_match_operators_and_single_tilde():
    source = "a ||> b <|| c <||| d &+ e %& f &% g ~x"
    lines = format_official_tokens(
        tokenize(source, skip_trivia=False, use_lex_filter=False)
    ).splitlines()

    assert "1:3:5\tINFIX_BAR_OP\t||>\t192\t3" in lines
    assert "1:9:11\tINFIX_COMPARE_OP\t<||\t194\t3" in lines
    assert "1:15:18\tINFIX_COMPARE_OP\t<|||\t194\t4" in lines
    assert "1:22:23\tINFIX_AMP_OP\t&+\t189\t2" in lines
    assert "1:27:28\tINFIX_STAR_DIV_MOD_OP\t%&\t190\t2" in lines
    assert "1:32:33\tINFIX_AMP_OP\t&%\t189\t2" in lines
    assert "1:37:37\tRESERVED\t~\t152\t1" in lines


def test_raw_indented_preprocessor_directives_match_official_shape():
    source = "\n".join(
        [
            "module M =",
            "    #if false",
            "    #define A",
            "    #elif not A && B",
            "    #define C",
            "    #else",
            "    #define D",
            "    #endif",
            "    #nowarn 40 41",
            "    #line 100 \"gen.fs\"",
        ]
    )
    lines = format_official_tokens(
        tokenize(source, skip_trivia=False, use_lex_filter=False)
    ).splitlines()

    assert "2:1:4\tWHITESPACE\t    \t6\t13" in lines
    assert "4:5:9\tHASH_IF\t#elif\t0\t20" in lines
    assert "4:14:20\tWHITESPACE\t A && B\t6\t20" in lines
    assert "6:5:9\tHASH_IF\t#else\t0\t9" in lines
    assert "7:5:5\tHASH\t#\t90\t1" in lines
    assert "7:13:13\tIDENT\tD\t197\t1" in lines
    assert "9:5:17\tWARN_DIRECTIVE\t#nowarn 40 41\t4\t17" in lines
    assert "10:5:5\tHASH\t#\t90\t1" in lines
    assert "10:10:10\tWHITESPACE\t \t6\t1" in lines
    assert "10:11:13\tINT32\t100\t182\t3" in lines


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
