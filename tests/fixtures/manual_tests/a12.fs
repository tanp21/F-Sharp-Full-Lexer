// ============================================================================
// F# LEXER STRESS TEST: APEX-II MODE (F# 8.0+ SPEC-COMPLIANT)
// ============================================================================
// Targets: dynamic operators (?/?<-), #indent <int>, #time "quiet", #line -N,
// complex #r argument parsing, multi-ref directives, protocol prefixes,
// verbatim/raw/interp directive args, trailing whitespace/comments after directives,
// overlapping #nowarn ranges, nested #if defined(), and operator boundary rules.

#light "off"
#indent "off"

namespace LexerApexII
open System

// ============================================================================
// 1. DYNAMIC LOOKUP OPERATORS & BOUNDARY RULES
// ============================================================================
module DynamicOperators =
    // ? is valid for dynamic invocation. ?<- is valid for assignment.
    let dyn = System.Dynamic.ExpandoObject()
    let get = dyn?Name
    let set = dyn?Name <- "value"
    
    // (?<-) is a valid infix operator definition
    let (?<-) (target: obj) (name: string) (value: obj) = target
    
    // Adjacency traps: ? vs ?- vs ?<- vs ? .
    let trap1 = obj?prop
    let trap2 = obj?prop <- 1
    let trap3 = (?<-) obj "name" 1
    let trap4 = obj? prop // syntax error, but lexically: obj ? prop
    let trap5 = obj ?<- prop // syntax error, but lexically: obj ?<- prop

// ============================================================================
// 2. EXPLICIT #INDENT & #TIME QUIET MODE
// ============================================================================
module IndentAndTimeDirectives =
    // #indent accepts quoted strings or bare integers (F# 8+)
    #indent 4
    #indent "off"
    #indent "on"
    
    // #time supports "on", "off", "quiet"
    #time "on"
    #time "off"
    #time "quiet"
    
    // Mid-toggle combinations
    #indent 8
    #time "quiet"
    #indent "on"

// ============================================================================
// 3. NEGATIVE #LINE & GENERATED CODE MAPPING
// ============================================================================
module NegativeLineMapping =
    // Negative line numbers are valid for transpiled/generated code headers
    #line -100 "transpiled.fs"
    #line 0 "generated.fs"
    #line 1 "source.fs"
    #line hidden
    #line default

// ============================================================================
// 4. COMPLEX #R DIRECTIVE ARGUMENT PARSING
// ============================================================================
module ReferenceDirectiveParsing =
    // Multiple protocols, version ranges, prerelease, commit SHAs, query params
    #r "nuget: FSharp.Core, 8.0.0"
    #r "nuget: Newtonsoft.Json, [13.0.1, 14.0.0)"
    #r "nuget: FSharp.Data, 5.0.0-beta.3"
    #r "github: fsprojects/FSharp.Control.Reactive, src/FSharp.Control.Reactive.fsx"
    #r "github: user/repo, abc123def"
    #r "http: https://example.com/lib.dll?token=abc&version=1"
    #r "file: C:/libs/custom.dll"
    
    // Multiple references on one line (F# 8+ FSI)
    #r "System.Runtime" "System.Collections" "System.Linq"
    
    // Trailing whitespace & comments after directive
    #r "System.Runtime"   
    #r "System.Collections" // standard lib
    #r "System.Linq"   // trailing spaces and comment
    
    // Mixed case protocols & missing protocol (defaults to assembly)
    #r "NuGet: FSharp.Core, 8.0.0"
    #r "GitHub: user/repo, main"
    #r "System.Runtime" // no protocol = assembly reference

// ============================================================================
// 5. VERBATIM/RAW/INTERPOLATED DIRECTIVE ARGUMENTS
// ============================================================================
module StringDirectiveArgs =
    // Verbatim paths with spaces & quotes
    #r @"C:\Program Files\MyLib.dll"
    #r @"C:\Path\With""Quotes\lib.dll"
    
    // Raw paths with custom delimiters
    #r #"C:\libs\MyLib.dll"#
    #r ##"C:\libs\MyLib##.dll"##
    
    // Interpolated paths (FSI only, but lexically valid)
    let libPath = "C:\libs"
    #r $"{libPath}\MyLib.dll"
    #r $"{libPath}""Core.dll"{libPath}""
    
    // Boundary traps: verbatim vs raw vs interpolated adjacency
    let edge1 = @"#{not raw}"
    let edge2 = """#{not raw}"""
    let edge3 = $"raw? #{not raw}"

// ============================================================================
// 6. OVERLAPPING #NOWARN RANGES & MULTIPLE ARGS
// ============================================================================
module WarningRanges =
    // Ranges, overlaps, gaps, mixed formats
    #nowarn 40 41 52 40-44 50-52 9 1000-1005 1003 45-48
    #nowarn 40-42 43-45 46-48 40-48
    
    // Negative/zero ranges are lexically invalid but tests recovery
    #nowarn 0 1 2 -5 3-1 0-0

