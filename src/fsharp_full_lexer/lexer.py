from __future__ import annotations

from collections.abc import Iterator
from dataclasses import dataclass, field
from decimal import Decimal
from pathlib import Path

import regex

from .diagnostics import DiagnosticSink
from .keywords import keyword_or_identifier
from .literals import (
    ESCAPES,
    checked_signed,
    checked_unsigned,
    decode_escape,
    parse_based_int,
    parse_decimal,
    parse_float32,
    parse_float64,
    remove_underscores,
)
from .position import Position, Range
from .preprocessor import eval_ifdef, parse_ifdef
from .tokens import Token, TokenKind

IDENT_START = r"[\p{Lu}\p{Ll}\p{Lt}\p{Lm}\p{Lo}\p{Nl}_]"
IDENT_CONT = r"[\p{Lu}\p{Ll}\p{Lt}\p{Lm}\p{Lo}\p{Nl}\p{Pc}\p{Mn}\p{Mc}\p{Cf}\p{Nd}']"
IDENT_RE = regex.compile(rf"{IDENT_START}{IDENT_CONT}*", regex.VERSION1)
INT_BODY = r"\p{Nd}(?:[\p{Nd}_]*\p{Nd})?"
XINT_BODY = (
    r"0[xX][0-9A-Fa-f](?:[0-9A-Fa-f_]*[0-9A-Fa-f])?"
    r"|0[oO][0-7](?:[0-7_]*[0-7])?"
    r"|0[bB][01](?:[01_]*[01])?"
)
FLOAT_RE = regex.compile(
    rf"(?:{INT_BODY}(?:\.(?:{INT_BODY})?)?[eE][+-]?{INT_BODY}|{INT_BODY}\.(?:{INT_BODY})?)",
    regex.VERSION1,
)
INT_RE = regex.compile(INT_BODY, regex.VERSION1)
XINT_RE = regex.compile(XINT_BODY, regex.VERSION1)


@dataclass
class LexArgs:
    conditional_defines: set[str] = field(default_factory=set)
    diagnostics: DiagnosticSink = field(default_factory=DiagnosticSink)
    apply_line_directives: bool = True


PUNCTUATION: list[tuple[str, TokenKind, object | None]] = [
    ("@@>|}", TokenKind.RQUOTE_BAR_RBRACE, ("<@@ @@>", True)),
    ("@>|}", TokenKind.RQUOTE_BAR_RBRACE, ("<@ @>", False)),
    ("@@>.", TokenKind.RQUOTE_DOT, ("<@@ @@>", True)),
    ("@>.", TokenKind.RQUOTE_DOT, ("<@ @>", False)),
    ("<@@", TokenKind.LQUOTE, ("<@@ @@>", True)),
    ("<@", TokenKind.LQUOTE, ("<@ @>", False)),
    ("@@>", TokenKind.RQUOTE, ("<@@ @@>", True)),
    ("@>", TokenKind.RQUOTE, ("<@ @>", False)),
    (">|]", TokenKind.GREATER_BAR_RBRACK, None),
    (">|}", TokenKind.GREATER_BAR_RBRACE, None),
    ("|]", TokenKind.BAR_RBRACK, None),
    ("|}", TokenKind.BAR_RBRACE, None),
    (">]", TokenKind.GREATER_RBRACK, None),
    ("[|", TokenKind.LBRACK_BAR, None),
    ("{|", TokenKind.LBRACE_BAR, None),
    ("[<", TokenKind.LBRACK_LESS, None),
    ("(*)", TokenKind.LPAREN_STAR_RPAREN, None),
    ("->", TokenKind.RARROW, None),
    ("??", TokenKind.QMARK_QMARK, None),
    ("..^", TokenKind.DOT_DOT_HAT, None),
    ("..", TokenKind.DOT_DOT, None),
    ("::", TokenKind.COLON_COLON, None),
    (":?>", TokenKind.COLON_QMARK_GREATER, None),
    (":>", TokenKind.COLON_GREATER, None),
    (":?", TokenKind.COLON_QMARK, None),
    (":=", TokenKind.COLON_EQUALS, None),
    (";;", TokenKind.SEMICOLON_SEMICOLON, None),
    ("<-", TokenKind.LARROW, None),
    ("&&", TokenKind.AMP_AMP, None),
    ("||", TokenKind.BAR_BAR, None),
    ("#", TokenKind.HASH, None),
    ("&", TokenKind.AMP, None),
    ("'", TokenKind.QUOTE, None),
    ("(", TokenKind.LPAREN, None),
    (")", TokenKind.RPAREN, None),
    ("*", TokenKind.STAR, None),
    (",", TokenKind.COMMA, None),
    ("?", TokenKind.QMARK, None),
    (".", TokenKind.DOT, None),
    (":", TokenKind.COLON, None),
    (";", TokenKind.SEMICOLON, None),
    ("=", TokenKind.EQUALS, None),
    ("[", TokenKind.LBRACK, None),
    ("]", TokenKind.RBRACK, None),
    ("{", TokenKind.LBRACE, None),
    ("}", TokenKind.RBRACE, None),
    ("<", TokenKind.LESS, False),
    (">", TokenKind.GREATER, False),
    ("|", TokenKind.BAR, None),
    ("$", TokenKind.DOLLAR, None),
    ("%", TokenKind.PERCENT_OP, "%"),
    ("-", TokenKind.MINUS, None),
]

FUNKY_OPERATORS = [
    ".[]<-",
    ".[,]<-",
    ".[,,]<-",
    ".[,,,]<-",
    ".[..,..,..,..]",
    ".[..,..,..]",
    ".[..,..]",
    ".[,,,]",
    ".[,,]",
    ".[,]",
    ".[..]",
    ".[]",
    ".()<-",
    ".()",
]


