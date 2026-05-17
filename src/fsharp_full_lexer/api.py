from __future__ import annotations

from collections.abc import Iterator

from .lexer import RawLexer
from .lexfilter import LexFilter
from .tokens import Token


def iter_tokens(
    source: str,
    *,
    defines=(),
    skip_trivia: bool = True,
    use_lex_filter: bool = True,
) -> Iterator[Token]:
    raw = RawLexer(source, defines=set(defines), skip_trivia=skip_trivia)
    if use_lex_filter:
        yield from LexFilter(raw)
    else:
        yield from raw


def tokenize(
    source: str,
    *,
    defines=(),
    skip_trivia: bool = True,
    use_lex_filter: bool = True,
) -> list[Token]:
    return list(
        iter_tokens(
            source,
            defines=defines,
            skip_trivia=skip_trivia,
            use_lex_filter=use_lex_filter,
        )
    )
