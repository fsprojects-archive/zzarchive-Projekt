module Projekt.Args

open Projekt.Types

module Args =
    open CommandLine

    [<Verb("init", HelpText = "create a new project")>]
    type public InitOptions = 
        {   [<Value(0, Required = true, MetaName = "project path")>]    path : string 
            [<Option(Default = "library")>]                             template : string
            [<Option(Default = "4.5")>]                                 frameworkVersion : string
            [<Option>]                                                  organization : string option    }
    with 
        member x.ToOperation =
            match x.path with
            | FullPath p -> 
                let template = x.template |> Template.Create |> Some
                let frmwkVersion = x.frameworkVersion |> FrameworkVersion.Create |> Some
                (ProjectInitData.create 
                    p 
                    template
                    frmwkVersion
                    x.organization) |> Init

            | _ -> failwith "not given a full path"

    [<Verb("reference", HelpText = "reference a dll")>]
    type public ReferenceOptions = 
        {   [<Value(0, Required = true, MetaName = "project path")>]    projectPath : string
            [<Value(1, Required = true, MetaName = "reference path")>]  referencePath : string  }
    with 
        member x.ToOperation =
            match x.projectPath, x.referencePath with
            | FullPath project, FullPath reference ->
                {ProjPath = project; Reference = reference} |> Reference
            | _,_ -> failwith "one or both paths were invalid"

    [<Verb("movefile", HelpText = "Move a file within a project")>]
    type public MoveFileOptions = 
        {   [<Value(0, Required = true, MetaName = "project path")>]    projectPath : string
            [<Value(1, Required = true, MetaName = "file path")>]       filePath : string
            [<Option(Required = true)>]                                 direction : string
            [<Option(Default = 1)>]                                     repeat : int    }
    with
        member x.ToOperation =
            match x.projectPath, x.filePath, Direction.Create x.direction with
            | FullPath project, FullPath file, dir -> 
                {ProjPath = project; FilePath = file; Direction = dir; Repeat = x.repeat} |> MoveFile
            | _,_,_ -> failwith "invalid paths or direction"

    [<Verb("addfile", HelpText = "Add a file to a project")>]
    type public AddFileOptions = 
        {   [<Value(0, Required = true, MetaName = "project path")>]    projectPath : string
            [<Value(1, Required = true, MetaName = "file path")>]       filePath : string
            [<Option>]                                                  link : string option
            [<Option(Default = true)>]                                  compile : bool  }
    with 
        member x.ToOperation =
            match x.projectPath, x.filePath with
            | FullPath project, FullPath file -> 
                {ProjPath = project; FilePath = file; Link = x.link; Compile = x.compile} |> AddFile 
            | _,_ -> failwith "invalid paths"

    [<Verb("delfile", HelpText = "Delete a file from a project")>]
    type public DelFileOptions = 
        {   [<Value(0, Required = true, MetaName = "project path")>]    projectPath : string
            [<Value(1, Required = true, MetaName = "file path")>]       filePath : string   }
    with
        member x.ToOperation =
            match x.projectPath, x.filePath with
            | FullPath project, FullPath file -> 
                // typing needed here because of the duplication between MoveFileData and DelFileData records
                // TODO: maybe consolidate?
                ({ProjPath = project; FilePath = file} : DelFileData) |> DelFile 
            | _,_ -> failwith "invalid paths"

    let parse args = 
        let parsed = CommandLine.Parser.Default.ParseArguments<InitOptions, ReferenceOptions, MoveFileOptions, AddFileOptions, DelFileOptions>(args)
        // tried to get fancy here with a statically resolved type param to invoke the ToOperation member on the individal option cases, but I couldn't get it to work....

        parsed.Return<InitOptions, ReferenceOptions, MoveFileOptions, AddFileOptions, DelFileOptions, Result<Operation>>(
            (fun (init : InitOptions) -> init.ToOperation |> Success), 
            (fun (ref : ReferenceOptions) -> ref.ToOperation |> Success), 
            (fun (mv : MoveFileOptions) -> mv.ToOperation |> Success), 
            (fun (add : AddFileOptions) -> add.ToOperation |> Success), 
            (fun (del : DelFileOptions) -> del.ToOperation |> Success), 
            (fun errs -> errs |> Seq.map (fun e -> e.ToString()) |> String.concat ";" |> Failure)
        )
        