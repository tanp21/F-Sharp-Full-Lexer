module IdentifierTests

// =========================
// 1. Standard Identifiers
// =========================

let simple = 1
let _underscoreStart = 2
let withDigits123 = 3
let with_apostrophe' = 4
let multiple''apostrophes = 5
let camelCaseIdentifier = 6
let PascalCaseIdentifier = 7

// Unicode letters (valid in F#)
let café = 8
let πValue = 9
let 变量 = 10

// =========================
// 2. Type Parameter Identifiers
// =========================

type Container<'T> = { Value: 'T }

type Pair<'Key, 'Value> =
    { Key: 'Key
      Value: 'Value }

let inline identity<'T> (x: 'T) = x

// Edge: apostrophe inside name (still valid)
type Weird<'TValue'> = { Inner: 'TValue' }

// =========================
// 3. Double-Backtick Identifiers
// =========================

let ``identifier with spaces`` = 1
let ``123 starts with digits`` = 2
let ``symbols !@#$%^&*()`` = 3
let ``keyword let`` = 4
let ``mixed 'quotes' and symbols`` = 5

// Using them
let result =
    ``identifier with spaces`` +
    ``123 starts with digits`` +
    ``symbols !@#$%^&*()`` +
    ``keyword let`` +
    ``mixed 'quotes' and symbols``

// =========================
// 4. Mixed Usage
// =========================

let normal = 10
let ``strange name`` = 20

let combined = normal + ``strange name``

// =========================
// 5. Negative / Edge Cases
// (Some of these may compile, but should NOT match strict regex)
// =========================

// Combining character (your lexer allows, spec regex does not clearly allow)
let é = 1   // 'e' + combining accent

// Formatting character (invisible)
let invisible‍char = 2

// Connector punctuation beyond underscore (may be accepted by your lexer)
let weird‿connector = 3

// =========================
// 6. Should FAIL in F# (commented out)
// =========================

// let 123invalid = 0              // cannot start with digit
// let has space = 1               // invalid without backticks
// let ``bad``name`` = 2           // invalid double backtick nesting
// let ' = 3                       // invalid type parameter


module IdentifierLexerEdgeCases

// ============================================
// 1. Keyword vs Identifier
// ============================================

// Reserved keywords cannot be used normally
// (should tokenize as KEYWORD, not IDENTIFIER)

// let let = 1
// let module = 2
// let type = 3

// But quoted identifiers ARE allowed
let ``let`` = 1
let ``module`` = 2
let ``type`` = 3
let ``match`` = 4
let ``namespace`` = 5

// Contextual keyword
let ``select`` = 6

// Future reserved keywords
let ``atomic`` = 7
let ``break`` = 8

// ============================================
// 2. Long Identifiers
// ============================================

let aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa = 1

let veryLongIdentifierWithNumbers123456789AndQuotes'''''''''''''''''''''''''''''''''''''''''''''''' = 2

// ============================================
// 3. Apostrophe Edge Cases
// ============================================

// Valid apostrophe placements
let a' = 1
let a'' = 2
let a''' = 3

let identifier''''with''''many''''quotes = 4

// Type-style names but actually normal identifiers
let 'notValid = 5      // should FAIL in real F#
                        // useful lexer test

// ============================================
// 4. Unicode Stress Tests
// ============================================

// Greek
let α = 1
let βγδεζηθ = 2

// Cyrillic
let Привет = 3

// Japanese
let こんにちは = 4

// Korean
let 안녕하세요 = 5

// Arabic
let مرحبا = 6

// Hebrew
let שלום = 7

// Mathematical Unicode letters
let 𝐀𝐁𝐂 = 8

// ============================================
// 5. Unicode Normalization Tests
// ============================================

// NFC form
let café = 1

// NFD form (different byte sequence)
let café = 2

// Lexer should treat both as identifiers,
// even though they are different Unicode sequences.

// ============================================
// 6. Connector / Formatting Characters
// ============================================

// These are excellent lexer edge cases because
// Unicode categories become tricky.

// UNDERTIE (connector punctuation)
let hello‿world = 1

// ZERO WIDTH JOINER
let joiner‍test = 2

// ZERO WIDTH NON-JOINER
let nonjoiner‌test = 3

// LEFT-TO-RIGHT MARK
let direction‎test = 4

// ============================================
// 7. Numeric Boundary Cases
// ============================================

// Digits after start are valid
let x1 = 1
let x123456789 = 2

// Unicode digits
let value١٢٣ = 3

// Invalid starts
// let 1abc = 1
// let ٩value = 2

// ============================================
// 8. Double Backtick Torture Tests
// ============================================

let `` `` = 1
let ``   `` = 2
let ``	`` = 3      // tab inside quoted identifier
                     // should FAIL according to regex

let ``line
break`` = 4          // should FAIL

let ``symbols ~!@#$%^&*()-=+[]{};:,.<>/?`` = 5

