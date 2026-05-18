module `FSharp.Identifier.Lexer.StressTest.V`

// ============================================================================
// 1. PRIVATE USE AREA (PUA) & NON-CHARACTERS
// ============================================================================
let \uE000 = 0
let \uF8FF = 0
let `󠁀󠁁󠁂󠁃` = 0 // Supplementary PUA (U+100000+)
let \uFFFE = 0 // Non-character (lexically valid Unicode, impl may warn)
let \uFFFF = 0 // Non-character
let `\uFFFD` = 0 // Replacement char

// ============================================================================
// 2. VARIATION SELECTORS & GRAPHEME CLUSTERS
// ============================================================================
let a\uFE00 = 0
let b\uFE0F = 0
let c\uFE01 = 0
let d\uFE0E = 0
let e\uFE00\uFE01 = 0
let `👨‍👩‍👧‍👦‍🐶‍🐈` = 0 // Complex ZWJ sequence
let `🏳️‍🌈‍⚧️` = 0
let `♾️‍🔥` = 0

// ============================================================================
// 3. EXTREME PRIME & TYPE-VARIANT CHAINING
// ============================================================================
let a = 0 // Anchor for adjacency tests
let b = 0
let x' = 0
let x'' = 0
let x''' = 0
let x'''' = 0
let x''''' = 0
let x'''''' = 0
let x''''''' = 0
let x'''''''' = 0
let x''''''''' = 0
let x'''''''''' = 0
let `'a'` = 0
let `''b''` = 0
let `'''c'''` = 0
let `'T` = 0
let `^U` = 0
let `''V` = 0
let `'''W` = 0
let `'a'b` = 0
let `^T123` = 0

// ============================================================================
// 4. ADJACENCY BOUNDARIES (ZERO WHITESPACE)
// ============================================================================
let dotAdj = a.b
let pipeAdj = a|>b
let backPipeAdj = a<|b
let compAdj = a>>b
let revCompAdj = a<<b
let consAdj = a::[b]
let rangeAdj = a..b
let assignAdj = ref a
let testAdj = a:?b
let downcastAdj = a:?>b
let arrowAdj = a->b
let updateAdj = a<-b
let bitAndAdj = a&&&b
let bitOrAdj = a|||b
let bitXorAdj = a^^^b
let notAdj = a~~~b
let dynGet = a?b
let dynSet = a?<-b
let hashAdj = a#b
let atAdj = a@b
let dollarAdj = a$b
let modAdj = a%b
let caretAdj = a^b
let ampAdj = a&b
let mulAdj = a*b
let addAdj = a+b
let subAdj = a-b
let tildeAdj = a~b
let barAdj = a|b
let slashAdj = a/b

// ============================================================================
// 5. CONTROL CHARACTERS & WHITESPACE IN BACKTICKS
// ============================================================================
let `	tab` = 0 // U+0009 (Valid)
let `space ` = 0 // U+0020
let `nobreak\u00A0space` = 0 // U+00A0
let `en\u2002space` = 0 // U+2002
let `em\u2003space` = 0 // U+2003
let `thin\u2009space` = 0 // U+2009
let `hair\u200Aspace` = 0 // U+200A
let `zero\u200Bwidth` = 0 // U+200B (Cf, allowed in backticks)
let `word\u2060joiner` = 0 // U+2060

// ============================================================================
// 6. MATHEMATICAL & SYMBOLIC ALPHABETS
// ============================================================================
let `𝐀𝐁𝐂` = 0 // Bold
let `𝑎𝑏𝑐` = 0 // Italic
let `𝒜ℬ𝒞` = 0 // Script
let `𝔸𝔹ℂ` = 0 // Double-struck
let `𝕒𝕓𝕔` = 0 // Sans-serif
let `𝟎𝟏𝟐` = 0 // Bold digits (Nd, valid continue)
let `αβγ` = 0 // Greek
let `∑∏∫` = 0 // Math symbols (So, valid in backticks)
let `⊕⊗⊘` = 0
let `≠≤≥` = 0
let `→←↑↓` = 0
let `⟨⟩⟪⟫` = 0

// ============================================================================
// 7. CONTEXTUAL DISAMBIGUATION (TYPE / VALUE / PATTERN)
// ============================================================================
type `'T` = class end
type `^U when ^U : null` = class end
type `''V` = struct val mutable x: int end
type `'''W` = class end
let f1 (x: 'T) = x
let f2 (x: 'a'b) = x
let f3 (x: '^T) = x
let f4 (x: ''T) = x
let inline f5 (x: ^T when ^T : (static member Zero: ^T)) = ^T.Zero
let (|`Div`|_|) `N` `X` = if `X` % `N` = 0 then Some () else None
let (|`A`|`B`|`C`|) `Y` = match `Y` with 1 -> `A` | 2 -> `B` | _ -> `C`

// ============================================================================
// 8. HOMOGRAPH & VISUAL SPOOFING CLUSTERS
// ============================================================================
let `аrе_тhеѕе` = 0 // Cyrillic/Latin
let `ορτιс_ѕсаn` = 0 // Greek/Cyrillic
let `еxесutе` = 0
let `раrѕе` = 0
let `𝐚` = 0
let `𝐴` = 0
let `𝕒` = 0
let `𝔸` = 0
let `𝟎` = 0
let `𝟘` = 0
let `🄰` = 0
let `🅰` = 0

// ============================================================================
// 9. EXTREME LENGTH & MIXED COMPOSITION
// ============================================================================
let `a very long identifier with spaces, numbers 0123456789, symbols @#$%^&*(), unicode letters αβγδεζηθικλμνξοπρστυφχψω, mathematical symbols 𝐀𝐁𝐂, combining marks e\u0301\u0302\u0308, zero-width chars \u200C\u200D\u2060, primes x''''''''', backticks, tabs, and newlines are forbidden` = 0
let `🌍🌎🌏🌐🗺️🧭🌕🌖🌗🌘🌑🌒🌓🌔🌙🌚🌛🌜🌡️☀️🌝🌞⭐🌟🌠🌌☁️⛅⛈️🌤️🌥️🌦️🌧️🌨️🌩️🌪️🌫️🌬️🌀🌈🌂☂️☔⚡❄️🔥💧🌊` = 0

// ============================================================================
// 10. ERROR RECOVERY BOUNDARIES (UNCOMMENT TO TEST LEXER ROBUSTNESS)
// ============================================================================
// let `newline\nbreak` = 0       // INVALID: U+000A forbidden in backticks
// let `cr\rreturn` = 0           // INVALID: U+000D forbidden
// let `null\u0000char` = 0       // INVALID: U+0000 forbidden
// let `backtick\`break` = 0      // INVALID: unescaped ` breaks token
// let `control\x01\x02\x03` = 0  // INVALID: C0 controls forbidden
// let `'prime_start` = 0         // INVALID: bare `'` cannot start value identifier
// let `123digit_start` = 0       // INVALID: bare digit cannot start identifier
// let `unclosed_quote = 0        // INVALID: missing closing `
// let `a``b` = 0                // INVALID: premature termination