namespace Sample.Core

module Math =
  let square x = x * x
  let inline clamp lo hi value =
    if value < lo then lo
    elif value > hi then hi
    else value

type Counter(initial: int) =
  let mutable value = initial
  member _.Increment() =
    value <- value + 1
    value
  member _.Value = value

