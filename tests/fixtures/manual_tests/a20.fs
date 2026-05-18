// ============================================================================
// F# LEXER STRESS TEST: OMEGA-III MODE (F# 8.0+ SPEC-COMPLIANT)
// ============================================================================
// Targets: directive protocols, open static/class/type, val extern sigs,
// #line default/hidden mid-block, #if bitwise nesting, #nowarn ranges,
// #pragma/#help/#time/#indent variants, extreme nesting, RTL/LTR overrides,
// hex float suffix adjacency, dynamic lookup boundaries, slice ^ indexing,
// raw/interp format specifiers, let...in nesting, function guards,
// explicit blocks, offside stack corruption, meta-ops, type providers,
// attributes, mixed line endings, and directive-to-expression handoff.

#light "off"\r#indent "off"\n#time "quiet"\r\n#line 0 "test.fs" 1

namespace LexerOmegaIII
open System
open System.Runtime.InteropServices

// ============================================================================
// 1. DIRECTIVE PROTOCOLS, PAKET & LOAD PATH MIXING
// ============================================================================
module DirectiveProtocols =
    #r "paket: FSharp.Core, 8.0.0"; let x = 1
    #r "nuget: Newtonsoft.Json, [13.0.1, 14.0.0)", let y = 2
    #r "github: fsprojects/FSharp.Data, src/FSharp.Data.fsx". let z = 3
    #r "http: https://cdn.example.com/lib.dll?token=abc&v=2"; let a = 4
    #r "$env:LIB_PATH%20/my%2Flib.dll", let b = 5
    #load "./scripts/1.fsx" "../lib/2.fsx" "C:\dev\3.fsx"; let c = 6
    #I "./bin" "../lib" "C:\\refs" "D:\packages"; let d = 7
    #line -50 "تقرير_البيانات.fs" 15; let e = 8
    #line 0 "generated_αβγ.fs" 0, let f = 9
    #line hidden. let g = 10
    #line default; let h = 11

// ============================================================================
// 2. OPEN VARIANTS, GLOBAL QUALIFIERS & ;; SEPARATORS
// ============================================================================
module OpenVariants =
    open static System.Math
    open class System.Object
    open type System.Collections.Generic.IEnumerable<_>
    let g = global::System.Int32.MaxValue
    let x = 1 ;; let y = 2 ;;
    let f () = let z = 3 ;; z + 1 ;;

// ============================================================================
// 3. VAL SIGNATURES, EXTERN MODIFIERS & TYPE PROVIDERS
// ============================================================================
module SignaturesAndProviders =
    val extern nativeint GetProcAddress: nativeint * string -> nativeint
    val compute: int -> int
    val mutable counter: int
    type Json = JsonProvider<"data.json", SampleIsList=true, ResolutionFolder=__SOURCE_DIRECTORY__>
    type Xml = XmlProvider<Schema="schema.xsd", Global=true, ResolutionFolder=__SOURCE_DIRECTORY__>
    type Csv = CsvProvider<"data.csv", HasHeaders=true, ResolutionFolder=__SOURCE_DIRECTORY__>

// ============================================================================
// 4. #IF BITWISE NESTING & #NOWARN RANGES
// ============================================================================
module BitwiseAndWarnings =
    #if ~~~(defined(DEBUG) &&& defined(TRACE)) ||| defined(RELEASE) ^^^ defined(EXP)
    #define MODE_BIT_COMPLEX
    #elif not defined(LEGACY) &&& (defined(ALPHA) ||| defined(BETA))
    #define MODE_BIT_FALLBACK
    #else
    #define MODE_BIT_DEFAULT
    #endif
    #undef MODE_BIT_COMPLEX
    #warning "Complex path disabled"
    #error "Legacy not supported"
    #nowarn -1 0.5 100-200 40-44 45
    #pragma warning disable 40,41
    #help
    #time "verbose"
    #indent 4
    #define _123
    #undef _123
    #invalidDirective "test"

// ============================================================================
// 5. MIXED LINE ENDINGS, WHITESPACE & TRAILING PUNCTUATION
// ============================================================================
module LineEndingsAndWhitespace =
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
    let directivePunct = 12; 13, 14. 15

