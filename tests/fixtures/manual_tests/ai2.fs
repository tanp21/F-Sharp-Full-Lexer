// ============================================================================
// F# LEXER STRESS TEST: HARD MODE
// ============================================================================
// This file is syntactically valid but lexically treacherous.
// Each section targets known lexer failure points in F# implementations.

#light "off"
#if DEBUG
#r "System.Runtime"
#load "helpers.fsx"
#nowarn 9 41 52
#line 100 "generated.fs"
#else
#light "on"
#endif

namespace LexerHardMode
open System

// ============================================================================
// 1. BACKTICK IDENTIFIERS & KEYWORD ESCAPING
// ============================================================================
module BacktickTraps =
    let `type`     = 1
    let `with`     = 2
    let `when`     = 3
    let `match`    = 4
    let `function` = 5
    let `module`   = 6
    let `namespace`= 7
    let `val`      = 8
    let `begin`    = 9
    let `end`      = 10
    let `let`      = 11
    let `do`       = 12
    let `if`       = 13
    let `then`     = 14
    let `else`     = 15
    let `try`      = 16
    let `finally`  = 17
    let `for`      = 18
    let `while`    = 19
    let `yield`    = 20
    let `return`   = 21
    let `lazy`     = 22
    let `inline`   = 23
    let `mutable`  = 24
    let `abstract` = 25
    let `override` = 26
    let `default`  = 27
    let `static`   = 28
    let `member`   = 29
    let `new`      = 30
    let `get`      = 31
    let `set`      = 32
    let `use`      = 33
    let `upcast`   = 34
    let `downcast` = 35
    let `typeof`   = typeof<int>
    let `sizeof`   = sizeof<int>
    let `enum`     = enum<Core>
    let `ref`      = ref 0
    let `addressof`= AddressOf
    let `fixed`    = 0
    let `lock`     = fun _ _ -> ()
    let `using`    = fun _ _ -> ()
    let `123`      = "starts with digit"
    let ` `        = "contains space"
    let `⚡⚡⚡`    = "emoji"
    let `α β γ`    = "greek with spaces"
    // F# spec: backticks CANNOT span lines. Lexer should reject or error on newline inside.
    // let `broken
    // identifier` = 0

// ============================================================================
// 2. NUMERIC LITERALS & DIGIT SEPARATORS
// ============================================================================
module NumericTraps =
    let a = 1.          // float without fractional part
    let b = .5          // float without integer part
    let c = 1e10        // scientific notation
    let d = 1.0e-5      // signed exponent
    let e = 0x_FF       // hex with separator
    let f = 0b_101      // bin with separator
    let g = 0o_755      // octal with separator
    let h = 1_000_000   // decimal separators
    let i = 1_000_000.0 // float with separators
    let j = 0uL         // suffix adjacency
    let k = 1uy         // unsigned byte
    let l = 2s          // int16
    let m = 3us         // uint16
    let n = 4L          // int64
    let o = 5UL         // uint64
    let p = 6n          // native int
    let q = 7un         // native uint
    let r = 8.f         // float32
    let s = 9.0m        // decimal
    let t = 10.D        // float64
    let u = 11.0M       // decimal uppercase
    let v = 0x0         // hex zero
    let w = 0b0         // bin zero
    let x = 0o0         // octal zero

// ============================================================================
// 3. STRING & CHARACTER LITERALS
// ============================================================================
module StringCharTraps =
    let s1 = @""                // empty verbatim
    let s2 = @"C:\path\to\file" // verbatim backslashes
    let s3 = @"He said ""Hi"""  // verbatim escaped quote
    let s4 = """a""b"""         // triple quote with quote inside
    let s5 = """"""             // triple quote containing ""
    let s6 = "\u0041\u0042"     // Unicode 4-digit escape
    let s7 = "\U00000043"       // Unicode 8-digit escape
    let s8 = "\n\t\r\b\f\v\a"   // classic escapes
    let s9 = @"\\n\t\r"         // verbatim literal backslash+n
    let c1 = '\''               // escaped single quote
    let c2 = '\"'               // escaped double quote
    let c3 = '\u0027'           // hex char escape
    let c4 = '\U00000022'       // 8-digit char escape
    let c5 = '\x41'             // hex escape
    let c6 = '\n'               // newline char
    let c7 = '\t'               // tab char

