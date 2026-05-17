from __future__ import annotations

from collections.abc import Callable
from dataclasses import dataclass
from typing import Any

import regex

from .position import Position, Range
from .tokens import Token, TokenKind

Action = Callable[["LexerRuntime", str, Range], Token | list[Token] | None]


@dataclass(frozen=True)
class Rule:
    name: str
    pattern: regex.Pattern[str]
    action: Action

    @classmethod
    def compile(cls, name: str, pattern: str, action: Action) -> Rule:
        return cls(name, regex.compile(pattern, regex.VERSION1), action)


class LexerRuntime:
    """Small fslex-style runtime: ordered longest-match rules and mutable lexbuf state."""

    def __init__(self, source: str, *, skip_trivia: bool = True, **options: Any) -> None:
        self.source = source.replace("\r\n", "\n")
        self.skip_trivia = skip_trivia
        self.options = options
        self.index = 0
        self.position = Position(0, 1, 0)
        self.pending: list[Token] = []
        self.string_nest: list[tuple[int, str, int]] = []

    def eof(self) -> bool:
        return self.index >= len(self.source)

    def range_for(self, text: str) -> Range:
        return Range(self.position, self.position.advance(text))

    def advance(self, text: str) -> Range:
        range_ = self.range_for(text)
        self.index += len(text)
        self.position = range_.end
        return range_

    def next_match(self, rules: list[Rule]) -> tuple[Rule, str] | None:
        best: tuple[int, int, Rule, str] | None = None
        for order, rule in enumerate(rules):
            match = rule.pattern.match(self.source, self.index)
            if not match:
                continue
            text = match.group(0)
            if text == "":
                continue
            candidate = (len(text), -order, rule, text)
            if best is None or candidate > best:
                best = candidate
        if best is None:
            return None
        return best[2], best[3]

    def scan(self, rules: list[Rule], eof_factory: Callable[[LexerRuntime], Token]) -> Token:
        if self.pending:
            return self.pending.pop(0)
        if self.eof():
            return eof_factory(self)

        match = self.next_match(rules)
        if match is None:
            text = self.source[self.index]
            range_ = self.advance(text)
            return Token(
                TokenKind.LEX_FAILURE,
                f"Unexpected character {text!r}",
                range_,
                text,
            )

        rule, text = match
        range_ = self.advance(text)
        result = rule.action(self, text, range_)
        if result is None:
            return self.scan(rules, eof_factory)
        if isinstance(result, list):
            self.pending.extend(result)
            return self.scan(rules, eof_factory)
        return result
