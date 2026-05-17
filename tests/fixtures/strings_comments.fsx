module Sample.Strings

/// Builds a report line.
let report name count =
  let escaped = "line one\nline two"
  let verbatim = @"c:\temp\reports"
  let triple = """alpha
beta
gamma"""
  let extended = $$"""items: %%04d {{count}}"""
  // The block comment includes nested delimiters.
  (* outer (* inner *) done *)
  $"{name}: {count} - {escaped} - {verbatim} - {triple} - {extended}"

