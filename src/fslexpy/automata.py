from __future__ import annotations

from collections import defaultdict, deque
from dataclasses import dataclass, field
from unicodedata import category

from .ast import (
    EOF,
    EPSILON,
    UNICODE_CATEGORIES,
    Input,
    InputKind,
    RegexKind,
    Regexp,
    Spec,
    decode_unicode_category,
    encode_unicode_category,
)


@dataclass
class NfaNode:
    id: int
    transitions: dict[int, list[NfaNode]] = field(default_factory=lambda: defaultdict(list))
    accepted: list[tuple[int, int]] = field(default_factory=list)


@dataclass
class DfaNode:
    id: int
    nfa_set: frozenset[int]
    transitions: dict[int, int] = field(default_factory=dict)
    accepted: list[tuple[int, int]] = field(default_factory=list)


@dataclass(frozen=True)
class RuleTables:
    name: str
    start_state: int
    action_sources: tuple[str, ...]


@dataclass(frozen=True)
class CompiledSpec:
    rules: tuple[RuleTables, ...]
    states: tuple[DfaNode, ...]
    unicode: bool


class NfaBuilder:
    def __init__(self) -> None:
        self.nodes: list[NfaNode] = []

    def new(self, accepted: list[tuple[int, int]] | None = None) -> NfaNode:
        node = NfaNode(len(self.nodes) + 1, accepted=accepted or [])
        self.nodes.append(node)
        return node


def alphabet_for_unicode() -> set[int]:
    return set(range(128)) | {encode_unicode_category(name) for name in UNICODE_CATEGORIES}


def expand_input(input_: Input, *, unicode: bool) -> frozenset[int]:
    if input_.kind == InputKind.CHAR:
        return frozenset([int(input_.value)])
    if input_.kind == InputKind.UNICODE_CATEGORY:
        if not unicode:
            raise ValueError("unicode category classes require unicode mode")
        return frozenset([encode_unicode_category(str(input_.value))])
    if input_.kind == InputKind.EOF:
        return frozenset([EOF])
    if input_.kind == InputKind.ANY:
        return frozenset(alphabet_for_unicode() if unicode else range(256))
    if input_.kind == InputKind.NOT_CHARSET:
        excluded = set(input_.value)  # type: ignore[arg-type]
        alphabet = alphabet_for_unicode() if unicode else set(range(256))
        if unicode:
            alphabet |= {encode_unicode_category(name) for name in UNICODE_CATEGORIES}
        return frozenset(alphabet - excluded)
    raise ValueError(input_.kind)


def compile_rule_regex(
    regexp: Regexp,
    dest: NfaNode,
    builder: NfaBuilder,
    macros: dict[str, Regexp],
    *,
    unicode: bool,
    case_insensitive: bool,
) -> NfaNode:
    if regexp.kind == RegexKind.ALT:
        start = builder.new()
        for item in regexp.value:  # type: ignore[union-attr]
            start.transitions[EPSILON].append(
                compile_rule_regex(
                    item,
                    dest,
                    builder,
                    macros,
                    unicode=unicode,
                    case_insensitive=case_insensitive,
                )
            )
        return start
    if regexp.kind == RegexKind.SEQ:
        node = dest
        for item in reversed(regexp.value):  # type: ignore[union-attr]
            node = compile_rule_regex(
                item, node, builder, macros, unicode=unicode, case_insensitive=case_insensitive
            )
        return node
    if regexp.kind == RegexKind.STAR:
        loop_dest = builder.new()
        loop_dest.transitions[EPSILON].append(dest)
        body = compile_rule_regex(
            regexp.value,
            loop_dest,
            builder,
            macros,
            unicode=unicode,
            case_insensitive=case_insensitive,
        )
        loop_dest.transitions[EPSILON].append(body)
        start = builder.new()
        start.transitions[EPSILON].extend([body, dest])
        return start
    if regexp.kind == RegexKind.MACRO:
        name = str(regexp.value)
        if name not in macros:
            raise ValueError(f"macro {name!r} is not defined")
        return compile_rule_regex(
            macros[name], dest, builder, macros, unicode=unicode, case_insensitive=case_insensitive
        )
    if regexp.kind == RegexKind.INP:
        start = builder.new()
        labels = expand_input(regexp.value, unicode=unicode)  # type: ignore[arg-type]
        for label in labels:
            for actual in case_variants(label, unicode=unicode, enabled=case_insensitive):
                start.transitions[actual].append(dest)
        return start
    raise ValueError(regexp.kind)