// ============================================================================
// 7. NESTED #IF DEFINED() & PARENTHESES
// ============================================================================
module DefinedExpressions =
    #if defined(DEBUG) && not defined(RELEASE) || (VER >= 8)
    #define MODE_ALPHA
    #elif defined(EXPERIMENTAL) && not (defined(DEBUG) || defined(RELEASE))
    #define MODE_BETA
    #elif defined(LEGACY)
    #define MODE_FALLBACK
    #else
    #define MODE_DEFAULT
    #endif
    
    #undef MODE_ALPHA
    #warning "Alpha undefined"
    #error "Legacy unsupported"

// ============================================================================
// 8. #LOAD & #I WITH MULTIPLE PATHS & SEPARATORS
// ============================================================================
module LoadAndInclude =
    // Multiple files, mixed separators, relative paths
    #load "a.fsx" "b.fsx" "c.fsx"
    #load "./scripts/1.fsx" "../lib/2.fsx" "C:\dev\3.fsx"
    
    // #I with multiple include directories
    #I "./bin" "../lib" "C:\\refs" "D:\packages"

// ============================================================================
// 9. OFFSIDE MID-TOGGLE & BLANK LINE HANDLING
// ============================================================================
module OffsideMidToggle =
    #light "off"
    begin
        let x = 1;
        #light "on"
        let y = 2
        x + y
    end;
    #light "off"
    #indent "off"
    
    // Blank lines MUST NOT trigger DEDENT in #light "on"
    let blankTest =
        if true then

            let x = 1
            x + 1
        else
            0

// ============================================================================
// 10. OPERATOR BOUNDARIES & LONGEST-MATCH REVISITED
// ============================================================================
module OperatorBoundaries =
    // ? and ?<- must not merge with adjacent symbols
    let op1 = obj?name
    let op2 = obj?name <- val
    let op3 = (?<-) obj "n" v
    
    // Standard symbolic chains (maximal munch)
    let ( +& ) a b = a + b
    let ( &+ ) a b = a + b
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
    let (→) a b = b a
    let (←) a b = a b

// ============================================================================
// 11. HEX FLOATS, DIGIT SEPARATORS & SCIENTIFIC EDGES
// ============================================================================
module NumericEdge =
    let hf1 = 0x1.8p1
    let hf2 = 0x1.0p-10
    let hf3 = 0xAp2f
    let hf4 = 0xFFp0d
    let ds1 = 1_2_3_4
    let ds2 = 1.0_0_0
    let sci1 = .5e2
    let sci2 = 1.e2
    let sci3 = 1.23e-4f

// ============================================================================
// 12. ACTIVE PATTERNS & QUOTATION DEPTH TRACKING
// ============================================================================
module APAndQuotations =
    let (|Div|_|) n x = if x % n = 0 then Some () else None
    let (|A|B|C|) x = match x with 1 -> A | 2 -> B | _ -> C
    let q1 = <@ 1 + 1 @>
    let q2 = <@@ fun x -> x @@>
    let q3 = <@ <@ nested @> @>
    let q4 = <@@ <@@ double @@> @@>

// ============================================================================
// 13. META-OPS, ENUMS, TYPEDEFS & GLOBAL::
// ============================================================================
module MetaAndOpens =
    type Core = A = 0 | B = 1
    let e = enum<Core> 1
    let t = typedefof<System.Collections.Generic.List<_>>
    let ty = typeof<int>
    let sz = sizeof<int>
    let n = nameof(System.String)
    let g = global::System.Int32.MaxValue

// ============================================================================
// 14. SIGNATURES, INTERFACES, DEFAULTS & ATTRIBUTES
// ============================================================================
module SigsAndAttrs =
    [<assembly: AssemblyVersion("1.0.0.0")>]
    [<module: CLIMutable>]
    [<method: Obsolete("Use new API", false)>]
    type ICalc =
        abstract member Add: int * int -> int
        default Multiply: int * int -> int
    interface ICalc with
        member _.Add(a, b) = a + b

// ============================================================================
// 15. COMMENT/STRING/QUOTE OVERLAP & ESCAPE TRAPS
// ============================================================================
module CommentStringOverlap =
    /// <summary>Test &quot;quotes&quot; &amp; <see cref="T:System.String"/> &lt;tags&gt;</summary>
    (** Block doc with ***)
    let docTrap = "http://example.com // not comment"
    let verbatimDoc = @"C:\path\(* not comment *)"
    let tripleDoc = """Line 1 // not comment
    Line 2 (* not block comment *)
    Line 3"""
    let rawDoc = #"raw // and (* not comment *)"#
    let interpDoc = $"path: {__SOURCE_DIRECTORY__} // not comment"