// ============================================================================
// 6. SLICE SYNTAX, ^ INDEXING & MULTI-DIMENSIONAL BOUNDS
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
// 7. STRING TO OPERATOR ADJACENCY & ESCAPE BOUNDARIES
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
// 8. QUOTATION DEPTH & CE CONTEXTUAL BANGS
// ============================================================================
module QuotationDepthAndCE =
    let q1 = <@ 1 + 1 @>
    let q2 = <@@ fun x -> x @@>
    let q3 = <@ <@ nested @> @>
    let q4 = <@@ <@@ double @@> @@>
    let q5 = <@ let! x = async { return 1 } in x @>
    let q6 = <@ match 1 with | 1 -> "a" | _ -> "b" @>
    let q7 = <@ try! async.Return 5 with | _ -> 0 @>
    let q8 = <@ yield! Seq.singleton 1 @>
    let q9 = <@ return! async { return 0 } @>
    let q10 = <@ use! r = new System.IO.MemoryStream() in r.Length @>
    let q11 = <@ do! Task.Delay 100 @>
    type TaskBuilder() =
        member _.Bind(t, f) = t.Bind(f)
        member _.Return(x) = Task.FromResult x
    let task = TaskBuilder()
    let compute () =
        task {
            let! (Some x) = Some 5
            use! r = new System.IO.MemoryStream()
            do! Task.Delay 100
            try! Task.FromResult 42
            with | _ -> 0
            match! Task.FromResult "ok" with
            | "ok" -> "success" | _ -> "fail"
            return x
        }

// ============================================================================
// 9. DYNAMIC LOOKUP, PIPE/COMPOSE CHAINS & SYMBOLIC TRAPS
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
// 10. HEX FLOATS, SCIENTIFIC EDGES & DIGIT SEPARATORS
// ============================================================================
module HexAndScientific =
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
// 11. (* VS ( * ) TRAPS & XML DOCS WITH CDATA/ENTITIES
// ============================================================================
module CommentOperatorTraps =
    let blocked = (* this is a comment *)
    let op1 = ( * ) 2 3
    let docTrap = "http://example.com // not comment"
    let verbatimDoc = @"C:\path\(* not comment *)"
    let tripleDoc = """Line 1 // not comment
    Line 2 (* not block comment *)
    Line 3"""
    let rawDoc = #"raw // and (* not comment *)"#
    let interpDoc = $"path: {__SOURCE_DIRECTORY__} // not comment"
    let rawInterp = $#"val: {42,10:f2} and {{braces}}"#
    /// <summary>
    /// <![CDATA[ <script>alert("nested")</script> ]]>
    /// &amp; &lt; &gt; &quot; &apos; &nbsp; &copy; &reg;
    /// </summary>
    /// <param name="x">Input with (* not comment *) and // not comment</param>
    (** Block doc with &amp; &lt; &gt; and <em>tags</em> **)
    let docEntityTrap = 1

// ============================================================================
// 12. LET...IN NESTING & FUNCTION GUARDS
// ============================================================================
module LetInAndFunction =
    let result =
        let a = 5 in
        let b = match a with
            | 1 | 2 | 3 as n when n > 0 -> n * 2
            | 4 | 5 -> 10
            | _ when a < 0 -> 0
            | _ -> -1
        in b + a
    let classify = function
        | [] -> "empty"
        | [_] -> "single"
        | [x; y] as lst when x < y -> "two ascending"
        | [x; y] -> "two"
        | _ -> "many"

// ============================================================================
// 13. EXPLICIT BLOCKS, SEMICOLONS & OFFSIDE RECOVERY
// ============================================================================
module ExplicitBlocksAndOffside =
    #light "off"
    begin
        let mutable state = 0;
        for i = 1 to 10 do
            state <- state + i;
            printfn "%d" state;
        done;
        while state > 0 do
            state <- state - 1;
        done;
        try
            failwith "test";
        with
            | _ -> 0;
        end;
    #light "on"
    #indent "on"
    let mixed = begin let x = 1; x + 2 end
    let blankTest =
        if true then

            let z = 3
            z + 1
        else
            0

// ============================================================================
// 14. META-OPS, ATTRIBUTES & SIGNATURES
// ============================================================================
module MetaAndAttributes =
    type Core = A = 0 | B = 1
    let e = enum<Core> 1
    let t = typedefof<System.Collections.Generic.List<_>>
    let ty = typeof<int>
    let sz = sizeof<int>
    let n = nameof(System.String)
    let g = global::System.Int32.MaxValue
    [<assembly: AssemblyVersion("1.0.0.0")>]
    [<module: CLIMutable>]
    [<method: Obsolete("Use new API", false)>]
    type ICalc =
        abstract member Add: int * int -> int
        default Multiply: int * int -> int
    interface ICalc with
        member _.Add(a, b) = a + b

// ============================================================================
// 15. EXTERN, ASSERT, CHECKED/UNCHECKED, LAZY & RECURSIVE BINDINGS
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
// 16. RAW/INTERPOLATED/VERBATIM/TRIPLE/CHAR OVERLAP & ESCAPES
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