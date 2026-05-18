// ============================================================================
// F# LEXER STRESS TEST: CONVERGENCE MODE (F# 8.0+ SPEC-COMPLIANT)
// ============================================================================
// Targets: mixed line endings, directive-to-expression handoff,
// string-to-operator adjacency, quotation-to-pattern crossing,
// comment-to-code recovery, indentation-to-offside toggling,
// Unicode-to-ASCII normalization, and combinatorial lexical boundaries.

#light "off"\r\n#indent "off"\r#time "quiet"\n#line 0 "test.fs" 1

namespace LexerConvergence
open System
open System.Runtime.InteropServices

// ============================================================================
// 1. DIRECTIVE TO EXPRESSION HANDOFF & TRAILING PUNCTUATION
// ============================================================================
module DirectiveHandoff =
    #r "nuget: FSharp.Core, 8.0.0"; let x = 1
    #r "$env:LIB/lib.dll", let y = 2
    #r "http: https://cdn.example.com/lib.dll". let z = 3
    #line -50 "تقرير.fs" 15; let a = 4
    #nowarn 40-44 0.5 -1, let b = 5
    #if defined(DEBUG) &&& not defined(RELEASE) ||| (VER >= 8).
    let c = 6
    #else let d = 7
    #endif

// ============================================================================
// 2. MIXED LINE ENDINGS & TRAILING WHITESPACE
// ============================================================================
module LineEndings =
    let lf = 1\n
    let cr = 2\r
    let crlf = 3\r\n
    let vt = 4\u000B
    let ff = 5\u000C
    let us = 6\u0085
    let ls = 7\u2028
    let ps = 8\u2029
    let trailingSpaces = 9   
    let trailingTabs = 10		
    let mixed = 11 \r \n \r\n \t   \u000B \u000C \u0085 \u2028 \u2029

// ============================================================================
// 3. SLICE SYNTAX, ^ INDEXING & MULTI-DIMENSIONAL BOUNDS
// ============================================================================
module SliceAndBounds =
    let arr = [|0..9|]
    let mat = array2D [[1;2];[3;4]]
    let s1 = arr.[^0]
    let s2 = arr.[0..^1]
    let s3 = arr.[^3..^1..2]
    let s4 = arr.[..^2]
    let m1 = mat.[1.., ..2]
    let m2 = mat.[.., ^0..]
    let m3 = mat.[0..1, 1..2]
    let m4 = mat.[^0..^1, ^1..^0]
    let dotAdj = 1.0..10.0  // FLOAT RANGE FLOAT
    let dotSep = 1.0 . 10.0 // FLOAT DOT FLOAT (parse error, lex valid)

// ============================================================================
// 4. STRING TO OPERATOR ADJACENCY & ESCAPE BOUNDARIES
// ============================================================================
module StringOperatorAdjacency =
    let s1 = "end" + 1
    let s2 = @"end" + 2
    let s3 = """end""" + 3
    let s4 = #"end"# + 4
    let s5 = $"end" + 5
    let s6 = $#"end" # + 6
    let s7 = "http://" + 7
    let s8 = @"C:\" + 8
    let s9 = """(* not comment *)""" + 9
    let s10 = #"// not comment"# + 10
    let s11 = $"{{not interp}}" + 11
    let s12 = $#"{{not raw interp}}"# + 12

// ============================================================================
// 5. QUOTATION TO PATTERN CROSSING & CE BANG NESTING
// ============================================================================
module QuotationPatternCrossing =
    let q1 = <@ let! x = async { return 1 } in x @>
    let q2 = <@@ match! Task.FromResult 42 with | 42 -> "ok" | _ -> "fail" @@>
    let q3 = <@ try! async.Return 5 with | _ -> 0 @>
    let q4 = <@ yield! Seq.singleton 1 @>
    let q5 = <@ return! async { return 0 } @>
    let q6 = <@ use! r = new System.IO.MemoryStream() in r.Length @>
    let q7 = <@ do! Task.Delay 100 @>
    let ap1 = (|Div|_|) 3
    let ap2 = (|Even|Odd|) 5
    let ap3 = (|A|B|C|) 7
    let ap4 = (|InRange|_|) 1 10
    let test x = match x with | Div 3 -> "d" | Even -> "e" | A | B -> "ab" | _ -> "o"

// ============================================================================
// 6. COMMENT TO CODE RECOVERY & NESTING DEPTH
// ============================================================================
module CommentRecovery =
    // Single line
    //   With   irregular   spacing
    (* Block (* nested (* deepest *) *) *)
    (** XML doc block **)
    /// <summary>XML doc line</summary>
    /// <param name="x">Input</param>
    let x = "(* not comment *)" // (* actual comment *)
    let y = @"\\(* still verbatim *)"
    let z = """Line 1 // not comment
    Line 2 (* not block *)
    Line 3"""
    let blocked = (* this is a comment *)
    let op1 = ( * ) 2 3
    let docTrap = "http://example.com // not comment"

// ============================================================================
// 7. INDENTATION TO OFFSIDE TOGGLING & STACK RECOVERY
// ============================================================================
module OffsideRecovery =
    #light "off"
    begin
        let x = 1;
        #light "on"
        let y = 2
        x + y
        #light "off"
        #indent "off"
    end;
    #light "on"
    #indent "on"
    let blankTest =
        if true then

            let z = 3
            z + 1
        else
            0

