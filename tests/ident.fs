module IdentifierTests


// 1. Standard Identifiers

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

// 2. Double-Backtick Identifiers

let ``identifier with spaces`` = 1
let ``123 starts with digits`` = 2
let ``symbols !@#$%^&*()`` = 3
let ``keyword let`` = 4
let ``mixed 'quotes' and symbols`` = 5

// 3. Should FAIL in F# (commented out)

let let = 1                     // reserved keyword
let 123invalid = 0              // cannot start with digit
let has space = 1               // invalid without backticks
let ``bad``name`` = 2           // invalid double backtick nesting