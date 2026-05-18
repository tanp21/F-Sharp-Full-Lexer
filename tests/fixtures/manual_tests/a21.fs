#light "off"
#indent "off"
#time "quiet"
#line -50 "تقرير.fs" 15
#r "nuget: Newtonsoft.Json, [13.0.1, 14.0.0)"; let x = 1
#r "$env:LIB/lib.dll", let y = 2
#r "http: cdn/lib.dll?token=a&v=1". let z = 3
open static System.Math
open class System.Object
open type System.Collections.Generic.IEnumerable<_>
let g = global::System.Int32.MaxValue
val extern nativeint GetProcAddress: nativeint * string -> nativeint
val compute: int -> int
val mutable counter: int
#if ~~~(defined(DEBUG) &&& defined(TRACE)) ||| defined(RELEASE) ^^^ defined(EXP)
#define MODE_A
#elif not defined(LEGACY) &&& (defined(ALPHA) ||| defined(BETA))
#define MODE_B
#else
#define MODE_C
#endif
#nowarn -1 0.5 100-200 40-44
#pragma warning disable 40
#help
#time "verbose"
#indent 4
let lf = 1
let cr = 2
let crlf = 3
let vt = 4
let ff = 5
let us = 6
let ls = 7
let ps = 8
let arr = [|0..9|]
let mat = array2D [[1;2];[3;4]]
let s1 = arr.[^0]
let s2 = arr.[0..^1]
let s3 = arr.[^3..^1..2]
let m1 = mat.[1.., ..2]
let m2 = mat.[.., ^0..]
let dotAdj = 1.0..10.0
let dotSep = 1.0 . 10.0
let str1 = "end" + 1
let str2 = @"end" + 2
let str3 = """end""" + 3
let str4 = #"end"# + 4
let str5 = $"end" + 5
let str6 = $#"end" # + 6
let str7 = "http://" + 7
let str8 = @"C:\" + 8
let str9 = """(* not *)""" + 9
let str10 = #"// not"# + 10
let str11 = $"{{not}}" + 11
let str12 = $#"{{raw}}"# + 12
let q1 = <@ 1 + 1 @>
let q2 = <@@ fun x -> x @@>
let q3 = <@ <@ n @> @>
let q4 = <@@ <@@ d @@> @@>
let q5 = <@ let! x = async { return 1 } in x @>
let q6 = <@ match 1 with | 1 -> "a" | _ -> "b" @>
let q7 = <@ try! async.Return 5 with | _ -> 0 @>
let q8 = <@ yield! Seq.singleton 1 @>
let q9 = <@ return! async { return 0 } @>
let q10 = <@ use! r = new System.IO.MemoryStream() in r.Length @>
let q11 = <@ do! Task.Delay 100 @>
type TaskBuilder() =
    member _.Bind(t, f) = t.Bind(f)
    member _.Return(x) = Task.FromResult x
let task = TaskBuilder()
let computeTask () =
    task {
        let! (Some v) = Some 5
        use! r = new System.IO.MemoryStream()
        do! Task.Delay 100
        try! Task.FromResult 42
        with | _ -> 0
        match! Task.FromResult "ok" with
        | "ok" -> "s" | _ -> "f"
        return v
    }
