namespace Projekt

(*
projekt (init|reference|newfile|addfile/addfile) /path/to/project {--template=(library|console)} {--solution=path/to/sln} --direction=(down|up) --repeat={int} --frameworkversion=(4.0|4.5|4.5.1)

--solution - when specificed add remove 
--direction - for move operations
--frameworkversion
--n - repeat action n times - ony works for move operations

examples:
projekt init /path/to/new/fsproj --template=library //creates a new project but does not add it to a solution

projekt reference /path/to/target {thingtoreference} //references project or binary

projekt newfile /path/to/project {FileName} //add file to project - could template files?

projekt addfile /path/to/project /path/to/file //add file to project - could template files?

projekt movefile /path/to/project --direction=up --n=3 //add file to project - could template files?

 *)

open System

[<AutoOpen>]
module Util =
    let (|ToLower|) (s: string) =
        s.ToLowerInvariant()

    let (|ToList|) = Seq.toList 

[<AutoOpen>]
module Types =

    type Template =
        | Console
        | Library

    type Direction =
        | Up
        | Down

    type Repeat = | Repeat of int

    type FrameworkVersion =
        | V4_0
        | V4_5
        | V4_5_1

    type ProjectInitData =
        { ProjPath: string
          Template: Template
          FrameworkVersion: FrameworkVersion
          Solution: string option }
    with 
        static member create (path, ?template, ?fxversion, ?solution) =
            { ProjPath = path
              Template = defaultArg template Library
              FrameworkVersion = defaultArg fxversion V4_5
              Solution = solution }

    type ProjectReferenceData =
        { ProjPath: string
          Target: string }

    type FileData =
        { ProjPath: string
          FilePath: string }

    type Operation =
        | Init of ProjectInitData //project path 
        | Reference of ProjectReferenceData
        | NewFile of FileData
        | AddFile of FileData
        | MoveFile of FileData * Direction * Repeat
        | Error

    
module Args =
    open System.IO
    open Nessos.UnionArgParser
    type private Args =
        | Template of string
        | FrameworkVersion of string
        | Direction of int
        | Repeat of int
    with
        interface IArgParserTemplate with
            member s.Usage = 
                match s with
                | Template _ -> "template"
                | Direction _ -> "direction"
                | Repeat _ -> "repeat"
                | FrameworkVersion _ -> "fxversion"

    let private parser = UnionArgParser.Create<Args>()

    let private (|Options|) (args : string list) =
        let results = parser.Parse(List.toArray args)
        results

    //splits the required arguments off from the options
    let split (args : string list) =
        List.partition (fun (s : string) -> not <| s.StartsWith "--" ) args

    let parse (ToList args) : Operation =
        let required, _ = split args
        match required with
        | ToLower "init" :: path :: Options opts -> 
            let template = opts.TryGetResult(<@ Template @>)
            let t =
                match template with
                | Some (ToLower "console") -> Console
                | Some (ToLower "library") -> Library
                | None -> Library
                | _ -> failwith "invalid template argument specified"
            Init (ProjectInitData.create(path, t))
        | _ -> failwith "not implemented yet"

module Main =
  [<EntryPoint>]
  let main argv =
    printfn "pre %A" argv
    let parsedArgs = Args.parse argv
    printfn "operation: %A parseArgs" parsedArgs
    0
