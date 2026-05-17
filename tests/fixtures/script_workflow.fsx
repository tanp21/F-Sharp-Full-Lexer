open System

let numbers = [| 1; 2; 3; 4 |]

let total =
  numbers
  |> Array.map (fun x -> x * x)
  |> Array.sum

printfn "total = %d" total

