// ============================================================================
// F# IDENTIFIER LEXER STRESS TEST IV (FOCUS: BOUNDARIES & UNICODE STRICTNESS)
// ============================================================================
// Targets: Unicode category compliance, combining mark stacking, grapheme clusters,
// surrogate pairs, bidirectional format controls, homoglyphs, prime chaining,
// backtick boundary precision, type-variable vs identifier disambiguation,
// contextual keyword overloading, and error-recovery traps.
// All uncommented constructs are lexically valid F# 8.0+.

module `FSharp.Identifier.Lexer.StressTest.IV`

// ============================================================================
// 1. UNICODE CATEGORY COMPLIANCE (START / CONTINUE RULES)
// ============================================================================
// Start: Lu, Ll, Lt, Lm, Lo, Nl, '_'
let `Α` = 0      // Lu
let `α` = 0      // Ll
let `Ǆ` = 0      // Lt
let `ʰ` = 0      // Lm
let `㐀` = 0      // Lo (CJK)
let `〇` = 0      // Nl (Ideographic Number)
let `_` = 0      // U+005F (Connector)
// Continue: + Nd, Mn, Mc, Cf, Pc
let `a0` = 0     // Nd
let `e\u0301` = 0// Mn (Combining Acute)
let `a\u0903` = 0// Mc (Devanagari Visarga)
let `a\u200C` = 0// Cf (ZWNJ)
let `a_` = 0     // Pc
let `a\u203F` = 0// Pc (Undertie)

// ============================================================================
// 2. COMBINING MARK STACKING & GRAPHEME CLUSTERS
// ============================================================================
let `base\u0300\u0301\u0302\u0303\u0304\u0305\u0306\u0307\u0308` = 0
let `i\u0307\u0301\u0302\u0303\u0304\u0306\u0308\u030A\u030B` = 0
let `o\u0327\u0301\u0308\u0311\u0312\u0313\u0314\u0315` = 0
let `cluster\u034F\u035C\u035D\u035E\u035F` = 0 // CGJ + below marks
let `stack\u1AB0\u1AB1\u1AB2\u1AB3\u1AB4` = 0 // Extended below
let `above\u1DC0\u1DC1\u1DC2\u1DC3\u1DC4` = 0 // Extended above

// ============================================================================
// 3. SUPPLEMENTARY PLANES & SURROGATE PAIRS
// ============================================================================
let `\U00010000` = 0 // Linear B Syllable B008 A (Lm)
let `\U0001D400` = 0 // Mathematical Bold Capital A (Lu)
let `\U0001F600` = 0 // Grinning Face (So) - valid in backticks
let `\U0001F170` = 0 // Negative Squared Latin Capital Letter A (So)
let `👨‍👩‍👧‍👦` = 0   // ZWJ sequence (4 code points, 1 grapheme)
let `🏳️‍🌈` = 0      // Rainbow flag (ZWJ)
let `🔟` = 0         // Keycap 10 (ZWJ)
let `🇺🇸` = 0        // US Flag (Regional Indicators)

// ============================================================================
// 4. BIDIRECTIONAL & FORMAT CONTROL CHARACTERS (CF CATEGORY)
// ============================================================================
let `\u200E` = 0     // LRM
let `\u200F` = 0     // RLM
let `\u202A` = 0     // LRE
let `\u202B` = 0     // RLE
let `\u202C` = 0     // PDF
let `\u202D` = 0     // LRO
let `\u202E` = 0     // RLO
let `\u2066` = 0     // LRI
let `\u2067` = 0     // RLI
let `\u2068` = 0     // FSI
let `\u2069` = 0     // PDI
let `\u061C` = 0     // ALM
let `\u200B` = 0     // ZWSP
let `\u2060` = 0     // WJ
let `\uFEFF` = 0     // BOM/ZWNJ
let `\u200C\u200D\u2060\uFEFF` = 0 // Mixed controls

