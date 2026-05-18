from __future__ import annotations

import pytest

from fsharp_full_lexer.generate import generate_all


@pytest.fixture(scope="session", autouse=True)
def regenerate_lexer_tables() -> None:
    generate_all(check=False)