def case_variants(label: int, *, unicode: bool, enabled: bool) -> set[int]:
    if not enabled or label == EOF:
        return {label}
    uc = decode_unicode_category(label)
    if uc:
        if uc in {"Lu", "Ll", "Lt"}:
            return {encode_unicode_category(name) for name in ("Lu", "Ll", "Lt")}
        return {label}
    ch = chr(label)
    return {ord(ch.lower()), ord(ch.upper())}


def lexer_state_to_nfa(
    clauses,
    macros: dict[str, Regexp],
    *,
    unicode: bool,
    case_insensitive: bool,
) -> tuple[NfaNode, tuple[str, ...], list[NfaNode]]:
    builder = NfaBuilder()
    actions: list[str] = []
    start = builder.new()
    for clause_order, clause in enumerate(clauses):
        action_id = len(actions)
        actions.append(clause.code.text)
        accept = builder.new([(clause_order, action_id)])
        branch = compile_rule_regex(
            clause.regexp,
            accept,
            builder,
            macros,
            unicode=unicode,
            case_insensitive=case_insensitive,
        )
        start.transitions[EPSILON].append(branch)
    return start, tuple(actions), builder.nodes


def epsilon_closure(node_ids: set[int], nodes: dict[int, NfaNode]) -> frozenset[int]:
    stack = list(node_ids)
    result = set(node_ids)
    while stack:
        nid = stack.pop()
        for dest in nodes[nid].transitions.get(EPSILON, []):
            if dest.id not in result:
                result.add(dest.id)
                stack.append(dest.id)
    return frozenset(result)


def nfa_to_dfa(
    start: NfaNode, nfa_nodes: list[NfaNode], first_dfa_id: int
) -> tuple[int, list[DfaNode]]:
    nodes = {node.id: node for node in nfa_nodes}
    start_set = epsilon_closure({start.id}, nodes)
    by_set: dict[frozenset[int], DfaNode] = {}
    queue: deque[frozenset[int]] = deque()

    def get(nfa_set: frozenset[int]) -> DfaNode:
        if nfa_set not in by_set:
            accepted = []
            for nid in sorted(nfa_set):
                accepted.extend(nodes[nid].accepted)
            accepted.sort()
            by_set[nfa_set] = DfaNode(first_dfa_id + len(by_set), nfa_set, accepted=accepted)
            queue.append(nfa_set)
        return by_set[nfa_set]

    get(start_set)
    while queue:
        nfa_set = queue.popleft()
        dfa = by_set[nfa_set]
        moves: dict[int, set[int]] = defaultdict(set)
        for nid in nfa_set:
            for label, dests in nodes[nid].transitions.items():
                if label != EPSILON:
                    for dest in dests:
                        moves[label].add(dest.id)
        for label, dests in moves.items():
            dfa.transitions[label] = get(epsilon_closure(dests, nodes)).id
    return by_set[start_set].id, sorted(by_set.values(), key=lambda node: node.id)


def compile_spec(
    spec: Spec, *, unicode: bool = True, case_insensitive: bool = False
) -> CompiledSpec:
    macros = {macro.name: macro.regexp for macro in spec.macros}
    rules: list[RuleTables] = []
    all_states: list[DfaNode] = []
    next_state_id = 0
    for rule in reversed(spec.rules):
        nfa_start, actions, nfa_nodes = lexer_state_to_nfa(
            rule.clauses, macros, unicode=unicode, case_insensitive=case_insensitive
        )
        start_state, dfa_states = nfa_to_dfa(nfa_start, nfa_nodes, next_state_id)
        next_state_id += len(dfa_states)
        all_states.extend(dfa_states)
        rules.append(RuleTables(rule.name, start_state, actions))
    rules.reverse()
    return CompiledSpec(tuple(rules), tuple(sorted(all_states, key=lambda node: node.id)), unicode)


def classify_char(ch: str) -> int:
    code = ord(ch)
    if code < 128:
        return code
    cat = category(ch)
    if cat in UNICODE_CATEGORIES:
        return encode_unicode_category(cat)
    return code
