// ============================================================================
// F# LEXER STRESS TEST: ZENITH MODE (F# 8.0+ SPEC-COMPLIANT)
// ============================================================================
// Targets: directive arg parsing, #if defined(), task CE, #raw delimiters,
// $interpolation formats, (* vs ( * ) traps, // vs /// vs (**), global::,
// open type, ;;, function, upcast/downcast, meta-ops, byref/fixed/ref struct,
// inline const, module rec, #seq<'T>, val sigs, attributes, preprocessor ranges,
// indentation toggling, zero-width/combining marks, backticks, comment overlap.

#light "off"
#indent "off"

namespace LexerZenith
open System

// ============================================================================
// 1. DIRECTIVE ARGUMENT PARSING & PROTOCOLS
// ============================================================================
module DirectiveArgParsing =
    // #nowwarn: space-separated, ranges, mixed
    #nowarn 40 41 52 40-44 50-52 9
    
    // #r with multiple protocols (F# 8+ scripting)
    #r "nuget: Newtonsoft.Json, 13.0.3"
    #r "package: System.Text.Json, 8.0.0"
    #r "github: fsprojects/FSharp.Data, src/FSharp.Data.fsx"
    #r "http: https://example.com/lib.dll"
    
    // #I & #load with multiple quoted args
    #I "./packages" "./lib" "C:\refs"
    #load "a.fsx" "b.fsx" "c.fsx"
    
    // #line variations
    #line 100 "generated.fs"
    #line default
    #line hidden
    
    // #time & #indent & #help
    #time "on"
    #indent "on"
    #help

// ============================================================================
// 2. PREPROCESSOR: defined(), RANGES, PARENTHESES, not
// ============================================================================
module PreprocessorVariants =
    #if (DEBUG || TEST) && not CI && defined(EXPERIMENTAL)
    #define MODE_ALPHA
    #warning "Experimental path active"
    #elif not (DEBUG || TEST) && defined(LEGACY)
    #define MODE_BETA
    #error "Legacy mode unsupported"
    #else
    #define MODE_FALLBACK
    #endif

// ============================================================================
// 3. F# 8.0 task CE & match!/try!/yield!/return!
// ============================================================================
module TaskCEAndBangs =
    type TaskBuilder() =
        member _.Bind(t, f) = t.Bind(f)
        member _.Return(x) = Task.FromResult(x)
        member _.Zero() = Task.CompletedTask
        member _.Combine(a, b) = a.ContinueWith(fun _ -> b)
        member _.Delay(f) = Task.Run(f)
        member _.For(xs, f) = Task.WhenAll(xs |> Seq.map f)
        member _.While(g, b) = Task.Run(fun () -> while g() do b())
        member _.TryFinally(b, c) = try b() finally c()
        member _.TryWith(b, h) = try b() with e -> h e

    let task = TaskBuilder()

    let computeAsync () =
        task {
            let! (Some x) = Some 5
            use! r = new System.IO.MemoryStream()
            do! Task.Delay 100
            try! Task.FromResult 42
            with | _ -> 0
            finally ()
            match! Task.FromResult "ok" with
            | "ok" -> "success"
            | _ -> "fail"
            return x
        }

    // Lexical note: `let!`, `use!`, `do!`, `try!`, `yield!`, `return!`, `match!`
    // are tokenized as KEYWORD + BANG by the lexer. Parser combines them.

// ============================================================================
// 4. # RAW STRINGS & $ INTERPOLATED FORMATS
// ============================================================================
module RawAndInterpolationFormats =
    // Exact delimiter counting required: N #s before " require N #s after "
    let r1 = #"raw with " quotes \n"#
    let r2 = ##"raw ## delimiter and " quotes "##
    let r3 = ###"raw ### delimiter "###
    
    // Interpolated raw with alignment & format specifiers
    let ir1 = $#"val: {42,10:f2}" #
    let ir2 = $##"nested: {sprintf "%d" (1+1),-5:E} {{escaped}}"##
    let ir3 = $###"fmt: {DateTime.Now:yyyy-MM-dd} and {{braces}}"###
    
    // Verbatim + Interpolated (F# 8+)
    let vi1 = $@"path: {__SOURCE_DIRECTORY__} and {{not escaped}}"
    
    // Boundary traps
    let edge1 = @"#{not raw}"          // verbatim starting with #
    let edge2 = """#{not raw}"""       // triple-quoted
    let edge3 = $"raw? #{not raw}"     // interpolated, raw-like but not
    let edge4 = $"""triple {{interp}}""" // triple-quoted interpolated

// ============================================================================
// 5. (* VS ( * ) VS // VS /// VS (**) TRAPS
// ============================================================================
module CommentAndOperatorTraps =
    // (* starts BLOCK COMMENT
    let blocked = (* this is a comment *)
    
    // ( * ) is INFIX OPERATOR (spaces prevent comment start)
    let op1 = ( * ) 2 3
    let op2 = 5 |> ( * ) 2
    
    // ( *  is invalid but lexer must tokenize as LPAREN OP
    // let broken = 1 ( * 2 // syntax error, but lexically LPAREN OP INT
    
    // // vs /// vs (***) vs (* *)
    /// <summary>XML doc line</summary>
    (** Block doc with ***)
    let x = "http://example.com // not comment"
    let y = @"C:\path\file // not comment"
    let z = """Line 1 // not comment
    Line 2 (* not block comment *)
    Line 3"""

