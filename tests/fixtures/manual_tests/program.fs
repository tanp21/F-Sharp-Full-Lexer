// Identifier zoo for F#

open System

// Normal identifiers
let normalIdentifier = 1
let snake_case_identifier = 2
let camelCaseIdentifier = 3

// Apostrophes are allowed
let value' = 4
let value'' = 5

// Unicode identifiers
let café = "coffee"
let π = Math.PI
let 变量 = "Chinese"
let данные = "Russian"
let Δx = 42
let λ = fun x -> x + 1

// Backtick identifiers
let ``identifier with spaces`` = 100
let ``123 starts with digits`` = 123
let ``punctuation !@#$%^&*()`` = "symbols"
let ``hello-world`` = "hyphenated"
let ``select`` = "SQL keyword"
let ``match`` = "F# keyword"

// Type with weird name
type ``Very Strange Type Name`` =
    {
        ``first name`` : string
        ``last name`` : string
        age' : int
    }

// DU cases with spaces
type Result =
    | ``It Worked``
    | ``It Failed`` of string

// Operator identifiers
let (+++) a b = a + b + 1000
let (=>) a b = (a, b)
let inline (|*|) a b = a * b

// Active pattern
let (|Even|Odd|) n =
    if n % 2 = 0 then Even else Odd

// Generic identifiers
let inline identity<'T> (x : 'T) = x

// Member names with backticks
type Person(name : string) =
    member _.Name = name
    member _.``say hello``() =
        printfn "Hello from %s" name

[<EntryPoint>]
let main argv =

    printfn "normalIdentifier = %d" normalIdentifier
    printfn "value'' = %d" value''
    printfn "café = %s" café
    printfn "π = %f" π
    printfn "变量 = %s" 变量
    printfn "данные = %s" данные
    printfn "Δx = %d" Δx
    printfn "λ 5 = %d" (λ 5)

    printfn "%d" ``identifier with spaces``
    printfn "%d" ``123 starts with digits``
    printfn "%s" ``punctuation !@#$%^&*()``
    printfn "%s" ``hello-world``
    printfn "%s" ``match``

    let person =
        {
            ``first name`` = "Ada"
            ``last name`` = "Lovelace"
            age' = 36
        }

    printfn "%s %s"
        person.``first name``
        person.``last name``

    let result = ``It Failed`` "oops"

    match result with
    | ``It Worked`` ->
        printfn "success"
    | ``It Failed`` msg ->
        printfn "failure: %s" msg

    printfn "1 +++ 2 = %d" (1 +++ 2)

    let pair = "age" => 42
    printfn "pair = %A" pair

    printfn "3 |*| 4 = %d" (3 |*| 4)

    match 10 with
    | Even -> printfn "10 is even"
    | Odd -> printfn "10 is odd"

    let p = Person("FSharp")
    p.``say hello``()

    printfn "identity = %A" (identity "generic")

    0