[<AutoOpen>]
module Projekt.Util

open System

let (|ToLower|) (s: string) =
    s.ToLowerInvariant()

let (|ToList|) = Seq.toList 

let (</>) a b = System.IO.Path.Combine(a,b)

let makeRelativePath (source: string) (target: string) =
    let source =
      if source.[source.Length - 1] = IO.Path.DirectorySeparatorChar ||
         source.[source.Length - 1] = IO.Path.AltDirectorySeparatorChar
      then
        source + (string IO.Path.DirectorySeparatorChar)
      else
        source
    (new Uri(source)).MakeRelativeUri(new Uri(target)).ToString()
