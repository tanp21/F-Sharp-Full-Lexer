module IdentifierTests

let let = 1

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
let ' = 3                       // invalid type parameter