// ============================================================================
// F# LEXER STRESS TEST: HORIZON MODE (F# 8.0+ SPEC-COMPLIANT)
// ============================================================================
// Targets: global::, open type, ;;, function shorthand, upcast/downcast,
// typedefof, enum<...>, & address-of, object expressions, default interfaces,
// while/for loops, #r protocols, #nowarn ranges, #if defined(), signature syntax,
// and the (* vs ( * ) lexical trap. All constructs are lexically valid.

#light "off"
#indent "off"

namespace LexerHorizon
open System

// ============================================================================
// 1. QUALIFIED NAMES, OPEN VARIANTS & ;; SEPARATORS
// ============================================================================
module QualifiedAndOpen =
    // global:: qualified names
    let globalInt = global::System.Int32.MaxValue
    let globalType = global::System.Object
    
    // open type & multiple opens on one line
    open type System.Math
    open System; open System.Collections.Generic
    
    // ;; evaluation separators (valid in scripts & compiled code)
    let x = 1
    let y = 2 ;;
    printfn "%d" (x + y) ;;
    
    // Nested ;; (lexically valid, parser ignores extras)
    let f () =
        let a = 1 ;;
        a + 2 ;;

// ============================================================================
// 2. FUNCTION SHORTHAND & MATCH GUARDS
// ============================================================================
module FunctionAndGuards =
    // `function` is a keyword shorthand for `match ... with`
    let classify = function
        | 0 -> "zero"
        | n when n > 0 -> "positive"
        | n when n < 0 && n > -10 -> "small negative"
        | _ -> "other"
        
    // `match` with guards, `|` alternation, `as` bindings
    let test x =
        match x with
        | 1 | 2 | 3 as n when n < 5 -> "low"
        | 4 | 5 -> "mid"
        | _ when x > 10 -> "high"
        | _ -> "other"

// ============================================================================
// 3. CASTS, META-OPS & ENUM/TYPEDOFS
// ============================================================================
module CastsAndMetaOps =
    // upcast/downcast are reserved keywords, NOT operators
    let toObj (x: string) = upcast x
    let fromObj (x: obj) = downcast x :?> string
    
    // typedefof (distinct from typeof)
    let t = typedefof<System.Collections.Generic.List<_>>
    
    // enum syntax & enum<...> operator
    type Core = A = 0 | B = 1 | C = 2
    let e = enum<Core> 1
    
    // nameof, typeof, sizeof adjacency
    let n = nameof(System.String)
    let ty = typeof<int>
    let sz = sizeof<int>

// ============================================================================
// 4. & ADDRESS-OF, REF PATTERNS & | IN TYPES VS PATTERNS
// ============================================================================
module PatternsAndTypeDefs =
    // & address-of vs & pattern (F# 8+ byref matching)
    let arr = [|1; 2; 3|]
    let addr = &arr.[0]
    
    let matchByref (x: int byref) =
        match &x with
        | &v -> v + 1
        
    // | in type definitions vs pattern matching
    type Shape =
        | Circle of float
        | Rectangle of float * float
        | Triangle of float * float * float
        
    // Pattern | vs operator | vs pipe |>
    let isCircleOrRect = function
        | Circle _ | Rectangle _ -> true
        | _ -> false
        
    let piped = 1 |> (+) 2 |> ( * ) 3 // ( * ) avoids (* comment trap

// ============================================================================
// 5. OBJECT EXPRESSIONS & DEFAULT INTERFACE IMPL
// ============================================================================
module ObjectExpressions =
    type IDoSomething =
        abstract member Do: unit -> unit
        default _.Do() = ()
        
    // Object expression
    let obj = { new IDoSomething with member _.Do() = printfn "Done" }
    
    // Default interface implementation (F# 8+)
    type IWithDefault =
        abstract member Run: unit -> unit
        default _.Run() = printfn "Default run"
        
    interface IWithDefault with
        member _.Run() = printfn "Custom run"

