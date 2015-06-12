[<AutoOpen>]
module Projekt.Util

let (|ToLower|) (s: string) =
    s.ToLowerInvariant()

let (|ToList|) = Seq.toList 

let (</>) a b = System.IO.Path.Combine(a,b)

let makeRelativePath (fromPath) (toPath) =
    ""
