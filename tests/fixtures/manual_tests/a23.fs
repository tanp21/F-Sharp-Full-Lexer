// ============================================================================
// F# IDENTIFIER LEXER STRESS TEST II (FOCUS: RECOGNITION & BOUNDARIES)
// ============================================================================
// Tests: Unicode start/continue rules, prime/apostrophe chaining, NFC/NFD traps,
// surrogate pairs, zero-width/formatting chars, backtick boundary precision,
// type-variable vs string-qualifier disambiguation, and extreme composition.
// All uncommented constructs are lexically valid F# 8.0+.

module `FSharp.Identifier.Lexer.StressTest.II`

// ============================================================================
// 1. ASCII, PRIME CHAINING & UNDERSCORE VARIANTS
// ============================================================================
let _ = 0
let __ = 0
let ___ = 0
let _a = 0
let a_ = 0
let a__b = 0
let ` _ ` = 0
let ` _ ` = 0
let x' = 0
let x'' = 0
let x''' = 0
let x'''' = 0
let x''''' = 0
let a'b = 0
let a''b = 0
let a'''b = 0
let `x'` = 0
let `'a` = 0
let `''` = 0
let `'''` = 0
let `a'b'c'` = 0
let `'T` = 0 // bare type var start
let `^T` = 0 // static member constraint var

// ============================================================================
// 2. UNICODE START CATEGORIES (Lu, Ll, Lt, Lm, Lo, Nl)
// ============================================================================
let Α = 0 // U+0391 (Lu)
let α = 0 // U+03B1 (Ll)
let ᾼ = 0 // U+1FBC (Ll with iota subscript)
let Ǆ = 0 // U+01C4 (Lt)
let ʰ = 0 // U+02B0 (Lm)
let 㐀 = 0 // U+3400 (Lo CJK)
let ٠ = 0 // U+0660 (Nd) - NOTE: Nl can start, Nd cannot
let 〇 = 0 // U+3007 (Nl)
let ℵ = 0 // U+2135 (Lo)
let ℹ = 0 // U+2139 (Lo)
let Ⲁ = 0 // U+2C80 (Ll Coptic)
let ꓮ = 0 // U+A4EE (Lo Lisu)
let 🅰 = 0 // U+1F170 (So) - invalid start alone, valid in backticks

// ============================================================================
// 3. UNICODE CONTINUE CATEGORIES (Nd, Mn, Mc, Cf, Pc)
// ============================================================================
let a0 = 0
let a123 = 0
let a\u0300 = 0 // Combining grave (Mn)
let a\u0301\u0302 = 0 // Combining acute + circumflex
let a\u0E31 = 0 // Thai Mai Han-Akat (Mn)
let a\u0903 = 0 // Devanagari Visarga (Mc)
let a\u200C = 0 // ZWNJ (Cf)
let a\u200D = 0 // ZWJ (Cf)
let a\uFEFF = 0 // BOM (Cf)
let a\u2060 = 0 // Word Joiner (Cf)
let a_ = 0 // U+005F (Pc)
let a\u203F = 0 // U+203F (Pc Undertie)
let a\u2040 = 0 // U+2040 (Pc Character Tie)
let a\uFE33 = 0 // U+FE33 (Pc Presentation Form)
let a\uFE34 = 0 // U+FE34 (Pc Presentation Form)

// ============================================================================
// 4. NFC vs NFD NORMALIZATION TRAPS
// ============================================================================
let café = 0          // NFC: U+00E9
let cafe\u0301 = 0    // NFD: U+0065 U+0301
let naïve = 0         // NFC
let nai\u0308ve = 0   // NFD
let ﬁle = 0           // Ligature U+FB01 (f+i)
let file = 0          // Decomposed
let ﬄight = 0        // Ligature U+FB04 (f+f+l)
let fflight = 0       // Decomposed
let Ångström = 0      // NFC
let A\u030Angstro\u0308m = 0 // NFD