let dyn = System.Dynamic.ExpandoObject()
let getDyn = dyn?Prop
let setDyn = dyn?Prop <- 1
let (?<-) (o: obj) (n: string) (v: obj) = o
let dyn1 = obj?name
let dyn2 = obj?name <- val
let dyn3 = (?<-) obj "n" v
let comp1 = f >> g >> h
let comp2 = h << g << f
let pipe1 = a |> b |> c |>! d |>! e
let pipe2 = a <| b <| c <! d <! e
let op1 = ( +& ) a b
let op2 = ( &+ ) a b
let op3 = ( ~+ ) a b
let op4 = ( +~ ) a b
let op5 = (⊕) a b
let op6 = (⊗) a b
let op7 = (≠) a b
let op8 = (≤) a b
let op9 = (→) a b
let op10 = (←) a b
let hf1 = 0x0.0p0f
let hf2 = 0x1.0p-0d
let hf3 = 0xFF.FFp10D
let hf4 = 0xAp-5M
let hf5 = 0x1.8p+1
let hf6 = 0x0p0
let hf7 = 0x1p0f
let hf8 = 0xFFp0d
let hex1 = 0xFFuL
let hex2 = 0o777s
let hex3 = 0b1010us
let hex4 = 1_000_000n
let hex5 = 1.0_0_0m
let hex6 = .5e2f
let hex7 = 1.e2d
let hex8 = 1_2e3_4
let hex9 = 1.0_0_0_0_0
let hex10 = 0x_F_F_F_F
let blocked = (* this is a comment *)
let opMul = ( * ) 2 3
let docTrap = "http://x.com // not"
let verbatimDoc = @"C:\p\(* not *)"
let tripleDoc = """L1 // not
L2 (* not *)
L3"""
let rawDoc = #"raw // (* not *)"#
let interpDoc = $"p: {__SOURCE_DIRECTORY__} // not"
let rawInterp = $#"v: {42,10:f2} {{b}}"#
/// <summary><![CDATA[ <script>alert("n")</script> ]]>&amp; &lt; &gt; &quot; &apos; &nbsp; &copy; &reg;</summary>
(** Block &amp; &lt; &gt; <em>t</em> **)
let docEntityTrap = 1
let result =
    let a = 5 in
    let b = match a with
        | 1 | 2 | 3 as n when n > 0 -> n * 2
        | 4 | 5 -> 10
        | _ when a < 0 -> 0
        | _ -> -1
    in b + a
let classify = function
    | [] -> "e"
    | [_] -> "s"
    | [x; y] as lst when x < y -> "ta"
    | [x; y] -> "t"
    | _ -> "m"
#light "off"
begin
    let mutable s = 0;
    for i = 1 to 10 do s <- s + i; printfn "%d" s; done;
    while s > 0 do s <- s - 1; done;
    try failwith "t"; with | _ -> 0;
end;
#light "on"
#indent "on"
let mixed = begin let x = 1; x + 2 end
let blankTest = if true then let z = 3; z + 1 else 0
type Core = A = 0 | B = 1
let e = enum<Core> 1
let t = typedefof<System.Collections.Generic.List<_>>
let ty = typeof<int>
let sz = sizeof<int>
let n = nameof(System.String)
let g2 = global::System.Int32.MaxValue
[<assembly: AssemblyVersion("1.0.0.0")>]
[<module: CLIMutable>]
[<method: Obsolete("U", false)>]
type ICalc =
    abstract member Add: int * int -> int
    default Multiply: int * int -> int
interface ICalc with member _.Add(a, b) = a + b
[<DllImport("kernel32.dll", SetLastError=true, EntryPoint="GetTickCount")>]
extern uint32 GetTickCount()
assert (1 + 1 = 2)
checked { let x = 2147483647 + 1; x }
unchecked { let y = 2147483647 + 1; y }
let lazyVal = lazy (printfn "C"; 42)
let rec a x = x and b y = y
let _ = a 1, b 2
let r1 = ###"raw ### with ### delimiter "###
let i1 = $###"fmt: {42,10:f2} and {{braces}}"###
let v1 = @"C:\P F\MyLib.dll"
let v2 = @"C:\P\With""Q\l.dll"
let t1 = """a""b"""
let t2 = """"""""""""""
let c1 = '\''
let c2 = '\"'
let c3 = '\u0027'
let c4 = '\U00000022'
let c5 = '\x41'
let c6 = '\n'
let c7 = '\t'
let c8 = '\r'
let c9 = '\0'
let c10 = '\u0000'
let arabic\u0627\u0644\u0639\u0631\u0628\u064A\u0629 = "Arabic"
let emoji\uFE0F = "variant"
let emoji\uFE00 = "base"
let cafe\u200C\u0301 = "coffee"
let family\u200D = "family"
let `α β γ δ ε ζ η θ` = "greek"
let `\u0041` = "A"
let `type` = 1
let `with	tab` = 2
let `123 456` = 3
let `∑ ∏ ∫ ∂ ∇` = 7
let `⊕ ⊗ ⊘ ≠ ≤ ≥ ≡ → ←` = 8
let ap1 = (|Div|_|) 3
let ap2 = (|Even|Odd|) 5
let ap3 = (|A|B|C|) 7
let ap4 = (|InRange|_|) 1 10
let ap5 = (|π|) 0
let ap6 = (|αβγ|_|) 0 0
let test x =
    match x with
    | Div 3 -> "d"
    | Even -> "e"
    | A | B -> "ab"
    | _ -> "o"