// ============================================================================
// F# LEXER STRESS TEST: SINGULARITY MODE (F# 7.0/8.0+ SPEC-COMPLIANT)
// ============================================================================
// Targets: CE contextual keywords, ref/fixed blocks, raw+interpolation formatting,
// preprocessor nesting, operator longest-match, offside toggling, active pattern
// delimiter slicing, quotation depth, Unicode zero-width/combining marks, and
// module/type signature syntax. All constructs are lexically valid.

#light "off"
#indent "off"

namespace LexerSingularity
open System

// ============================================================================
// 1. COMPUTATION EXPRESSION CONTEXTUAL KEYWORDS
// ============================================================================
module ComputationExpressionKeywords =
    type AsyncBuilder() =
        member _.Bind(x, f) = async.Bind(x, f)
        member _.Return(x) = async.Return(x)
        member _.Zero() = async.Zero()
        member _.Combine(a, b) = async.Combine(a, b)
        member _.Delay(f) = async.Delay(f)
        member _.For(xs, f) = async.For(xs, f)
        member _.While(g, b) = async.While(g, b)
        member _.TryFinally(b, c) = async.TryFinally(b, c)
        member _.TryWith(b, h) = async.TryWith(b, h)
        member _.Using(r, f) = async.Using(r, f)

    let async = AsyncBuilder()

    let compute x =
        async {
            let! a = async.Return x
            use! r = async.Return (new System.IO.MemoryStream())
            do! async.Sleep 10
            try! async.Return (a + 1)
            with | _ -> 0
            finally ()
            for i in 1..3 do
                yield! async.Return i
            return! async.Return (a * 2)
        }

    // Lexical note: `let!`, `use!`, `do!`, `try!`, `yield!`, `return!` are
    // tokenized as KEYWORD + OP by the spec, but CE parsers treat them as units.
    // Your lexer must emit them as separate tokens unless context-aware.

// ============================================================================
// 2. REF STRUCT, FIXED BLOCKS & BYREF/INREF/OUTREF
// ============================================================================
module RefStructAndFixed =
    type Span<'T> = ref struct
        val mutable pointer: nativeint
        val mutable length: int
    end

    let arr = [|1; 2; 3; 4; 5|]
    let inline addByref (x: byref<int>) (y: inref<int>) = x + y
    let outParam (z: outref<string>) = z <- "hello"

    fixed (p = &arr.[0], q = &arr.[1]) do
        let v1 = NativePtr.read p
        let v2 = NativePtr.read q
        v1 + v2

    // Ref cell operators vs type annotations
    let r = ref 0
    let _ = r := 5        // := COLONEQUALS
    let typed (x: int) = x + 1 // : COLON

// ============================================================================
// 3. RAW/INTERPOLATED STRINGS WITH FORMAT SPECIFIERS
// ============================================================================
module RawInterpolationFormatting =
    // Delimiter counting must be exact: N #s before " require N #s after "
    let r1 = #"raw with " quotes and \n"#
    let r2 = ##"raw ## delimiter and " quotes "##
    let r3 = ###"raw ### delimiter "###

    // Interpolated raw with format specifiers & alignment
    let ir1 = $#"val: {42,10:f2}" #
    let ir2 = $##"nested: {sprintf "%d" (1+1),-5:E} {{escaped}}"##
    let ir3 = $###"fmt: {DateTime.Now:yyyy-MM-dd} and {{braces}}"###

    // Boundary traps
    let edge1 = @"#{not raw}"          // verbatim starting with #
    let edge2 = """#{not raw}"""       // triple-quoted
    let edge3 = $"raw? #{not raw}"     // interpolated, raw-like but not
    let edge4 = $"""triple {{interp}}""" // triple-quoted interpolated

// ============================================================================
// 4. PREPROCESSOR NESTING & DIRECTIVE ARGUMENTS
// ============================================================================
module PreprocessorNesting =
    #if (DEBUG || TEST) && not CI
    #if VER >= 8
    #define MODE_ALPHA
    #warning "Using experimental features"
    #else
    #define MODE_BETA
    #error "Legacy mode unsupported"
    #endif
    #elif RELEASE
    #define MODE_RELEASE
    #endif

    #nowarn 9 40 41 52 44
    #r "System.Runtime"
    #r "nuget: Newtonsoft.Json, 13.0.1"
    #load "helpers.fsx" "utils.fsx"
    #I "./packages" "./lib"
    #line 100 "gen.fs"
    #time "on"
    #indent "on"

