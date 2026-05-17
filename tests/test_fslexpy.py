from pathlib import Path

from fslexpy.generator import generate_text
from fslexpy.parser import parse_fsl
from fslexpy.runtime import LexBuffer

MINI = """
{
module Mini
}

let digit = ['0'-'9']
let letter = ['A'-'Z'] | ['a'-'z']
let ident = letter (letter | digit)*

rule token = parse
 | ident { IDENT }
 | digit+ { INT }
 | "==" { EQEQ }
 | '=' { EQ }
 | eof { EOF }
"""


def test_parse_fsl_spec_shape():
    spec = parse_fsl(MINI)
    assert [macro.name for macro in spec.macros] == ["digit", "letter", "ident"]
    assert [rule.name for rule in spec.rules] == ["token"]
    assert len(spec.rules[0].clauses) == 5


def test_generated_table_longest_match_and_earliest_clause(tmp_path):
    output = tmp_path / "mini_tables.py"
    output.write_text(generate_text(MINI, module_name="mini_tables"))

    namespace = {}
    exec(output.read_text(), namespace)

    lexbuf = LexBuffer.from_string("abc123")
    assert namespace["interpret"]("token", lexbuf) == 0
    assert lexbuf.lexeme == "abc123"

    lexbuf = LexBuffer.from_string("==")
    assert namespace["interpret"]("token", lexbuf) == 2
    assert lexbuf.lexeme == "=="

    lexbuf = LexBuffer.from_string("=")
    assert namespace["interpret"]("token", lexbuf) == 3
    assert lexbuf.lexeme == "="


def test_generated_table_eof_rule(tmp_path):
    output = tmp_path / "mini_tables.py"
    output.write_text(generate_text(MINI, module_name="mini_tables"))

    namespace = {}
    exec(output.read_text(), namespace)

    lexbuf = LexBuffer.from_string("")
    assert namespace["interpret"]("token", lexbuf) == 4
    assert lexbuf.lexeme == ""


def test_official_fsharp_specs_are_generated():
    generated = Path("src/fsharp_full_lexer/generated")
    assert (generated / "lexer_tables.py").exists()
    assert (generated / "pplexer_tables.py").exists()