// ============================================================================
// 5. BACKTICK: KEYWORDS, OPERATORS, PUNCTUATION & SPACES
// ============================================================================
let `type` = 0
let `let!` = 0
let `match` = 0
let `->` = 0
let `<-` = 0
let `:=` = 0
let `|>` = 0
let `||>` = 0
let `<||` = 0
let `..` = 0
let `::` = 0
let `..>` = 0
let `->` = 0
let `<-` = 0
let `!` = 0
let `?` = 0
let `??` = 0
let `?<-` = 0
let `(*` = 0
let `*)` = 0
let `//` = 0
let `///` = 0
let `(**)` = 0
let `@` = 0
let `#` = 0
let `$` = 0
let `%` = 0
let `^` = 0
let `&` = 0
let `*` = 0
let `+` = 0
let `-` = 0
let `=` = 0
let `~` = 0
let `|` = 0
let `\` = 0
let `:` = 0
let `;` = 0
let `,` = 0
let `.` = 0
let `/` = 0
let ` ` = 0
let `  ` = 0
let `	tab` = 0
let `a b c` = 0
let ` 1 2 3 ` = 0

// ============================================================================
// 6. BACKTICK: EMOJI, SURROGATES, RTL/LTR, FORMATTING
// ============================================================================
let `🚀` = 0
let `👨‍👩‍👧‍👦` = 0
let `🇺🇸` = 0
let `🔟` = 0
let `⌨️` = 0
let `🏳️‍🌈` = 0
let `αβγ` = 0
let `∑∏∫` = 0
let `⊕⊗⊘` = 0
let `≠≤≥≈` = 0
let `→←↑↓` = 0
let `⇐⇒⇑⇓` = 0
let `⟨⟩⟪⟫` = 0
let `⟦⟧⟨⟩` = 0
let `🔤🔡🔠` = 0
let `‏RTL‪LRE‬PDF` = 0
let `‎LTR‫RLE‭RLI‮LRI` = 0
let `⁰¹²³⁴⁵⁶⁷⁸⁹` = 0
let `⁽⁾⁺⁻` = 0
let `₀₁₂₃₄₅₆₇₈₉` = 0
let `₊₋₌` = 0

// ============================================================================
// 7. TYPE VARIABLES & STATIC RESOLUTION CONSTRAINTS
// ============================================================================
type `'T` = class end
type `'_` = class end
type `'a'b` = class end
type `'T123` = class end
type `^T` = class end
type `^U` = class end
type `^V when ^V : (static member (+): ^V * ^V -> ^V)` = class end
type `'W when 'W : null` = class end
type `''X` = class end // double prime type var (byref in some contexts)
type `'''Y` = class end
let f1 (x: 'T) = x
let f2 (x: 'a'b) = x
let f3 (x: '^T) = x
let f4 (x: ''T) = x
let inline f5 (x: ^T when ^T : (static member Zero: ^T)) = ^T.Zero

// ============================================================================
// 8. IDENTIFIERS IN TYPE DEFINITIONS (RECORDS, UNIONS, INTERFACES)
// ============================================================================
type `1Record` = { `2Field`: int; `3 Mutable`: int }
type `4Union` = | `5Case` of `6Data`: string | `7Empty`
type `8Enum` = `9A` = 0 | `10 B` = 1
type `11Interface` =
    abstract `12Member`: unit -> unit
    abstract `13Property`: int with get, set
type `14Delegate` = delegate of `15Arg`: obj -> bool
exception `16Exception` of `17Message`: string
type `18Struct` = struct val `19X`: int end
type `20Module` = begin static let `21Val` = 0 end

// ============================================================================
// 9. IDENTIFIERS IN PATTERNS, ACTIVE PATTERNS & CE
// ============================================================================
let `22Func` `23Param` = `23Param`
let (|`24Div`|_|) `25N` `26X` = if `26X` % `25N` = 0 then Some () else None
let (|`27A`|`28B`|`29C`|) `30X` = match `30X` with 1 -> `27A` | 2 -> `28B` | _ -> `29C`
let `31CE` () = `31CE` {
    let! `32Bound` = Some 1
    use! `33Res` = new System.IO.MemoryStream()
    do! System.Threading.Tasks.Task.Delay 10
    try! System.Threading.Tasks.Task.FromResult 42
    with | _ -> 0
    match! System.Threading.Tasks.Task.FromResult "ok" with
    | "ok" -> "s" | _ -> "f"
    return! System.Threading.Tasks.Task.FromResult `32Bound`
}
let `34Quote` = <@ `35Q` @>
let `36Typed` = <@@ `37TQ` @@>

// ============================================================================
// 10. ATTRIBUTES, SIGNATURES & MODULE QUALIFIERS
// ============================================================================
[<`38Attribute`>]
let `39Func` (`40Arg`: int) : `40Arg` = `40Arg`
[<assembly: `41Target`>]
[<module: `42Target`>]
[<method: `43Target`>]
[<param: `44Target`>]
[<return: `45Target`>]
val `46Signature`: int -> int
val mutable `47State`: int
module `48Inner` = let `49Val` = 0
namespace `50Outer`

// ============================================================================
// 11. EXTREME COMPOSITION & LENGTH
// ============================================================================
let `a very long identifier with spaces and numbers 12345 and symbols @#$% and unicode αβγδεζηθ and combining marks café\u0301 and zero width \u200C\u200D and formatting \u2060\uFEFF and primes a'b'c' and underscores a__b and ligatures ﬂight` = 0
let `🔥🚀🌍🌊🌋⚡️💧🌬️🌪️🌈` = 0
let `ΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟΠΡΣΤΥΦΧΨΩ` = 0
let `אבגדהוזחטיכלמנסעפצקרשת` = 0
let `กขคงจฉชซญฎฏฐฑฒณดตถทธนบปผฝพฟภมยรลวศษสหอฮ` = 0
let `가나다라마바사아자차카타파하` = 0
let `᚛᚜ᚐᚑᚒᚓᚔᚕᚖᚗᚘᚙᚚ᚛` = 0
let `ꓲꓳꓴꓵꓶꓷꓸꓹꓺꓻꓼꓽ꓾꓿` = 0

// ============================================================================
// 12. ERROR RECOVERY BOUNDARIES (COMMENTED FOR LEXER TESTING)
// ============================================================================
// let 123start = 0              // INVALID: bare identifier cannot start with digit
// let `newline\nbreak` = 0      // INVALID: U+000A not allowed in backticks
// let `carriage\rreturn` = 0    // INVALID: U+000D not allowed in backticks
// let `null\u0000char` = 0      // INVALID: control characters forbidden
// let `backtick\`inside` = 0    // INVALID: unescaped backtick breaks token
// let `open quote` = 0          // INVALID: missing closing backtick
// let `'` = 0                   // INVALID: bare prime not valid identifier start
// let `''` = 0                  // VALID in backticks, but lexer must distinguish from type var syntax
// let `a`b` = 0                 // INVALID: premature backtick termination
// let `control\x01\x02` = 0     // INVALID: C0 controls forbidden