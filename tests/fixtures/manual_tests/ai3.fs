// ============================================================================
// F# LEXER STRESS TEST: EXTREME MODE (F# 7.0/8.0+)
// ============================================================================
// Targets: raw strings, interpolation, slice notation, byref/pointers,
// symbolic adjacencies, contextual keyword overloading, indentation extremes.

#light "off"
#indent "off"

namespace LexerExtreme
open System

// ============================================================================
// 1. RAW STRINGS & INTERPOLATION (F# 7/8)
// ============================================================================
module RawAndInterpolation =
    // Custom delimiters allow embedded quotes without escaping
    let r1 = #"He said "Hi" and \n"#
    let r2 = ##"Double # quote " and backslash \ "##
    let r3 = ###"Triple ### quotes ' \n \t "###
    
    // Interpolated strings (F# 8+)
    let i1 = $"Value: {42}"
    let i2 = $"List: [{1; 2; 3}]"
    let i3 = $"Nested: {sprintf "%d" (1 + 1)}"
    
    // Raw + Interpolated combination
    let ri1 = $#"Path: {__SOURCE_DIRECTORY__}\file" #
    let ri2 = $##"Quotes: " and {1 + 1} "##
    
    // Adjacent tokens that look like raw strings but aren't
    let fake1 = @"#"
    let fake2 = """#"""

// ============================================================================
// 2. SLICE NOTATION, RANGE OPERATORS & SYMBOLIC ADJACENCIES
// ============================================================================
module SliceAndRangeAmbiguities =
    let arr = [|1; 2; 3; 4; 5|]
    let mat = array2D [[1;2];[3;4]]
    
    // Slice boundaries: `.` vs `..` vs `.` `[`
    let s1 = arr.[0..]
    let s2 = arr.[..^1]
    let s3 = arr.[0..2..^1]
    let s4 = mat.[1,0..]
    
    // Range vs dot-dot adjacency
    let range1 = [1..10]
    let range2 = [1 .. 10]
    let range3 = [1.0..10.0]
    let range4 = [1 .. 2 .. 10]
    
    // `..` as token vs `.` `.` token boundary
    let dotAdj = 1.0..10.0  // FLOAT `1.0`, RANGE `..`, FLOAT `10.0`
    let dotSep = 1.0 . 10.0 // FLOAT `1.0`, DOT `.`, FLOAT `10.0` (syntax error but lexically valid)
    
    // `::` cons vs `:` `:` adjacency
    let cons1 = 1 :: [2; 3]
    let cons2 = 1 :: 2 :: []
    
    // Ref assignment vs type annotation vs colon-equals
    let r = ref 0
    let _ = r := 5        // `:` `=` vs `:=` token
    let typed (x: int) = x + 1 // `:` `int`

// ============================================================================
// 3. BYREF, POINTERS, FIXED BLOCKS & REF CELLS
// ============================================================================
module ByrefAndPointerSyntax =
    let inline addByref (x: byref<int>) (y: inref<int>) = x + y
    let outParam (z: outref<string>) = z <- "hello"
    
    // `&` address-of, `&&` is invalid but lexer sees `&` `&`
    let p1 = &arr.[0]
    let p2 = &&arr.[0] // Lexer: `&` `&` `arr` `.` `[` ...
    
    fixed (p = &arr.[0]) do
        let val_ = NativePtr.read p
        
    // Ref cell operators
    let refCell = ref 10
    let _ = refCell := 20
    let _ = !refCell

// ============================================================================
// 4. CONTEXTUAL KEYWORD OVERLOAD & PATTERN MATCHING
// ============================================================================
module ContextualKeywordTraps =
    // `as` in pattern vs type alias
    let testAs (x: obj) =
        match x with
        | :? string as s -> s.Length
        | _ -> 0

    // `when` in guard vs type constraint
    let testWhen x =
        match x with
        | n when n > 0 -> "pos"
        | _ -> "neg"

    // `with` in try, object expr, record update, interface
    let testWith () =
        try failwith "x" with _ -> 0
        let rec f x = { X = x; Y = 0 }
        let g () = { f 1 with Y = 1 }
        interface IDisposable with member _.Dispose() = ()

    // `and` in recursive bindings vs type definitions
    let rec a x = x and b y = y
    type A = { Val: int } and B = { Str: string }