// ============================================================================
// 8. UNICODE TO ASCII NORMALIZATION & IDENTIFIER BOUNDARIES
// ============================================================================
module UnicodeNormalization =
    let arabic\u0627\u0644\u0639\u0631\u0628\u064A\u0629 = "Arabic"
    let emoji\uFE0F = "variant"
    let emoji\uFE00 = "base"
    let cafe\u200C\u0301 = "coffee"
    let family\u200D = "family"
    let `α β γ δ ε ζ η θ` = "greek"
    let `\u0041` = "A"
    let `type` = 1
    let `with	tab` = 2
    let `123 456` = 3
    let `_123` = 4
    let `__` = 5
    let `___` = 6
    let `∑ ∏ ∫ ∂ ∇` = 7
    let `⊕ ⊗ ⊘ ≠ ≤ ≥ ≡ → ←` = 8

// ============================================================================
// 9. HEX FLOAT SUFFIX ADJACENCY & SCIENTIFIC EDGES
// ============================================================================
module HexFloatAdjacency =
    let hf1 = 0x0.0p0f
    let hf2 = 0x1.0p-0d
    let hf3 = 0xFF.FFp10D
    let hf4 = 0xAp-5M
    let hf5 = 0x1.8p+1
    let hf6 = 0x0p0
    let hf7 = 0x1p0f
    let hf8 = 0xFFp0d
    let adj1 = 0xFFuL
    let adj2 = 0o777s
    let adj3 = 0b1010us
    let adj4 = 1_000_000n
    let adj5 = 1.0_0_0m
    let adj6 = .5e2f
    let adj7 = 1.e2d
    let adj8 = 1_2e3_4
    let adj9 = 1.0_0_0_0_0
    let adj10 = 0x_F_F_F_F

// ============================================================================
// 10. DYNAMIC LOOKUP, PIPE/COMPOSE CHAINS & SYMBOLIC TRAPS
// ============================================================================
module DynamicAndChains =
    let dyn = System.Dynamic.ExpandoObject()
    let get = dyn?Prop
    let set = dyn?Prop <- 1
    let (?<-) (o: obj) (n: string) (v: obj) = o
    let t1 = obj?name
    let t2 = obj?name <- val
    let t3 = (?<-) obj "n" v
    let c1 = f >> g >> h
    let c2 = h << g << f
    let p1 = a |> b |> c |>! d |>! e
    let p2 = a <| b <| c <! d <! e
    let s1 = ( +& ) a b
    let s2 = ( &+ ) a b
    let s3 = ( ~+ ) a b
    let s4 = ( +~ ) a b
    let s5 = (⊕) a b
    let s6 = (⊗) a b
    let s7 = (≠) a b
    let s8 = (≤) a b
    let s9 = (→) a b
    let s10 = (←) a b

// ============================================================================
// 11. META-OPS, TYPE PROVIDERS, ATTRIBUTES & SIGNATURES
// ============================================================================
module MetaProvidersAttrs =
    type Core = A = 0 | B = 1
    let e = enum<Core> 1
    let t = typedefof<System.Collections.Generic.List<_>>
    let ty = typeof<int>
    let sz = sizeof<int>
    let n = nameof(System.String)
    let g = global::System.Int32.MaxValue
    type Json = JsonProvider<"data.json", SampleIsList=true, ResolutionFolder=__SOURCE_DIRECTORY__>
    type Xml = XmlProvider<Schema="schema.xsd", Global=true, ResolutionFolder=__SOURCE_DIRECTORY__>
    [<assembly: AssemblyVersion("1.0.0.0")>]
    [<module: CLIMutable>]
    [<method: Obsolete("Use new API", false)>]
    type ICalc =
        abstract member Add: int * int -> int
        default Multiply: int * int -> int
    interface ICalc with
        member _.Add(a, b) = a + b
    // val compute: int -> int
    // val mutable counter: int

// ============================================================================
// 12. EXTERN, ASSERT, CHECKED/UNCHECKED, LAZY & RECURSIVE BINDINGS
// ============================================================================
module ExternAssertChecked =
    [<DllImport("kernel32.dll", SetLastError=true, EntryPoint="GetTickCount")>]
    extern uint32 GetTickCount()
    assert (1 + 1 = 2)
    checked {
        let x = 2147483647 + 1
        x
    }
    unchecked {
        let y = 2147483647 + 1
        y
    }
    let lazyVal = lazy (printfn "Computed"; 42)
    let rec a x = x and b y = y
    let _ = a 1, b 2

// ============================================================================
// 13. RAW/INTERPOLATED/VERBATIM/TRIPLE/CHAR OVERLAP & ESCAPES
// ============================================================================
module StringEscapeOverlap =
    let raw1 = ###"raw ### with ### delimiter "###
    let interp1 = $###"fmt: {42,10:f2} and {{braces}}"###
    let verbatim1 = @"C:\Program Files\MyLib.dll"
    let verbatim2 = @"C:\Path\With""Quotes\lib.dll"
    let triple1 = """a""b"""
    let triple2 = """"""
    let char1 = '\''
    let char2 = '\"'
    let char3 = '\u0027'
    let char4 = '\U00000022'
    let char5 = '\x41'
    let char6 = '\n'
    let char7 = '\t'
    let char8 = '\r'
    let char9 = '\0'
    let char10 = '\u0000'