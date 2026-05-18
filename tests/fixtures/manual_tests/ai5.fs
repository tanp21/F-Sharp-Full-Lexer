// ============================================================================
// F# LEXER STRESS TEST: APEX MODE (F# 7.0/8.0 SPEC-COMPLIANT)
// ============================================================================
// Targets: raw/interpolated string escaping, hex float exponents, slice ^ syntax,
// explicit #light toggling, preprocessor parentheses, token boundary collisions,
// and virtual indentation edge cases.

#light "off"
#indent "off"

namespace LexerApex
open System

// ============================================================================
// 1. RAW/INTERPOLATED STRINGS & ESCAPED BRACES
// ============================================================================
module StringInterpolationEscaping =
    // Custom delimiters: 1-99 '#' allowed. Ends only when " matches delimiter count.
    let r1 = #"raw " with quotes \n"#
    let r2 = ##"raw ## with delimiter " quotes "##
    let r3 = ###"raw ### delimiter "###
    
    // Interpolated raw strings with escaped braces {{ }}
    let ir1 = $#"path: {__SOURCE_DIRECTORY__} and {{escaped}} " #
    let ir2 = $##"nested {sprintf "%d" (1 + 1)} {{not interpolated}} "##
    
    // Format specifiers inside interpolation
    let fmt1 = $"{123.456:0.00}"
    let fmt2 = $"{DateTime.Now:yyyy-MM-dd}"
    let fmt3 = $"{1.23e+4:E}"
    
    // Boundary traps: verbatim vs raw vs interpolated adjacency
    let edge1 = @"#{not raw}"          // verbatim string starting with #
    let edge2 = """#{not raw}"""       // triple-quoted string
    let edge3 = $"raw? #{not raw}"     // interpolated, then raw-like but not
    let edge4 = $"""triple {{interp}}""" // triple-quoted interpolated

// ============================================================================
// 2. HEX FLOATS, EXPONENTS & DIGIT SEPARATOR EDGE CASES
// ============================================================================
module NumericHexFloats =
    // Hexadecimal floats (F# 7+)
    let hf1 = 0x1.8p1
    let hf2 = 0x1.0p-10
    let hf3 = 0xAp2f   // float32 suffix
    let hf4 = 0xFFp0d  // float64 suffix
    let hf5 = 0x0p+5   // positive exponent sign
    
    // Digit separator boundaries (must be between digits only)
    let ds1 = 1_2_3_4
    let ds2 = 1.0_0_0
    let ds3 = 0x_FF_FF
    let ds4 = 0b_1010_0101
    let ds5 = 1_2e3_4  // valid: _ between digits in mantissa/exponent
    
    // Scientific notation edge cases
    let sci1 = 1e+10
    let sci2 = 1e-10
    let sci3 = .5e2
    let sci4 = 1.e2
    let sci5 = 1.23e-4f
    
    // Underscore identifiers vs compiler-injected vs numeric
    let _ = 0
    let __ = 1
    let ___ = 2
    let _1 = 3
    let __SOURCE_FILE__ = __SOURCE_FILE__
    let __LINE__ = __LINE__

// ============================================================================
// 3. SYMBOLIC OPERATORS & LONGEST-MATCH COLLISIONS
// ============================================================================
module OperatorCollisions =
    // Chained pipe/apply (longest match required)
    let p1 = a |> b |> c
    let p2 = a ||> b ||> c
    let p3 = a <|| b
    let p4 = a <||| b
    
    // Prefix/Infix tilde & bang chains
    let tilde1 = ~x
    let tilde2 = ~~x
    let bang1 = !x
    let bang2 = !!x
    
    // Mixed symbolic operators (maximal munch)
    let ( +& ) a b = a + b
    let ( &+ ) a b = a + b
    let ( +! ) a b = a + b
    let ( !+ ) a b = a + b
    let ( ~+ ) a b = a
    let ( +~ ) a b = b
    
    // Unicode math symbols (Sm/So categories)
    let (⊕) a b = a + b
    let (⊗) a b = a * b
    let (⊘) a b = a / b
    let (≠) a b = a <> b
    let (≤) a b = a <= b
    let (≥) a b = a >= b
    let (≡) a b = true
    
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