// ============================================================================
// 5. PRIME CHAINING & TYPE-VARIABLE DISAMBIGUATION
// ============================================================================
let `x'` = 0
let `x''` = 0
let `x'''` = 0
let `x''''` = 0
let `x'''''` = 0
let `x''''''` = 0
let `x'''''''` = 0
let `x''''''''` = 0
let `'T` = 0        // Bare type variable (lexer emits TYPEVAR)
let `^U` = 0        // Bare static param (lexer emits STATICTYPEVAR)
let `''V` = 0       // Double prime (byref context)
let `'''W` = 0      // Triple prime
let `'a'b` = 0      // Mixed type var + identifier
let `^T123` = 0     // Static param with digits
let `'T when 'T : null` = 0 // Contextual keyword collision

// ============================================================================
// 6. BACKTICK: KEYWORDS, OPERATORS & PUNCTUATION ESCAPING
// ============================================================================
let `type` = 0
let `let!` = 0
let `match` = 0
let `with` = 0
let `when` = 0
let `as` = 0
let `and` = 0
let `or` = 0
let `not` = 0
let `true` = 0
let `false` = 0
let `null` = 0
let `|>` = 0
let `<|` = 0
let `>>` = 0
let `<<` = 0
let `::` = 0
let `..` = 0
let `:=` = 0
let `:?` = 0
let `:?>` = 0
let `->` = 0
let `<-` = 0
let `=` = 0
let `#` = 0
let `@` = 0
let `$` = 0
let `%` = 0
let `^` = 0
let `&` = 0
let `*` = 0
let `+` = 0
let `-` = 0
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
let `1 2 3` = 0
let `(* not comment *)` = 0
let `// not line comment` = 0
let `/// not xml doc` = 0

// ============================================================================
// 7. HOMOGLYPHS & VISUAL CONFUSABLES
// ============================================================================
let `аrе_тhеѕе` = 0  // Cyrillic а(U+0430), е(U+0435), т(U+0442), ѕ(U+0455)
let `ορτιс_ѕсаn` = 0  // Greek ο(U+03BF), ρ(U+03C1), τ(U+03C4), ι(U+03B9), с(U+0441), ѕ(U+0455)
let `еxесutе` = 0     // Mixed Cyrillic/Latin
let `раrѕе` = 0       // Cyrillic р(U+0440), а(U+0430), ѕ(U+0455)
let `𝐚` = 0           // U+1D41A (Mathematical Bold Small A)
let `𝐴` = 0           // U+1D434 (Mathematical Italic Capital A)
let `𝕒` = 0           // U+1D552 (Mathematical Double-Struck Small A)
let `𝔸` = 0           // U+1D538 (Mathematical Double-Struck Capital A)
let `𝟎` = 0           // U+1D7CE (Mathematical Bold Digit Zero)
let `𝟘` = 0           // U+1D7D8 (Mathematical Double-Struck Digit Zero)
let `🄰` = 0           // U+1F130 (Squared Latin Capital Letter A)
let `🅰` = 0           // U+1F170 (Negative Squared Latin Capital Letter A)

// ============================================================================
// 8. IDENTIFIERS IN TYPE, CONSTRAINT & SIGNATURE POSITIONS
// ============================================================================
type `1Type`<'T when 'T : null and 'T : unmanaged> = class end
type `2Union`<'U when 'U :> `1Type`<'U>> = | `3Case` of `4Data`: 'U
type `5Record`<'V when 'V : equality and 'V : comparison> = { `6Field`: 'V }
type `7Interface` = abstract `8Member`: unit -> unit
type `9Delegate` = delegate of `10Arg`: obj -> bool
exception `11Error` of `12Msg`: string
type `13Struct` = struct val `14X`: int; val mutable `15Y`: float end
type `16Module` = begin static let `17Val` = 0 end
val `18Signature`: int -> string -> `1Type`<int>
val mutable `19State`: int
[<assembly: `20Target`>]
[<module: `21Target`>]
[<method: `22Target`>]
[<param: `23Target`>]
[<return: `24Target`>]
[<type: `25Target`>]

