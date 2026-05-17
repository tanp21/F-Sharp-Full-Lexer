from __future__ import annotations

from dataclasses import dataclass

from .ast import Clause, Code, Input, InputKind, Macro, Regexp, Rule, RuleArgument, Spec


@dataclass(frozen=True)
class Tok:
    kind: str
    value: object
    line: int
    column: int


class FslSyntaxError(SyntaxError):
    pass


class FslScanner:
    def __init__(self, text: str, filename: str = "<string>") -> None:
        self.text = text.replace("\r\n", "\n")
        self.filename = filename
        self.index = 0
        self.line = 1
        self.column = 0

    def eof(self) -> bool:
        return self.index >= len(self.text)

    def peek(self, n: int = 0) -> str:
        i = self.index + n
        return "" if i >= len(self.text) else self.text[i]

    def startswith(self, s: str) -> bool:
        return self.text.startswith(s, self.index)

    def advance(self, s: str) -> None:
        for ch in s:
            self.index += 1
            if ch == "\n":
                self.line += 1
                self.column = 0
            else:
                self.column += 1

    def skip_ws_and_comments(self) -> None:
        while not self.eof():
            if self.peek() in " \t\n":
                self.advance(self.peek())
            elif self.startswith("//"):
                while not self.eof() and self.peek() != "\n":
                    self.advance(self.peek())
            elif self.startswith("(*"):
                self.advance("(*")
                depth = 1
                while not self.eof() and depth:
                    if self.startswith("(*"):
                        depth += 1
                        self.advance("(*")
                    elif self.startswith("*)"):
                        depth -= 1
                        self.advance("*)")
                    else:
                        self.advance(self.peek())
            else:
                return

    def code_block(self) -> Tok:
        line, column = self.line, self.column
        self.advance("{")
        depth = 1
        out: list[str] = []
        while not self.eof() and depth:
            if self.startswith("//"):
                while not self.eof() and self.peek() != "\n":
                    out.append(self.peek())
                    self.advance(self.peek())
            elif self.startswith("(*"):
                out.append("(*")
                self.advance("(*")
                comment_depth = 1
                while not self.eof() and comment_depth:
                    if self.startswith("(*"):
                        comment_depth += 1
                        out.append("(*")
                        self.advance("(*")
                    elif self.startswith("*)"):
                        comment_depth -= 1
                        out.append("*)")
                        self.advance("*)")
                    else:
                        out.append(self.peek())
                        self.advance(self.peek())
            elif self.peek() == "'":
                out.append(self.peek())
                self.advance("'")
                while not self.eof():
                    ch = self.peek()
                    out.append(ch)
                    self.advance(ch)
                    if ch == "\\" and not self.eof():
                        out.append(self.peek())
                        self.advance(self.peek())
                    elif ch == "'":
                        break
            elif self.peek() == '"':
                out.append(self.peek())
                self.advance('"')
                while not self.eof():
                    ch = self.peek()
                    out.append(ch)
                    self.advance(ch)
                    if ch == "\\" and not self.eof():
                        out.append(self.peek())
                        self.advance(self.peek())
                    elif ch == '"':
                        break
            elif self.peek() == "{":
                depth += 1
                out.append("{")
                self.advance("{")
            elif self.peek() == "}":
                depth -= 1
                if depth:
                    out.append("}")
                self.advance("}")
            else:
                out.append(self.peek())
                self.advance(self.peek())
        if depth:
            raise FslSyntaxError(f"unterminated code block at {line}:{column}")
        return Tok("CODE", Code("".join(out), line, column), line, column)

    def string(self) -> Tok:
        line, column = self.line, self.column
        self.advance('"')
        out: list[str] = []
        while not self.eof():
            ch = self.peek()
            if ch == '"':
                self.advance('"')
                return Tok("STRING", "".join(out), line, column)
            if ch == "\\":
                self.advance("\\")
                esc = self.peek()
                self.advance(esc)
                out.append({"n": "\n", "t": "\t", "b": "\b", "r": "\r"}.get(esc, esc))
            else:
                out.append(ch)
                self.advance(ch)
        raise FslSyntaxError(f"unterminated string at {line}:{column}")

    def char_or_category(self) -> Tok:
        line, column = self.line, self.column
        self.advance("'")
        is_unicode_category = (
            self.peek() == "\\"
            and self.peek(1).isupper()
            and self.peek(2).islower()
            and self.peek(3) == "'"
        )
        if is_unicode_category:
            self.advance("\\")
            value = self.peek() + self.peek(1)
            self.advance(value)
            self.advance("'")
            return Tok("UNICODE_CATEGORY", value, line, column)
        if self.peek() == "\\":
            self.advance("\\")
            esc = self.peek()
            self.advance(esc)
            if esc == "x":
                digits = self.text[self.index : self.index + 2]
                self.advance(digits)
                value = chr(int(digits, 16))
            elif esc == "u":
                digits = self.text[self.index : self.index + 4]
                self.advance(digits)
                value = chr(int(digits, 16))
            elif esc == "U":
                digits = self.text[self.index : self.index + 8]
                self.advance(digits)
                value = chr(int(digits, 16))
            elif esc.isdigit():
                digits = esc + self.text[self.index : self.index + 2]
                self.advance(digits[1:])
                value = chr(int(digits, 10))
            else:
                value = {"n": "\n", "t": "\t", "b": "\b", "r": "\r"}.get(esc, esc)
        else:
            value = self.peek()
            self.advance(value)
        if self.peek() != "'":
            raise FslSyntaxError(f"unterminated char literal at {line}:{column}")
        self.advance("'")
        return Tok("CHAR", value, line, column)

    def ident(self) -> Tok:
        line, column = self.line, self.column
        start = self.index
        while not self.eof() and (self.peek().isalnum() or self.peek() in "_'"):
            self.advance(self.peek())
        value = self.text[start : self.index]
        keywords = {"rule": "RULE", "parse": "PARSE", "let": "LET", "and": "AND", "eof": "EOF"}
        return Tok(keywords.get(value, "IDENT"), value, line, column)

    def next(self) -> Tok:
        self.skip_ws_and_comments()
        if self.eof():
            return Tok("EOF_INPUT", "", self.line, self.column)
        ch = self.peek()
        if ch == "{":
            return self.code_block()
        if ch == '"':
            return self.string()
        if ch == "'":
            return self.char_or_category()
        if ch.isalpha():
            return self.ident()
        mapping = {
            "|": "BAR",
            ".": "DOT",
            "+": "PLUS",
            "*": "STAR",
            "?": "QMARK",
            "=": "EQUALS",
            "_": "UNDERSCORE",
            "[": "LBRACK",
            "]": "RBRACK",
            "(": "LPAREN",
            ")": "RPAREN",
            ":": "COLON",
            "^": "HAT",
            "-": "DASH",
        }
        if ch in mapping:
            self.advance(ch)
            return Tok(mapping[ch], ch, self.line, self.column - 1)
        raise FslSyntaxError(f"unexpected character {ch!r} at {self.line}:{self.column}")


