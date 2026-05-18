// ============================================================================
// F# LEXER STRESS TEST: QUANTUM MODE (F# 8.0+ SPEC-COMPLIANT)
// ============================================================================
// Targets: directive protocols, units-of-measure, flexible types, static constraints,
// #light mid-toggle, Unicode normalization, active patterns, quotation depth,
// dynamic lookup, pipe/compose chains, hex float adjacency, (* vs ( * ) traps,
// XML doc CDATA/entities, let...in nesting, function guards, explicit blocks,
// extern P/Invoke, assert/checked/unchecked, val sigs, attributes, type providers,
// and comment/string overlap. All constructs are lexically valid.

#light "off"
#indent "off"

namespace LexerQuantum
open System
open System.Runtime.InteropServices

// ============================================================================
// 1. DIRECTIVE PROTOCOL PARSING & TRAILING PUNCTUATION
// ============================================================================
module DirectiveProtocolsAndEdges =
    #r "nuget: Newtonsoft.Json, [13.0.1, 14.0.0)";
    #r "$env:LIB_PATH%20/my%2Flib.dll",
    #r "http: https://cdn.example.com/lib.dll?token=abc&v=2".
    #line -50 "تقرير_البيانات.fs" 15;
    #line 0 "generated_αβγ.fs" 0,
    #line hidden.
    #nowarn -1 0.5 100-200 40-44 45
    #if ~~~(defined(DEBUG) &&& defined(TRACE)) ||| defined(RELEASE) ^^^ defined(EXP)
    #define MODE_BIT_COMPLEX
    #elif not defined(LEGACY) &&& (defined(ALPHA) ||| defined(BETA))
    #define MODE_BIT_FALLBACK
    #endif
    #undef MODE_BIT_COMPLEX
    #warning "Complex path disabled"
    #error "Legacy not supported"
    #pragma warning disable 40,41
    #help
    #time "verbose"
    #indent 4
    #define _123
    #invalidDirective "test"

// ============================================================================
// 2. UNITS OF MEASURE & FLEXIBLE TYPES
// ============================================================================
module UnitsAndFlexibleTypes =
    type kg = kg
    type m = m
    type s = s
    let force = 10.0<kg*m/s^2>
    let energy = 5.0<kg*m^2/s^2>
    let zero = 0.0<kg*m/s>
    type Physics<'T when 'T : (static member (+): 'T * 'T -> 'T) and 'T : null> =
        member __.Mass : 'T<kg> = Unchecked.defaultof<'T>
    let inline scale<'T, 'U when 'U : (static member (*): 'T * float -> 'U)> (x: 'T) = x * 1.0<unit>
    let sum (xs: #seq<int>) = Seq.sum xs
    let print (items: #seq<string>) = Seq.iter printfn "%s" items

// ============================================================================
// 3. #LIGHT MID-EXPRESSION TOGGLING & STACK RECOVERY
// ============================================================================
module MidToggleStackRecovery =
    let toggleTest =
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
        if true then
            let z = 3
            z + 1
        else
            0

// ============================================================================
// 4. EXTREME UNICODE, BIDIRECTIONAL & NORMALIZATION
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

// ============================================================================
// 5. ACTIVE PATTERNS & PARAMETERIZED NAMES
// ============================================================================
module ActivePatterns =
    let (|Div|_|) n x = if x % n = 0 then Some () else None
    let (|π|) x = x + 1
    let (|A|B|C|) x = match x with 1 -> A | 2 -> B | _ -> C
    let (|InRange|_|) min max x = if x >= min && x <= max then Some () else None
    let (|Even|Odd|) x = if x % 2 = 0 then Even else Odd
    let (|Single|) x = x + 1
    let (|αβγ|_|) n x = if x % n = 0 then Some () else None

// ============================================================================
// 6. QUOTATION DEPTH & CE CONTEXTUAL BANGS
// ============================================================================
module QuotationsAndCEs =
    let q1 = <@ 1 + 1 @>
    let q2 = <@@ fun x -> x @@>
    let q3 = <@ <@ nested @> @>
    let q4 = <@@ <@@ double @@> @@>
    let q5 = <@ let! x = async { return 1 } in x @>
    let q6 = <@ match 1 with | 1 -> "a" | _ -> "b" @>
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
// 7. DYNAMIC LOOKUP & PIPE/COMPOSE CHAINS
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

// ============================================================================
// 8. HEX FLOATS, SCIENTIFIC EDGES & DIGIT SEPARATORS
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

// ============================================================================
// 9. (* VS ( * ) TRAPS & XML DOCS WITH CDATA/ENTITIES
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
// 10. LET...IN NESTING & FUNCTION GUARDS
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
// 11. EXPLICIT BLOCKS, SEMICOLONS & OFFSIDE RECOVERY
// ============================================================================
module ExplicitBlocks =
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
            let x = 1
            x + 1
        else
            0

// ============================================================================
// 12. META-OPS, TYPE PROVIDERS, ATTRIBUTES & SIGNATURES
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
// 13. EXTERN, ASSERT, CHECKED/UNCHECKED, LAZY & RECURSIVE BINDINGS
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
// 14. RAW/INTERPOLATED/VERBATIM/TRIPLE/CHAR OVERLAP & ESCAPES
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