// ============================================================================
// 4. OPERATOR BOUNDARIES & LONGEST MATCH
// ============================================================================
module OperatorTraps =
    let (!+) x = x              // prefix
    let (+!) x = x              // prefix (unusual but valid)
    let (++) a b = a + b        // infix
    let (-->) a b = a, b        // arrow-ish
    let (|>>) a b = b a         // triple pipe
    let (=>) a b = a, b         // fat arrow
    let (§) x = x               // section symbol
    let (∑) xs = List.sum xs    // summation
    let (≤) a b = a <= b        // unicode leq
    let (≥) a b = a >= b        // unicode geq
    let (≠) a b = a <> b        // unicode neq
    let (≈) a b = abs(a - b) < 1e-6
    let a = 1+2                 // no spaces
    let b = 1 +2                // space after +
    let c = 1+ 2                // space before +
    let d = 1 + 2               // normal
    let e = -1                  // unary minus
    let f = 1 - 2               // binary minus
    let g = 1 - -2              // binary + unary
    let h = !true               // bang
    let i = not true            // keyword negation
    let j = ~~~1                // tilde chain
    let k = 1 <<< 2             // bit shift
    let l = 1 >>> 2             // unsigned shift
    let m = 1 &&& 2             // bitwise and
    let n = 1 ||| 2             // bitwise or
    let o = 1 ^^^ 2             // bitwise xor
    let p = a |> b |> c         // forward pipe
    let q = a <| b <| c         // backward pipe
    let r = a >> b >> c         // composition
    let s = a << b << c         // reverse composition

// ============================================================================
// 5. ACTIVE PATTERNS & PIPE DELIMITERS
// ============================================================================
module PatternTraps =
    let (|A|B|) x = if x > 0 then A else B
    let (|Div|_|) n x = if x % n = 0 then Some() else None
    let (|α|β|γ|) x = if x < 0 then α elif x = 0 then β else γ
    let (|Empty|Cons|) = function [] -> Empty | h::t -> Cons(h,t)
    let test x =
        match x with
        | Div 3 -> "div3"
        | α | β -> "alpha_or_beta"
        | γ -> "gamma"
        | _ -> "other"

// ============================================================================
// 6. QUOTATIONS & NESTED BRACKETS
// ============================================================================
module QuotationTraps =
    let q1 = <@ 1 + 1 @>
    let q2 = <@@ fun x -> x @@>
    let q3 = <@ let y = 1 in y @>
    let q4 = <@ <@ 1 @> @>
    let q5 = <@@ <@ 2 @> @@>
    let q6 = <@ match 1 with | 1 -> "a" | _ -> "b" @>
    let q7 = <@ try 1 with | :? exn -> 0 @>
    let q8 = <@ for i in 1..10 do yield i @>

// ============================================================================
// 7. INDENTATION / OFFSIDE RULE TRAPS
// ============================================================================
module IndentationTraps =
    let f x =
        if x > 0 then
            let y = x * 2
            y + 1
        else
            0

    let g () =
        match 1 with
        | 1 ->
            "one"
        | 2 ->
            "two"
        | _ ->
            "other"

    let h =
        seq {
            for i in 1..5 do
                yield i * 2
            yield! [1;2;3]
            return 0
        }

    let i =
        [|
            1
            2
            3
        |]

    // Blank lines should NOT reset indentation level
    let j =
        if true then

            let k = 1
            k
        else
            0

// ============================================================================
// 8. COMMENTS & XML DOCS
// ============================================================================
module CommentTraps =
    // Single line
    //   With   irregular   spacing
    (* Block comment
       (* nested block *)
       spanning lines *)
    (** XML doc block *)
    /// <summary>XML doc line</summary>
    /// <param name="x">Input</param>
    let x = 1 // trailing comment

// ============================================================================
// 9. CONTEXTUAL KEYWORDS IN VALID SYNTAX
// ============================================================================
module ContextualKeywordTraps =
    type Point = { X: float; Y: float }
    type Shape =
        | Circle of radius: float
        | Rectangle of width: float * height: float
    interface IComparable with
        member this.CompareTo(other: obj) = 0
    type Vector<'a when 'a : (static member (+): 'a * 'a -> 'a)> (x: 'a, y: 'a) =
        member val X = x with get, set
        member val Y = y with get, set
    let inline add a b = a + b
    let lazyVal = lazy (1 + 2)
    let asyncVal = async { return 0 }
    let seqVal = seq { yield 1; yield 2 }
    let listVal = [1; 2; 3]
    let arrayVal = [|1; 2; 3|]

// ============================================================================
// 10. PREPROCESSOR & DIRECTIVE EDGE CASES
// ============================================================================
#if true && false || not false
#define TEST_MODE
#nowarn 40 41
#line 200 "generated.fs"
#time "on"
#endif