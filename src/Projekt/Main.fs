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
    with 
        override x.ToString () =
            match x with
            | V4_0 -> "4.0"
            | V4_5 -> "4.5"
            | V4_5_1 -> "4.5.1"

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

module Template = 
    open System.IO
    open System.Xml.Linq

    let (</>) a b = Path.Combine(a,b)
    let xname s = XName.Get s
    let copy src dst =
        File.Copy(src, dst)

    let replace (o: string) n (s : string) =
        s.Replace(o, n)

    let copyToTarget templatesDir (data : ProjectInitData) =
        let name = Path.GetFileNameWithoutExtension data.ProjPath
        let targetDir = Path.GetDirectoryName data.ProjPath
        let templateDir = templatesDir </> sprintf "%A" data.Template

        if Directory.Exists targetDir then
            failwith "target directory already exists"
        
        let _ = Directory.CreateDirectory targetDir 
        let p = templateDir </> (sprintf "%A.fsproj" data.Template)
        copy p (targetDir </> sprintf "%s.fsproj" name)

        let files = 
            Directory.GetFiles templateDir 
            |> Seq.filter (fun f -> Path.GetExtension f <> ".fsproj")

        for file in files do
            let name = Path.GetFileName file
            copy file (targetDir </> name)


    let update (data: ProjectInitData) =
        let name = Path.GetFileNameWithoutExtension data.ProjPath
        let targetDir = Path.GetDirectoryName data.ProjPath
        let guid1 = Guid.NewGuid() |> string
        let org = "FSharp"
        Directory.GetFiles targetDir 
        |> Seq.map (fun f -> f, File.ReadAllText f)
        |> Seq.map (fun (f, text) ->
            //I am not too proud for a bit of crummy string replacement :)
            f,  replace "$safeprojectname$" name text
                |> replace "$targetframeworkversion$" (string data.FrameworkVersion) //TODO override ToString
                |> replace "$guid1$" guid1 
                |> replace "$projectname$" name 
                |> replace "$registeredorganization$" org 
                |> replace "$year$" (DateTime.Now.Year.ToString()) 
                |> replace "$registeredorganization$" org )
        |> Seq.iter (fun (f, text) ->
            File.WriteAllText(f, text))

    let init (templatesDir : string) (data : ProjectInitData) =
        copyToTarget templatesDir data
        update data
        
    
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
        | _ -> failwith "not implemented yet"

module Main =
  [<EntryPoint>]
  let main argv =
    printfn "pre %A" argv
    let op = Args.parse argv

    match op with
    | Init data ->
        Template.init (IO.Path.GetFullPath "templates") data
    | _ -> failwith "not implemented yet"
    printfn "operation: %A parseArgs" op
    0
