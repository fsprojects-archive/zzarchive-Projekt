module Projekt.Args

open Projekt.Types

module NewArgs =
    open CommandLine

    [<Verb("init", HelpText = "create a new project")>]
    type public InitOptions = {
        [<Value(0, Required = true)>]   path : string 
        [<Option(Default = "library")>] template : string
        [<Option(Default = "4.5")>]       frameworkVersion : string
        [<Option>]                      organization : string option
    }
    with 
        member x.ToOperation =
            match x.path with
            | FullPath p -> 
                ProjectInitData.create p (x.template |> Some |> Template.Create |> Some) (x.frameworkVersion |> Some |> FrameworkVersion.Create |> Some) x.organization
                |> Init
            | _ -> failwith "not given a full path"

    [<Verb("reference", HelpText = "reference a dll")>]
    type public ReferenceOptions = {
        [<Value(0, Required = true)>]   projectPath : string
        [<Value(1, Required = true)>]   referencePath : string
    }
    with 
        member x.ToOperation =
            match x.projectPath, x.referencePath with
            | FullPath project, FullPath reference -> {ProjPath = project; Reference = reference} |> Reference
            | _,_ -> failwith "one or both paths were invalid"

    [<Verb("movefile", HelpText = "Move a file within a project")>]
    type public MoveFileOptions = {
        [<Value(0, Required = true)>]   projectPath : string
        [<Value(1, Required = true)>]   filePath : string
        [<Option(Required = true)>]     direction : string
        [<Option(Default = 1)>]         repeat : int
    }
    with
        member x.ToOperation =
            match x.projectPath, x.filePath, Direction.Create x.direction with
            | FullPath project, FullPath file, dir -> {ProjPath = project; FilePath = file; Direction = dir; Repeat = x.repeat} |> MoveFile
            | _,_,_ -> failwith "invalid paths or direction"

    [<Verb("addfile", HelpText = "Add a file to a project")>]
    type public AddFileOptions = {
        [<Value(0, Required = true)>]   projectPath : string
        [<Value(1, Required = true)>]   filePath : string
        [<Option>]                      link : string option
        [<Option(Default = true)>]      compile : bool 
    }
    with 
        member x.ToOperation =
            match x.projectPath, x.filePath with
            | FullPath project, FullPath file -> {ProjPath = project; FilePath = file; Link = x.link; Compile = x.compile} |> AddFile 
            | _,_ -> failwith "invalid paths"

    [<Verb("delfile", HelpText = "Delete a file from a project")>]
    type public DelFileOptions = {
        [<Value(0, Required = true)>]   projectPath : string
        [<Value(1, Required = true)>]   filePath : string
    }
    with
        member x.ToOperation =
            match x.projectPath, x.filePath with
            | FullPath project, FullPath file -> ({ProjPath = project; FilePath = file} : DelFileData) |> DelFile
            | _,_ -> failwith "invalid paths"

    [<Verb("version", HelpText = "Print out the version of this tool")>]
    type public VersionOptions = {
        [<Option>] noArg : string option
    }
    with 
        member x.ToOperation = Version

    let parse args = 
        let parsed = CommandLine.Parser.Default.ParseArguments<InitOptions, ReferenceOptions, MoveFileOptions, AddFileOptions, DelFileOptions, VersionOptions>(args)
        // tried to get fancy here with a statically resolved type param to invoke the ToOperation member on the individal option cases, but I couldn't get it to work....

        result {
            try 
                return 
                    parsed.Return<InitOptions, ReferenceOptions, MoveFileOptions, AddFileOptions, DelFileOptions, VersionOptions, Operation>(
                        (fun (init : InitOptions) -> init.ToOperation), 
                        (fun (ref : ReferenceOptions) -> ref.ToOperation), 
                        (fun (mv : MoveFileOptions) -> mv.ToOperation), 
                        (fun (add : AddFileOptions) -> add.ToOperation), 
                        (fun (del : DelFileOptions) -> del.ToOperation), 
                        (fun (ver : VersionOptions) -> ver.ToOperation), 
                        (fun errs -> printfn "%A" errs; Operation.Exit)
                    )
            with
                | _ -> return Operation.Exit
        }
        

module OldArgs =
    open Nessos.UnionArgParser

    let commandUsage = "projekt (init|reference|movefile|addfile|delfile|version) /path/to/project [/path/to/(file|project)]"

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
                | FrameworkVersion _ -> "init -- specify the framework version (4.0|4.5|4.5.1) [default: 4.5]"
                | Organisation _ -> "init -- specify the organisation"
                | Direction _ -> "movefile -- specify the direction (down|up)"
                | Repeat _ -> "movefile -- specify the distance [default: 1]"
                | Link _ -> "addfile -- specify an optional Link attribute"
                | Compile _ -> "addfile -- should the file be compiled or not  [default: true]"

    let private templateArg (res : ArgParseResults<Args>) = res.TryGetResult(<@ Template @>) |> Template.Create

    let private frameworkVersionArg (res : ArgParseResults<Args>) = res.TryGetResult(<@ FrameworkVersion @>) |> FrameworkVersion.Create

    let private parseDirection s = s |> Direction.Create
        
    let private parser = UnionArgParser.Create<Args>()

    let private (|Options|) (args : string list) =
        let results = parser.Parse(List.toArray args)
        results


    let parse (ToList args) : Result<Operation> =
        try
            match args with
            | [] -> 
                parser.Usage commandUsage 
                |> Failure

            | ToLower "version" :: _ ->
                Success Version

            | ToLower "init" :: FullPath path :: Options opts -> 
                let template = templateArg opts
                let fxv = frameworkVersionArg opts
                let org = 
                    match opts.TryGetResult(<@ Organisation @>) with
                    | Some org -> org
                    | None -> "MyOrg"
                ProjectInitData.create path (Some template) (Some fxv) (Some org) |> Init |> Success
            
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