let ``Unicode π 变量 Привет مرحبا`` = 6

let ``contains ` single backtick`` = 7

// Invalid nested backticks
// let ``bad `` nested`` = 8

// ============================================
// 9. Lexer Boundary Tests
// ============================================

let abc=1
let abc =1
let abc= 1

let x+y = 1
let x'+y' = 2

// Ensure lexer separates:
// IDENTIFIER OP IDENTIFIER

// ============================================
// 10. Dot-Separated Long Identifiers
// ============================================

module A.B.C.D.E

let System.Console.WriteLine = 1

// Lexer should tokenize:
// IDENTIFIER DOT IDENTIFIER DOT ...

// ============================================
// 11. Generic Type Parameter Edge Cases
// ============================================

type Box<'T> = { Value : 'T }

type Pair<'Key,'Value> =
    { Key : 'Key
      Value : 'Value }

type Strange<'TValue''''> =
    { Inner : 'TValue'''' }

// Invalid generic identifiers

// type Bad<'123> = int
// type Bad<' space> = int
// type Bad<''> = int

// ============================================
// 12. Maximal Munch Tests
// ============================================

// Lexer must consume the LONGEST valid identifier

let abc'''def123ghi = 1

// Should become ONE identifier token

let ``abc def``ghi = 2

// Should tokenize as:
// QUOTED_IDENTIFIER IDENTIFIER

// ============================================
// 13. EOF Edge Cases
// ============================================

let endsImmediatelyAfterIdentifier=1
let trailingQuote'
let ``unfinished

// useful for testing lexer recovery/error handling

// ============================================
// 14. Invalid Unicode Surrogates
// ============================================

// These may not even parse in source files,
// but are useful if testing raw lexer streams.

// lone high surrogate
// \uD800

// lone low surrogate
// \uDC00

// ============================================
// 15. Reserved Operator Confusion
// ============================================

let mutable' = 1
let rec' = 2
let function' = 3

// should be identifiers, NOT keywords

// ============================================
// 16. Realistic F# Library Naming
// ============================================

let map = 1
let foldBack = 2
let asyncWorkflow = 3
let taskBuilder = 4
let valueOption = 5
let tryParse = 6
let ofSeq = 7
let toArray = 8

module ComprehensiveIdentifierEdgeCases

// ============================================
// 1. Standard Identifier - Basic Valid Cases
// ============================================

let a = 1
let A = 2
let abc = 3
let ABC = 4
let camelCase = 5
let PascalCase = 6
let snake_case = 7
let _underscore = 8
let __doubleUnderscore = 9
let with123digits = 10
let a1b2c3 = 11

// ============================================
// 2. Standard Identifier - Apostrophe Cases
// ============================================

let a' = 1
let a'' = 2
let a''' = 3

let identifier' = 4
let identifier'' = 5

let abc'def = 6
let abc''def = 7
let abc'''def123ghi = 8

let x'''''''''''''''''''''''''''''''' = 9

// ============================================
// 3. Standard Identifier - Unicode Letters
// ============================================

// Latin extended
let café = 1
let naïve = 2
let Ångström = 3

// Greek
let π = 4
let αβγδε = 5

// Cyrillic
let Привет = 6

// Chinese
let 变量 = 7

// Japanese
let こんにちは = 8

// Korean
let 안녕하세요 = 9

// Arabic
let مرحبا = 10

// Hebrew
let שלום = 11

// Mathematical unicode letters
let 𝐀𝐁𝐂 = 12

// ============================================
// 4. Standard Identifier - Unicode Edge Cases
// ============================================

// Combining mark (visually valid, regex may reject)
let é = 1

// Zero-width joiner
let invisible‍char = 2

// Zero-width non-joiner
let invisible‌char2 = 3

// Connector punctuation
let weird‿connector = 4

// Direction mark
let direction‎mark = 5

// ============================================
// 5. Standard Identifier - Numeric Edge Cases
// ============================================

let x1 = 1
let x123456789 = 2
let value١٢٣ = 3

// Invalid starts
// let 1abc = 1
// let ٩value = 2

// ============================================
// 6. Standard Identifier - Longest Match
// ============================================

// Lexer should consume ENTIRE identifier

let abc123def456ghi789 = 1
let long''''''''''''identifier''''''''''''test123 = 2

// ============================================
// 7. Standard Identifier - Keyword Confusion
// ============================================

// Should be IDENTIFIER, not KEYWORD

let let' = 1
let module' = 2
let type' = 3
let mutable' = 4
let function' = 5
let rec' = 6
let match' = 7

// ============================================
// 8. Standard Identifier - Invalid Cases
// ============================================

// let has space = 1
// let has-hyphen = 2
// let has.dot = 3
// let has+plus = 4
// let has/slash = 5
// let has:semicolon = 6
// let has:semicolon; = 7
// let @invalid = 8
// let #invalid = 9
// let ?invalid = 10

// ============================================
// 9. Type Parameter Identifiers - Basic Cases
// ============================================

type Box<'T> =
    { Value : 'T }

type Pair<'Key,'Value> =
    { Key : 'Key
      Value : 'Value }

type Triple<'A,'B,'C> =
    { A : 'A
      B : 'B
      C : 'C }

// ============================================
// 10. Type Parameter Identifiers - Apostrophe Cases
// ============================================

type Weird<'T'> =
    { Value : 'T' }

type VeryWeird<'T''''> =
    { Value : 'T'''' }

type Complex<'LongTypeParameter123''''> =
    { Value : 'LongTypeParameter123'''' }

// ============================================
// 11. Type Parameter Identifiers - Unicode Cases
// ============================================

type Greek<'π> =
    { Value : 'π }

type Chinese<'变量> =
    { Value : '变量 }

type Cyrillic<'Привет> =
    { Value : 'Привет }

// ============================================
// 12. Type Parameter Identifiers - Invalid Cases
// ============================================

// type Bad<'123> = int
// type Bad<' space> = int
// type Bad<''> = int
// type Bad<'-invalid> = int
// type Bad<'invalid-type> = int

// ============================================
// 13. Double-Backtick Identifiers - Basic Cases
// ============================================

let ``simple`` = 1
let ``identifier with spaces`` = 2
let ``123 starts with digits`` = 3
let ``symbols !@#$%^&*()`` = 4
let ``mixed 'quotes' and symbols`` = 5

// ============================================
// 14. Double-Backtick Identifiers - Keyword Cases
// ============================================

let ``let`` = 1
let ``module`` = 2
let ``type`` = 3
let ``match`` = 4
let ``function`` = 5
let ``namespace`` = 6
let ``select`` = 7
let ``const`` = 8
let ``break`` = 9

// ============================================
// 15. Double-Backtick Identifiers - Unicode Cases
// ============================================

let ``π value`` = 1
let ``变量 名称`` = 2
let ``Привет мир`` = 3
let ``مرحبا بالعالم`` = 4

// ============================================
// 16. Double-Backtick Identifiers - Symbol Cases
// ============================================

let ``+`` = 1
let ``-`` = 2
let ``*`` = 3
let ``/`` = 4
let ``->`` = 5
let ``=>`` = 6
let ``===`` = 7

let ``a+b-c*d/e`` = 8

// ============================================
// 17. Double-Backtick Identifiers - Backtick Edge Cases
// ============================================

// Single backtick inside is valid
let ``contains ` single backtick`` = 1

