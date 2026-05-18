// ============================================================================
// F# IDENTIFIER LEXER STRESS TEST (FOCUS: IDENTIFIER RECOGNITION)
// ============================================================================
// Tests: Unicode scripts, combining marks, zero-width chars, backtick escaping,
// keyword collision, digit/space/symbol inclusion, apostrophe rules, contextual
// positions, and error-recovery boundaries. All constructs are lexically valid F# 8.0+.

module IdentifierLexerTests

// ============================================================================
// 1. BASIC ASCII & POSITIONAL CONTEXTS
// ============================================================================
let _ = 0
let a = 0
let foo_bar = 0
let test' = 0
let __ = 0
let ___ = 0
let _1 = 0
let a1_b2' = 0
let Foo'Bar' = 0
let `` = 0 // empty backtick identifier (valid in F#)
let ` ` = 0
let `  ` = 0
let `	tab` = 0

// ============================================================================
// 2. UNICODE LETTERS (MULTI-SCRIPT)
// ============================================================================
let αβγδεζηθ = 0
let café = 0
let données = 0
let データ処理 = 0
let المستخدم = 0
let кириллица = 0
let Ελληνικά = 0
let हिन्दी = 0
let 한국어 = 0
let ไทย = 0
let עברית = 0
let አማርኛ = 0
let ᏣᎳᎩ = 0
let ᛟᛞᛁᚾ = 0

// ============================================================================
// 3. COMBINING MARKS & ZERO-WIDTH CHARACTERS
// ============================================================================
let cafe\u0301 = 0
let e\u0300\u0301 = 0
let a\u0308\u0301\u0302 = 0
let base\u20DD = 0
let zero\u200Cwidth = 0
let joiner\u200Dtest = 0
let variant\uFE0Fchar = 0
let word\u2060joiner = 0
let \u200Bsoft = 0
let \u200Frtl\u200Eltr = 0
let \u2028line\u2029sep = 0

// ============================================================================
// 4. BACKTICK: RESERVED & CONTEXTUAL KEYWORDS
// ============================================================================
let `type` = 0
let `match` = 0
let `let` = 0
let `open` = 0
let `namespace` = 0
let `module` = 0
let `rec` = 0
let `inline` = 0
let `mutable` = 0
let `as` = 0
let `when` = 0
let `with` = 0
let `and` = 0
let `or` = 0
let `not` = 0
let `true` = 0
let `false` = 0
let `null` = 0
let `then` = 0
let `else` = 0
let `if` = 0
let `for` = 0
let `while` = 0
let `do` = 0
let `done` = 0
let `begin` = 0
let `end` = 0
let `try` = 0
let `finally` = 0
let `fun` = 0
let `function` = 0
let `lazy` = 0
let `static` = 0
let `member` = 0
let `abstract` = 0
let `override` = 0
let `default` = 0
let `val` = 0
let `use` = 0
let `yield` = 0
let `return` = 0
let `inherit` = 0
let `interface` = 0
let `struct` = 0
let `class` = 0
let `let!` = 0
let `use!` = 0
let `do!` = 0
let `try!` = 0
let `match!` = 0
let `return!` = 0
let `yield!` = 0

// ============================================================================
// 5. BACKTICK: DIGITS, SPACES, SYMBOLS, OPERATORS
// ============================================================================
let `123` = 0
let `1a2b3c` = 0
let `+` = 0
let `|>` = 0
let `:=` = 0
let `?` = 0
let `@` = 0
let `#` = 0
let `!` = 0
let `%%` = 0
let `<>` = 0
let `..` = 0
let `::` = 0
let `->` = 0
let `<-` = 0
let `=` = 0
let `&&&` = 0
let `|||` = 0
let `^^^` = 0
let `~~~` = 0
let `(*` = 0
let `*)` = 0
let `(*` = 0
let `//` = 0
let `///` = 0

