// ============================================================================
// LEXER TEST SUITE: F#
// Copy-paste into a .fsx file or pipe through your lexer.
// ============================================================================

// 1. PREPROCESSOR & LIGHT MODE DIRECTIVES
#light "off"
#if DEBUG
#r "System.Core"
#load "helpers.fsx"
#nowarn "9" "41"
module LexerTest.Debug
#else
module LexerTest.Release
#endif
#light "on"

// 2. XML DOCUMENTATION COMMENTS
/// <summary>
/// Tests XML doc tokenization.
/// <para>Handles <c>inline code</c> and <see cref="System.String"/> references.</para>
/// </summary>
/// <param name="x">Input value</param>
/// <returns>Result</returns>
/// <exception cref="System.ArgumentException">Thrown when x < 0</exception>

open System
open Microsoft.FSharp.Quotations
open System.Collections.Generic

module LexerTokenCoverage =

    // 3. LINE & BLOCK COMMENTS (INCLUDING NESTING)
    // Single line comment
    //    With    irregular    spacing
    (* Block comment
       (* nested block *)
       spanning lines *)
    // Comment after code
    let x = 1 // trailing comment

    // 4. NUMERIC LITERALS (ALL BASES & SUFFIXES)
    let intDec   = 42
    let intHex   = 0xFF_00
    let intOct   = 0o755
    let intBin   = 0b1010_1100
    let floatSci = 1.23e-4
    let floatF   = 3.14f
    let floatD   = 2.71828
    let floatM   = 1.5m
    let byteVal  = 255uy
    let sbyteVal = -1s
    let int16    = 32767s
    let uint16   = 65535us
    let int32    = 2147483647
    let uint32   = 4294967295u
    let int64    = 9223372036854775807L
    let uint64   = 18446744073709551615UL
    let nint     = 1n
    let unint    = 1un

    // 5. CHARACTER & STRING LITERALS (ESCAPES, UNICODE, VERBATIM, TRIPLE)
    let charA    = 'A'
    let charEsc  = '\n'
    let charTab  = '\t'
    let charUni  = '\u0041'
    let strNorm  = "Hello \"world\"!\n"
    let strVerb  = @"C:\Path\To\`file`"
    let strTriple = """Line 1
    Line 2 with "quotes"
    Line 3 \n not escaped"""
    let strUnicode = "こんにちは 世界 🌍"

    // 6. BACKTICK IDENTIFIERS & KEYWORD ESCAPING
    let `type`     = 1
    let `match`    = "test"
    let `operator +` a b = a + b
    let ` `        = "space"
    let `123`      = "starts with digit"
    let `!@#$%`    = "symbols"

    // 7. OPERATORS & SYMBOLS (BUILT-IN, CUSTOM, UNICODE, PREFIX/INFIX)
    let (!+) x = x + 1
    let (+=) a b = a + b
    let (=>) a b = a, b
    let (§) x = x * 2
    let (∑) xs = List.sum xs
    let (≤) a b = a <= b
    let (|>) f x = x |> f // shadowing pipe
    let inline (>>|) f g x = g (f x)

    // 8. SIGNIFICANT WHITESPACE & OFFSIDE RULE
    let indentationTest n =
        if n > 0 then
            let y = n * 2
            y + 1
        else if n = 0 then
            0
        else
            let z = abs n
            z - 1

    // 9. TYPE DEFINITIONS (RECORDS, UNIONS, INTERFACES, CLASSES, CONSTRAINTS)
    type Point = { X: float; Y: float }

    type Shape =
        | Circle of radius: float
        | Rectangle of width: float * height: float
        | Polygon of vertices: Point list

    type IComparer<'T> =
        abstract Compare: 'T * 'T -> int

    type Vector2D<'a when 'a : (static member (+): 'a * 'a -> 'a) and 'a : (static member Zero: 'a)>
        (x: 'a, y: 'a) =
        member val X = x with get, set
        member val Y = y with get, set
        static member (+) (a: Vector2D<'a>, b: Vector2D<'a>) = Vector2D(a.X + b.X, a.Y + b.Y)
        static member Zero = Vector2D('a.Zero, 'a.Zero)

    // 10. PATTERN MATCHING & ACTIVE PATTERNS
    let (|DivisibleBy|_|) n x = if x % n = 0 then Some () else None
    let (|Even|Odd|) x = if x % 2 = 0 then Even else Odd

    let classify = function
        | 0 -> "zero"
        | 1 -> "one"
        | DivisibleBy 3 -> "div3"
        | Even -> "even"
        | Odd -> "odd"
        | _ -> "other"

    // 11. COMPUTATION EXPRESSIONS
    type MaybeBuilder() =
        member _.Bind(x, f) = Option.bind f x
        member _.Return(x) = Some x
        member _.Zero() = None
        member _.Combine(a, b) = Option.orElse b a
        member _.Delay(f) = f()
        member _.For(sequence: seq<_>, body) = Seq.tryPick body sequence
        member _.While(guard, body) = if not (guard()) then None else body()
        member _.TryFinally(body, compensation) = try body() finally compensation()
        member _.TryWith(body, handler) = try body() with e -> handler e

    let maybe = MaybeBuilder()

    let compute x =
        maybe {
            let! a = Some x
            let! b = Some (a + 1)
            do! if b > 10 then None else Some ()
            for i in [1..3] do
                yield! Some i
            return b * 2
        }

    // 12. QUOTATIONS
    let expr = <@ fun (x: int) -> x * x @>
    let exprTyped = <@@ fun (x: int) -> x * x @@>
    let exprWithLet = <@ let y = 5 in y + 1 @>

    // 13. ATTRIBUTES & TYPE PROVIDERS (SYNTAX ONLY)
    [<Sealed; Serializable>]
    type ConfigProvider() =
        static member GetSetting(key: string) : string =
            failwith "Not implemented"

    // 14. UNICODE IDENTIFIERS & MIXED TOKEN DENSITY
    let π = 3.1415926535
    let α β γ = α + β + γ
    let `∂/∂x` f x = f x + 1.0
    let test = α π (γ π) + `∂/∂x` (fun x -> x * x) 1.0

    // 15. EDGE CASES FOR LEXER ROBUSTNESS
    let spaced = "ok"
    let	tabbed	= "also ok"
    let _ = ()
    let rec loop i = if i = 0 then 0 else loop (i - 1) + i