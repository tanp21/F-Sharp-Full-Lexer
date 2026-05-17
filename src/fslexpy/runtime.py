from __future__ import annotations

from collections.abc import Callable
from dataclasses import dataclass, field
from typing import Any
from unicodedata import category

from .ast import EOF, UNICODE_CATEGORIES, encode_unicode_category

SENTINEL = 65535


@dataclass(frozen=True)
class Position:
    line: int = 1
    column: int = 0
    absolute: int = 0
    filename: str = ""

    def end_of_token(self, text: str) -> Position:
        line = self.line
        column = self.column
        absolute = self.absolute
        for ch in text:
            absolute += 1
            if ch == "\n":
                line += 1
                column = 0
            else:
                column += 1
        return Position(line, column, absolute, self.filename)


@dataclass
class LexBuffer:
    text: str
    filename: str = ""
    index: int = 0
    start_pos: Position = field(default_factory=Position)
    end_pos: Position = field(default_factory=Position)
    lexeme: str = ""
    local_store: dict[str, Any] = field(default_factory=dict)
    is_past_end_of_stream: bool = False

    @classmethod
    def from_string(cls, text: str, filename: str = "") -> LexBuffer:
        pos = Position(filename=filename)
        return cls(text.replace("\r\n", "\n"), filename, 0, pos, pos)


Action = Callable[[LexBuffer], Any]


class TableLexer:
    def __init__(
        self,
        transitions: dict[int, dict[int, int]],
        actions: dict[int, int],
        action_handlers: dict[int, Action],
    ) -> None:
        self.transitions = transitions
        self.actions = actions
        self.action_handlers = action_handlers

    def interpret(self, start_state: int, lexbuf: LexBuffer) -> int:
        state = start_state
        index = lexbuf.index
        last_accept_action: int | None = None
        last_accept_index = index

        while True:
            action = self.actions.get(state, SENTINEL)
            if action != SENTINEL:
                last_accept_action = action
                last_accept_index = index
            label = EOF if index >= len(lexbuf.text) else classify(lexbuf.text[index])
            next_state = self.transitions.get(state, {}).get(label, SENTINEL)
            if next_state == SENTINEL:
                break
            if label == EOF:
                state = next_state
                action = self.actions.get(state, SENTINEL)
                if action != SENTINEL:
                    last_accept_action = action
                    last_accept_index = index
                lexbuf.is_past_end_of_stream = True
                break
            index += 1
            state = next_state

        if last_accept_action is None:
            raise ValueError("unrecognized input")
        lexbuf.start_pos = lexbuf.end_pos
        lexbuf.lexeme = lexbuf.text[lexbuf.index : last_accept_index]
        lexbuf.end_pos = lexbuf.start_pos.end_of_token(lexbuf.lexeme)
        lexbuf.index = last_accept_index
        return last_accept_action

    def run(self, start_state: int, lexbuf: LexBuffer) -> Any:
        action_id = self.interpret(start_state, lexbuf)
        return self.action_handlers[action_id](lexbuf)


def classify(ch: str) -> int:
    code = ord(ch)
    if code < 128:
        return code
    cat = category(ch)
    if cat in UNICODE_CATEGORIES:
        return encode_unicode_category(cat)
    return code
