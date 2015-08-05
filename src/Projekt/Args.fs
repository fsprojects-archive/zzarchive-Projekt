module Projekt.Args

open Projekt.Types

module Args =
    open CommandLine
    open CommandLine.Text

    [<Verb("init", HelpText = "create a new project")>]
    type InitOptions = 
        { [<Value(0, Required = true, MetaName = "project path")>] Path : string 
          [<Option(Default = "library")>] Template : string
          [<Option(Default = "4.5")>] FrameworkVersion : string
          [<Option>] Organization : string option }
    with 
        member x.ToOperation =
            match x.Path with
            | FullPath p -> 
                let template = x.Template |> Template.Create |> Some
                let frmwkVersion = x.FrameworkVersion |> FrameworkVersion.Create |> Some
                (ProjectInitData.create 
                    p 
                    template
                    frmwkVersion
                    x.Organization) |> Init

            | _ -> failwith "not given a full path"
        [<Usage(ApplicationAlias = "projekt")>]
        static member Examples 
            with get () = 
                seq {
                    yield Example("normal usage", {Path = @"c:\code\projekt\"; Template = ""; FrameworkVersion = ""; Organization = None})
                    yield Example("make an exe project", {Path = @"c:\code\projekt\"; Template = "console"; FrameworkVersion = ""; Organization = None})
                    yield Example("target .net 4.0", {Path = @"c:\code\projekt\"; Template = ""; FrameworkVersion = "4.0"; Organization = None})
                }

    [<Verb("reference", HelpText = "reference another dependency in this project")>]
    type private ReferenceOptions = 
        { [<Value(0, Required = true, MetaName = "project path")>] ProjectPath : string
          [<Value(1, Required = true, MetaName = "reference path")>] ReferencePath : string }
    with 
        member x.ToOperation =
            match x.ProjectPath, x.ReferencePath with
            | FullPath project, FullPath reference ->
                { ProjPath = project
                  Reference = reference }
                |> Reference
            | _,_ -> failwith "one or both paths were invalid"

    [<Verb("movefile", HelpText = "Move a file within a project")>]
    type private MoveFileOptions = 
        { [<Value(0, Required = true, MetaName = "project path")>] ProjectPath : string
          [<Value(1, Required = true, MetaName = "file path")>] FilePath : string
          [<Option(Required = true)>] direction : string
          [<Option(Default = 1)>] repeat : int }
    with
        member x.ToOperation =
            match x.ProjectPath, x.FilePath, Direction.Create x.direction with
            | FullPath project, FullPath file, dir -> 
                { ProjPath = project
                  FilePath = file
                  Direction = dir
                  Repeat = x.repeat }
                |> MoveFile
            | _,_,_ -> failwith "invalid paths or direction"

    [<Verb("addfile", HelpText = "Add a file to a project")>]
    type private AddFileOptions = 
        { [<Value(0, Required = true, MetaName = "project path")>] ProjectPath : string
          [<Value(1, Required = true, MetaName = "file path")>] FilePath : string
          [<Option>] link : string option
          [<Option(Default = true)>] compile : bool }
    with 
        member x.ToOperation =
            match x.ProjectPath, x.FilePath with
            | FullPath project, FullPath file -> 
                { ProjPath = project
                  FilePath = file
                  Link = x.link
                  Compile = x.compile } 
                |> AddFile 
            | _,_ -> failwith "invalid paths"

    [<Verb("delfile", HelpText = "Delete a file from a project")>]
    type private DelFileOptions = 
        { [<Value(0, Required = true, MetaName = "project path")>] ProjectPath : string
          [<Value(1, Required = true, MetaName = "file path")>] FilePath : string   }
    with
        member x.ToOperation =
            match x.ProjectPath, x.FilePath with
            | FullPath project, FullPath file -> 
                // typing needed here because of the duplication between MoveFileData and DelFileData records
                // TODO: maybe consolidate?
                ({ ProjPath = project 
                   FilePath = file } : DelFileData)
                |> DelFile 
            | _,_ -> failwith "invalid paths"

    let private parser = CommandLine.Parser.Default

    let parse args = 
        let parsed = parser.ParseArguments<InitOptions, ReferenceOptions, MoveFileOptions, AddFileOptions, DelFileOptions>(args)
        // tried to get fancy here with a statically resolved type param to invoke the ToOperation member on the individal option cases, but I couldn't get it to work....

        parsed.Return<InitOptions, ReferenceOptions, MoveFileOptions, AddFileOptions, DelFileOptions, Result<Operation>>(
            (fun (init : InitOptions) -> init.ToOperation |> Success), 
            (fun (ref : ReferenceOptions) -> ref.ToOperation |> Success), 
            (fun (mv : MoveFileOptions) -> mv.ToOperation |> Success), 
            (fun (add : AddFileOptions) -> add.ToOperation |> Success), 
            (fun (del : DelFileOptions) -> del.ToOperation |> Success), 
            (fun errs -> errs |> Seq.map (fun e -> e.ToString()) |> String.concat ";" |> Failure)
        )
        