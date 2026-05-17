from __future__ import annotations

from collections.abc import Iterable, Iterator

from .tokens import Token, TokenKind

LAYOUT_OPENERS = {
    TokenKind.EQUALS,
    TokenKind.THEN,
    TokenKind.ELSE,
    TokenKind.DO,
    TokenKind.RARROW,
    TokenKind.WITH,
    TokenKind.FINALLY,
}


class LexFilter:
    """Stateful post-lexer filter.

    This mirrors the public purpose of FSharp.Compiler.LexFilter: convert a raw
    token stream into a parser-facing stream by inserting offside/layout tokens
    and marking a few context-sensitive tokens. It is intentionally conservative:
    it handles normal indentation blocks and leaves ambiguous parser-only
    transformations as raw tokens.
    """

    def __init__(self, tokens: Iterable[Token]) -> None:
        self.tokens = iter(tokens)
        self.pending: list[Token] = []
        self.indents: list[int] = [0]
        self.after_layout_opener = False
        self.prev: Token | None = None

    def __iter__(self) -> Iterator[Token]:
        while True:
            tok = self.get_token()
            yield tok
            if tok.kind == TokenKind.EOF:
                break

    def get_token(self) -> Token:
        if self.pending:
            return self.pending.pop(0)
        tok = next(self.tokens)
        if tok.kind == TokenKind.EOF:
            while len(self.indents) > 1:
                self.indents.pop()
                self.pending.append(Token(TokenKind.OBLOCKEND, tok.range, tok.range, ""))
            self.pending.append(tok)
            return self.pending.pop(0)

        self.apply_typeapp_marks(tok)
        self.apply_layout(tok)
        self.prev = tok
        if self.pending:
            self.pending.append(tok)
            return self.pending.pop(0)
        return tok

    def apply_typeapp_marks(self, tok: Token) -> None:
        if tok.kind == TokenKind.LESS:
            object.__setattr__(tok, "value", False)
        elif tok.kind == TokenKind.GREATER:
            object.__setattr__(tok, "value", False)

    def apply_layout(self, tok: Token) -> None:
        if tok.range is None:
            return
        col = tok.range.start.column
        if self.after_layout_opener:
            if col > self.indents[-1]:
                self.indents.append(col)
                self.pending.append(Token(TokenKind.OBLOCKBEGIN, None, tok.range, ""))
            self.after_layout_opener = False
        elif col < self.indents[-1] and tok.range.start.column >= 0:
            while len(self.indents) > 1 and col < self.indents[-1]:
                self.indents.pop()
                self.pending.append(Token(TokenKind.OBLOCKEND, None, tok.range, ""))
        elif col == self.indents[-1] and self.prev is not None:
            prev_line = self.prev.range.end.line if self.prev.range else tok.range.start.line
            if tok.range.start.line > prev_line:
                self.pending.append(Token(TokenKind.OBLOCKSEP, None, tok.range, ""))

        if tok.kind in LAYOUT_OPENERS:
            self.after_layout_opener = True
        if tok.kind == TokenKind.LET:
            object.__setattr__(tok, "kind", TokenKind.OLET)
        elif tok.kind == TokenKind.DO:
            object.__setattr__(tok, "kind", TokenKind.ODO)
        elif tok.kind == TokenKind.DO_BANG:
            object.__setattr__(tok, "kind", TokenKind.ODO_BANG)
        elif tok.kind == TokenKind.THEN:
            object.__setattr__(tok, "kind", TokenKind.OTHEN)
        elif tok.kind == TokenKind.ELSE:
            object.__setattr__(tok, "kind", TokenKind.OELSE)
        elif tok.kind == TokenKind.WITH:
            object.__setattr__(tok, "kind", TokenKind.OWITH)
        elif tok.kind == TokenKind.FUN:
            object.__setattr__(tok, "kind", TokenKind.OFUN)
        elif tok.kind == TokenKind.FUNCTION:
            object.__setattr__(tok, "kind", TokenKind.OFUNCTION)
