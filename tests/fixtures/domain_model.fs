module Sample.Domain

type EmailAddress = private EmailAddress of string

type Person =
  {
    Name: string
    Age: int
    Email: EmailAddress option
  }

type Shape =
  | Circle of radius: decimal
  | Rectangle of width: decimal * height: decimal
  | Named of string * Shape

let describe shape =
  match shape with
  | Circle radius -> $"circle: {radius}"
  | Rectangle (width, height) -> $"rectangle: {width}x{height}"
  | Named (name, _) -> name

