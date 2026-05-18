// ============================================================================
// F# LEXER STRESS TEST: NUCLEAR MODE (F# 7.0/8.0+)
// ============================================================================
// Syntactically valid but lexically designed to break naive DFAs, longest-match,
// indentation tracking, string interpolation, and contextual keyword handling.

#light "off"
#indent "off"

namespace LexerNuclear
open System

// ============================================================================
// 1. F# 8.0 RAW + INTERPOLATED STRINGS & FORMAT SPECIFIERS
// ============================================================================
module RawInterpolationExtremes =
    // Custom delimiters: 1-99 '#' allowed. Lexer must count delimiter length.
    let r1 = #"raw with " quotes and \n"#
    let r2 = ##"raw with ## delimiter and " quotes "##
    let r3 = ###"raw with ### delimiter "###

    // Interpolated raw strings
    let ir1 = $#"path: {__SOURCE_DIRECTORY__}\file" #
    let ir2 = $##"nested {sprintf "%d" (1 + 1)} and {{escaped braces}} "##

    // Format specifiers inside interpolation
    let fmt1 = $"{123.456:0.00}"
    let fmt2 = $"{DateTime.Now:yyyy-MM-dd}"
    let fmt3 = $"{1.23e+4:E}"

    // Boundary traps: verbatim/raw/interpolated adjacency
    let edge1 = @"#{not raw}"          // verbatim string starting with #
    let edge2 = """#{not raw}"""       // triple-quoted string
    let edge3 = $"raw? #{not raw}"     // interpolated, then raw-like but not

// ============================================================================
// 2. HEX FLOATS, DIGIT SEPARATORS & NUMERIC PATHOLOGIES
// ============================================================================
module NumericPathologies =
    // Hexadecimal floats (F# 7+)
    let hf1 = 0x1.8p1
    let hf2 = 0x1.0p-10
    let hf3 = 0xAp2f   // float32 suffix
    let hf4 = 0xFFp0d  // float64 suffix

    // Digit separator edge cases
    let ds1 = 1_2_3_4
    let ds2 = 1.0_0_0
    let ds3 = 0x_FF_FF
    let ds4 = 0b_1010_0101
    let ds5 = 1_2e3_4  // valid: _ between digits in mantissa/exponent

    // Scientific notation boundaries
    let sci1 = 1e+10
    let sci2 = 1e-10
    let sci3 = .5e2
    let sci4 = 1.e2
    let sci5 = 1.23e-4f

    // Underscore identifiers vs numeric vs compiler-injected
    let underscore = 0
    let __ = 1
    let ___ = 2
    let _1 = 3
    let __SOURCE_FILE__ = __SOURCE_FILE__
    let __LINE__ = __LINE__

// ============================================================================
// 3. SYMBOLIC OPERATORS & LONGEST-MATCH AMBIGUITIES
// ============================================================================
module OperatorAmbiguities =
    // Chained pipe/apply operators
    let p1 = a |> b |> c
    let p2 = a ||> b ||> c
    let p3 = a <|| b
    let p4 = a <||| b

    // Prefix/Infix tilde & bang chains
    let tilde1 = ~x
    let tilde2 = ~~x
    let bang1 = !x
    let bang2 = !!x

    // Mixed symbolic operators (longest match required)
    let ( +& ) a b = a + b
    let ( &+ ) a b = a + b
    let ( +! ) a b = a + b
    let ( !+ ) a b = a + b
    let ( ~+ ) a b = a
    let ( +~ ) a b = b
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

    // Token boundary traps
    let trap1 = 1 + 2  // INT OP INT
    let trap2 = 1+2    // INT OP INT (no space)
    let trap3 = a..b   // IDENT DOTDOT IDENT
    let trap4 = a.b    // IDENT DOT IDENT
    let trap5 = a::b   // IDENT CONS IDENT
    let trap6 = a:b    // IDENT COLON IDENT
    let trap7 = a:?b   // IDENT COLONQUESTION IDENT
    let trap8 = a:?>b  // IDENT COLONQUESTIONGREATER IDENT
    let trap9 = a:=b   // IDENT COLONEQUALS IDENT