// ============================================================================
// 6. CONTROL FLOW: WHILE, FOR TO/DOWNTO, TRY/WITH/FINALLY
// ============================================================================
module ControlFlow =
    // while...do
    let rec loop n =
        while n > 0 do
            printfn "%d" n
            n <- n - 1
            
    // for...to/downto...do
    let countUp () =
        for i = 1 to 10 do
            printfn "%d" i
            
    let countDown () =
        for i = 10 downto 1 do
            printfn "%d" i
            
    // try...with / try...finally
    let safeDiv a b =
        try
            a / b
        with
        | :? System.DivideByZeroException -> 0
        
    let withCleanup () =
        try
            printfn "Running"
        finally
            printfn "Cleanup"

// ============================================================================
// 7. USE BINDINGS, LAZY VALUES & TRY! IN CE
// ============================================================================
module UseAndLazy =
    // use binding
    let useStream () =
        use s = new System.IO.MemoryStream()
        s.Length
        
    // lazy values
    let lazyVal = lazy (printfn "Computed"; 42)
    let extracted = lazyVal.Force()
    
    // CE try! (tokenizes as `try` `!`)
    type AsyncBuilder() =
        member _.Bind(x, f) = async.Bind(x, f)
        member _.Return(x) = async.Return(x)
    let async = AsyncBuilder()
    
    let compute () =
        async {
            try! async.Return 42
            with | _ -> 0
            return 0
        }

// ============================================================================
// 8. PREPROCESSOR: PROTOCOLS, RANGES, DEFINED()
// ============================================================================
module PreprocessorProtocols =
    // #r with protocols (F# 8+ scripting)
    #r "package: Newtonsoft.Json, 13.0.1"
    #r "github: fsprojects/FSharp.Data, src/FSharp.Data.fsx"
    #r "http: https://example.com/lib.dll"
    
    // #line variations
    #line 10 "file.fs"
    #line default
    #line hidden
    
    // #nowarn with ranges
    #nowarn 40-44 50-52 9
    
    // #if defined() syntax
    #if defined(DEBUG) || defined(TEST)
    let debugMode = true
    #elif not defined(RELEASE)
    let debugMode = false
    #else
    let debugMode = true
    #endif

// ============================================================================
// 9. ATTRIBUTES & SIGNATURE SYNTAX
// ============================================================================
module AttributesAndSignatures =
    [<Literal>]
    let MaxSize = 100
    
    [<Measure>] type kg
    
    [<AutoOpen>]
    module Helpers = let help () = "help"
    
    [<RequireQualifiedAccess>]
    type Status = Active | Inactive
    
    // Signature-file syntax (valid in .fsi, tests lexer robustness)
    // val declarations & abstract members
    val compute: int -> int
    val mutable counter: int
    
    type ICalc =
        abstract member Add: int * int -> int
        abstract member Multiply: int * int -> int
        default Multiply: int * int -> int
        
    // module aliases
    module M = System.Collections.Generic

// ============================================================================
// 10. THE CLASSIC (* VS ( * ) LEXICAL TRAP
// ============================================================================
module CommentOperatorTrap =
    // (* starts a BLOCK COMMENT. Lexer must NOT tokenize as ( *
    let blocked = (* this is a comment *)
    
    // ( * ) is a valid INFIX OPERATOR (spaces prevent comment tokenization)
    let op1 = ( * ) 2 3
    let op2 = 5 |> ( * ) 2
    
    // */ is NOT a valid F# token, but lexer should handle gracefully
    // let broken = 1 */ 2 // Syntax error, but lexer must emit *, /
    
    // // inside strings vs comments
    let strWithSlash = "http://example.com // not a comment"
    let verbatimSlash = @"C:\path\file // not comment"
    let tripleSlash = """Line 1 // not comment
    Line 2 (* not comment *)
    Line 3"""
    
    // XML doc vs block comment adjacency
    /// <summary>Doc</summary>
    (** Block doc *)
    let x = 1