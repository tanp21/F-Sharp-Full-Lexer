// ============================================================================
// F# LEXER STRESS TEST: OMEGA MODE (F# 8.0+ SPEC-COMPLIANT)
// ============================================================================
// Targets: interpolation formatting, unit syntax, flexible types, attribute targets,
// prefix/infix disambiguation, preprocessor parentheses, zero-width characters,
// slice bounds, and explicit/implicit offside mixing.

#light "off"
#indent "off"

namespace LexerOmega
open System

// ============================================================================
// 1. INTERPOLATION WITH ALIGNMENT & FORMAT SPECIFIERS
// ============================================================================
module InterpolationFormatSpecifiers =
    // Alignment + format: {expr,alignment:format}
    let fmt1 = $"{123,10:f2}"
    let fmt2 = $"{DateTime.Now,-20:yyyy-MM-dd HH:mm}"
    let fmt3 = $"{Math.PI,0:E4}"
    let fmt4 = $"{value,10}"  // alignment only
    let fmt5 = $"{value:f2}"  // format only
    
    // Escaped braces inside interpolation
    let esc1 = $"{{literal}} and {42}"
    let esc2 = $@"\{not interp\} and {{escaped}}"
    let esc3 = $"""Triple {{escaped}} and {value}"""
    
    // Nested expressions in interpolation
    let nest1 = $"{(1 + 2) * 3}"
    let nest2 = $"{match x with | 1 -> "one" | _ -> "other"}"
    let nest3 = $"{if flag then "yes" else "no"}"

// ============================================================================
// 2. UNITS OF MEASURE & GENERIC CONSTRAINTS
// ============================================================================
module UnitsOfMeasureConstraints =
    type kg = kg
    type m = m
    type s = s
    
    // Units with operators inside < >
    let force = 10.0<kg*m/s^2>
    let energy = 5.0<kg*m^2/s^2>
    let zero = 0.0<kg*m/s>
    
    // Type constraints with units
    type Physics<'T when 'T : (static member (+): 'T * 'T -> 'T) and 'T : null> =
        member __.Mass : 'T<kg> = Unchecked.defaultof<'T>
    
    // Static member constraints with units
    let inline scale<'T, 'U when 'U : (static member (*): 'T * float -> 'U)> (x: 'T) = x * 1.0<unit>

