from __future__ import annotations

from dataclasses import dataclass, field

from .position import Range


@dataclass(frozen=True)
class Diagnostic:
    code: str
    message: str
    range: Range | None = None
    severity: str = "error"


@dataclass
class DiagnosticSink:
    diagnostics: list[Diagnostic] = field(default_factory=list)

    def error(self, code: str, message: str, range_: Range | None = None) -> None:
        self.diagnostics.append(Diagnostic(code, message, range_, "error"))

    def warning(self, code: str, message: str, range_: Range | None = None) -> None:
        self.diagnostics.append(Diagnostic(code, message, range_, "warning"))
