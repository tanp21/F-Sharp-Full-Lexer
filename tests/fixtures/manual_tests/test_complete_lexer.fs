// ============================================================================
// F# COMPLETE LEXER TEST FILE
// This file is NOT meant to compile — it tests every token type and edge case
// that a comprehensive F# lexer MUST handle correctly.
// ============================================================================

// ---------------------------------------------------------------------------
// §1 COMMENTS — LINE COMMENTS
// ---------------------------------------------------------------------------

// single-line comment
let x = 1  // trailing comment on code line

/// XML doc comment — exactly 3 slashes
/// <summary>XML doc on function</summary>
/// <param name="x">The input</param>
let documented x = x + 1

//// 4+ slashes is a regular comment, NOT XML doc
let y = 2

#! shebang line (only valid at file start — here just testing token recognition)

// ---------------------------------------------------------------------------
// §2 COMMENTS — BLOCK COMMENTS (NESTED)
// ---------------------------------------------------------------------------

(* simple block comment *)
let afterBlock = 42

(* outer (* nested level 1 (* nested level 2 *) back to level 1 *) back to outer *)
let nestedComment = 100

(* empty comment *)
(*)  // <-- this is LPAREN_STAR_RPAREN token, not a comment

(* block comment with symbols: let x = 1; + - * / :: @ ^ |> <| && || ! = <> <= >= -> <- *)