// ============================================================================
// 4. SLICE NOTATION & FROM-END INDEXING (F# 6+)
// ============================================================================
module SliceFromEnd =
    let arr = [|0; 1; 2; 3; 4; 5|]
    let mat = array2D [[1;2];[3;4]]
    
    // Standard slices
    let s1 = arr.[0..2]
    let s2 = arr.[1..]
    let s3 = arr.[..^1]
    let s4 = arr.[0..2..^1]
    
    // From-end indexing
    let fe1 = arr.[^0]  // last
    let fe2 = arr.[^1]  // second last
    let fe3 = arr.[0..^2]
    let fe4 = arr.[^3..^1..2]
    
    // Matrix slicing
    let ms1 = mat.[1,0..]
    let ms2 = mat.[.., ^0]
    
    // `..` vs `.` `.` vs `..` as range token
    let dotRange = 1..10   // INT RANGE INT
    let dotDot = 1.0..10.0 // FLOAT RANGE FLOAT
    let dotSep = 1.0 . 10.0 // FLOAT DOT FLOAT (lex valid, parse error)

// ============================================================================
// 5. EXPLICIT OFFSIDE TOGGLING & SEMICOLON BLOCKS
// ============================================================================
module ExplicitSemicolonBlocks =
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
    
    // Mixed explicit/implicit after mode switch
    let mixed =
        let a = 1;
        let b = 2
        a + b

// ============================================================================
// 6. PREPROCESSOR EXPRESSIONS & PARENTHETICAL DIRECTIVES
// ============================================================================
module PreprocessorExpressions =
    #if (DEBUG || RELEASE) && not TEST && (VER > 3)
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
// 7. NESTED QUOTATIONS & ACTIVE PATTERN SLICES
// ============================================================================
module QuotationsAndActivePatterns =
    let q1 = <@ 1 + 1 @>
    let q2 = <@@ fun x -> x @@>
    let q3 = <@ <@ nested @> @>
    let q4 = <@@ <@@ double nested @@> @@>
    
    let ap1 = (|Div|_|) 3
    let ap2 = (|Even|Odd|) 5
    let ap3 = (|A|B|C|) x
    let ap4 = (|Single|) 1
    
    let test x =
        match x with
        | Div 3 -> "div"
        | Even -> "even"
        | A -> "a"
        | B -> "b"
        | C -> "c"
        | _ -> "other"

// ============================================================================
// 8. CONTEXTUAL KEYWORDS IN TYPE/VALUE BINDINGS
// ============================================================================
module ContextualKeywordBinding =
    type Gen<'T when 'T : null and 'T : unmanaged and 'T : not struct> = class end
    
    let testWhen x = match x with | n when n > 0 -> "pos" | _ -> "neg"
    
    let testWith () =
        try failwith "x" with _ -> 0
        let f () = { X = 1; Y = 2 }
        let g () = { f() with Y = 3 }
        interface IDisposable with member _.Dispose() = ()
    
    let rec a x = x and b y = y
    type A = { V: int } and B = { S: string }
    
    let testAs (x: obj) = match x with | :? string as s -> s.Length | _ -> 0
    
    let lazyVal = lazy (1 + 2)
    type S = struct val X: int end
    let n = null
    let bv (x: byref<int>) = x <- 1
    let ir (x: inref<int>) = x + 1
    let or_ (x: outref<string>) = z <- "hello"
    
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
// 9. COMMENT NESTING, XML DOC & STRING COMMENT OVERLAP
// ============================================================================
module CommentStringOverlap =
    // Single line
    //    With   irregular   spacing
    (* Block comment
       (* nested block *)
       spanning lines *)
    (** XML doc block *)
    /// <summary>XML doc line</summary>
    /// <param name="x">Input</param>
    let x = "(* not a comment *)" // (* actual comment *)
    let y = @"\\(* still verbatim *)"

// ============================================================================
// 10. UNICODE IDENTIFIERS, COMBINING MARKS & SYMBOLIC NAMES
// ============================================================================
module UnicodeIdentifiers =
    // Combining acute accent (U+0301) after 'e' -> é
    let cafe\u0301 = "coffee"
    let naïve = "na\u00EFve"
    let αβγδεζηθ = 1
    let ∑∏∫∂∇ = 2
    
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

// ============================================================================
// 11. BYREF, REFCELL & POINTER BOUNDARIES
// ============================================================================
module ByrefRefcellPointers =
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