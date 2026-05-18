// ============================================================================
// F# LEXER STRESS TEST: NEXUS MODE (F# 8.0+ SPEC-COMPLIANT)
// ============================================================================
// Targets: directive trailing punctuation, #line columns, bitwise #if, $env: paths,
// CDATA/XML entities, let...in nesting, function/match alternation, begin/end/do/done,
// surrogates/variation selectors, hex float boundaries, type-provider static params,
// dynamic lookup, (* vs ( * ) traps, and explicit-offside recovery.

#light "off"
#indent "off"

namespace LexerNexus
open System

// ============================================================================
// 1. DIRECTIVE TRAILING PUNCTUATION, PROTOCOLS & COLUMN SPECS
// ============================================================================
module DirectiveEdgeCases =
    // #r with protocols, query strings, env vars, trailing punctuation
    #r "nuget: Newtonsoft.Json, 13.0.3";
    #r "$env:LIB_PATH/mylib.dll",
    #r "http: https://cdn.example.com/lib.dll?token=abc&v=2".
    #r "github: fsprojects/FSharp.Data, src/FSharp.Data.fsx"
    
    // #line with negative, zero, column spec
    #line -50 "transpiled.fs" 12
    #line 0 "generated.fs" 0
    #line 100 "source.fs" 45;
    #line hidden,
    #line default.
    
    // #indent / #time variants
    #indent 4
    #indent "off"
    #time "quiet"
    #time "on",
    #help.

// ============================================================================
// 2. BITWISE PREPROCESSOR & DEFINED() NESTING
// ============================================================================
module BitwisePreprocessor =
    #if defined(DEBUG) &&& defined(TRACE) ||| defined(RELEASE)
    #define MODE_BIT
    #elif ~~~ defined(EXPERIMENTAL) ^^^ defined(LEGACY)
    #define MODE_BIT_FALLBACK
    #else
    #define MODE_BIT_DEFAULT
    #endif
    
    #undef MODE_BIT
    #warning "Bitwise path undefined"
    #error "Legacy unsupported"

// ============================================================================
// 3. XML DOCS WITH CDATA, ENTITIES & EMBEDDED CODE
// ============================================================================
module XMLDocEntities =
    /// <summary>
    /// Test &amp; &lt; &gt; &quot; &apos; &nbsp; &copy; &reg;
    /// <![CDATA[ <script>alert("not tokenized")</script> ]]>
    /// <see cref="T:System.String"/> <paramref name="x"/>
    /// </summary>
    /// <param name="x">Input with (* not comment *) and // not comment</param>
    (** Block doc with &amp; &lt; &gt; and <em>tags</em> **)
    let docEntityTrap = 1

// ============================================================================
// 4. LET...IN NESTING, FUNCTION/MATCH ALTERNATION & GUARDS
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
// 5. EXPLICIT BLOCKS, SEMICOLONS & OFFSIDE RECOVERY
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

// ============================================================================
// 6. SURROGATE PAIRS, VARIATION SELECTORS & ZERO-WIDTH MARKS
// ============================================================================
module UnicodeSurrogates =
    let emoji\uFE0F = "variant"
    let emoji\uFE00 = "base"
    let music = "\U0001D11E"
    let cafe\u200C\u0301 = "coffee variant"
    let family\u200D = "family"
    let `type` = 1
    let `with	tab` = 2
    let `123 456` = 3
    let `α β γ` = 4
    let `∑ ∏ ∫` = 5

// ============================================================================
// 7. HEX FLOAT BOUNDARIES & SUFFIX ADJACENCY
// ============================================================================
module HexFloatBoundaries =
    let hf1 = 0x0.0p0
    let hf2 = 0x1.0p-0
    let hf3 = 0xFF.FFp10f
    let hf4 = 0xAp-5d
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

// ============================================================================
// 8. DYNAMIC LOOKUP & SYMBOLIC CHAIN REVISITED
// ============================================================================
module DynamicAndChains =
    let dyn = System.Dynamic.ExpandoObject()
    let get = dyn?Prop
    let set = dyn?Prop <- 1
    let (?<-) (o: obj) (n: string) (v: obj) = o
    let t1 = obj?name
    let t2 = obj?name <- val
    let t3 = (?<-) obj "n" v
    let ( +& ) a b = a + b
    let ( &+ ) a b = a + b
    let ( ~+ ) a b = a
    let ( +~ ) a b = b
    let (⊕) a b = a + b
    let (⊗) a b = a * b
    let (≠) a b = a <> b
    let (≤) a b = a <= b

// ============================================================================
// 9. QUOTATION DEPTH, ACTIVE PATTERNS & PIPE COLLISIONS
// ============================================================================
module QuotationsAndPipes =
    let q1 = <@ 1 + 1 @>
    let q2 = <@@ fun x -> x @@>
    let q3 = <@ <@ nested @> @>
    let q4 = <@@ <@@ double @@> @@>
    let q5 = <@ let! x = async { return 1 } in x @>
    let q6 = <@ match 1 with | 1 -> "a" | _ -> "b" @>
    let (|α-β|_|) n x = if x % n = 0 then Some () else None
    let (|π/2|) x = x + 1
    let (|A|B|C|) x = match x with 1 -> A | 2 -> B | _ -> C
    let test x =
        match x with
        | α-β 3 -> "div"
        | A | B -> "ab"
        | C -> "c"
        | _ -> "other"
    let piped = 1 |> (+) 2 |> ( * ) 3

// ============================================================================
// 10. META-OPS, TYPE PROVIDERS & GLOBAL::
// ============================================================================
module MetaAndTypeProviders =
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
// 11. ATTRIBUTES, DEFAULTS & SIGNATURE SYNTAX
// ============================================================================
module AttrsAndSigs =
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
// 12. COMMENT/STRING OVERLAP & ESCAPE TRAPS
// ============================================================================
module CommentStringOverlap =
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