(* string inside comment: "hello (* still in string *) world" — the (* is inside string *)
let s = "string with (* not a comment *) inside"

(* ((( *))) odd nesting *)
let weirdNesting = "test"

// ---------------------------------------------------------------------------
// §3 PREPROCESSOR DIRECTIVES
// ---------------------------------------------------------------------------

#if DEBUG
let debugOnly = true
#endif

#if DEBUG && !CI  // logical NOT and AND
let conditional = 42
#elif RELEASE
let conditional = 0
#else
let conditional = -1
#endif

#if DEBUG || TRACE
let traceOrDebug = true
#endif

#if (COMPILED && !INTERACTIVE)
let compiledOnly = 1
#endif

#if NET8_0 || NET9_0
let modern = true
#endif

#if FOO // comment after directive
let fooDefined = 1
#else // else comment
let fooNotDefined = 0
#endif

// ---------------------------------------------------------------------------
// §4 OTHER HASH DIRECTIVES
// ---------------------------------------------------------------------------

#nowarn FS0025 "42" FS0007
#nowarn "25"
# 42
#line 100
# 42 "somefile.fs"
#line 100 "somefile.fs"
# 42 @"verbatim\path.fs"
#line 100 @"verbatim\path.fs"
#r "nuget: SomePackage"
#r @"dll\path.dll"
#load "helper.fsx"
#load @"scripts\utils.fsx"
#time
#I "include/path"
#I @"include\verbatim\path"

// ---------------------------------------------------------------------------
// §5 LITERALS — BOOLEAN, UNIT, NULL
// ---------------------------------------------------------------------------

let b1 = true
let b2 = false
let u = ()
let n = null

// ---------------------------------------------------------------------------
// §6 LITERALS — INTEGER LITERALS (DECIMAL)
// ---------------------------------------------------------------------------

let i0 = 0
let i1 = 42
let i2 = 1234567890
let i3 = 1_000_000
let i4 = 1_2_3_4_5_6
let i5 = 0_000_042

// ---------------------------------------------------------------------------
// §7 LITERALS — INTEGER SUFFIXES
// ---------------------------------------------------------------------------

let iy = 42y          // sbyte
let iuy = 42uy        // byte
let is_ = 42s         // int16
let ius = 42us        // uint16
let il = 42l          // int32
let iul = 42ul        // uint32
let iL = 42L          // int32 (capital)
let iUL = 42UL        // uint32
let in_ = 42n         // nativeint
let iun = 42un        // unativeint
let iI = 42I          // bigint
let iZ = 42Z          // bigint (F# 9+)

// ---------------------------------------------------------------------------
// §8 LITERALS — INTEGER WITH BASE PREFIXES
// ---------------------------------------------------------------------------

let hex0 = 0xFF
let hex1 = 0xDEAD_BEEF
let hex2 = 0x123abc
let hex3 = 0x0
let hex4 = 0xFFy         // hex + sbyte
let hex5 = 0xFFuy        // hex + byte
let hex6 = 0xABCDus      // hex + uint16
let hex7 = 0xFFn         // hex + nativeint

let oct0 = 0o777
let oct1 = 0o0
let oct2 = 0o777uy       // octal + byte suffix
let oct3 = 0o123_456     // octal with underscore

let bin0 = 0b1010
let bin1 = 0b1111_0000
let bin2 = 0b0
let bin3 = 0b1010y       // binary + sbyte

// ---------------------------------------------------------------------------
// §9 LITERALS — FLOAT LITERALS
// ---------------------------------------------------------------------------

let f0 = 3.14
let f1 = 0.001
let f2 = 42.0
let f3 = 1_000.5
let f4 = 3.14f
let f5 = 3.14F
let f6 = 3.14M           // decimal
let f7 = 3.14LF          // float (F# 7 bifloat)

// ---------------------------------------------------------------------------
// §10 LITERALS — FLOAT WITH EXPONENT
// ---------------------------------------------------------------------------

let fe0 = 1.0e10
let fe1 = 2.5E-5
let fe2 = 3.14e+10
let fe3 = 4e3
let fe4 = 5E10f
let fe5 = 6.022e23M

// ---------------------------------------------------------------------------
// §11 LITERALS — HEX FLOATS (F# 9+)
// ---------------------------------------------------------------------------

let hf1 = 0x1.FFFFp10LF

// ---------------------------------------------------------------------------
// §12 LITERALS — STRING LITERALS (REGULAR)
// ---------------------------------------------------------------------------

let s1 = "hello"
let s2 = ""
let s3 = "multi\nline\tescape"
let s4 = "unicode: \u0041 \U0001F600"
let s5 = "hex: \x41"
let s6 = "decimal trigraph: \065 \066 \067"
let s7 = "escapes: \\ \" \' \a \b \f \n \r \t \v"
let s8 = "line\
         continuation"     // backslash-newline trims whitespace
let s9 = "string with Unicode: αβγ 名前 café"

// ---------------------------------------------------------------------------
// §13 LITERALS — VERBATIM STRINGS
// ---------------------------------------------------------------------------

let vs1 = @"C:\Windows\System32\"
let vs2 = @"no\escape\sequences"
let vs3 = @"contains "" double quote"
let vs4 = @"multi
line
verbatim"
let vs5 = @""   // empty verbatim

// ---------------------------------------------------------------------------
// §14 LITERALS — TRIPLE-QUOTED STRINGS
// ---------------------------------------------------------------------------

let tq1 = """triple-quoted string"""
let tq2 = """contains " double quotes """
let tq3 = """
multi-line
triple-quoted
"""
let tq4 = """no \" escape \n processing \t here"""
let tq5 = """"four quotes means one literal quote in content""""

// ---------------------------------------------------------------------------
// §15 LITERALS — BYTE ARRAY STRINGS
// ---------------------------------------------------------------------------

let ba1 = "hello"B
let ba2 = @"verbatim"B
let ba3 = """triple"""B

// ---------------------------------------------------------------------------
// §16 LITERALS — STRING INTERPOLATION
// ---------------------------------------------------------------------------

let interp1 = $"hello {name}"
let interp2 = $"value = {x + y}"
let interp3 = $"{x:N3}"
let interp4 = $"{{ is literal brace }}"
let interp5 = $"{x,5:N2}"
let interp6 = $"circle: {radius:F2}"
let interp7 = $""

let interpV1 = $@"path: {dir}\files"
let interpV2 = $@"literal "" quote and {expr}"
let interpV3 = @$"reversed order: {expr}"

let interpTQ1 = $"""triple quoted interpolated: {x}"""
let interpTQ2 = $"""contains " and {y}"""

// ---------------------------------------------------------------------------
// §17 LITERALS — EXTENDED INTERPOLATED STRINGS
// ---------------------------------------------------------------------------

let ext1 = $$"""value: {{x}}"""
let ext2 = $$"""format: %%04d {{count}}"""
let ext3 = $$"""{{literal}} and {{expr}}"""
let ext4 = $$$"""three dollars: {{{x}}}"""
let ext5 = $$$$"""four dollars: {{{{y}}}}"""

// ---------------------------------------------------------------------------
// §18 LITERALS — CHARACTER LITERALS
// ---------------------------------------------------------------------------

let c1 = 'a'
let c2 = 'Z'
let c3 = '0'
let c4 = ' '
let c5 = '\n'
let c6 = '\t'
let c7 = '\r'
let c8 = '\\'
let c9 = '\''
let c10 = '\"'
let c11 = '\b'
let c12 = '\a'
let c13 = '\f'
let c14 = '\v'
let c15 = '\065'       // decimal trigraph
let c16 = '\x41'        // hex
let c17 = '\u0041'      // UTF-16
let c18 = '\U00000041'  // UTF-32

let ch1 = '中'
let ch2 = 'α'

let bc1 = 'a'B   // byte character
let bc2 = '\n'B
let bc3 = '\x41'B

// ---------------------------------------------------------------------------
// §19 KEYWORDS — ALL F# RESERVED KEYWORDS
// ---------------------------------------------------------------------------

let keywordTest () =
    abstract and as assert base begin class const default delegate do done
    downcast downto elif else end exception extern false finally fixed
    for fun function global if in inherit inline interface internal
    lazy let match member module mutable namespace new not null of
    open or override private public rec return sig static struct then
    to true try type upcast use val void when while with yield ()

// ---------------------------------------------------------------------------
// §20 KEYWORDS — BANG KEYWORDS (Computation Expressions)
// ---------------------------------------------------------------------------

let compExpr = async {
    let! x = async { return 1 }
    do! Async.Sleep 100
    match! someAsync with
    | Some v -> return! async { return v }
    | None -> yield! [1; 2; 3]
    and! other = async { return 0 }
    while! conditionCheck () do
        ()
}

// ---------------------------------------------------------------------------
// §21 KEYWORDS — RESERVED FOR FUTURE USE
// ---------------------------------------------------------------------------

let futureTest () =
    let ``break`` = 1
    let ``checked`` = 2
    let ``component`` = 3
    let ``constraint`` = 4
    let ``continue`` = 5
    let ``event`` = 6
    let ``external`` = 7
    let ``include`` = 8
    let ``mixin`` = 9
    let ``parallel`` = 10
    let ``params`` = 11
    let ``process`` = 12
    let ``protected`` = 13
    let ``pure`` = 14
    let ``sealed`` = 15
    let ``tailcall`` = 16
    let ``trait`` = 17
    let ``virtual`` = 18
    ()

// ---------------------------------------------------------------------------
// §22 KEYWORDS — FORMERLY RESERVED (NOW FREE IDENTIFIERS)
// ---------------------------------------------------------------------------

let atomic = 1
let constructor = 2
let eager = 3
let functor = 4
let measure = 5
let method = 6
let object = 7
let recursive = 8
let volatile = 9

// ---------------------------------------------------------------------------
// §23 KEYWORDS — OCAML COMPATIBILITY RESERVED
// ---------------------------------------------------------------------------

let ``asr`` = 1
let ``land`` = 2
let ``lor`` = 3
let ``lsl`` = 4
let ``lsr`` = 5
let ``lxor`` = 6
let ``mod`` = 7

// ---------------------------------------------------------------------------
// §24 IDENTIFIERS — STANDARD
// ---------------------------------------------------------------------------

let camelCase = 1
let PascalCase = 2
let Snake_case = 3
let UPPER_CASE = 4
let mixed_Case123 = 5
let x' = 6          // single trailing prime
let y'' = 7         // double trailing prime
let z''' = 8        // triple prime
let _private = 9
let __dunder__ = 10
let _ = ()          // wildcard discard (UNDERSCORE token)

// ---------------------------------------------------------------------------
// §25 IDENTIFIERS — BACKTICK (VERBATIM)
// ---------------------------------------------------------------------------

let ``let`` = "keyword as identifier"
let ``my variable with spaces`` = 42
let ``special!@#$%^&*()`` = 100
let ``_`` = "single underscore via backtick"
let ``hello`world`` = "single backtick inside"
let ``x'`` = "prime inside backtick"
let ``\n\t\r`` = "escapes are literal inside backticks"
let ``type`` = "another keyword"
let ``member`` = "keyword as name"
let ``inline`` = "keyword"
let ``fun`` = "keyword"
let ``match`` = "keyword"
let ``->`` = "arrow symbol as identifier"
let ``<-`` = "left arrow as identifier"

// ---------------------------------------------------------------------------
// §26 IDENTIFIERS — UNICODE
// ---------------------------------------------------------------------------

let α = 1.0            // Greek alpha
let β = 2.0            // Greek beta
let γ = 3.0            // Greek gamma
let δ = 4.0            // Greek delta
let 名前 = "name"       // CJK
let 年龄 = 25           // CJK
let café = "coffee"    // Latin with diacritic
let naïve = "naive"    // Latin with diaeresis
let façade = "facade"  // Latin with cedilla
let résumé = "resume"  // Latin with accents
let ñoño = "silly"     // Spanish
let Straße = "street"  // German eszett

// ---------------------------------------------------------------------------
// §27 IDENTIFIERS — KEYWORD IDENTIFIERS (Source Directives)
// ---------------------------------------------------------------------------

let sourceDir = __SOURCE_DIRECTORY__
let sourceFile = __SOURCE_FILE__
let lineNum = __LINE__

// ---------------------------------------------------------------------------
// §28 OPERATORS — DELIMITERS
// ---------------------------------------------------------------------------

let delimitersTest () =
    let _ = ( )       // LPAREN RPAREN
    let _ = [ ]       // LBRACK RBRACK
    let _ = {| |}     // LBRACE_BAR BAR_RBRACE (anonymous record)
    let _ = [| |]     // LBRACK_BAR BAR_RBRACK (empty array)
    let _ = [||]      // LBRACK_BAR BAR_RBRACK
    let _ = [| 1; 2; 3 |]   // array literal
    let _ = [ 1; 2; 3 ]     // list literal
    let _ = [ ]              // empty list
    let _ = [| for i in 1..10 -> i * 2 |]  // array comprehension
    let arr = [|1;2|]
    arr.[0] <- 10     // .[] slice get, <- set
    arr.[1..3]        // .. range in slice
    arr.[*]           // * in slice
    arr.[..2]         // .. from start
    arr.[1..]         // .. to end
    ()

// ---------------------------------------------------------------------------
// §29 OPERATORS — ARITHMETIC
// ---------------------------------------------------------------------------

let arith1 = 1 + 2
let arith2 = 5 - 3
let arith3 = 4 * 5
let arith4 = 10 / 2
let arith5 = 11 % 3
let arith6 = 2 ** 8        // exponentiation
let arith7 = -42           // unary minus
let arith8 = +42           // unary plus
let arith9 = 1 +. 2.0      // dotted operators
let arith10 = 3.0 -. 1.0
let arith11 = 2.0 *. 3.0
let arith12 = 6.0 /. 2.0

// ---------------------------------------------------------------------------
// §30 OPERATORS — COMPARISON
// ---------------------------------------------------------------------------

let comp1 = x = y
let comp2 = x <> y
let comp3 = x < y
let comp4 = x > y
let comp5 = x <= y
let comp6 = x >= y
let comp7 = x = y = z    // chained comparison (parses left-assoc)

// ---------------------------------------------------------------------------
// §31 OPERATORS — BOOLEAN / LOGICAL
// ---------------------------------------------------------------------------

let bool1 = a && b
let bool2 = a || b
let bool3 = not a
let bool4 = not (a && b)

// ---------------------------------------------------------------------------
// §32 OPERATORS — BITWISE
// ---------------------------------------------------------------------------

let bit1 = x &&& y
let bit2 = x ||| y
let bit3 = x ^^^ y
let bit4 = ~~~x
let bit5 = x <<< 1
let bit6 = x >>> 2

// ---------------------------------------------------------------------------
// §33 OPERATORS — PIPE / COMPOSITION
// ---------------------------------------------------------------------------

let pipe1 = x |> f
let pipe2 = f <| x
let pipe3 = xs ||> f
let pipe4 = f <|| xs
let pipe5 = xs |||> f
let pipe6 = f <||| xs

let comp1 = f >> g
let comp2 = g << f
let comp3 = f1 >> f2 >> f3

// ---------------------------------------------------------------------------
// §34 OPERATORS — OTHER INFIX
// ---------------------------------------------------------------------------

let cons = 1 :: [2; 3]       // cons
let append = [1; 2] @ [3; 4] // list append
let concat = "hello " ^ "world"  // string concatenation
let refAssign = r := 42      // ref cell assignment
let deref = !r               // ref cell dereference
let arrow = x -> x + 1       // F# arrow (fun arg -> body)

// ---------------------------------------------------------------------------
// §35 OPERATORS — CASTING / TYPE TEST
// ---------------------------------------------------------------------------

let up = x :> BaseType
let down = x :?> DerivedType
let test = x :? string

// ---------------------------------------------------------------------------
// §36 OPERATORS — NULLABLE
// ---------------------------------------------------------------------------

let nc1 = a ?+ b
let nc2 = a ?- b
let nc3 = a ?* b
let nc4 = a ?/ b
let nc5 = a ?% b
let nc6 = a ?= b
let nc7 = a ?<> b
let nc8 = a ?< b
let nc9 = a ?> b
let nc10 = a ?<= b
let nc11 = a ?>= b
let nc12 = a ?? b          // null coalescing
let nc13 = a ?.Property    // null-conditional access (if lexed as op)
let nc14 = a ?+? b
let nc15 = a ?-? b
let nc16 = a ?*? b

// ---------------------------------------------------------------------------
// §37 OPERATORS — QUOTATIONS
// ---------------------------------------------------------------------------

let q1 = <@ 1 + 2 @>
let q2 = <@@ 1 + 2 @@>
let q3 = <@ %x @>
let q4 = <@@ %%x @@>

// ---------------------------------------------------------------------------
// §38 OPERATORS — CUSTOM / USER-DEFINED
// ---------------------------------------------------------------------------

let custom1 = x +*+ y       // custom infix
let custom2 = x |-| y       // custom infix
let custom3 = x >?= y       // custom infix
let custom4 = x <!> y       // custom infix
let custom5 = x .>>. y      // custom
let custom6 = x >>= y       // custom monadic bind
let custom7 = |@| x y       // prefix custom
let custom8 = !% x          // prefix custom
let custom9 = ~& x          // prefix custom

// ---------------------------------------------------------------------------
// §39 SPECIAL TOKENS — .[] AND .()
// ---------------------------------------------------------------------------

let slice1 = arr.[0]
let slice2 = arr.[1..3]
let slice3 = arr.[*]
let slice4 = matrix.[0, 1]
let slice5 = arr.[0] <- 99
let slice6 = matrix.[0, 1] <- 42

// PAREN_GET / PAREN_SET (FUNKY_OPERATOR_NAME in compiler)
let pget1 = dict.("key")
let pset1 = dict.("key") <- "value"

// ---------------------------------------------------------------------------
// §40 SYNTAX — TUPLE
// ---------------------------------------------------------------------------

let tup0 = ()
let tup1 = (1, 2)
let tup2 = (1, "two", 3.0)
let tup3 = struct (1, 2)
let tup4 = (1, (2, 3), 4)
let tup5 = 1, 2, 3      // tuple without parens

// ---------------------------------------------------------------------------
// §41 SYNTAX — RECORD
// ---------------------------------------------------------------------------

type Person = { Name: string; Age: int }
let record1 = { Name = "Alice"; Age = 30 }
let record2 = { record1 with Age = 31 }
let record3 = {| Name = "Bob"; Age = 25 |}   // anonymous record

// ---------------------------------------------------------------------------
// §42 SYNTAX — DISCRIMINATED UNION
// ---------------------------------------------------------------------------

type Shape =
    | Circle of radius: float
    | Rectangle of width: float * height: float
    | Triangle

type Option<'T> = None | Some of 'T

// ---------------------------------------------------------------------------
// §43 SYNTAX — IF / THEN / ELIF / ELSE
// ---------------------------------------------------------------------------

let cond1 = if x > 0 then "positive" else "non-positive"
let cond2 =
    if x > 10 then "big"
    elif x > 5 then "medium"
    elif x > 0 then "small"
    else "zero or negative"

// ---------------------------------------------------------------------------
// §44 SYNTAX — MATCH
// ---------------------------------------------------------------------------

let matchEx =
    match x with
    | 0 -> "zero"
    | 1 | 2 -> "one or two"
    | n when n < 0 -> "negative"
    | _ -> "other"

let funcMatch = function
    | [] -> 0
    | h :: t -> 1 + h

let tryMatch =
    try
        riskyOperation ()
    with
    | :? System.Exception as ex -> printfn "%s" ex.Message
    | ex -> reraise ()
    finally
        cleanup ()

// ---------------------------------------------------------------------------
// §45 SYNTAX — FOR / WHILE LOOPS
// ---------------------------------------------------------------------------

let forLoop1 = for i in 0..9 do printfn "%d" i
let forLoop2 = for i in 10..-1..0 do printfn "%d" i
let forLoop3 = for i = 10 downto 0 do printfn "%d" i

let whileLoop = while x < 10 do x <- x + 1

// ---------------------------------------------------------------------------
// §46 SYNTAX — TYPE ANNOTATIONS
// ---------------------------------------------------------------------------

let typed1: int = 42
let typed2: string = "hello"
let typed3: float = 3.14
let typed4: bool = true
let typed5: char = 'x'
let typed6: unit = ()
let typed7: obj = box 42

let typed8: int list = [1; 2; 3]
let typed9: string option = Some "value"
let typed10: Map<string, int> = Map.empty

let typed11: int * string = (1, "one")
let typed12: int -> string = fun x -> string x
let typed13: int -> int -> int = fun x y -> x + y

let typed14: int[] = [|1; 2; 3|]
let typed15: int[,] = array2D [[1; 2]; [3; 4]]
let typed16: int[,,] = Array3D.zeroCreate 2 2 2

let typed17: 'a = Unchecked.defaultof<'a>
let typed18: 'T option = None
let typed19: (int * string) = (1, "one")

let typed20: System.Collections.Generic.List<int> = new System.Collections.Generic.List<int>()
let typed21: int nativeint = 0n

// Flexible type
let typed22: #IDisposable = {new IDisposable with member _.Dispose() = ()}

// Nullable types
let typed23: int Nullable = Nullable(42)
let typed24: Nullable<int> = Nullable 42

// ---------------------------------------------------------------------------
// §47 SYNTAX — SRTP (Statically Resolved Type Parameters)
// ---------------------------------------------------------------------------

let inline srtpTest< ^T when ^T : (member GetValue: unit -> int)> (x: ^T) =
    (^T : (member GetValue: unit -> int) x)

let inline add< ^T when ^T : (static member (+): ^T * ^T -> ^T)> (a: ^T) (b: ^T) =
    a + b

// ---------------------------------------------------------------------------
// §48 SYNTAX — UNITS OF MEASURE
// ---------------------------------------------------------------------------

[<Measure>] type m
[<Measure>] type s
[<Measure>] type kg

let distance: float<m> = 100.0<m>
let time: float<s> = 9.8<s>
let speed: float<m/s> = distance / time
let speed2: float<m/s^2> = 9.8<m/s^2>

// ---------------------------------------------------------------------------
// §49 SYNTAX — ATTRIBUTES
// ---------------------------------------------------------------------------

[<Literal>]
let literalVal = "compile-time"

[<Struct>]
type MyStruct = { X: int; Y: int }

[<RequireQualifiedAccess>]
module QualifiedModule =
    let x = 1

[<AutoOpen>]
module AutoModule =
    let auto = 42

[<Obsolete("Use newMethod instead")>]
let oldMethod () = ()

[<EntryPoint>]
let main argv = 0

// ---------------------------------------------------------------------------
// §50 SYNTAX — CLASS / INTERFACE / INHERIT
// ---------------------------------------------------------------------------

type IShape =
    abstract Area: float
    abstract Name: string

type Circle(radius: float) =
    member _.Radius = radius
    member _.Area = System.Math.PI * radius * radius

type ColoredCircle(radius: float, color: string) =
    inherit Circle(radius)
    member _.Color = color
    override _.Area = base.Area

type MutablePoint() =
    let mutable x = 0
    let mutable y = 0
    member _.X with get () = x and set v = x <- v
    member _.Y with get () = y and set v = y <- v

// ---------------------------------------------------------------------------
// §51 SYNTAX — OBJECT EXPRESSION
// ---------------------------------------------------------------------------

let disposable =
    { new System.IDisposable with
        member _.Dispose() = printfn "disposed" }

let comparer =
    { new System.Collections.Generic.IComparer<int> with
        member _.Compare(x, y) = x - y }

// ---------------------------------------------------------------------------
// §52 SYNTAX — LAZY / DELEGATE
// ---------------------------------------------------------------------------

let lazyVal = lazy (expensiveComputation ())
let del = System.Func<int, int>(fun x -> x * 2)

// ---------------------------------------------------------------------------
// §53 SYNTAX — COMPUTATION EXPRESSIONS (Full)
// ---------------------------------------------------------------------------

let asyncExpr = async {
    let! data = fetchAsync url
    let processed = data |> processData
    do! Async.Sleep 100
    match! checkStatus processed with
    | Ok result ->
        return! async { return result }
    | Error msg ->
        yield! seq { yield msg }
    return processed
}

let taskExpr = task {
    let! result = someTask
    return result + 1
}

let seqExpr = seq {
    for i in 0..9 do
        if i % 2 = 0 then
            yield i
        else
            yield! [i * 10]
}

let queryExpr = query {
    for p in products do
    where (p.Price > 100.0)
    sortBy p.Name
    select (p.Name, p.Price)
}

// ---------------------------------------------------------------------------
// §54 SYNTAX — TYPE EXTENSIONS
// ---------------------------------------------------------------------------

type System.String with
    member this.IsLong = this.Length > 100
    static member EmptyString = ""

type System.Int32 with
    static member Zero = 0

// ---------------------------------------------------------------------------
// §55 SYNTAX — ACTIVE PATTERNS
// ---------------------------------------------------------------------------

let (|Even|Odd|) n = if n % 2 = 0 then Even else Odd
let (|Integer|_|) (s: string) =
    match System.Int32.TryParse s with
    | true, n -> Some n
    | _ -> None

let (|DivisibleBy|_|) divisor n =
    if n % divisor = 0 then Some DivisibleBy else None

// ---------------------------------------------------------------------------
// §56 SYNTAX — BYREF / POINTER TYPES
// ---------------------------------------------------------------------------

let byrefTest (x: byref<int>) = x <- x + 1
let inrefTest (x: inref<int>) = printfn "%d" x
let outrefTest (x: outref<int>) = x <- 42

let nativePtr: nativeptr<int> = NativePtr.ofNativeInt 0n
let voidPtr: voidptr = NativePtr.toVoidPtr nativePtr

// ---------------------------------------------------------------------------
// §57 SYNTAX — FIXED / EXTERN
// ---------------------------------------------------------------------------

[<System.Runtime.InteropServices.DllImport("kernel32.dll")>]
extern int GetTickCount()

// ---------------------------------------------------------------------------
// §58 SYNTAX — MODULE / NAMESPACE / OPEN
// ---------------------------------------------------------------------------

namespace MyApp.Core

module MathUtils =
    let add x y = x + y

module StringUtils =
    open System
    let toUpper (s: string) = s.ToUpper()

namespace MyApp.UI

open MyApp.Core.MathUtils
open System.IO
open type System.Math

// ---------------------------------------------------------------------------
// §59 SYNTAX — LET REC / AND (Mutual Recursion)
// ---------------------------------------------------------------------------

let rec factorial n =
    if n <= 1 then 1
    else n * factorial (n - 1)

let rec isEven n =
    match n with
    | 0 -> true
    | n -> isOdd (n - 1)
and isOdd n =
    match n with
    | 0 -> false
    | n -> isEven (n - 1)

let rec map f xs =
    match xs with
    | [] -> []
    | h :: t -> f h :: map f t
and filter pred xs =
    match xs with
    | [] -> []
    | h :: t when pred h -> h :: filter pred t
    | _ :: t -> filter pred t

// ---------------------------------------------------------------------------
// §60 SYNTAX — LAMBDA / FUN
// ---------------------------------------------------------------------------

let lambda1 = fun x -> x + 1
let lambda2 = fun x y -> x + y
let lambda3 = fun (x, y) -> x + y
let lambda4 = fun () -> 42

// ---------------------------------------------------------------------------
// §61 SYNTAX — MUTABLE / REF / MUTATION
// ---------------------------------------------------------------------------

let mutable counter = 0
counter <- counter + 1
counter <- 100

type MutableRecord = { mutable Count: int; mutable Label: string }
let mr = { Count = 0; Label = "start" }
mr.Count <- 5
mr.Label <- "updated"

let refCell = ref 0
refCell := 10
let refValue = !refCell
refCell.Value <- 20

// ---------------------------------------------------------------------------
// §62 SYNTAX — USE / USE!
// ---------------------------------------------------------------------------

let readFile path =
    use reader = new System.IO.StreamReader(path)
    reader.ReadToEnd()

let asyncRead path = async {
    use! reader = File.OpenText(path) |> Async.AwaitTask
    return! reader.ReadToEndAsync() |> Async.AwaitTask
}

// ---------------------------------------------------------------------------
// §63 SYNTAX — DO
// ---------------------------------------------------------------------------

do printfn "Module initialization"

type MyClass() =
    do printfn "Constructor initialization"

// ---------------------------------------------------------------------------
// §64 SYNTAX — PATTERNS (All Forms)
// ---------------------------------------------------------------------------

let patId x = x                        // identifier pattern
let patWild _ = ()                     // wildcard pattern
let patTuple (x, y) = x + y           // tuple pattern
let patCons h :: t = h :: t            // cons pattern
let patAs (x, y) as pair = pair       // as pattern
let patTyped (x: int) = x             // typed pattern
let patRecord { Name = n; Age = a } = (n, a)  // record pattern
let patOr (1 | 2 | 3) as n = n        // or-pattern
let patNested ((x, y), z) = x + y + z // nested pattern
let patStruct struct (x, y) = x + y   // struct tuple pattern
let patNull null = "null"             // null pattern
let patList [] = 0                    // empty list pattern
let patList2 [x] = x                  // single-element list pattern
let patList3 [x; y; z] = x + y + z   // multi-element list pattern

// ---------------------------------------------------------------------------
// §65 MISCELLANEOUS — DOT_DOT / RANGES
// ---------------------------------------------------------------------------

let range1 = [0..9]
let range2 = [0..2..10]
let range3 = [0..]
let range4 = [..9]
let range5 = [0..^1]           // F# 8+ range (DOT_DOT_HAT)
let range6 = [0..2..^2]

// ---------------------------------------------------------------------------
// §66 MISCELLANEOUS — EDGE CASES
// ---------------------------------------------------------------------------

// ;; (double semicolon — interactive mode terminator)
let a = 1;;
let b = 2;;

// Identifiers with leading numbers (invalid but lexer should handle as IDENT or error)
// 1st is not valid F# — skipped

// Empty backtick pair (should error)
// let `` `` = 0

// Backtick with newline (should error)
// let ``multi
// line`` = 0

// Interpolated string with complex expression holes
let complexInterp = $"result: {if x > 0 then "positive" else "negative"}"
let formatSpec = $"%d{x}"
let formatSpec2 = $"%04d{x}"
let alignment = $"{x,10:N2}"

// Multiple adjacent strings (concatenation)
let adj s = "hello" + "world"
let adjV s = @"path\" + @"more"

// Double quote inside verbatim
let vquote = @"He said ""Hello"" to me"

// Triple-quote with embedded quotes
let tq6 = """He said "Hello" and I replied "Hi" """

// ---------------------------------------------------------------------------
// §67 ENSURE NO TABS (tabs are errors in F#)
// ---------------------------------------------------------------------------

// This file should NOT contain tab characters — F# lexer rejects them.

// ---------------------------------------------------------------------------
// §68 SEMICOLON-SEPARATED LINES (for light syntax off testing)
// ---------------------------------------------------------------------------

let multiSemi x = let a = x + 1 in let b = a * 2 in b

// ---------------------------------------------------------------------------
// §69 BYREF / POINTER OPERATIONS
// ---------------------------------------------------------------------------

let byrefOp (x: byref<int>) =
    x <- x + 1          // mutation through byref

let nativeintOp (p: nativeint) =
    let q = p + 1n      // nativeint arithmetic

// ---------------------------------------------------------------------------
// §70 NAMESPACE GLOBAL QUALIFIER
// ---------------------------------------------------------------------------

let globalType: global.System.Int32 = 42

// ---------------------------------------------------------------------------
// §71 STRING CONCATENATION WITH NEWLINES
// ---------------------------------------------------------------------------

let multiLineStr =
    "line one " +
    "line two " +
    "line three"

let multiLineVerbatim =
    @"C:\path\one\" +
    @"C:\path\two\"

// ---------------------------------------------------------------------------
// §72 UNDERSCORE IN NUMERIC LITERALS — EDGE POSITIONS
// ---------------------------------------------------------------------------

let u1 = 1_000_000
let u2 = 0xDEAD_BEEF
let u3 = 0o777_000
let u4 = 0b1010_0101
let u5 = 1.000_000_5

// ---------------------------------------------------------------------------
// §73 SIGNED LITERALS in patterns (negative literals)
// ---------------------------------------------------------------------------

let negPat =
    match -1 with
    | -1 -> "negative one"
    | _ -> "other"

// ---------------------------------------------------------------------------
// §74 VERBOSE SYNTAX
// ---------------------------------------------------------------------------

let verbose =
    begin
        let x = 1
        let y = 2
        x + y
    end

let verboseMatch =
    match x with
    | 0 ->
        begin
            printfn "zero"
            0
        end
    | _ -> -1

// ---------------------------------------------------------------------------
// §75 ABSTRACT / DEFAULT / OVERRIDE
// ---------------------------------------------------------------------------

[<AbstractClass>]
type Animal() =
    abstract Speak: unit -> string
    default _.Speak() = "..."

type Dog() =
    inherit Animal()
    override _.Speak() = "Woof!"

// ---------------------------------------------------------------------------
// §76 STATIC / MEMBER / VAL
// ---------------------------------------------------------------------------

type StaticExample =
    static member Create() = StaticExample()
    member _.InstanceMethod() = ()
    val mutable field: int

// ---------------------------------------------------------------------------
// §77 INTERFACE / INHERIT
// ---------------------------------------------------------------------------

type IExample =
    inherit System.IDisposable
    abstract DoWork: int -> string

// ---------------------------------------------------------------------------
// §78 CONST
// ---------------------------------------------------------------------------

type ConstProvider =
    [<Literal>]
    let MaxItems = 100

// ---------------------------------------------------------------------------
// §79 EXCEPTION DECLARATION
// ---------------------------------------------------------------------------

exception MyError of string * int
exception FatalError

let raiseEx = raise (MyError("bad", 42))
let failEx = failwith "oops"

// ---------------------------------------------------------------------------
// §80 ENUM / FLAGS
// ---------------------------------------------------------------------------

type Colors =
    | Red = 0
    | Green = 1
    | Blue = 2

[<System.Flags>]
type Permissions =
    | None = 0
    | Read = 1
    | Write = 2
    | Execute = 4

// ---------------------------------------------------------------------------
// §81 STRUCT (VALUE TYPE)
// ---------------------------------------------------------------------------

[<Struct>]
type Point3D =
    val X: float
    val Y: float
    val Z: float
    new (x, y, z) = { X = x; Y = y; Z = z }

// ---------------------------------------------------------------------------
// §82 TYPE PROVIDER DIRECTIVES
// ---------------------------------------------------------------------------

// JSON type provider etc — not testable without provider, but `let` in type providers
type MyProvider = FSharp.Data.JsonProvider<"sample.json">

// ---------------------------------------------------------------------------
// §83 STRING ESCAPE EDGE CASES
// ---------------------------------------------------------------------------

let escEdge1 = "\""
let escEdge2 = "\\"
let escEdge3 = "\'"
let escEdge4 = "\n\r\t\b\a\f\v"
let escEdge5 = "\000"    // null character via decimal
let escEdge6 = "\255"    // max decimal trigraph
let escEdge7 = "\x00"
let escEdge8 = "\xFF"

// ---------------------------------------------------------------------------
// §84 STRING WITH % FORMAT SPECIFIERS (printf contexts)
// ---------------------------------------------------------------------------

let printfString = sprintf "%s: %d, %f, %A, %O" name count price obj other
let printfFormat = $"%s{name}: %d{count}"

// ---------------------------------------------------------------------------
// §85 OPERATOR COMBINATIONS (Adversarial)
// ---------------------------------------------------------------------------

let opCombo1 = x -- y
let opCombo2 = x +++ y
let opCombo3 = x <+> y
let opCombo4 = x <?> y
let opCombo5 = x <*> y
let opCombo6 = x <|> y
let opCombo7 = x &&& y ||| z
let opCombo8 = x <<< y >>> z
let opCombo9 = x |||> (fun a b -> a + b)

// ---------------------------------------------------------------------------
// §86 NESTED PARENS / BRACKETS
// ---------------------------------------------------------------------------

let nestedParens = (((1 + 2) * (3 - 4)) / 5)
let nestedBrackets = [[1; 2]; [3; 4]]
let nestedArrays = [|[|1; 2|]; [|3; 4|]|]
let nestedRecords = {| x = {| y = {| z = 1 |} |} |}

// ---------------------------------------------------------------------------
// §87 LET BINDING WITH TYPE AND MUTABLE COMBINATIONS
// ---------------------------------------------------------------------------

let mutable typedMut: int = 0
typedMut <- typedMut + 1

// ---------------------------------------------------------------------------
// §88 LET REC WITH TYPE ANNOTATIONS
// ---------------------------------------------------------------------------

let rec factTyped (n: int): int =
    if n <= 1 then 1
    else n * factTyped (n - 1)

// ---------------------------------------------------------------------------
// §89 INLINE FUNCTION
// ---------------------------------------------------------------------------

let inline addInline a b = a + b
let inline (++) a b = a + b

// ---------------------------------------------------------------------------
// §90 LAZY PATTERNS
// ---------------------------------------------------------------------------

let lazyPat (lazy x) = x

// ---------------------------------------------------------------------------
// §91 NULLABLE VALUE OPTIONS
// ---------------------------------------------------------------------------

let nov1: int voption = ValueSome 42
let nov2: int voption = ValueNone

// ---------------------------------------------------------------------------
// §92 EMPTY STRINGS (ALL FORMS)
// ---------------------------------------------------------------------------

let emptyStr = ""
let emptyVerbatim = @""
let emptyTriple = """"""
let emptyByte = ""B
let emptyInterp = $""

// ---------------------------------------------------------------------------
// §93 SPECIAL REAL LITERALS (INFINITY, NAN)
// ---------------------------------------------------------------------------

let infFloat: float = infinity
let negInfFloat: float = -infinity
let nanFloat: float = nan
let infFloat32: float32 = infinityf
let negInfFloat32: float32 = -infinityf
let nanFloat32: float32 = nanf

// ---------------------------------------------------------------------------
// END OF COMPLETE LEXER TEST FILE
// NOTE: This file is intentionally NOT compilable. It exists only to exercise
// the lexer. Some constructs here would be rejected by the parser or type-checker.
// ============================================================================