class FslParser:
    def __init__(self, text: str, filename: str = "<string>") -> None:
        self.scanner = FslScanner(text, filename)
        self.current = self.scanner.next()
        self.clause_index = 0

    def bump(self) -> Tok:
        tok = self.current
        self.current = self.scanner.next()
        return tok

    def accept(self, kind: str) -> Tok | None:
        if self.current.kind == kind:
            return self.bump()
        return None

    def expect(self, kind: str) -> Tok:
        tok = self.accept(kind)
        if tok is None:
            raise FslSyntaxError(
                f"expected {kind}, got {self.current.kind} "
                f"at {self.current.line}:{self.current.column}"
            )
        return tok

    def parse(self) -> Spec:
        top = self.parse_codeopt()
        macros: list[Macro] = []
        while self.current.kind == "LET":
            macros.append(self.parse_macro())
        self.expect("RULE")
        rules = [self.parse_rule()]
        while self.accept("AND"):
            rules.append(self.parse_rule())
        bottom = self.parse_codeopt()
        self.expect("EOF_INPUT")
        return Spec(top, tuple(macros), tuple(rules), bottom)

    def parse_codeopt(self) -> Code:
        if self.current.kind == "CODE":
            return self.bump().value  # type: ignore[return-value]
        return Code("", self.current.line, self.current.column)

    def parse_macro(self) -> Macro:
        self.expect("LET")
        name = self.expect("IDENT").value
        self.expect("EQUALS")
        return Macro(str(name), self.parse_regexp())

    def parse_rule(self) -> Rule:
        name = str(self.expect("IDENT").value)
        args: list[RuleArgument] = []
        while self.current.kind in {"IDENT", "LPAREN"}:
            if self.accept("LPAREN"):
                arg = str(self.expect("IDENT").value)
                self.expect("COLON")
                typ = str(self.expect("IDENT").value)
                self.expect("RPAREN")
                args.append(RuleArgument(arg, typ))
            else:
                args.append(RuleArgument(str(self.expect("IDENT").value)))
        self.expect("EQUALS")
        self.expect("PARSE")
        self.accept("BAR")
        clauses = [self.parse_clause()]
        while self.accept("BAR"):
            clauses.append(self.parse_clause())
        return Rule(name, tuple(args), tuple(clauses))

    def parse_clause(self) -> Clause:
        regexp = self.parse_regexp()
        code = self.expect("CODE").value
        clause = Clause(regexp, code, self.clause_index)  # type: ignore[arg-type]
        self.clause_index += 1
        return clause

    def parse_regexp(self, min_prec: int = 0) -> Regexp:
        left = self.parse_postfix()
        while True:
            if self.current.kind == "BAR" and min_prec <= 1:
                self.bump()
                left = Regexp.alt([left, self.parse_regexp(2)])
            elif self.starts_atom(self.current.kind) and min_prec <= 2:
                left = Regexp.seq([left, self.parse_regexp(3)])
            else:
                return left

    def parse_postfix(self) -> Regexp:
        expr = self.parse_atom()
        while self.current.kind in {"PLUS", "STAR", "QMARK"}:
            kind = self.bump().kind
            if kind == "STAR":
                expr = Regexp.star(expr)
            elif kind == "PLUS":
                expr = Regexp.seq([expr, Regexp.star(expr)])
            else:
                expr = Regexp.alt([Regexp.seq([]), expr])
        return expr

    def parse_atom(self) -> Regexp:
        tok = self.current
        if tok.kind == "CHAR":
            self.bump()
            return Regexp.inp(Input(InputKind.CHAR, ord(str(tok.value))))
        if tok.kind == "UNICODE_CATEGORY":
            self.bump()
            return Regexp.inp(Input(InputKind.UNICODE_CATEGORY, str(tok.value)))
        if tok.kind == "EOF":
            self.bump()
            return Regexp.inp(Input(InputKind.EOF))
        if tok.kind == "UNDERSCORE":
            self.bump()
            return Regexp.inp(Input(InputKind.ANY))
        if tok.kind == "STRING":
            self.bump()
            return Regexp.seq([Regexp.inp(Input(InputKind.CHAR, ord(ch))) for ch in str(tok.value)])
        if tok.kind == "IDENT":
            self.bump()
            return Regexp.macro(str(tok.value))
        if tok.kind == "LPAREN":
            self.bump()
            expr = self.parse_regexp()
            self.expect("RPAREN")
            return expr
        if tok.kind == "LBRACK":
            return self.parse_charset_atom()
        raise FslSyntaxError(f"expected regexp atom, got {tok.kind} at {tok.line}:{tok.column}")

    def parse_charset_atom(self) -> Regexp:
        self.expect("LBRACK")
        negated = bool(self.accept("HAT"))
        chars = self.parse_charset()
        self.expect("RBRACK")
        if negated:
            return Regexp.inp(Input(InputKind.NOT_CHARSET, frozenset(chars)))
        return Regexp.alt([Regexp.inp(Input(InputKind.CHAR, ch)) for ch in sorted(chars)])

    def parse_charset(self) -> set[int]:
        chars: set[int] = set()
        while self.current.kind == "CHAR":
            start = ord(str(self.expect("CHAR").value))
            if self.accept("DASH"):
                end = ord(str(self.expect("CHAR").value))
                chars.update(range(start, end + 1))
            else:
                chars.add(start)
        return chars

    @staticmethod
    def starts_atom(kind: str) -> bool:
        return kind in {
            "CHAR",
            "UNICODE_CATEGORY",
            "EOF",
            "UNDERSCORE",
            "STRING",
            "IDENT",
            "LPAREN",
            "LBRACK",
        }


def parse_fsl(text: str, filename: str = "<string>") -> Spec:
    return FslParser(text, filename).parse()
