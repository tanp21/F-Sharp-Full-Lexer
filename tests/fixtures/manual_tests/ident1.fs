module AllIdentifierTests

open System

// =====================================
// 1. Standard Identifiers
// =====================================

let simple = 1
let _underscore = 2
let a1b2c3 = 3
let with_apostrophe' = 4
let ``multiple
apostrophes`` = 5
let camelCase = 6
let PascalCase = 7

// Long identifier
let thisIsAnExtremelyLongIdentifierNameDesignedToStressLexer1234567890'abc = 8

// =====================================
// 2. Unicode Identifiers
// =====================================

let café = 1
let πValue = 2
let 变量 = 3
let привет = 4
let こんにちは = 5
let مرحبا = 6

// Mixed Unicode
let αβγ123abc = 7
let naïve_value = 8

// Combining marks (edge)
let é = 9
let å = 10

// Formatting (invisible)
let hidden‍joiner = 11

// Connector punctuation beyond underscore
let weird‿connector = 12

// =====================================
// 3. Apostrophe Edge Cases
// =====================================

let a'''''''' = 1
let a'b'c'd = 2
let x1'2'3 = 3

// =====================================
// 4. Type Parameters
// =====================================

type Container<'T> = { Value: 'T }

type Pair<'Key,'Value> =
    { Key: 'Key
      Value: 'Value }

let inline identity<'T> (x:'T) = x

type Weird<'TValue'''> = { Inner: 'TValue''' }

// =====================================
// 5. Double-Backtick Identifiers
// =====================================

let ``simple`` = 1
let ``with space`` = 2
let ``123 starts with digits`` = 3
let ``symbols !@#$%^&*()`` = 4
let ``keyword let`` = 5
let ``mixed 'quotes' and symbols`` = 6

let backtickSum =
    ``simple`` +
    ``with space`` +
    ``123 starts with digits``

// Backtick torture
let ``a`b`c`d`` = 10

// =====================================
// 6. Mixed Usage
// =====================================

let normal = 10
let ``strange name`` = 20
let combined = normal + ``strange name``

// =====================================
// 7. Shadowing
// =====================================

let value = 1
let value' = 2
let value'' = 3

let shadowTest =
    let value = 10
    let value' = 20
    value + value'

// =====================================
// 8. Pattern Matching
// =====================================

let testMatch x =
    match x with
    | someValue -> someValue
    | anotherValue' -> anotherValue'
    | _ -> 0

// =====================================
// 9. Token Boundary Tests
// =====================================

let ab cd = 1
let letValue = 2
let matchValue = 3

let x' y = 4
let x''y = 5

let ``a``b = 6
let a``b`` = 7

// =====================================
// 10. Pathological Stress
// =====================================

let a________________________________________________________________________b = 1

let α1β2γ3δ4ε5 = 2

let invisible‍chain = 3

let é́́́́́́́ = 4

let a1_b2'c3_d4'e5 = 5

// Extremely long apostrophe chain
let a'''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''' = 6

// =====================================
// 11. Lexer Boundary Edge
// =====================================

let endsAtEOF = 1

// =====================================
// 12. Fuzz Generator (Optional)
// =====================================

module Fuzz =

    let rand = Random()

    let letters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"
    let digits = "0123456789"
    let extra = "_'"

    let genIdent () =
        let len = rand.Next(1, 40)

        let first =
            string letters.[rand.Next(letters.Length)]

        let rest =
            [ for _ in 1..len ->
                let pool = letters + digits + extra
                pool.[rand.Next(pool.Length)] ]
            |> Array.ofList
            |> String

        first + rest

    let genTypeParam () =
        "'" + genIdent()

    let genBacktick () =
        let len = rand.Next(1, 30)
        let chars = " abcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()-_=+"

        let body =
            [ for _ in 1..len ->
                chars.[rand.Next(chars.Length)] ]
            |> Array.ofList
            |> String

        "``" + body.Replace("``", "`") + "``"

    let generate n =
        [ for _ in 1..n ->
            match rand.Next(3) with
            | 0 -> genIdent()
            | 1 -> genTypeParam()
            | _ -> genBacktick() ]

// =====================================
// 13. Invalid Cases (commented)
// =====================================

// let 123abc = 0
// let has space = 1
// let ``bad``name`` = 2
// let ``unterminated = 3
// let ' = 4
// let ``line
// break`` = 5