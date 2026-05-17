from __future__ import annotations

import struct
from decimal import Decimal


def remove_underscores(text: str) -> str:
    return text.replace("_", "")


def parse_based_int(text: str) -> int:
    s = remove_underscores(text)
    sign = -1 if s.startswith("-") else 1
    if s[0] in "+-":
        s = s[1:]
    if len(s) >= 2 and s[0] == "0" and s[1].lower() in {"x", "o", "b"}:
        base = {"x": 16, "o": 8, "b": 2}[s[1].lower()]
        return sign * int(s[2:], base)
    return sign * int(s, 10)


def checked_signed(value: int, bits: int) -> tuple[int, bool]:
    min_v = -(1 << (bits - 1))
    max_v = (1 << (bits - 1)) - 1
    if value == 1 << (bits - 1):
        return min_v, True
    if not min_v <= value <= max_v:
        raise OverflowError(value)
    return value, False


def checked_unsigned(value: int, bits: int) -> int:
    if not 0 <= value <= (1 << bits) - 1:
        raise OverflowError(value)
    return value


def parse_float64(text: str) -> float:
    return float(remove_underscores(text))


def parse_float32(text: str) -> float:
    value = float(remove_underscores(text[:-1]))
    return struct.unpack("<f", struct.pack("<f", value))[0]


def parse_decimal(text: str) -> Decimal:
    return Decimal(remove_underscores(text[:-1]))


ESCAPES = {
    "\\": "\\",
    '"': '"',
    "'": "'",
    "a": "\a",
    "f": "\f",
    "v": "\v",
    "n": "\n",
    "t": "\t",
    "b": "\b",
    "r": "\r",
}


def decode_escape(seq: str) -> str:
    if len(seq) == 2 and seq[0] == "\\" and seq[1] in ESCAPES:
        return ESCAPES[seq[1]]
    if seq.startswith("\\x"):
        return chr(int(seq[2:], 16))
    if seq.startswith("\\u"):
        return chr(int(seq[2:], 16))
    if seq.startswith("\\U"):
        return chr(int(seq[2:], 16))
    if seq.startswith("\\") and len(seq) == 4 and seq[1:].isdigit():
        return chr(int(seq[1:], 10))
    raise ValueError(seq)
