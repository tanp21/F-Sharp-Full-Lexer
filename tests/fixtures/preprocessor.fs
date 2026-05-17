module Sample.Preprocessor

#if DEBUG && !CI
let mode = "debug"
#elif RELEASE
let mode = "release"
#else
let mode = "default"
#endif

let enabled = true

