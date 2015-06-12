module Projekt.Args

open Projekt.Types
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

let private templateArg (res : ArgParseResults<Args>) =
    match res.TryGetResult(<@ Template @>) with
    | Some (ToLower "console") -> Console
    | Some (ToLower "library") -> Library
    | None -> Library
    | _ -> failwith "invalid template argument specified"

let private parser = UnionArgParser.Create<Args>()

let private (|Options|) (args : string list) =
    let results = parser.Parse(List.toArray args)
    results

let (|FullPath|_|) (path : string) =
    try 
        Path.GetFullPath path |> Some
    with
    | _ -> None

//splits the required arguments off from the options
let split (args : string list) =
    List.partition (fun (s : string) -> not <| s.StartsWith "--" ) args

let parse (ToList args) : Operation =
    let required, _ = split args
    match required with
    | ToLower "init" :: FullPath path :: Options opts -> 
        let template = templateArg opts
        Init (ProjectInitData.create (path, template))
    | [ToLower "newfile"; FullPath project; FullPath file] -> 
        NewFile { ProjPath = project; FilePath = file }
    | [ToLower "reference"; FullPath project; FullPath reference] -> 
        Reference { ProjPath = project; Reference = reference }
    | _ -> failwith "not implemented yet"
