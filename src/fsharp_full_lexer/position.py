from __future__ import annotations

from dataclasses import dataclass


@dataclass(frozen=True, order=True)
class Position:
    index: int
    line: int
    column: int

    def advance(self, text: str) -> Position:
        line = self.line
        column = self.column
        for ch in text:
            if ch == "\n":
                line += 1
                column = 0
            else:
                column += 1
        return Position(self.index + len(text), line, column)


@dataclass(frozen=True)
class Range:
    start: Position
    end: Position

    @property
    def start_line(self) -> int:
        return self.start.line

    @property
    def start_column(self) -> int:
        return self.start.column
