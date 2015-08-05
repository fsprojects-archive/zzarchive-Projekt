[<AutoOpen>]
module Projekt.Util

open System
open System.IO

let (|ToLower|) (s: string) =
    s.ToLowerInvariant()

let (|ToList|) = Seq.toList 

let (|FullPath|_|) (path : string) =
        try 
            Path.GetFullPath path |> Some
        with
        | _ -> None

let (</>) a b = System.IO.Path.Combine(a,b)

let csfunc f = System.Func<_,_>(f)

let makeRelativePath (source: string) (target: string) =
    let source =
      if source.[source.Length - 1] = IO.Path.DirectorySeparatorChar ||
         source.[source.Length - 1] = IO.Path.AltDirectorySeparatorChar
      then
        source + (string IO.Path.DirectorySeparatorChar)
      else
        source
    (new Uri(source)).MakeRelativeUri(new Uri(target)).ToString()