// ============================================================================
// 5. TYPE CONSTRAINTS, GENERICS & STRUCT/UNMANAGED
// ============================================================================
module TypeConstraintExtremes =
    // Multiple constraints, unmanaged, enum, not struct, null
    type Generic<'T when 'T :> IDisposable 
                     and 'T : not struct 
                     and 'T : unmanaged 
                     and 'T : null 
                     and 'T : enum<int>> 
        (x: 'T) =
        member __.Value = x

    // Struct records/unions
    [<Struct>]
    type Point = struct val X: float val Y: float end

    [<Struct>]
    type Shape = 
        | Circle of R: float
        | Rect of W: float * H: float

    // Type test & downcast adjacency
    let cast (x: obj) = 
        match x with
        | :? string as s -> s :?> System.Object
        | _ -> x

// ============================================================================
// 6. INDENTATION & OFFSIDE RULE PATHOLOGIES
// ============================================================================
module IndentationExtremes =
    // `#light "off"` means explicit `begin`/`end` required, but we're toggling back
    #light "on"
    
    let f x =
        if x > 0 then
            let y = x * 2
            y + 1
        else
            // Blank lines inside block MUST NOT trigger DEDENT
            0

    let g () =
        match 1 with
        | 1 ->
            "one"
        | 2 ->
            "two"
        | _ ->
            "other"

    // Mixed tabs/spaces (lexer should normalize or warn)
    let h = 
        seq {
            for i in 1..5 do
	            yield i * 2  // tab here
            yield! [1;2;3]
        }

    // `begin`/`end` explicit blocks
    let i = begin
        let x = 1
        x + 1
    end

    // DEDENT after `else`/`then`/`with` alignment
    let j = if true then 1 else 2

// ============================================================================
// 7. PREPROCESSOR & DIRECTIVE EDGE CASES
// ============================================================================
#if true && false || not false
#define TEST_MODE
#nowarn 40 41
#line 200 "generated.fs"
#time "on"
#indent "off"
#endif

module DirectiveTraps =
    #light "on"
    #load "helpers.fsx"
    #r "System.Runtime"
    #r "nuget: Newtonsoft.Json"
    
    let _ = ()

// ============================================================================
// 8. UNICODE, COMBINING MARKS & IDENTIFIER RULES
// ============================================================================
module UnicodeIdentifiers =
    // Combining characters: e + ́ = é
    let cafe\u0301 = "coffee"
    let naïve = "na\u00EFve"
    
    // Type variables with constraints
    let inline addGeneric<'a when 'a : (static member (+): 'a * 'a -> 'a)> x y = x + y
    
    // Unicode operators (valid F# symbol categories: Sm, So, Sk)
    let (⊕) a b = a + b
    let (⊗) a b = a * b
    let (≈) a b = abs(a - b) < 1e-6
    
    // `nameof` built-in
    let name = nameof(System.String)

// ============================================================================
// 9. TOKEN BOUNDARY & LONGEST-MATCH STRESS
// ============================================================================
module BoundaryStress =
    // `|>` vs `|` `>`
    let pipe1 = 1 |> (+) 2
    let pipe2 = 1 |>(+) 2
    
    // `<@` vs `<` `@`
    let q1 = <@ 1 + 1 @>
    let q2 = <@@ fun x -> x @@>
    
    // `:=` vs `:` `=`
    let refTest = ref 0; refTest := 5
    
    // `:?` vs `:` `?`
    let typeTest (x: obj) = match x with | :? string -> true | _ -> false
    
    // `:?>` vs `:` `?` `>`
    let downcastTest (x: obj) = x :?> string
    
    // `..` vs `.` `.`
    let dotTest = 1..10
    
    // `::` vs `:` `:`
    let consTest = 1 :: []
    
    // `~` prefix vs `~` in operator
    let prefixTilde x = ~x
    let opTilde (~~) a b = a + b