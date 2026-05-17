from __future__ import annotations

from collections.abc import Callable
from dataclasses import dataclass
from enum import Enum

EOF = 0xFFFF_FFFE
EPSILON = 0xFFFF_FFFF
UNICODE_CATEGORY_BASE = 0xFFFF_FF00

UNICODE_CATEGORIES = [
    "Pe",
    "Pc",
    "Cc",
    "Sc",
    "Pd",
    "Nd",
    "Me",
    "Pf",
    "Cf",
    "Pi",
    "Nl",
    "Zl",
    "Ll",
    "Sm",
    "Lm",
    "Sk",
    "Mn",
    "Ps",
    "Lo",
    "Cn",
    "No",
    "Po",
    "So",
    "Zp",
    "Co",
    "Zs",
    "Mc",
    "Cs",
    "Lt",
    "Lu",
]


def encode_unicode_category(name: str) -> int:
    return UNICODE_CATEGORY_BASE + UNICODE_CATEGORIES.index(name)


def decode_unicode_category(value: int) -> str | None:
    index = value - UNICODE_CATEGORY_BASE
    if 0 <= index < len(UNICODE_CATEGORIES):
        return UNICODE_CATEGORIES[index]
    return None


class InputKind(Enum):
    CHAR = "char"
    UNICODE_CATEGORY = "unicode_category"
    ANY = "any"
    NOT_CHARSET = "not_charset"
    EOF = "eof"


@dataclass(frozen=True)
class Input:
    kind: InputKind
    value: int | str | frozenset[int] | None = None


class RegexKind(Enum):
    ALT = "alt"
    SEQ = "seq"
    INP = "inp"
    STAR = "star"
    MACRO = "macro"


@dataclass(frozen=True)
class Regexp:
    kind: RegexKind
    value: object

    @staticmethod
    def alt(items: list[Regexp]) -> Regexp:
        return Regexp(RegexKind.ALT, tuple(items))

    @staticmethod
    def seq(items: list[Regexp]) -> Regexp:
        flat: list[Regexp] = []
        for item in items:
            if item.kind == RegexKind.SEQ:
                flat.extend(item.value)  # type: ignore[arg-type]
            else:
                flat.append(item)
        return Regexp(RegexKind.SEQ, tuple(flat))

    @staticmethod
    def inp(input_: Input) -> Regexp:
        return Regexp(RegexKind.INP, input_)

    @staticmethod
    def star(item: Regexp) -> Regexp:
        return Regexp(RegexKind.STAR, item)

    @staticmethod
    def macro(name: str) -> Regexp:
        return Regexp(RegexKind.MACRO, name)


@dataclass(frozen=True)
class Code:
    text: str
    line: int
    column: int


@dataclass(frozen=True)
class RuleArgument:
    name: str
    type_name: str | None = None


@dataclass(frozen=True)
class Clause:
    regexp: Regexp
    code: Code
    index: int


@dataclass(frozen=True)
class Rule:
    name: str
    args: tuple[RuleArgument, ...]
    clauses: tuple[Clause, ...]


@dataclass(frozen=True)
class Macro:
    name: str
    regexp: Regexp


@dataclass(frozen=True)
class Spec:
    top_code: Code
    macros: tuple[Macro, ...]
    rules: tuple[Rule, ...]
    bottom_code: Code


CharSetFactory = Callable[[bool], frozenset[int]]
