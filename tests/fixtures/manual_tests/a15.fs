// ============================================================================
// F# LEXER STRESS TEST: ECLIPSE MODE (F# 8.0+ SPEC-COMPLIANT)
// ============================================================================
// Targets: extern/DllImport, assert, checked/unchecked, #line Unicode/cols,
// #r URL-encoded, #if bitwise nesting, #define/#undef edges, #pragma/#help/#time,
// #nowarn recovery, extreme nesting, RTL/variation identifiers, hex float adjacency,
// dynamic lookup boundaries, pipe/compose chains, offside stack corruption, and
// directive punctuation leakage. All constructs are lexically valid.

#light "off"
#indent "off"

namespace LexerEclipse
open System
open System.Runtime.InteropServices

// ============================================================================
// 1. EXTERN P/INVOKE & ASSERT/CHECKED/UNCHECKED KEYWORDS
// ============================================================================
module ExternAndContextualKeywords =
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

// ============================================================================
// 2. #LINE UNICODE FILENAMES & COLUMN SPECS
// ============================================================================
module LineUnicodeColumns =
    #line -50 "تقرير_البيانات.fs" 15
    #line 0 "generated_αβγ.fs" 0
    #line 100 "source_文件.fs" 45;
    #line hidden,
    #line default.

// ============================================================================
// 3. #R URL-ENCODED PATHS & PROTOCOL PARSING
// ============================================================================
module ReferenceURLEncoded =
    #r "file: C:/libs/My%20Library.dll"
    #r "nuget: FSharp.Data, 5.0.0-beta.3"
    #r "http: https://cdn.example.com/lib%2Fv2.0%2Fdata.dll"
    #r "github: fsprojects/FSharp.Control.Reactive, main"
    #r "$env:NUGET_PACKAGES/FSharp.Core/8.0.0/lib/netstandard2.1/FSharp.Core.dll";

// ============================================================================
// 4. EXTREME #IF BITWISE NESTING & DEFINED()
// ============================================================================
module BitwisePreprocessorExtreme =
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

// ============================================================================
// 5. #DIRECTIVE EDGE CASES & PUNCTUATION LEAKAGE
// ============================================================================
module DirectiveEdges =
    #pragma warning disable 40,41  // F# ignores #pragma, lexer tokenizes
    #help
    #time "verbose"
    #indent 2.5
    #nowarn -1 0.5 100-200
    #define _123
    #undef _123
    #invalidDirective "test"

// ============================================================================
// 6. EXTREME COMMENT/QUOTE/QUOTATION NESTING
// ============================================================================
module ExtremeNesting =
    (* Outer (* Inner (* Deepest *) Still Inner *) Back to Outer *)
    /// <summary>
    /// <![CDATA[ <script>alert("nested")</script> ]]>
    /// &amp; &lt; &gt; &quot; &apos; &nbsp; &copy; &reg;
    /// </summary>
    let q1 = <@ <@ <@ nested @> @> @>
    let q2 = <@@ <@@ <@@ typed @@> @@> @@>
    let raw1 = ###"raw ### with ### delimiter "###
    let interp1 = $###"fmt: {42,10:f2} and {{braces}}"###

// ============================================================================
// 7. RTL, VARIATION SELECTORS & COMBINING MARKS
// ============================================================================
module RTLAndVariation =
    let arabic\u0627\u0644\u0639\u0631\u0628\u064A\u0629 = "Arabic"
    let emoji\uFE0F = "variant"
    let emoji\uFE00 = "base"
    let cafe\u200C\u0301 = "coffee"
    let family\u200D = "family"
    let `‏RTL\‪LRE‬` = "bidirectional"
    let `α β γ δ ε ζ η θ` = "greek"

// ============================================================================
// 8. HEX FLOAT ADJACENCY & SCIENTIFIC EDGES
// ============================================================================
module HexAndScientificEdges =
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

// ============================================================================
// 10. OFFSIDE STACK CORRUPTION & MID-BLOCK TOGGLING
// ============================================================================
module OffsideStackCorruption =
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
    let blank =
        if true then

            let z = 3
            z + 1
        else
            0

// ============================================================================
// 11. LET...IN, FUNCTION/MATCH ALTERNATION & GUARDS
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
// 12. EXPLICIT BLOCKS, SEMICOLONS & RECOVERY
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

// ============================================================================
// 13. META-OPS, TYPE PROVIDERS & GLOBAL QUALIFIERS
// ============================================================================
module MetaAndProviders =
    type Core = A = 0 | B = 1
    let e = enum<Core> 1
    let t = typedefof<System.Collections.Generic.List<_>>
    let ty = typeof<int>
    let sz = sizeof<int>
    let n = nameof(System.String)
    let g = global::System.Int32.MaxValue
    type Json = JsonProvider<"data.json", SampleIsList=true, ResolutionFolder=__SOURCE_DIRECTORY__>
    type Xml = XmlProvider<Schema="schema.xsd", Global=true, ResolutionFolder=__SOURCE_DIRECTORY__>

// ============================================================================
// 14. ATTRIBUTES, DEFAULTS & SIGNATURE SYNTAX
// ============================================================================
module AttrsAndDefaults =
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