from __future__ import annotations

from dataclasses import dataclass

import regex


@dataclass(frozen=True)
class IfdefExpr:
    op: str
    value: str | None = None
    left: IfdefExpr | None = None
    right: IfdefExpr | None = None


class PPParser:
    def __init__(self, text: str) -> None:
        self.tokens = regex.findall(r"#if|#elif|[A-Za-z_][\p{L}\p{N}_']*|&&|\|\||!|\(|\)", text)
        self.index = 0
        if self.tokens and self.tokens[0] in {"#if", "#elif"}:
            self.index = 1

    def peek(self) -> str | None:
        return self.tokens[self.index] if self.index < len(self.tokens) else None

    def pop(self) -> str:
        tok = self.peek()
        if tok is None:
            raise SyntaxError("unexpected end of preprocessor expression")
        self.index += 1
        return tok

    def parse(self) -> IfdefExpr:
        expr = self.parse_or()
        return expr

    def parse_or(self) -> IfdefExpr:
        expr = self.parse_and()
        while self.peek() == "||":
            self.pop()
            expr = IfdefExpr("or", left=expr, right=self.parse_and())
        return expr

    def parse_and(self) -> IfdefExpr:
        expr = self.parse_not()
        while self.peek() == "&&":
            self.pop()
            expr = IfdefExpr("and", left=expr, right=self.parse_not())
        return expr

    def parse_not(self) -> IfdefExpr:
        if self.peek() == "!":
            self.pop()
            return IfdefExpr("not", left=self.parse_not())
        return self.parse_atom()

    def parse_atom(self) -> IfdefExpr:
        tok = self.pop()
        if tok == "(":
            expr = self.parse_or()
            if self.pop() != ")":
                raise SyntaxError("missing ')'")
            return expr
        if tok in {"&&", "||", ")"}:
            raise SyntaxError(f"unexpected token {tok!r}")
        return IfdefExpr("id", tok)


def parse_ifdef(text: str) -> IfdefExpr:
    return PPParser(text).parse()


def eval_ifdef(expr: IfdefExpr, defines: set[str]) -> bool:
    if expr.op == "id":
        return bool(expr.value in defines)
    if expr.op == "not":
        return not eval_ifdef(expr.left, defines)  # type: ignore[arg-type]
    if expr.op == "and":
        return eval_ifdef(expr.left, defines) and eval_ifdef(expr.right, defines)  # type: ignore[arg-type]
    if expr.op == "or":
        return eval_ifdef(expr.left, defines) or eval_ifdef(expr.right, defines)  # type: ignore[arg-type]
    raise ValueError(expr.op)