// ============================================================================
// 6. QUALIFIED NAMES, OPEN VARIANTS & ;; SEPARATORS
// ============================================================================
module QualifiedAndOpenVariants =
    // global:: qualified names
    let globalInt = global::System.Int32.MaxValue
    let globalType = global::System.Object
    
    // open type & multiple opens on one line
    open type System.Math
    open System; open System.Collections.Generic
    
    // ;; evaluation separators
    let x = 1
    let y = 2 ;;
    printfn "%d" (x + y) ;;
    
    // Nested ;;
    let f () =
        let a = 1 ;;
        a + 2 ;;

// ============================================================================
// 7. FUNCTION SHORTHAND, UP/DOWNCAST, META-OPS
// ============================================================================
module FunctionCastsMetaOps =
    // `function` keyword shorthand
    let classify = function
        | 0 -> "zero"
        | n when n > 0 -> "positive"
        | _ -> "other"
        
    // upcast/downcast are reserved keywords
    let toObj (x: string) = upcast x
    let fromObj (x: obj) = downcast x :?> string
    
    // meta-ops: typedefof, typeof, sizeof, nameof, enum<...>
    let t = typedefof<System.Collections.Generic.List<_>>
    let ty = typeof<int>
    let sz = sizeof<int>
    let n = nameof(System.String)
    type Core = A = 0 | B = 1
    let e = enum<Core> 1

// ============================================================================
// 8. BYREF/FIXED/REF STRUCT & & PATTERNS
// ============================================================================
module ByrefFixedRefStruct =
    type Span<'T> = ref struct
        val mutable pointer: nativeint
        val mutable length: int
    end
    
    let arr = [|1; 2; 3|]
    let inline addByref (x: byref<int>) (y: inref<int>) = x + y
    let outParam (z: outref<string>) = z <- "hello"
    
    fixed (p = &arr.[0], q = &arr.[1]) do
        let v1 = NativePtr.read p
        let v2 = NativePtr.read q
        v1 + v2
        
    // & address-of vs & pattern
    let matchByref (x: int byref) =
        match &x with
        | &v -> v + 1

// ============================================================================
// 9. INLINE CONST, MODULE REC, #SEQ, VAL SIGS
// ============================================================================
module InlineConstModuleRec =
    inline const Max = 100
    
    module rec RecA =
        type A = { Val: int }
        let process (x: RecB.B) = x.Val
    and RecB =
        type B = { Val: int }
        let create v = { Val = v }
        
    // Flexible type syntax
    let sum (xs: #seq<int>) = Seq.sum xs
    
    // Signature-file syntax (valid in .fsi, tests lexer robustness)
    // val compute: int -> int
    // val mutable counter: int
    
    type ICalc =
        abstract member Add: int * int -> int
        abstract member Multiply: int * int -> int
        default Multiply: int * int -> int

// ============================================================================
// 10. ZERO-WIDTH/COMBINING MARKS, BACKTICKS, UNICODE
// ============================================================================
module UnicodeAndBackticks =
    // Zero-width chars (UAX #31 compliant)
    let cafe\u200C = "coffee"
    let family\u200D = "family"
    let zeroWidthSpace\u200B = "zwsp"
    
    // Combining marks
    let naïve = "na\u00EFve"
    let café = "cafe\u0301"
    
    // Backtick identifiers
    let `type` = 1
    let `with` = 2
    let `when` = 3
    let `123` = "starts with digit"
    let ` ` = "contains space"
    let `α β` = "greek with spaces"
    let `(+@)` = fun a b -> a + b
    let `∑!` = fun xs -> List.sum xs
    
    // Newline inside backtick is ILLEGAL (lexer must error/reject)
    // let `broken\nid` = 0

// ============================================================================
// 11. ATTRIBUTES & DEFAULT INTERFACE IMPL
// ============================================================================
module AttributesAndDefaults =
    [<assembly: AssemblyVersion("1.0.0.0")>]
    [<module: CLIMutable>]
    [<method: Obsolete("Use new API", false)>]
    [<param: ParamArray>]
    [<return: MarshalAs(UnmanagedType.BStr)>]
    [<Literal; Measure>] type kg
    
    [<AutoOpen>]
    module Helpers = let help () = "help"
    
    [<RequireQualifiedAccess>]
    type Status = Active | Inactive
    
    type IDoSomething =
        abstract member Do: unit -> unit
        default _.Do() = ()
        
    interface IDoSomething with
        member _.Do() = printfn "Done"

// ============================================================================
// 12. OFFSIDE TOGGLING & BLANK LINES
// ============================================================================
module OffsideToggleAndBlanks =
    #light "off"
    begin
        let x = 1;
        let y = 2;
        if true then
            let z = 3;
            z + x
        else
            y;
    end;
    #light "on"
    
    // Mixed explicit/implicit after toggle
    let mixed =
        let a = 1;
        let b = 2
        a + b
    
    // Blank lines MUST NOT trigger DEDENT
    let blankTest =
        if true then

            let x = 1
            x + 1
        else
            0