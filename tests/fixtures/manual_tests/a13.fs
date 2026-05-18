// ============================================================================
// F# LEXER STRESS TEST: OMEGA-II MODE (F# 8.0+ SPEC-COMPLIANT)
// ============================================================================
// Targets: #line column specs, bitwise #if, $env: directive args, CDATA/XML in docs,
// let...in nesting, function/match alternation, begin/end/do/done blocks,
// surrogate pairs/variation selectors, hex float boundaries, directive arg punctuation,
// and explicit-offside recovery. All constructs are lexically valid.

#light "off"
#indent "off"

namespace LexerOmegaII
open System

// ============================================================================
// 1. #LINE COLUMN SPECS & NEGATIVE/ZERO LINE NUMBERS
// ============================================================================
module LineColumnSpecs =
    // #line supports optional column: #line <line> "<file>" <column>
    #line -50 "transpiled.fs" 12
    #line 0 "generated.fs" 0
    #line 100 "source.fs" 45
    #line hidden
    #line default

// ============================================================================
// 2. BITWISE PREPROCESSOR OPS & DEFINED() NESTING
// ============================================================================
module BitwisePreprocessor =
    // F# preprocessor supports bitwise: &&& (AND), ||| (OR), ^^^ (XOR), ~~~ (NOT)
    #if defined(DEBUG) &&& defined(TRACE) ||| defined(RELEASE)
    #define MODE_BIT
    #elif ~~~ defined(EXPERIMENTAL) ^^^ defined(LEGACY)
    #define MODE_BIT_FALLBACK
    #else
    #define MODE_BIT_DEFAULT
    #endif

// ============================================================================
// 3. ENVIRONMENT VARIABLES IN #R & TRAILING PUNCTUATION
// ============================================================================
module EnvVarDirectives =
    // FSI supports $env: protocol for environment variables
    #r "$env:LIB_PATH/mylib.dll"
    #r "$env:NUGET_PACKAGES/FSharp.Core/8.0.0/lib/netstandard2.1/FSharp.Core.dll"
    
    // Trailing semicolons, commas, dots after directives (lexer must not consume them)
    #r "System.Runtime";
    #r "System.Collections",
    #r "System.Linq".
    #nowarn 40 41.
    #line 100 "file.fs";
    #time "on",
    #indent 4.

// ============================================================================
// 4. XML DOCS WITH CDATA, ENTITIES & EMBEDDED SYMBOLS
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
// 5. LET...IN NESTING & FUNCTION/MATCH ALTERNATION
// ============================================================================
module LetInAndFunction =
    // Nested let...in with pattern guards and as-bindings
    let result =
        let a = 5 in
        let b = match a with
            | 1 | 2 | 3 as n when n > 0 -> n * 2
            | 4 | 5 -> 10
            | _ when a < 0 -> 0
            | _ -> -1
        in b + a
        
    // function keyword with exhaustive patterns
    let classify = function
        | [] -> "empty"
        | [_] -> "single"
        | [x; y] as lst when x < y -> "two ascending"
        | [x; y] -> "two"
        | _ -> "many"

// ============================================================================
// 6. EXPLICIT BLOCKS: BEGIN/END/DO/DONE & SEMICOLONS
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
    
    // Mixed explicit/implicit after toggle
    let mixed =
        begin
            let x = 1
            x + 2
        end

// ============================================================================
// 7. SURROGATE PAIRS, VARIATION SELECTORS & ZERO-WIDTH MARKS
// ============================================================================
module UnicodeSurrogates =
    // Variation Selector-1 (U+FE00) to VS-16 (U+FE0F)
    let emoji\uFE0F = "variant"
    let emoji\uFE00 = "base"
    
    // Surrogate pairs (outside BMP, e.g., musical symbol G clef U+1D11E)
    // Represented as high/low surrogate in UTF-16, but valid in F# source as \U0001D11E
    let music = "\U0001D11E"
    
    // Zero-width non-joiner/joiner + combining acute
    let cafe\u200C\u0301 = "coffee variant"
    let family\u200D = "family"
    
    // Backticks with tabs, multiple spaces, Unicode, digits
    let `type` = 1
    let `with	tab` = 2
    let `123 456` = 3
    let `α β γ` = 4
    let `∑ ∏ ∫` = 5

// ============================================================================
// 8. HEX FLOAT BOUNDARIES & SUFFIX ADJACENCY
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
    
    // Adjacent suffixes and bases
    let adj1 = 0xFFuL
    let adj2 = 0o777s
    let adj3 = 0b1010us
    let adj4 = 1_000_000n
    let adj5 = 1.0_0_0m
    let adj6 = .5e2f
    let adj7 = 1.e2d

// ============================================================================
// 9. DYNAMIC LOOKUP & SYMBOLIC CHAIN REVISITED
// ============================================================================
module DynamicAndChains =
    let dyn = System.Dynamic.ExpandoObject()
    let get = dyn?Prop
    let set = dyn?Prop <- 1
    
    // (?<-) definition
    let (?<-) (o: obj) (n: string) (v: obj) = o
    
    // Adjacency traps
    let t1 = obj?name
    let t2 = obj?name <- val
    let t3 = (?<-) obj "n" v
    
    // Symbolic chains
    let ( +& ) a b = a + b
    let ( &+ ) a b = a + b
    let ( ~+ ) a b = a
    let ( +~ ) a b = b
    let (⊕) a b = a + b
    let (⊗) a b = a * b
    let (≠) a b = a <> b
    let (≤) a b = a <= b

// ============================================================================
// 10. QUOTATION DEPTH, ACTIVE PATTERNS & PIPE COLLISIONS
// ============================================================================
module QuotationsAndPipes =
    let q1 = <@ 1 + 1 @>
    let q2 = <@@ fun x -> x @@>
    let q3 = <@ <@ nested @> @>
    let q4 = <@@ <@@ double @@> @@>
    let q5 = <@ let x = 1 in x @>
    let q6 = <@ match 1 with | 1 -> "a" | _ -> "b" @>
    
    let (|Div|_|) n x = if x % n = 0 then Some () else None
    let (|A|B|C|) x = match x with 1 -> A | 2 -> B | _ -> C
    
    let test x =
        match x with
        | Div 3 -> "div"
        | A | B -> "ab"
        | C -> "c"
        | _ -> "other"
    
    // Pipe vs pattern |
    let piped = 1 |> (+) 2 |> ( * ) 3

// ============================================================================
// 11. META-OPS, GLOBAL::, & TYPE CONSTRAINT REVISITED
// ============================================================================
module MetaAndConstraints =
    type Core = A = 0 | B = 1
    let e = enum<Core> 1
    let t = typedefof<System.Collections.Generic.List<_>>
    let ty = typeof<int>
    let sz = sizeof<int>
    let n = nameof(System.String)
    let g = global::System.Int32.MaxValue
    
    type Gen<'T when 'T : unmanaged and 'T : comparison and 'T : null> =
        ref struct
            val mutable ptr: nativeint
            val mutable len: int
        end

// ============================================================================
// 12. ATTRIBUTES, DEFAULTS & SIGNATURE SYNTAX
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
        
    // val declarations (valid in .fsi)
    // val compute: int -> int
    // val mutable counter: int