// ============================================================================
// 9. ACTIVE PATTERNS, COMPUTATION EXPRESSIONS & QUOTATIONS
// ============================================================================
let (|`26AP`|_|) `27N` `28X` = if `28X` % `27N` = 0 then Some () else None
let (|`29A`|`30B`|`31C`|) `32X` = match `32X` with 1 -> `29A` | 2 -> `30B` | _ -> `31C`
type `33CEBuilder` () =
    member _.Bind(x, f) = f x
    member _.Return x = x
    member _.Zero () = ()
    member _.Delay f = f
let `33CE` = `33CEBuilder` ()
let `34CEUsage` () = `33CE` {
    let! `35Bound` = Some 1
    use! `36Res` = new System.IO.MemoryStream()
    do! System.Threading.Tasks.Task.Delay 10
    try! System.Threading.Tasks.Task.FromResult 42
    with | _ -> 0
    match! System.Threading.Tasks.Task.FromResult "ok" with
    | "ok" -> "s" | _ -> "f"
    return! System.Threading.Tasks.Task.FromResult `35Bound`
}
let `37Quote` = <@ `38Q` @>
let `39Typed` = <@@ `40TQ` @@>
let `41Nested` = <@ <@ `42NN` @> @>

// ============================================================================
// 10. EXTREME COMPOSITION & MULTI-SCRIPT MIXING
// ============================================================================
let `αβγ_𝐀𝐚_e\u0301\u0302_\u200C\u200D_x'''_café_🚀_аrе_тhеѕе_∑∏∫_𝟎𝟘_Ǆ_ʰ_㐀_〇_a0_a\u0301_a\u0903_a\u200C_a_\u203F` = 0
let `🌍🌎🌏🌐🗺️🧭🌕🌖🌗🌘🌑🌒🌓🌔🌙🌚🌛🌜🌡️☀️🌝🌞⭐🌟🌠🌌☁️⛅⛈️🌤️🌥️🌦️🌧️🌨️🌩️🌪️🌫️🌬️🌀🌈🌂☂️☔⚡❄️🔥💧🌊` = 0
let `᚛᚜ᚐᚑᚒᚓᚔᚕᚖᚗᚘᚙᚚ᚛_𐀀𐀁𐀂𐀃𐀄𐀅𐀆𐀇𐀈𐀉𐀊𐀋𐀌𐀍𐀎𐀏_𐌰𐌱𐌲𐌳𐌴𐌵𐌶𐌷𐌸𐌹𐌺𐌻𐌼𐌽𐌾𐌿_ꓲꓳꓴꓵꓶꓷꓸꓹꓺꓻꓼꓽ꓾꓿` = 0

// ============================================================================
// 11. ERROR RECOVERY BOUNDARIES (COMMENTED FOR LEXER TESTING)
// ============================================================================
// let `newline\nbreak` = 0       // INVALID: U+000A forbidden in backticks
// let `cr\rreturn` = 0           // INVALID: U+000D forbidden
// let `null\u0000char` = 0       // INVALID: U+0000 forbidden
// let `backtick\`inside` = 0     // INVALID: unescaped ` breaks token
// let `control\x01\x02\x03` = 0  // INVALID: C0 controls (U+0001-U+001F) forbidden
// let `control\x7F\x80\x9F` = 0  // INVALID: DEL & C1 controls forbidden
// let `'prime_start` = 0         // INVALID: bare `'` cannot start identifier (needs backticks or type context)
// let `123digit_start` = 0       // INVALID: bare digit cannot start identifier
// let `unclosed_quote = 0        // INVALID: missing closing `
// let `a``b` = 0                // INVALID: premature termination
// let `tab\t` = 0                // VALID: U+0009 is explicitly allowed
// let `space ` = 0               // VALID: U+0020 is allowed in backticks