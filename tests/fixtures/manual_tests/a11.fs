// ============================================================================
// F# LEXER STRESS TEST: VERTEX MODE (F# 8.0+ SPEC-COMPLIANT)
// ============================================================================
// Targets: directive protocols, open variants, #if defined(), XML doc markup,
// let...in, task CE bangs, ref struct constraints, #raw delimiter counting,
// $interpolation formats, (* vs ( * ) traps, #light mid-block toggling,
// zero-width/combining marks, backtick newline rejection, hex floats,
// active patterns, quotation depth, meta-ops, global::, ;;, val sigs,
// default interfaces, comment/string overlap, and indentation stack drift.

#light "off"
#indent "off"

namespace LexerVertex
open System

// ============================================================================
// 1. DIRECTIVE PROTOCOLS, RANGES & PREPROCESSOR DEFINED()
// ============================================================================
module DirectiveProtocols =
    #r "nuget: Newtonsoft.Json, 13.0.3; FSharp.Data, 4.2.5"
    #r "github: fsprojects/FSharp.Control.Reactive, src/FSharp.Control.Reactive.fsx"
    #r "http: https://example.com/lib.dll"
    #r "file: C:/libs/custom.dll"
    #I "./bin" "../lib" "C:\\refs"
    #load "a.fsx" "b.fsx" "c.fsx"
    #nowarn 40-44 50 52 9 1000-1005
    #if defined(DEBUG) && not defined(RELEASE) || (VER >= 8)
    #elif defined(EXPERIMENTAL)
    #define MODE_EXP
    #else
    #define MODE_FALLBACK
    #endif
    #line 200 "gen.fs"
    #line hidden
    #line default
    #time "on"
    #help

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
// 3. XML DOC MARKUP & COMMENT TRAPS
// ============================================================================
module XMLDocAndComments =
    /// <summary>Test &quot;quotes&quot; &amp; <see cref="T:System.String"/> &lt;tags&gt;</summary>
    /// <param name="x">Input with (* not a comment *) inside</param>
    (** Block doc with < and > symbols **)
    let docTrap = "http://example.com // not comment"
    let verbatimDoc = @"C:\path\(* not comment *)"
    let tripleDoc = """Line 1 // not comment
    Line 2 (* not block comment *)
    Line 3"""

// ============================================================================
// 4. LET...IN NESTING, FUNCTION SHORTHAND & TASK CE BANGS
// ============================================================================
module CEAndInNesting =
    let nestedIn = let a = 1 in let b = 2 in a + b
    let classify = function | 0 -> "zero" | n when n > 0 -> "pos" | _ -> "neg"
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
// 5. REF STRUCT, INLINE/STATIC MEMBERS & TYPE CONSTRAINTS
// ============================================================================
module RefAndConstraints =
    [<Struct>]
    type Span<'T when 'T : unmanaged and 'T : not null and 'T : comparison> =
        ref struct
            val mutable pointer: nativeint
            val mutable length: int
        end
    type Container<'T when 'T : equality> =
        inline static member Create(v: 'T) = { pointer = nativeint 0; length = 0 }

// ============================================================================
// 6. # RAW STRINGS & $ INTERPOLATED FORMATS
// ============================================================================
module StringFormats =
    let r1 = #"raw " quotes \n"#
    let r2 = ##"raw ## delimiter " quotes "##
    let ir1 = $#"val: {42,10:f2}" #
    let ir2 = $##"nested: {sprintf "%d" (1+1),-5:E} {{escaped}}"##
    let vi1 = $@"path: {__SOURCE_DIRECTORY__} and {{not escaped}}"
    let edge1 = @"#{not raw}"
    let edge2 = """#{not raw}"""
    let edge3 = $"raw? #{not raw}"

// ============================================================================
// 7. (* VS ( * ) VS ( * VS * ) TRAPS
// ============================================================================
module CommentOperatorTraps =
    let blocked = (* this is a comment *)
    let op1 = ( * ) 2 3
    let op2 = 5 |> ( * ) 2
    // Lexically valid but syntactically broken: tests state machine recovery
    // let partial1 = 1 ( * 2   // INT LPAREN OP INT
    // let partial2 = 1 * ) 2   // INT OP RPAREN INT

// ============================================================================
// 8. #LIGHT TOGGLING MID-BLOCK & INDENTATION STACK DRIFT
// ============================================================================
module OffsideMidBlock =
    #light "off"
    begin
        let x = 1;
        #light "on"
        let y = 2
        x + y
    end;
    #light "off"
    #indent "off"

// ============================================================================
// 9. BACKTICKS, UNICODE, ZERO-WIDTH & NEWLINE REJECTION
// ============================================================================
module UnicodeBackticks =
    let `type` = 1
    let `with` = 2
    let `\u0041` = "A"
    let cafe\u200C = "coffee"
    let family\u200D = "family"
    let café = "cafe\u0301"
    let `α β` = "greek"
    // ILLEGAL: lexer must reject or emit ERROR_TOKEN for newline inside backticks
    // let `broken\nid` = 0

// ============================================================================
// 10. HEX FLOATS, DIGIT SEPARATORS & SCIENTIFIC EDGES
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
// 11. ACTIVE PATTERNS & QUOTATION DEPTH TRACKING
// ============================================================================
module APAndQuotations =
    let (|Div|_|) n x = if x % n = 0 then Some () else None
    let (|A|B|C|) x = match x with 1 -> A | 2 -> B | _ -> C
    let q1 = <@ 1 + 1 @>
    let q2 = <@@ fun x -> x @@>
    let q3 = <@ <@ nested @> @>
    let q4 = <@@ <@@ double @@> @@>

// ============================================================================
// 12. META-OPS, ENUMS, TYPEDEFS & GLOBAL::
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
// 13. SIGNATURES, INTERFACES, DEFAULTS & ATTRIBUTES
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