class RawLexer:
    def __init__(
        self,
        source: str,
        *,
        defines: set[str] | None = None,
        skip_trivia: bool = True,
        filename: str | None = None,
    ) -> None:
        self.source = source.replace("\r\n", "\n")
        self.args = LexArgs(set(defines or ()))
        self.skip_trivia = skip_trivia
        self.filename = filename
        self.index = 0
        self.pos = Position(0, 1, 0)
        self.pending: list[Token] = []
        self.ifdef_stack: list[tuple[str, bool]] = []
        self.finished = False

    @classmethod
    def from_file(cls, path: str | Path, **kwargs) -> RawLexer:
        p = Path(path)
        return cls(p.read_text(encoding="utf-8-sig"), filename=str(p), **kwargs)

    def __iter__(self) -> Iterator[Token]:
        while True:
            tok = self.next_token()
            yield tok
            if tok.kind == TokenKind.EOF:
                break

    def range_for(self, text: str) -> Range:
        return Range(self.pos, self.pos.advance(text))

    def position_at(self, index: int) -> Position:
        return Position(0, 1, 0).advance(self.source[:index])

    def range_for_span(self, start: int, end: int) -> Range:
        return Range(self.position_at(start), self.position_at(end))

    def token_for_span(
        self, kind: TokenKind, start: int, end: int, value=None, text: str | None = None
    ) -> Token:
        if text is None:
            text = self.source[start:end]
        return Token(kind, value, self.range_for_span(start, end), text)

    def advance(self, text: str) -> Range:
        range_ = self.range_for(text)
        self.index += len(text)
        self.pos = range_.end
        return range_

    def startswith(self, text: str) -> bool:
        return self.source.startswith(text, self.index)

    def rest_of_line(self) -> str:
        end = self.source.find("\n", self.index)
        if end < 0:
            return self.source[self.index :]
        return self.source[self.index : end]

    def emit(
        self,
        kind: TokenKind,
        value=None,
        text: str | None = None,
        range_: Range | None = None,
    ) -> Token:
        if text is None:
            text = ""
        if range_ is None:
            range_ = self.range_for(text)
        return Token(kind, value, range_, text)

    def next_token(self) -> Token:
        if self.pending:
            return self.pending.pop(0)
        if self.finished:
            return self.emit(TokenKind.EOF, text="")
        if self.index >= len(self.source):
            self.finished = True
            return self.emit(TokenKind.EOF, text="")

        if self.in_inactive_code():
            tok = self.scan_inactive()
            if tok is None:
                return self.next_token()
            return tok

        tok = self.scan_active()
        if tok is None:
            return self.next_token()
        return tok

    def in_inactive_code(self) -> bool:
        return bool(self.ifdef_stack and not self.ifdef_stack[-1][1])

    def scan_active(self) -> Token | None:
        ch = self.source[self.index]
        if ch in " \t":
            return self.scan_whitespace()
        if ch == "\n":
            r = self.advance("\n")
            if self.skip_trivia:
                return None
            return Token(TokenKind.WHITESPACE, None, r, "\n")
        if self.is_line_directive_position() and ch == "#":
            directive = self.scan_directive(active=True)
            if directive is not False:
                return directive
            skipped = self.scan_line_directive_name_as_hash()
            if skipped is not None:
                return skipped
        if self.startswith("//"):
            return self.scan_line_comment()
        if self.startswith("(*)"):
            return self.scan_punctuation()
        if self.startswith("(*"):
            r = self.advance("(*")
            return self.scan_block_comment(1, r.start)
        extended_triple = regex.match(r"\$+\"\"\"", self.source[self.index :])
        if self.startswith('"""') or self.startswith('$"""') or extended_triple:
            return self.scan_triple_or_extended_string()
        if self.startswith('@"') or self.startswith('$@"') or self.startswith('@$"'):
            return self.scan_verbatim_string()
        if self.startswith('$"') or ch == '"':
            return self.scan_regular_string()
        if ch == "`" and (not self.skip_trivia or self.startswith("``")):
            return self.scan_backtick_identifier()
        if ch == "'":
            char = self.scan_char_literal()
            if char is not None:
                return char
        special = self.scan_special_word()
        if special is not None:
            return special
        number = self.scan_number()
        if number is not None:
            return number
        ident = self.scan_identifier()
        if ident is not None:
            return ident
        funky = self.scan_funky_operator()
        if funky is not None:
            return funky
        punct = self.scan_punctuation()
        if punct is not None:
            return punct
        op = self.scan_operator()
        if op is not None:
            return op
        if not self.skip_trivia and (ord(ch) > 0x7F or ch == "\\"):
            text = self.rest_of_line() or ch
            self.advance(text)
            return None
        r = self.advance(ch)
        return Token(TokenKind.LEX_FAILURE, f"Unexpected character {ch!r}", r, ch)

    def scan_whitespace(self) -> Token | None:
        start = self.index
        while self.index < len(self.source) and self.source[self.index] in " \t":
            self.index += 1
        text = self.source[start : self.index]
        range_ = self.range_for(text)
        self.pos = range_.end
        if "\t" in text:
            self.args.diagnostics.error("FSLEX_TAB", "Tabs are not allowed by the F# lexer", range_)
        if self.skip_trivia:
            return None
        value = None
        if self.index < len(self.source) and self.source[self.index] == "#":
            line_start = self.source.rfind("\n", 0, start) + 1
            if all(ch in " \t" for ch in self.source[line_start:start]):
                line_end = self.source.find("\n", self.index)
                if line_end < 0:
                    line_end = len(self.source)
                stripped = self.source[self.index : line_end].strip()
                directive = stripped.split(None, 1)[0] if stripped else ""
                if directive in {"#if", "#elif", "#else", "#endif", "#nowarn", "#warnon"}:
                    value = {"full_length": line_end - line_start}
        return Token(TokenKind.WHITESPACE, value, range_, text)

    def is_line_directive_position(self) -> bool:
        line_start = self.source.rfind("\n", 0, self.index) + 1
        return all(ch in " \t" for ch in self.source[line_start : self.index])

    def scan_special_word(self) -> Token | None:
        specials = {
            "return!": (TokenKind.YIELD_BANG, False),
            "yield!": (TokenKind.YIELD_BANG, True),
            "match!": (TokenKind.MATCH_BANG, None),
            "while!": (TokenKind.WHILE_BANG, None),
            "and!": (TokenKind.AND_BANG, False),
            "do!": (TokenKind.DO_BANG, None),
        }
        for text, (kind, value) in sorted(specials.items(), key=lambda x: len(x[0]), reverse=True):
            if self.startswith(text) and self.boundary(self.index + len(text)):
                r = self.advance(text)
                return Token(kind, value, r, text)
        return None

    def boundary(self, index: int) -> bool:
        if index >= len(self.source):
            return True
        return not regex.match(r"[\p{L}\p{N}_']", self.source[index], regex.VERSION1)

    def scan_identifier(self) -> Token | None:
        match = IDENT_RE.match(self.source, self.index)
        if not match:
            return None
        text = match.group(0)
        r = self.advance(text)
        if self.index < len(self.source) and self.source[self.index] == "!":
            bang_range = self.range_for("!")
            self.advance("!")
            full_range = Range(r.start, bang_range.end)
            full_text = text + "!"
            if keyword_or_identifier(text, r).kind == TokenKind.LET:
                return Token(TokenKind.BINDER, text, Range(r.start, bang_range.end), text + "!")
            return keyword_or_identifier(full_text, full_range)
        return keyword_or_identifier(text, r)

    def scan_number(self) -> Token | None:
        text = self.source[self.index :]
        xmatch = XINT_RE.match(text)
        fmatch = FLOAT_RE.match(text)
        imatch = INT_RE.match(text)
        if imatch and text.startswith("..", imatch.end()):
            base = imatch.group(0)
            full = base + ".."
            r = self.advance(full)
            try:
                value = checked_signed(parse_based_int(base), 32)
                return Token(TokenKind.INT32_DOT_DOT, value, r, full)
            except Exception as ex:
                self.args.diagnostics.error("FSLEX_NUMERIC", str(ex), r)
                return Token(TokenKind.LEX_FAILURE, "Invalid numeric literal", r, full)
        candidates: list[tuple[str, str]] = []
        if xmatch:
            candidates.append(("xint", xmatch.group(0)))
        if fmatch:
            candidates.append(("float", fmatch.group(0)))
        if imatch:
            candidates.append(("int", imatch.group(0)))
        if not candidates:
            return None
        typ, base = max(candidates, key=lambda x: len(x[1]))
        suffix = self.numeric_suffix_after(len(base), typ == "float")
        full = base + suffix
        if self.index + len(full) < len(self.source) and regex.match(
            r"[\p{L}\p{N}_']", self.source[self.index + len(full)], regex.VERSION1
        ):
            while self.index + len(full) < len(self.source) and regex.match(
                r"[\p{L}\p{N}_']", self.source[self.index + len(full)], regex.VERSION1
            ):
                full += self.source[self.index + len(full)]
            r = self.advance(full)
            self.args.diagnostics.error("FSLEX_NUMERIC", "Invalid numeric literal", r)
            return Token(TokenKind.INT32, (0, False), r, full)
        r = self.advance(full)
        return self.make_number_token(base, suffix, typ, full, r)

    def numeric_suffix_after(self, offset: int, is_float: bool) -> str:
        tail = self.source[self.index + offset :]
        if is_float:
            for suffix in ("f", "F", "m", "M"):
                if tail.startswith(suffix):
                    return suffix
            return ""
        suffixes = (
            "uy",
            "us",
            "ul",
            "un",
            "uL",
            "UL",
            "lf",
            "lF",
            "LF",
            "y",
            "s",
            "l",
            "u",
            "n",
            "L",
            "I",
            "N",
            "Z",
            "Q",
            "R",
            "G",
            "f",
            "F",
            "m",
            "M",
        )
        for suffix in suffixes:
            if tail.startswith(suffix):
                return suffix
        return ""

    def make_number_token(
        self, base: str, suffix: str, typ: str, full: str, range_: Range
    ) -> Token:
        try:
            if suffix in {"f", "F"}:
                if typ == "int":
                    return Token(TokenKind.IEEE32, parse_float32(full), range_, full)
                return Token(TokenKind.IEEE32, parse_float32(full), range_, full)
            if typ == "float":
                if suffix in {"m", "M"}:
                    return Token(TokenKind.DECIMAL, parse_decimal(full), range_, full)
                return Token(TokenKind.IEEE64, parse_float64(full), range_, full)
            if suffix in {"m", "M"}:
                return Token(TokenKind.DECIMAL, Decimal(remove_underscores(base)), range_, full)
            if suffix in {"I", "N", "Z", "Q", "R", "G"}:
                return Token(TokenKind.BIGNUM, (remove_underscores(base), suffix), range_, full)
            value = parse_based_int(base)
            if suffix == "y":
                return Token(TokenKind.INT8, checked_signed(value, 8), range_, full)
            if suffix == "s":
                return Token(TokenKind.INT16, checked_signed(value, 16), range_, full)
            if suffix == "l":
                return Token(TokenKind.INT32, checked_signed(value, 32), range_, full)
            if suffix == "L":
                return Token(TokenKind.INT64, checked_signed(value, 64), range_, full)
            if suffix == "n":
                return Token(TokenKind.NATIVEINT, checked_signed(value, 64), range_, full)
            if suffix == "uy":
                return Token(TokenKind.UINT8, checked_unsigned(value, 8), range_, full)
            if suffix == "us":
                return Token(TokenKind.UINT16, checked_unsigned(value, 16), range_, full)
            if suffix in {"u", "ul"}:
                return Token(TokenKind.UINT32, checked_unsigned(value, 32), range_, full)
            if suffix in {"uL", "UL"}:
                return Token(TokenKind.UINT64, checked_unsigned(value, 64), range_, full)
            if suffix == "un":
                return Token(TokenKind.UNATIVEINT, checked_unsigned(value, 64), range_, full)
            return Token(TokenKind.INT32, checked_signed(value, 32), range_, full)
        except Exception as ex:
            self.args.diagnostics.error("FSLEX_NUMERIC", str(ex), range_)
            return Token(TokenKind.LEX_FAILURE, "Invalid numeric literal", range_, full)

    def scan_char_literal(self) -> Token | None:
        i = self.index
        if i + 2 >= len(self.source) or self.source[i] != "'":
            return None
        j = i + 1
        try:
            if self.source[j] == "\\":
                if j + 1 < len(self.source) and self.source[j + 1] in ESCAPES:
                    content = self.source[j : j + 2]
                    j += 2
                elif self.source.startswith("\\x", j):
                    content = self.source[j : j + 4]
                    j += 4
                elif self.source.startswith("\\u", j):
                    content = self.source[j : j + 6]
                    j += 6
                elif self.source.startswith("\\U", j):
                    content = self.source[j : j + 10]
                    j += 10
                elif self.source[j + 1 : j + 4].isdigit():
                    content = self.source[j : j + 4]
                    j += 4
                else:
                    return None
                value = decode_escape(content)
            elif self.source[j] not in "\n\r\t\b":
                value = self.source[j]
                j += 1
            else:
                return None
            if j < len(self.source) and self.source[j] == "'":
                j += 1
                byte = j < len(self.source) and self.source[j] == "B"
                if byte:
                    j += 1
                text = self.source[i:j]
                r = self.advance(text)
                if byte:
                    code = ord(value)
                    if code > 127:
                        return Token(TokenKind.LEX_FAILURE, "Invalid ASCII byte literal", r, text)
                    return Token(TokenKind.UINT8, code, r, text)
                return Token(TokenKind.CHAR, value, r, text)
        except Exception:
            return None
        return None

    def scan_backtick_identifier(self) -> Token:
        start = self.index
        if not self.startswith("``"):
            r = self.advance("`")
            return Token(TokenKind.IDENT, "`", r, "`")
        end = self.source.find("``", start + 2)
        if end < 0:
            text = self.source[start:]
            r = self.advance(text)
            return Token(TokenKind.IDENT, text[2:], r, text)
        text = self.source[start : end + 2]
        r = self.advance(text)
        return Token(TokenKind.IDENT, text[2:-2], r, text)

    def return_chunks(self, chunks: list[Token]) -> Token | None:
        if not chunks:
            return None
        self.pending.extend(chunks[1:])
        return chunks[0]

    def split_comment_chunks(self, start: int, end: int, kind: TokenKind) -> list[Token]:
        chunks: list[Token] = []
        i = start
        while i < end:
            if self.source[i] == "\n":
                i += 1
                continue
            if self.source.startswith("///", i):
                j = i + 3
            elif (
                self.source.startswith("//", i)
                or (
                    kind != TokenKind.LINE_COMMENT
                    and (
                        self.source.startswith("(*", i)
                        or self.source.startswith("*)", i)
                    )
                )
            ):
                j = i + 2
            elif self.source[i] in " \t":
                j = i + 1
                while j < end and self.source[j] in " \t":
                    j += 1
            else:
                j = i + 1
                while (
                    j < end
                    and self.source[j] not in " \t\n"
                    and (
                        kind == TokenKind.LINE_COMMENT
                        or (
                            not self.source.startswith("(*", j)
                            and not self.source.startswith("*)", j)
                        )
                    )
                ):
                    j += 1
            chunks.append(self.token_for_span(kind, i, j))
            i = j
        return chunks

    def split_string_text_chunks(
        self,
        start: int,
        end: int,
        *,
        escape_backslash: bool = True,
        doubled_quote_escape: bool = False,
    ) -> list[Token]:
        chunks: list[Token] = []
        i = start
        while i < end:
            ch = self.source[i]
            if ch == "\n":
                i += 1
                continue
            if ch in " \t":
                j = i + 1
                while j < end and self.source[j] in " \t":
                    j += 1
            elif self.source.startswith("{{", i) or self.source.startswith("}}", i):
                j = i + 2
            elif doubled_quote_escape and self.source.startswith('""', i):
                j = i + 2
            elif escape_backslash and ch == "\\":
                j = min(i + self.escape_len(i), end)
            elif ch == "%":
                j = i + 1
                while j < end and self.source[j] == "%":
                    j += 1
            else:
                j = self.match_string_text_word_end(i, end)
            chunks.append(self.token_for_span(TokenKind.STRING_TEXT, i, j))
            i = j
        return chunks

    def match_string_text_word_end(self, start: int, end: int) -> int:
        matches = [
            match.end()
            for regex_ in (IDENT_RE, INT_RE, XINT_RE)
            if (match := regex_.match(self.source, start)) and match.end() <= end
        ]
        return max(matches, default=start + 1)

    def add_interpolated_chunks(
        self, chunks: list[Token], content_start: int, content_end: int, delimiter_len: int
    ) -> bool:
        i = content_start
        first = True
        text_start = i
        while i < content_end:
            if self.source[i] == "\n":
                chunks.extend(self.split_string_text_chunks(text_start, i))
                i += 1
                text_start = i
                continue
            if delimiter_len == 1 and (
                self.source.startswith("{{", i) or self.source.startswith("}}", i)
            ):
                i += 2
                continue
            if self.source.startswith("{" * delimiter_len, i):
                chunks.extend(self.split_string_text_chunks(text_start, i))
                open_end = i + delimiter_len
                kind = TokenKind.INTERP_STRING_BEGIN_PART if first else TokenKind.INTERP_STRING_PART
                chunks.append(self.token_for_span(kind, i, open_end))
                close = self.source.find("}" * delimiter_len, open_end)
                if close < 0 or close > content_end:
                    chunks.extend(self.split_string_text_chunks(open_end, content_end))
                    return False
                if not self.add_interpolation_expr_chunks(chunks, open_end, close):
                    return False
                for brace_index in range(close, close + delimiter_len):
                    chunks.append(
                        self.token_for_span(TokenKind.STRING_TEXT, brace_index, brace_index + 1)
                    )
                i = close + delimiter_len
                text_start = i
                first = False
            else:
                i += 1
        chunks.extend(self.split_string_text_chunks(text_start, content_end))
        return True

    def add_interpolation_expr_chunks(self, chunks: list[Token], start: int, end: int) -> bool:
        i = start
        while i < end:
            if self.source[i] in " \t":
                j = i + 1
                while j < end and self.source[j] in " \t":
                    j += 1
                chunks.append(self.token_for_span(TokenKind.WHITESPACE, i, j))
                i = j
                continue
            if self.source[i] == "\n":
                i += 1
                continue
            if self.source[i] == '"':
                close = self.find_regular_string_end(i + 1)
                close = min(close, end)
                chunks.append(self.token_for_span(TokenKind.STRING_TEXT, i, i + 1))
                chunks.extend(self.split_string_text_chunks(i + 1, close))
                if close < end and self.source[close] == '"':
                    chunks.append(self.token_for_span(TokenKind.STRING, close, close + 1))
                    i = close + 1
                else:
                    i = close
                continue
            if self.source[i] == "\\":
                return False
            floating = FLOAT_RE.match(self.source, i)
            if floating and floating.end() <= end:
                text = floating.group(0)
                chunks.append(
                    self.token_for_span(TokenKind.IEEE64, i, floating.end(), parse_float64(text))
                )
                i = floating.end()
                continue
            ident = IDENT_RE.match(self.source, i)
            if ident and ident.end() <= end:
                text = ident.group(0)
                r = self.range_for_span(i, ident.end())
                chunks.append(keyword_or_identifier(text, r))
                i = ident.end()
                continue
            integer = INT_RE.match(self.source, i)
            if integer and integer.end() <= end:
                chunks.append(
                    self.token_for_span(
                        TokenKind.INT32,
                        i,
                        integer.end(),
                        (int(remove_underscores(integer.group(0))), False),
                    )
                )
                i = integer.end()
                continue
            matched_punctuation = False
            for text, kind, value in PUNCTUATION:
                if self.source.startswith(text, i) and i + len(text) <= end:
                    chunks.append(self.token_for_span(kind, i, i + len(text), value))
                    i += len(text)
                    matched_punctuation = True
                    break
            if matched_punctuation:
                continue
            punctuation_kinds = {
                "(": TokenKind.LPAREN,
                ")": TokenKind.RPAREN,
                "[": TokenKind.LBRACK,
                "]": TokenKind.RBRACK,
                ";": TokenKind.SEMICOLON,
                "+": TokenKind.PLUS_MINUS_OP,
                "-": TokenKind.MINUS,
                "*": TokenKind.STAR,
                ".": TokenKind.DOT,
                ":": TokenKind.COLON,
                ",": TokenKind.COMMA,
            }
            kind = punctuation_kinds.get(self.source[i])
            if kind is not None:
                text = self.source[i : i + 1]
                value = text if kind in {TokenKind.PLUS_MINUS_OP} else None
                chunks.append(self.token_for_span(kind, i, i + 1, value))
                i += 1
                continue
            # Fall back to normal string text for unsupported interpolation contents.
            chunks.extend(self.split_string_text_chunks(i, end))
            return True
        return True

    def find_regular_string_end(self, content_start: int) -> int:
        i = content_start
        while i < len(self.source):
            if self.source[i] == "\\":
                i += max(1, self.escape_len(i))
                continue
            if self.source[i] == '"':
                return i
            i += 1
        return len(self.source)

    def find_interpolated_regular_string_end(self, content_start: int) -> int:
        i = content_start
        brace_depth = 0
        while i < len(self.source):
            ch = self.source[i]
            if ch == "\\" and brace_depth == 0:
                i += max(1, self.escape_len(i))
                continue
            if ch == "{":
                brace_depth += 1
                i += 1
                continue
            if ch == "}" and brace_depth:
                brace_depth -= 1
                i += 1
                continue
            if ch == '"':
                if brace_depth == 0:
                    return i
                inner_end = self.find_regular_string_end(i + 1)
                i = min(inner_end + 1, len(self.source))
                continue
            i += 1
        return len(self.source)

    def scan_regular_string_tokens(self) -> Token:
        start = self.index
        interpolated = self.startswith('$"')
        opener_len = 2 if interpolated else 1
        content_start = start + opener_len
        close = (
            self.find_interpolated_regular_string_end(content_start)
            if interpolated
            else self.find_regular_string_end(content_start)
        )
        end = min(close + 1, len(self.source))
        chunks = [self.token_for_span(TokenKind.STRING_TEXT, start, content_start)]
        if interpolated:
            ok = self.add_interpolated_chunks(chunks, content_start, close, 1)
            if ok and close < len(self.source):
                chunks.append(self.token_for_span(TokenKind.INTERP_STRING_END, close, end))
        else:
            chunks.extend(self.split_string_text_chunks(content_start, close))
            if close < len(self.source):
                close_kind = (
                    TokenKind.BYTEARRAY if self.source.startswith('"B', close) else TokenKind.STRING
                )
                close_end = close + 2 if close_kind == TokenKind.BYTEARRAY else end
                chunks.append(self.token_for_span(close_kind, close, close_end))
                end = close_end
        self.index = end
        self.pos = self.position_at(end)
        return self.return_chunks(chunks) or Token(
            TokenKind.EOF, None, self.range_for_span(start, end), ""
        )

    def find_verbatim_string_end(self, content_start: int) -> int:
        i = content_start
        while i < len(self.source):
            if self.source.startswith('""', i):
                i += 2
                continue
            if self.source[i] == '"':
                return i
            i += 1
        return len(self.source)

    def scan_verbatim_string_tokens(self) -> Token:
        start = self.index
        interpolated = self.startswith('$@"') or self.startswith('@$"')
        opener_len = 3 if interpolated else 2
        content_start = start + opener_len
        close = self.find_verbatim_string_end(content_start)
        end = min(close + 1, len(self.source))
        chunks = [self.token_for_span(TokenKind.STRING_TEXT, start, content_start)]
        if interpolated:
            ok = self.add_interpolated_chunks(chunks, content_start, close, 1)
            if ok and close < len(self.source):
                chunks.append(self.token_for_span(TokenKind.INTERP_STRING_END, close, end))
        else:
            chunks.extend(
                self.split_string_text_chunks(
                    content_start, close, escape_backslash=False, doubled_quote_escape=True
                )
            )
            if close < len(self.source):
                chunks.append(self.token_for_span(TokenKind.STRING, close, end))
        self.index = end
        self.pos = self.position_at(end)
        return self.return_chunks(chunks) or Token(
            TokenKind.EOF, None, self.range_for_span(start, end), ""
        )

    def scan_triple_string_tokens(self) -> Token:
        start = self.index
        dollar_count = 0
        while start + dollar_count < len(self.source) and self.source[start + dollar_count] == "$":
            dollar_count += 1
        opener_len = dollar_count + 3
        content_start = start + opener_len
        close = self.source.find('"""', content_start)
        if close < 0:
            close = len(self.source)
        end = min(close + 3, len(self.source))
        chunks = [self.token_for_span(TokenKind.STRING_TEXT, start, content_start)]
        if dollar_count:
            ok = self.add_interpolated_chunks(chunks, content_start, close, dollar_count)
            if ok and close < len(self.source):
                chunks.append(self.token_for_span(TokenKind.INTERP_STRING_END, close, end))
        else:
            chunks.extend(
                self.split_string_text_chunks(content_start, close, escape_backslash=False)
            )
            if close < len(self.source):
                chunks.append(self.token_for_span(TokenKind.STRING, close, end))
        self.index = end
        self.pos = self.position_at(end)
        return self.return_chunks(chunks) or Token(
            TokenKind.EOF, None, self.range_for_span(start, end), ""
        )

    def scan_line_comment(self) -> Token | None:
        start = self.index
        text = self.rest_of_line()
        r = self.advance(text)
        if self.skip_trivia:
            return None
        chunks = self.split_comment_chunks(start, start + len(text), TokenKind.LINE_COMMENT)
        return self.return_chunks(chunks) or Token(TokenKind.LINE_COMMENT, None, r, text)

    def scan_block_comment(self, depth: int, start: Position) -> Token | None:
        text_parts = ["(*"]
        while self.index < len(self.source):
            if self.startswith("(*"):
                depth += 1
                text_parts.append("(*")
                self.advance("(*")
            elif self.startswith("*)"):
                depth -= 1
                text_parts.append("*)")
                r_end = self.advance("*)")
                if depth == 0:
                    text = "".join(text_parts)
                    r = Range(start, r_end.end)
                    if self.skip_trivia:
                        return None
                    chunks = self.split_comment_chunks(
                        r.start.index, r.end.index, TokenKind.COMMENT
                    )
                    return self.return_chunks(chunks) or Token(TokenKind.COMMENT, None, r, text)
            else:
                ch = self.source[self.index]
                text_parts.append(ch)
                self.advance(ch)
        r = Range(start, self.pos)
        return Token(TokenKind.EOF, None, r, "".join(text_parts))

    def scan_regular_string(self) -> Token:
        if not self.skip_trivia:
            return self.scan_regular_string_tokens()
        interpolated = self.startswith('$"')
        opener = '$"' if interpolated else '"'
        start = self.pos
        self.advance(opener)
        chars: list[str] = []
        while self.index < len(self.source):
            if self.startswith('"B') and not interpolated:
                end = self.advance('"B')
                return Token(
                    TokenKind.BYTEARRAY,
                    bytes("".join(chars), "latin1", "ignore"),
                    Range(start, end.end),
                    "",
                )
            if self.startswith('"'):
                end = self.advance('"')
                kind = TokenKind.INTERP_STRING_BEGIN_END if interpolated else TokenKind.STRING
                return Token(kind, ("".join(chars), "Regular"), Range(start, end.end), "")
            if self.startswith("\\\n"):
                self.advance("\\\n")
                while self.index < len(self.source) and self.source[self.index] in " \t":
                    self.advance(self.source[self.index])
                continue
            if self.startswith("\\"):
                esc_len = self.escape_len(self.index)
                seq = self.source[self.index : self.index + esc_len]
                try:
                    chars.append(decode_escape(seq))
                except Exception:
                    chars.append(seq)
                self.advance(seq)
                continue
            chars.append(self.source[self.index])
            self.advance(self.source[self.index])
        return Token(TokenKind.EOF, None, Range(start, self.pos), "")

    def scan_verbatim_string(self) -> Token:
        if not self.skip_trivia:
            return self.scan_verbatim_string_tokens()
        interpolated = self.startswith('$@"') or self.startswith('@$"')
        opener = '$@"' if self.startswith('$@"') else '@$"' if self.startswith('@$"') else '@"'
        start = self.pos
        self.advance(opener)
        chars: list[str] = []
        while self.index < len(self.source):
            if self.startswith('""'):
                chars.append('"')
                self.advance('""')
                continue
            if self.startswith('"B') and not interpolated:
                end = self.advance('"B')
                return Token(
                    TokenKind.BYTEARRAY,
                    bytes("".join(chars), "latin1", "ignore"),
                    Range(start, end.end),
                    "",
                )
            if self.startswith('"'):
                end = self.advance('"')
                kind = TokenKind.INTERP_STRING_BEGIN_END if interpolated else TokenKind.STRING
                return Token(kind, ("".join(chars), "Verbatim"), Range(start, end.end), "")
            chars.append(self.source[self.index])
            self.advance(self.source[self.index])
        return Token(TokenKind.EOF, None, Range(start, self.pos), "")

    def scan_triple_or_extended_string(self) -> Token:
        if not self.skip_trivia:
            return self.scan_triple_string_tokens()
        start = self.pos
        dollar_count = 0
        while self.index < len(self.source) and self.source[self.index] == "$":
            dollar_count += 1
            self.advance("$")
        self.advance('"""')
        chars: list[str] = []
        while self.index < len(self.source):
            if self.startswith('"""B') and dollar_count == 0:
                end = self.advance('"""B')
                return Token(
                    TokenKind.BYTEARRAY,
                    bytes("".join(chars), "utf-8"),
                    Range(start, end.end),
                    "",
                )
            if self.startswith('"""'):
                end = self.advance('"""')
                kind = TokenKind.INTERP_STRING_BEGIN_END if dollar_count else TokenKind.STRING
                style = "ExtendedInterpolated" if dollar_count > 1 else "TripleQuote"
                return Token(kind, ("".join(chars), style), Range(start, end.end), "")
            chars.append(self.source[self.index])
            self.advance(self.source[self.index])
        return Token(TokenKind.EOF, None, Range(start, self.pos), "")

    def escape_len(self, index: int) -> int:
        if self.source.startswith("\\x", index):
            return 4
        if self.source.startswith("\\u", index):
            return 6
        if self.source.startswith("\\U", index):
            return 10
        if index + 4 <= len(self.source) and self.source[index + 1 : index + 4].isdigit():
            return 4
        return 2

    def scan_funky_operator(self) -> Token | None:
        for op in FUNKY_OPERATORS:
            if self.startswith(op):
                r = self.advance(op)
                return Token(TokenKind.FUNKY_OPERATOR_NAME, op, r, op)
        return None

    def scan_punctuation(self) -> Token | None:
        multi_operator_kinds = {
            "-->": TokenKind.PLUS_MINUS_OP,
            "|>>": TokenKind.INFIX_BAR_OP,
            "||>": TokenKind.INFIX_BAR_OP,
            "<|||": TokenKind.INFIX_COMPARE_OP,
            "<||": TokenKind.INFIX_COMPARE_OP,
            "<<<": TokenKind.INFIX_COMPARE_OP,
            "&&&": TokenKind.INFIX_AMP_OP,
            "|||": TokenKind.INFIX_BAR_OP,
            "&+": TokenKind.INFIX_AMP_OP,
            "&%": TokenKind.INFIX_AMP_OP,
            "%&": TokenKind.INFIX_STAR_DIV_MOD_OP,
            "<<": TokenKind.INFIX_COMPARE_OP,
            "<|": TokenKind.INFIX_COMPARE_OP,
        }
        for text, kind in multi_operator_kinds.items():
            if self.startswith(text):
                r = self.advance(text)
                return Token(kind, text, r, text)
        if self.startswith("=>"):
            r = self.advance("=>")
            return Token(TokenKind.INFIX_COMPARE_OP, "=>", r, "=>")
        if self.startswith(">>|"):
            full_value = {"full_length": 3}
            chunks = [
                self.token_for_span(TokenKind.GREATER, self.index, self.index + 1, full_value),
                self.token_for_span(TokenKind.GREATER, self.index + 1, self.index + 2, full_value),
                self.token_for_span(
                    TokenKind.INFIX_BAR_OP, self.index + 2, self.index + 3, full_value
                ),
            ]
            self.advance(">>|")
            return self.return_chunks(chunks)
        for text in (">>>", ">>"):
            if self.startswith(text):
                full_value = {"full_length": len(text)}
                chunks = [
                    self.token_for_span(
                        TokenKind.GREATER, self.index + offset, self.index + offset + 1, full_value
                    )
                    for offset in range(len(text))
                ]
                self.advance(text)
                return self.return_chunks(chunks)
        if self.startswith("$%"):
            r = self.advance("$%")
            return Token(TokenKind.INFIX_STAR_DIV_MOD_OP, "$%", r, "$%")
        if self.startswith("|>"):
            r = self.advance("|>")
            return Token(TokenKind.INFIX_BAR_OP, "|>", r, "|>")
        if self.startswith(".!"):
            full_value = {"full_length": 2}
            chunks = [
                self.token_for_span(TokenKind.DOT, self.index, self.index + 1, full_value),
                self.token_for_span(
                    TokenKind.PREFIX_OP, self.index + 1, self.index + 2, full_value
                ),
            ]
            self.advance(".!")
            return self.return_chunks(chunks)
        for text, kind in (
            ("**", TokenKind.INFIX_STAR_STAR_OP),
            ("*.", TokenKind.INFIX_STAR_DIV_MOD_OP),
        ):
            if self.startswith(text):
                r = self.advance(text)
                return Token(kind, text, r, text)
        for text, kind, value in PUNCTUATION:
            if self.startswith(text):
                r = self.advance(text)
                return Token(kind, value, r, text)
        return None

    def scan_operator(self) -> Token | None:
        op_chars = "!$%&*+-./<=>?@^|~:"
        ignored = ".$?"
        if self.source[self.index] not in op_chars:
            return None
        start = self.index
        while self.index < len(self.source) and self.source[self.index] in ignored:
            self.index += 1
        if self.index >= len(self.source) or self.source[self.index] not in op_chars:
            self.index = start
            return None
        first = self.source[self.index]
        while self.index < len(self.source) and self.source[self.index] in op_chars:
            self.index += 1
        text = self.source[start : self.index]
        r = self.range_for(text)
        self.pos = r.end
        if "**" in text:
            return Token(TokenKind.INFIX_STAR_STAR_OP, text, r, text)
        if first in "*/%":
            return Token(TokenKind.INFIX_STAR_DIV_MOD_OP, text, r, text)
        if first in "+-":
            return Token(TokenKind.PLUS_MINUS_OP, text, r, text)
        if first in "@^":
            return Token(TokenKind.INFIX_AT_HAT_OP, text, r, text)
        if first in "=<$>":
            return Token(TokenKind.INFIX_COMPARE_OP, text, r, text)
        if first == "&":
            return Token(TokenKind.INFIX_AMP_OP, text, r, text)
        if first == "|":
            return Token(TokenKind.INFIX_BAR_OP, text, r, text)
        if first in "!~":
            if text == "~":
                return Token(TokenKind.RESERVED, text, r, text)
            return Token(TokenKind.PREFIX_OP, text, r, text)
        return Token(TokenKind.PREFIX_OP, text, r, text)

    def scan_line_directive_name_as_hash(self) -> Token | None:
        directive_names = ("define", "load", "line", "time", "help", "r", "I")
        if not self.startswith("#"):
            return None
        for name in sorted(directive_names, key=len, reverse=True):
            if self.source.startswith(name, self.index + 1) and self.boundary(
                self.index + 1 + len(name)
            ):
                r = self.advance("#")
                self.advance(name)
                if self.skip_trivia:
                    return None
                return Token(TokenKind.HASH, None, r, "#")
        return None

    def scan_directive(self, *, active: bool) -> Token | None | bool:
        line = self.rest_of_line()
        stripped = line.strip()
        directive = stripped.split(None, 1)[0] if stripped else ""
        known = {"#if", "#else", "#elif", "#endif", "#nowarn", "#warnon", "#light", "#indent"}
        if directive not in known and not regex.match(r"#(?:line\s+)?\d+", stripped):
            return False
        text = line
        start_index = self.index
        r = self.advance(text)
        if directive == "#if":
            expr = parse_ifdef(stripped)
            result = eval_ifdef(expr, self.args.conditional_defines)
            self.ifdef_stack.append(("if", result))
            return None if self.skip_trivia else self.split_directive_tokens(start_index, len(text))
        if directive == "#elif":
            if not self.ifdef_stack:
                return Token(TokenKind.LEX_FAILURE, "#elif without #if", r, text)
            previous_taken = self.ifdef_stack[-1][0] == "elif-taken"
            result = (
                False
                if previous_taken
                else eval_ifdef(parse_ifdef(stripped), self.args.conditional_defines)
            )
            self.ifdef_stack[-1] = ("elif-taken" if result or previous_taken else "if", result)
            return None if self.skip_trivia else self.split_directive_tokens(start_index, len(text))
        if directive == "#else":
            if not self.ifdef_stack:
                return Token(TokenKind.LEX_FAILURE, "#else without #if", r, text)
            previous_taken = self.ifdef_stack[-1][0] == "elif-taken" or self.ifdef_stack[-1][1]
            self.ifdef_stack[-1] = ("else", not previous_taken)
            return None if self.skip_trivia else self.split_directive_tokens(start_index, len(text))
        if directive == "#endif":
            if not self.ifdef_stack:
                return Token(TokenKind.LEX_FAILURE, "#endif without #if", r, text)
            self.ifdef_stack.pop()
            return None if self.skip_trivia else self.split_directive_tokens(start_index, len(text))
        if directive in {"#nowarn", "#warnon"}:
            if self.skip_trivia:
                return None
            line_start = self.source.rfind("\n", 0, start_index) + 1
            return Token(
                TokenKind.WARN_DIRECTIVE,
                {"full_length": r.end.index - line_start},
                r,
                text,
            )
        if regex.match(r"#(?:line\s+)?\d+", stripped):
            line_start = self.source.rfind("\n", 0, start_index) + 1
            if start_index != line_start:
                self.index = start_index
                self.pos = self.position_at(start_index)
                return False
            return None if self.skip_trivia else Token(TokenKind.HASH_LINE, None, r, text)
        return None

    def split_directive_tokens(self, start: int, length: int) -> Token | None:
        end = start + length
        line_start = self.source.rfind("\n", 0, start) + 1
        full_value = {"full_length": end - line_start}
        chunks: list[Token] = []
        i = start
        directive_text = ""
        emitted_ident = False
        if i < end and self.source[i] == "#":
            j = i + 1
            while j < end and self.source[j].isalpha():
                j += 1
            directive_text = self.source[i:j]
            chunks.append(self.token_for_span(TokenKind.HASH_IF, i, j, full_value))
            i = j
        while i < end:
            if self.source[i] in " \t":
                j = i + 1
                while j < end and self.source[j] in " \t":
                    j += 1
                # The FCS tokenizer keeps the rest of complex conditional expressions as whitespace.
                if (
                    (directive_text in {"#if", "#elif"} and emitted_ident)
                    or (
                        j < end
                        and not (self.source[j].isalpha() or self.source[j] == "_")
                        and not (
                            directive_text in {"#if", "#elif"}
                            and self.source[j] == "("
                            and j + 1 < end
                            and regex.match(IDENT_START, self.source[j + 1], regex.VERSION1)
                        )
                    )
                ):
                    j = end
                chunks.append(self.token_for_span(TokenKind.WHITESPACE, i, j, full_value))
                i = j
                continue
            ident = IDENT_RE.match(self.source, i)
            if ident and ident.end() <= end:
                if (
                    directive_text in {"#if", "#elif"}
                    and ident.group(0) == "defined"
                    and ident.end() < end
                    and self.source[ident.end()] == "("
                ):
                    close = self.source.find(")", ident.end() + 1, end)
                    if close >= 0:
                        chunks.append(
                            self.token_for_span(TokenKind.IDENT, i, close + 1, full_value)
                        )
                        emitted_ident = True
                        i = close + 1
                        continue
                chunks.append(self.token_for_span(TokenKind.IDENT, i, ident.end(), full_value))
                emitted_ident = True
                i = ident.end()
                continue
            if (
                directive_text in {"#if", "#elif"}
                and self.source[i] == "("
                and i + 1 < end
                and (ident := IDENT_RE.match(self.source, i + 1))
            ):
                chunks.append(self.token_for_span(TokenKind.IDENT, i, ident.end(), full_value))
                emitted_ident = True
                i = ident.end()
                continue
            chunks.append(self.token_for_span(TokenKind.WHITESPACE, i, end, full_value))
            break
        return self.return_chunks(chunks)

    def scan_inactive(self) -> Token | None:
        if self.index >= len(self.source):
            return None
        if self.source[self.index] in " \t":
            line_start = self.source.rfind("\n", 0, self.index) + 1
            if self.index == line_start:
                j = self.index
                while j < len(self.source) and self.source[j] in " \t":
                    j += 1
                line_end = self.source.find("\n", j)
                if line_end < 0:
                    line_end = len(self.source)
                stripped = self.source[j:line_end].strip()
                name = stripped.split(None, 1)[0] if stripped else ""
                if name in {"#if", "#elif", "#else", "#endif"}:
                    r = self.advance(self.source[self.index:j])
                    if self.skip_trivia:
                        return None
                    return Token(
                        TokenKind.WHITESPACE,
                        {"full_length": line_end - line_start},
                        r,
                        self.source[r.start.index : r.end.index],
                    )
        if self.is_line_directive_position() and self.source[self.index] == "#":
            stripped = self.rest_of_line().strip()
            name = stripped.split(None, 1)[0] if stripped else ""
            if name in {"#if", "#elif", "#else", "#endif"}:
                directive = self.scan_directive(active=False)
                if directive is not False:
                    return directive
        text = self.rest_of_line()
        if not text and self.startswith("\n"):
            text = "\n"
        if not text:
            text = self.source[self.index]
        r = self.advance(text)
        if self.skip_trivia:
            return None
        chunks = self.split_inactive_chunks(r.start.index, r.end.index)
        return self.return_chunks(chunks) or Token(TokenKind.INACTIVECODE, None, r, text)

    def split_inactive_chunks(self, start: int, end: int) -> list[Token]:
        chunks: list[Token] = []
        i = start
        while i < end:
            if self.source[i] == "\n":
                i += 1
                continue
            if self.source[i] in " \t":
                j = i + 1
                while j < end and self.source[j] in " \t":
                    j += 1
            elif self.source[i] == '"':
                close = self.source.find('"', i + 1, end)
                if close >= 0 and not any(ch in " \t" for ch in self.source[i + 1 : close]):
                    j = close + 1
                else:
                    j = i + 1
                    while j < end and self.source[j] not in " \t\n":
                        j += 1
            elif self.source[i] == "#" and i + 1 < end and self.source[i + 1].isalpha():
                j = i + 2
                while j < end and (self.source[j].isalnum() or self.source[j] == "_"):
                    j += 1
            elif self.source[i].isalpha() or self.source[i] == "_":
                j = i + 1
                while j < end and (
                    self.source[j].isalnum() or self.source[j] == "_" or self.source[j] == "."
                ):
                    j += 1
                if j < end and self.source[j] == '"':
                    j += 1
            elif self.source[i].isdigit():
                j = i + 1
                while j < end and self.source[j].isdigit():
                    j += 1
            else:
                j = i + 1
            chunks.append(self.token_for_span(TokenKind.INACTIVECODE, i, j))
            i = j
        return chunks
