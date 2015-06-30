module Projekt.Args

open Projekt.Types
open System.IO
open Nessos.UnionArgParser

type private Args =
    | Template of string
    | [<AltCommandLine("-fxv")>] FrameworkVersion of string
    | Organisation of string
    | Direction of string
    | Repeat of int
    | Link of string
    | Compile of bool
with
    interface IArgParserTemplate with
        member s.Usage = 
            match s with
            | Template _ -> "init -- specify the template (library|console) [default: library]"
            | Direction _ -> "movefile -- specify the direction (down|up)"
            | Repeat _ -> "movefile -- specify the distance [default: 1]"
            | FrameworkVersion _ -> "init -- specify the framework version (4.0|4.5|4.5.1) [default: 4.5]"
            | Organisation _ -> "init -- specify the organisation"
            | Link _ -> "addfile -- specify an optional Link attribute"
            | Compile _ -> "addfile -- should the file be compiled or not  [default: true]"

let private templateArg (res : ArgParseResults<Args>) =
    match res.TryGetResult(<@ Template @>) with
    | Some (ToLower "console") -> Console
    | Some (ToLower "library") -> Library
    | None -> Library
    | _ -> failwith "invalid template argument specified"

let private frameworkVersionArg (res : ArgParseResults<Args>) =
    match res.TryGetResult(<@ FrameworkVersion @>) with
    | Some "4.0" -> V4_0
    | Some "4.5" -> V4_5 
    | Some "4.5.1" -> V4_5_1
    | None -> V4_5
    | _ -> failwith "invalid framework version argument specified"

let private parseDirection s =
    match s with
    | ToLower "up" -> Up
    | ToLower "down" -> Down
    | _ -> failwith "invalid direction specified"

let private parser = UnionArgParser.Create<Args>()

let private (|Options|) (args : string list) =
    let results = parser.Parse(List.toArray args)
    results

let (|FullPath|_|) (path : string) =
    try 
        Path.GetFullPath path |> Some
    with
    | _ -> None

let commandUsage = "projekt (init|reference|newfile|addfile|delfile) /path/to/project [/path/to/(file|project)]"

let parse (ToList args) : Result<Operation> =
    try
        match args with
        | [] -> 
            parser.Usage commandUsage 
            |> Failure

        | ToLower "init" :: FullPath path :: Options opts -> 
            let template = templateArg opts
            let fxv = frameworkVersionArg opts
            let org = 
                match opts.TryGetResult(<@ Organisation @>) with
                | Some org -> org
                | None -> "MyOrg"
            Init (ProjectInitData.create (path, template, fxv, org)) |> Success
            
        | ToLower "addfile" :: FullPath project :: FullPath file :: Options opts ->
            let compile = opts.GetResult(<@ Compile @>, true)
            AddFile { ProjPath = project
                      FilePath = file
                      Link = opts.TryGetResult <@ Link @>
                      Compile = compile }
            |> Success
            
        | [ToLower "delfile"; FullPath project; FullPath file] -> 
            DelFile { ProjPath = project; FilePath = file }
            |> Success
            
        | ToLower "movefile" :: FullPath project :: FullPath file :: Options opts
                when opts.Contains <@ Direction @> ->

            let direction = opts.PostProcessResult(<@ Direction @>, parseDirection)
            MoveFile { ProjPath = project
                       FilePath = file
                       Direction = direction
                       Repeat = opts.GetResult(<@ Repeat @>, 1)}
            |> Success
            
        | [ToLower "reference"; FullPath project; FullPath reference] -> 
            Reference { ProjPath = project; Reference = reference } |> Success

        | _ -> Failure (parser.Usage (sprintf "Error: '%s' is not a recognized command or received incorrect arguments.\n\n%s" args.Head commandUsage))
    with
    | :? System.ArgumentException as e ->
            let lines = e.Message.Split([|'\n'|])
            let msg = parser.Usage (sprintf "%s\n\n%s" lines.[0] commandUsage)
            Failure msg