// Invalid nested backticks
// let ``bad `` nested`` = 2
// let ``bad``name`` = 3

// ============================================
// 18. Double-Backtick Identifiers - Whitespace Edge Cases
// ============================================

let `` `` = 1
let ``   `` = 2

// Invalid according to strict regex
// let ``line
// break`` = 3

// let ``tab	inside`` = 4

// ============================================
// 19. Dot-Separated Long Identifiers
// ============================================

module A.B.C.D.E

let System.Console.WriteLine = 1

// Lexer should tokenize:
// IDENTIFIER DOT IDENTIFIER DOT ...

// ============================================
// 20. Maximal Munch Tests
// ============================================

// Lexer must consume the LONGEST valid identifier

let abc'''def123ghi = 1

// Should become ONE identifier token

let ``abc def``ghi = 2

// Should tokenize as:
// QUOTED_IDENTIFIER IDENTIFIER

// ============================================
// 21. EOF Edge Cases
// ============================================

let endsImmediatelyAfterIdentifier=1
let trailingQuote'
let ``unfinished

// useful for testing lexer recovery/error handling

// ============================================
// 22. Invalid Unicode Surrogates
// ============================================

// These may not even parse in source files,
// but are useful if testing raw lexer streams.

// lone high surrogate
// \uD800

// lone low surrogate
// \uDC00

// ============================================
// 23. Realistic F# Naming Patterns
// ============================================

let map = 1
let foldBack = 2
let asyncWorkflow = 3
let taskBuilder = 4
let valueOption = 5
let tryParse = 6
let ofSeq = 7
let toArray = 8
let defaultArg = 9
let choose = 10
let collect = 11
let bindAsync = 12
let resultBuilder = 13

// ============================================
// 24. Boundary / Token Separation Cases
// ============================================

let abc=1
let abc =1
let abc= 1

let x+y = 1
let x'+y' = 2

// Ensure lexer separates:
// IDENTIFIER OP IDENTIFIER

// ============================================
// 25. Stress Tests
// ============================================

let aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa = 1

let veryLongIdentifierWithNumbers123456789AndQuotes'''''''''''''''''''''''''''''''''''''''''''''''' = 2

let ``very very very very very very very very very very long quoted identifier with spaces and symbols !@#$%^&*()`` = 3