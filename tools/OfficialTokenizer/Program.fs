open System
open System.IO
open FSharp.Compiler.Tokenization

let usage () =
    eprintfn "Usage: dotnet run --project tools/OfficialTokenizer -- [--define NAME ...] FILE.fs"
    2

let rec parseArgs defines file args =
    match args with
    | [] -> defines, file
    | "--define" :: name :: rest -> parseArgs (name :: defines) file rest
    | "--define" :: [] ->
        eprintfn "--define requires a value"
        Environment.Exit 2
        defines, file
    | arg :: rest when arg.StartsWith("--define=", StringComparison.Ordinal) ->
        parseArgs (arg.Substring("--define=".Length) :: defines) file rest
    | arg :: rest when arg.StartsWith("-", StringComparison.Ordinal) ->
        eprintfn "Unknown option: %s" arg
        Environment.Exit 2
        defines, file
    | arg :: rest ->
        match file with
        | Some _ ->
            eprintfn "Only one input file is supported"
            Environment.Exit 2
            defines, file
        | None -> parseArgs defines (Some arg) rest

let escape (text: string) =
    text
        .Replace("\\", "\\\\")
        .Replace("\t", "\\t")
        .Replace("\r", "\\r")
        .Replace("\n", "\\n")

let scanLine (lineNo: int) (line: string) (state: FSharpTokenizerLexState ref) (tokenizer: FSharpLineTokenizer) =
    let rec loop () =
        match tokenizer.ScanToken state.Value with
        | Some tok, nextState ->
            let text = line.Substring(tok.LeftColumn, tok.RightColumn - tok.LeftColumn + 1)
            printfn
                "%d:%d:%d\t%s\t%s\t%d\t%d"
                (lineNo + 1)
                (tok.LeftColumn + 1)
                (tok.RightColumn + 1)
                tok.TokenName
                (escape text)
                tok.Tag
                tok.FullMatchedLength
            state.Value <- nextState
            loop ()
        | None, nextState -> state.Value <- nextState

    loop ()

[<EntryPoint>]
let main argv =
    let defines, file = argv |> Array.toList |> parseArgs [] None

    match file with
    | None -> usage ()
    | Some path when not (File.Exists path) ->
        eprintfn "File not found: %s" path
        1
    | Some path ->
        let source = FSharpSourceTokenizer(List.rev defines, Some path, None, None)
        let state = ref FSharpTokenizerLexState.Initial

        File.ReadAllLines path
        |> Array.iteri (fun lineNo line ->
            let tokenizer = source.CreateLineTokenizer line
            scanLine lineNo line state tokenizer)

        0