// ============================================================================
// 3. FLEXIBLE TYPES & TYPE TEST/CAST OPERATORS
// ============================================================================
module FlexibleTypesAndCasts =
    // #seq<'T> flexible type syntax
    let sum (xs: #seq<int>) = Seq.sum xs
    let print (items: #seq<string>) = Seq.iter printfn "%s" items
    
    // Type test/cast operators
    let cast (x: obj) =
        match x with
        | :? string as s -> s.Length
        | :? int as i -> i + 1
        | _ -> x :?> System.Object
    
    // Ref assignment vs type annotation
    let r = ref 0
    let _ = r := 5        // := operator
    let typed (x: int) = x + 1 // : annotation

// ============================================================================
// 4. ATTRIBUTE TARGETS & TYPE PROVIDER SYNTAX
// ============================================================================
module AttributesAndProviders =
    // Attribute targets
    [<assembly: AssemblyVersion("1.0.0.0")>]
    [<module: CLIMutable>]
    [<method: Obsolete("Use new API", false)>]
    [<param: ParamArray>]
    [<return: MarshalAs(UnmanagedType.BStr)>]
    type Targeted() = ()
    
    // Type provider static parameters
    type Json = JsonProvider<"data.json", SampleIsList=true, ResolutionFolder=__SOURCE_DIRECTORY__>
    type Xml = XmlProvider<Schema="schema.xsd", Global=true>
    
    // Custom attribute with named/positional args
    [<Category("Math"); Description("Adds two values"); Priority=1>]
    let add a b = a + b

// ============================================================================
// 5. PREFIX/INFIX OPERATOR DISAMBIGUATION
// ============================================================================
module PrefixInfixOperators =
    // Prefix unary negation (float)
    let ( ~-. ) x = -x
    let neg = ~-. 3.14
    
    // Prefix unary plus (int)
    let ( ~+ ) x = x
    let pos = ~+ 5
    
    // Infix exponentiation
    let ( ** ) x y = System.Math.Pow(x, y)
    let pow = 2.0 ** 3.0
    
    // Mixed prefix/infix symbolic chains
    let (!.) x = ref x
    let (.!) r = !r
    let ( +. ) a b = a + b
    let ( *. ) a b = a * b
    
    // Longest-match traps
    let trap1 = a + b  // + infix
    let trap2 = ~ a    // ~ prefix (if defined)
    let trap3 = a ~+ b // ~+ infix
    let trap4 = ~+ a   // ~+ prefix
    let trap5 = a ||> b // ||> triple pipe
    let trap6 = a <|| b // <|| left triple pipe
    let trap7 = a <| b  // <| left pipe

// ============================================================================
// 6. SLICE BOUNDS, FROM-END INDEXING & RANGE OPERATORS
// ============================================================================
module SliceAndRangeExtremes =
    let arr = [|0; 1; 2; 3; 4; 5; 6; 7; 8; 9|]
    let mat = array2D [[1;2;3];[4;5;6];[7;8;9]]
    
    // From-end indexing with step
    let s1 = arr.[^0]       // last
    let s2 = arr.[^3..^1]   // range from end
    let s3 = arr.[0..^1..2] // start to end with step
    let s4 = arr.[..^2]     // start to ^2
    
    // Matrix slicing with flexible bounds
    let m1 = mat.[1.., ..2]
    let m2 = mat.[.., ^0..]
    let m3 = mat.[0..1, 1..2]
    
    // Range vs dot adjacency
    let dotRange = 1..10
    let dotFloat = 1.0..10.0
    let dotSep = 1.0 . 10.0  // lex valid, parse error
    let dotSlice = arr.[0..]  // slice open end

// ============================================================================
// 7. PREPROCESSOR EXPRESSIONS & PARENTHETICAL DIRECTIVES
// ============================================================================
module PreprocessorComplex =
    #if (DEBUG || TEST) && not CI && (VER >= 8)
    #define MODE_ALPHA
    #undef MODE_BETA
    #elif not (DEBUG || RELEASE) && (MODE_GAMMA = true)
    #define MODE_BETA
    #else
    #define MODE_FALLBACK
    #endif
    
    #nowarn 9 40 41 52
    #r "System.Runtime"
    #r "nuget: Newtonsoft.Json, 13.0.1"
    #load "helpers.fsx"
    #I "./packages"
    #line 100 "gen.fs"
    #time "on"
    #indent "on"

// ============================================================================
// 8. INDENTATION, OFFSIDE TOGGLING & EXPLICIT SEMICOLONS
// ============================================================================
module IndentationOffsideToggles =
    #light "off"
    begin
        let x = 1;
        let y = 2;
        let z = x + y;
        if true then
            let a = 3;
            a + 1
        else
            0;
    end;
    #light "on"
    
    // Mixed explicit/implicit after toggle
    let mixed =
        let a = 1;
        let b = 2
        a + b
        
    // Blank lines inside block must NOT trigger DEDENT
    let blankLines =
        if true then
            
            let x = 1
            x + 1
        else
            0

// ============================================================================
// 9. ZERO-WIDTH CHARACTERS, COMBINING MARKS & BACKTICK EDGE CASES
// ============================================================================
module UnicodeZeroWidthBackticks =
    // Zero-width non-joiner (U+200C) and joiner (U+200D)
    let cafe\u200C = "coffee"
    let family\u200D = "family"
    
    // Combining marks
    let naïve = "na\u00EFve"
    let café = "cafe\u0301"
    
    // Backtick identifiers with spaces, digits, symbols, Unicode
    let `type` = 1
    let `with` = 2
    let `when` = 3
    let `123` = "starts with digit"
    let ` ` = "contains space"
    let `⚡⚡⚡` = "emoji"
    let `α β γ` = "greek with spaces"
    let `(+@)` = fun a b -> a + b
    let `∑!` = fun xs -> List.sum xs
    
    // Newline inside backtick is ILLEGAL (lexer must emit error or reject)
    // let `broken\nid` = 0

// ============================================================================
// 10. NESTED QUOTATIONS, ACTIVE PATTERNS & CONTEXTUAL COLLISIONS
// ============================================================================
module QuotationsActivePatterns =
    let q1 = <@ 1 + 1 @>
    let q2 = <@@ fun x -> x @@>
    let q3 = <@ <@ nested @> @>
    let q4 = <@@ <@@ double nested @@> @@>
    
    let ap1 = (|Div|_|) 3
    let ap2 = (|Even|Odd|) 5
    let ap3 = (|A|B|C|) x
    let ap4 = (|Single|) 1
    
    let test x =
        match x with
        | Div 3 -> "div"
        | Even -> "even"
        | A | B | C -> "abc"
        | _ -> "other"
        
    // Contextual keyword collisions
    let testWhen x = match x with | n when n > 0 -> "pos" | _ -> "neg"
    let testAs (x: obj) = match x with | :? string as s -> s.Length | _ -> 0
    let testWith () =
        try failwith "x" with _ -> 0
        interface IDisposable with member _.Dispose() = ()
    let rec a x = x and b y = y
    type A = { V: int } and B = { S: string }