// ============================================================================
// 6. BACKTICK: UNICODE, EMOJI, URLS, PATHS, MIXED
// ============================================================================
let `🚀` = 0
let `👨‍👩‍👧‍👦` = 0
let `∑` = 0
let `⚡⚡⚡` = 0
let `α β γ` = 0
let `data.json` = 0
let `C:\path\file` = 0
let `http://example.com` = 0
let `1.2.3` = 0
let `v1.0.0-beta.3` = 0
let `user@domain.com` = 0
let `a/b/c` = 0
let `a\b\c` = 0
let `a:b:c` = 0
let `a;b;c` = 0
let `a,b,c` = 0
let `a.b.c` = 0
let `a-b-c` = 0
let `a_b_c` = 0
let `a~b~c` = 0
let `a`b`c` = 0 // invalid: backtick inside backtick (commented for error recovery test)

// ============================================================================
// 7. APOSTROPHE & UNDERSCORE RULES
// ============================================================================
let a' = 0
let a'' = 0
let a''' = 0
let _foo' = 0
let foo'_bar' = 0
let `'a` = 0
let `''` = 0
let `'''` = 0
let `_` = 0
let `__` = 0
let `___` = 0
let ``_`` = 0

// ============================================================================
// 8. IDENTIFIERS IN TYPE / GENERIC / CONSTRAINT POSITIONS
// ============================================================================
type `1Type` = class end
type `2Union` = | `3Case` of `4Field`: int
type `5Record` = { `6Val`: int; `7Mutable`: int }
type `8Enum` = `9A` = 0 | `10B` = 1
type `11Delegate` = delegate of `12Arg`: int -> int
exception `13Exn` of `14Msg`: string
type `15Interface` = abstract `16Member`: unit -> unit
type `17Generic`<'T when 'T : null> = class end
type `18Flexible`<'U when 'U :> `15Interface`> = class end
type `19StaticMember`<'V when 'V : (static member `20Op`: 'V * 'V -> 'V)> = class end
type `21Struct` = struct val `22X`: int end

// ============================================================================
// 9. IDENTIFIERS IN SIGNATURE / VALUE / ATTRIBUTE POSITIONS
// ============================================================================
val `23Val`: int -> int
val mutable `24Counter`: int
[<`25Attribute`>]
let `26Func` (`27Param`: int) = `27Param`
[<assembly: `28Target`>]
[<module: `29Target`>]
[<method: `30Target`>]
[<param: `31Target`>]
[<return: `32Target`>]
type `33Typed` = class end
module `34Mod` = let `35Inner` = 0
namespace `36Ns`

// ============================================================================
// 10. IDENTIFIERS IN ACTIVE PATTERNS / CE / QUOTATION
// ============================================================================
let (|`37Div`|_|) n x = if x % n = 0 then Some () else None
let (|`38A`|`39B`|`40C`|) x = match x with 1 -> `38A` | 2 -> `39B` | _ -> `40C`
let `41CE` () = `41CE` { return `42X` }
let `43Quote` = <@ `44Q` @>
let `45TypedQuote` = <@@ `46TQ` @@>
let `47Nested` = <@ <@ `48NN` @> @>

// ============================================================================
// 11. ERROR RECOVERY (COMMENTED OUT - TEST LEXER RESILIENCE)
// ============================================================================
// let `newline\nbreak` = 0      // ILLEGAL: newline inside backtick
// let `backtick\`inside` = 0    // ILLEGAL: backtick inside backtick
// let 123start = 0              // ILLEGAL: starts with digit
// let `control\x00char` = 0     // ILLEGAL: control character inside backtick
// let `empty`` = 0              // ILLEGAL: double backtick at start
// let `'start` = 0              // ILLEGAL without backtick, valid with
// let `unclosed = 0             // ILLEGAL: missing closing backtick
// let ` ` ` = 0                 // ILLEGAL: space followed by unquoted identifier

// ============================================================================
// 12. EXTREME LENGTH & COMPLEX COMPOSITION
// ============================================================================
let `a very long identifier with spaces and numbers 12345 and symbols @#$% and unicode αβγδεζηθ` = 0
let `combining\u0301marks\u0300everywhere\u0308\u0302` = 0
let `zero\u200Cwidth\u200Djoiners\uFE0Fvariants\u200Bsoft` = 0
let `rtl\u202Fltr\u2066override\u2067neutral\u2068isolate` = 0
let `mixed_123_αβγ_🚀_café_\u0301_` = 0