// ============================================================================
// 4. CONTEXTUAL KEYWORD COLLISIONS & PATTERN/TYPE CONTEXTS
// ============================================================================
module ContextualCollisions =
    // `when` in guard vs type constraint
    type Gen<'T when 'T : null> = class end
    let testWhen x = match x with | n when n > 0 -> "pos" | _ -> "neg"

    // `with` in try, object expr, record update, interface
    let testWith () =
        try failwith "x" with _ -> 0
        let f () = { X = 1; Y = 2 }
        let g () = { f() with Y = 3 }
        interface IDisposable with member _.Dispose() = ()

    // `and` in rec bindings vs type defs
    let rec a x = x and b y = y
    type A = { V: int } and B = { S: string }

    // `as` in pattern vs type alias
    let testAs (x: obj) = match x with | :? string as s -> s.Length | _ -> 0

    // `lazy`, `struct`, `null`, `byref`, `inref`, `outref`
    let lazyVal = lazy (1 + 2)
    type S = struct val X: int end
    let n = null
    let bv (x: byref<int>) = x <- 1
    let ir (x: inref<int>) = x + 1
    let or_ (x: outref<int>) = x <- 1

    // `val`, `override`, `abstract`, `default`, `member`, `static`, `inline`, `mutable`
    type Base() =
        abstract Do: unit -> unit
        default _.Do() = ()
    type Derived() =
        inherit Base()
        override _.Do() = ()
        member val X = 1 with get, set
        static member Y = 2
        inline _.Add a b = a + b
        mutable let _ = 0

// ============================================================================
// 5. EXPLICIT SEMICOLONS VS OFFSIDE RULE (#light "off" mode)
// ============================================================================
module ExplicitSemicolons =
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
    end
    #light "on"

    // Mixed explicit/implicit after mode switch
    let mixed =
        let a = 1;
        let b = 2
        a + b

// ============================================================================
// 6. PREPROCESSOR EXPRESSIONS & DIRECTIVE PARSING
// ============================================================================
module PreprocessorTraps =
    #if true && false || not false
    #define MODE_A
    #elif not MODE_A && MODE_B
    #define MODE_C
    #else
    #define MODE_FALLBACK
    #endif

    #nowarn 40 41 52
    #r "System.Runtime"
    #r "nuget: Newtonsoft.Json, 13.0.1"
    #load "helpers.fsx"
    #I "./packages"
    #line 100 "gen.fs"
    #time "on"
    #indent "on"

// ============================================================================
// 7. UNICODE NORMALIZATION & COMBINING MARKS
// ============================================================================
module UnicodeNormalization =
    // Combining acute accent (U+0301) after 'e' -> é
    let cafe\u0301 = "coffee"
    // Script mixing
    let αβγδεζηθ = 1
    let ∑∏∫∂∇ = 2
    // Identifier starting with _ then digit vs numeric
    let _1 = 1
    let __ = 2
    let ___ = 3

// ============================================================================
// 8. NESTED QUOTATIONS & ACTIVE PATTERNS
// ============================================================================
module QuotationActivePatterns =
    let q1 = <@ 1 + 1 @>
    let q2 = <@@ fun x -> x @@>
    let q3 = <@ <@ nested @> @>
    let q4 = <@@ <@@ double nested @@> @@>

    let ap1 = (|Div|_|) 3
    let ap2 = (|Even|Odd|) 5
    let ap3 = (|A|B|C|) x

    let test x =
        match x with
        | Div 3 -> "div"
        | Even -> "even"
        | A -> "a"
        | B -> "b"
        | C -> "c"
        | _ -> "other"

// ============================================================================
// 9. COMMENT NESTING & XML DOC EDGE CASES
// ============================================================================
module CommentExtremes =
    // Single line
    //    With   irregular   spacing
    (* Block comment
       (* nested block *)
       spanning lines *)
    (** XML doc block *)
    /// <summary>XML doc line</summary>
    /// <param name="x">Input</param>
    let x = 1 // trailing comment

// ============================================================================
// 10. BYREF, REFCELL & POINTER AMBIGUITIES
// ============================================================================
module ByrefRefcell =
    let arr = [|1; 2; 3|]
    let p1 = &arr.[0]
    let p2 = &&arr.[0] // Lexer: `&` `&` `arr` `.` `[` `0` `]`
    fixed (p = &arr.[0]) do
        let v = NativePtr.read p

    let refCell = ref 0
    let _ = refCell := 5
    let _ = !refCell
    let inline addByref (x: byref<int>) (y: inref<int>) = x + y
    let outParam (z: outref<string>) = z <- "hello"