// ============================================================================
// 5. SYMBOLIC OPERATORS & LONGEST-MATCH COLLISIONS
// ============================================================================
module OperatorCollisions =
    // Chained pipes & applies (maximal munch required)
    let p1 = a |> b |> c
    let p2 = a ||> b ||> c
    let p3 = a <|| b
    let p4 = a <||| b
    let p5 = a |>! b
    let p6 = a !> b

    // Prefix/Infix tilde, bang, dot chains
    let ( ~-. ) x = -x
    let ( ~+ ) x = x
    let (!.) x = ref x
    let (.!) r = !r
    let ( +. ) a b = a + b
    let ( *. ) a b = a * b

    // Mixed symbolic operators (longest match)
    let ( +& ) a b = a + b
    let ( &+ ) a b = a + b
    let ( ~+ ) a b = b
    let ( +~ ) a b = a
    let ( %& ) a b = a
    let ( &% ) a b = b

    // Unicode math symbols (Sm/So categories)
    let (⊕) a b = a + b
    let (⊗) a b = a * b
    let (⊘) a b = a / b
    let (≠) a b = a <> b
    let (≤) a b = a <= b
    let (≥) a b = a >= b
    let (≡) a b = true
    let (→) a b = b a
    let (←) a b = a b

    // Token boundary traps (no whitespace)
    let trap1 = 1+2
    let trap2 = a..b
    let trap3 = a.b
    let trap4 = a::b
    let trap5 = a:b
    let trap6 = a:?b
    let trap7 = a:?>b
    let trap8 = a:=b
    let trap9 = a|>b
    let trap10 = a<|b

// ============================================================================
// 6. EXPLICIT OFFSIDE TOGGLING & SEMICOLON BLOCKS
// ============================================================================
module ExplicitOffsideToggles =
    #light "off"
    begin
        let x = 1;
        let y = 2;
        let z = x + y;
        if true then
            let a = 3;
            a + 1
        else
            0;
    end;
    #light "on"

    // Mixed explicit/implicit after toggle
    let mixed =
        let a = 1;
        let b = 2
        a + b

    // Blank lines MUST NOT trigger DEDENT in #light "on"
    let blankLines =
        if true then

            let x = 1
            x + 1
        else
            0

// ============================================================================
// 7. ACTIVE PATTERNS, PARTIAL PATTERNS & DELIMITER SLICING
// ============================================================================
module ActivePatternDelimiters =
    // Total active patterns
    let (|Even|Odd|) x = if x % 2 = 0 then Even else Odd
    
    // Partial active patterns
    let (|Div|_|) n x = if x % n = 0 then Some() else None
    
    // Multi-return & single-return
    let (|A|B|C|) x = match x with 1 -> A | 2 -> B | 3 -> C | _ -> C
    let (|Single|) x = x + 1
    
    // Parameterized patterns
    let (|InRange|_|) min max x = if x >= min && x <= max then Some() else None
    
    let test x =
        match x with
        | Div 3 -> "div3"
        | Even -> "even"
        | A | B -> "ab"
        | C -> "c"
        | InRange 5 10 -> "range"
        | _ -> "other"

    // Lexical note: `(|` and `|)` are pattern delimiters. `|` inside is separator.
    // Your lexer must not split `(|` into `(` `|` unless explicitly stateless.

// ============================================================================
// 8. QUOTATIONS, NESTED AST & DEPTH TRACKING
// ============================================================================
module QuotationDepthTracking =
    let q1 = <@ 1 + 1 @>
    let q2 = <@@ fun x -> x @@>
    let q3 = <@ <@ nested @> @>
    let q4 = <@@ <@@ double nested @@> @@>
    let q5 = <@ match 1 with | 1 -> "a" | _ -> "b" @>
    let q6 = <@ try 1 with | :? exn -> 0 @>
    let q7 = <@ for i in 1..10 do yield i @>

    // Lexical note: `<@` and `@>` are atomic quotation tokens. Depth must track.
    // `<@@`/`@@>` are distinct from `<@`/`@>`. Nested must not break string/char parsing.

// ============================================================================
// 9. MODULE SIGNATURES, REC MODULES & TYPE CONSTRAINTS
// ============================================================================
module ModuleSignaturesAndConstraints =
    module rec RecA =
        type A = { Val: int }
        let process (x: B) = x.Val
    and RecB =
        type B = { Val: int }
        let create v = { Val = v }

    // Type constraints with units & static members
    type Physics<'T when 'T : (static member (+): 'T * 'T -> 'T) and 'T : null> =
        member __.Mass : 'T<kg> = Unchecked.defaultof<'T>
    
    // Flexible types
    let sum (xs: #seq<int>) = Seq.sum xs
    
    // Inline const & nameof
    let inline const Max = 100
    let name = nameof(System.String)

// ============================================================================
// 10. UNICODE ZERO-WIDTH, COMBINING MARKS & BACKTICK EDGE CASES
// ============================================================================
module UnicodeZeroWidthBackticks =
    // Zero-width non-joiner (U+200C) and joiner (U+200D)
    let cafe\u200C = "coffee"
    let family\u200D = "family"
    
    // Combining marks (U+0301 acute, U+00EF ï)
    let naïve = "na\u00EFve"
    let café = "cafe\u0301"
    
    // Backtick identifiers with spaces, digits, symbols, Unicode
    let `type` = 1
    let `with` = 2
    let `when` = 3
    let `123` = "starts with digit"
    let ` ` = "contains space"
    let `⚡⚡⚡` = "emoji"
    let `α β γ` = "greek with spaces"
    let `(+@)` = fun a b -> a + b
    let `∑!` = fun xs -> List.sum xs
    
    // Newline inside backtick is ILLEGAL (lexer must emit error or reject)
    // let `broken\nid` = 0