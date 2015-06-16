module Projekt.Main
let help = """
projekt (init|reference|newfile|addfile|renamefile) /path/to/project {--template=(library|console)} {--solution=path/to/sln} --direction=(down|up) --repeat={int} --frameworkversion=(4.0|4.5|4.5.1)

--solution - when specificed add remove 
--direction - for move operations
--frameworkversion
--n - repeat action n times - ony works for move operations
--compile=(true|false) - when adding file this flag allows you to exclude it from compilation

examples:

projekt init /path/to/new/fsproj --template=library //creates a new project but does not add it to a solution

projekt reference /path/to/target {thingtoreference} //references project or binary

projekt addfile /path/to/project /path/to/file //add file to project (create if not exists) - could template files?

projekt movefile /path/to/project --direction=up --n=3 //adjust file position

"""

open System

[<EntryPoint>]
let main argv =
    printfn "pre %A" argv
    let op = Args.parse argv

    let save (el : System.Xml.Linq.XElement) (path: string) =
        try
            el.Save path
            0
        with
        | ex ->
            printfn "err: failed to save %s. Message: %s" path ex.Message
            1

    match op with
    | Init data ->
        Template.init "templates" data
        0
    | AddFile data ->
        if not (IO.File.Exists data.FilePath) then
            (IO.File.Create data.FilePath).Close()
        let el = Project.addFile data.ProjPath data.FilePath
        save el data.ProjPath
    | DelFile data ->
        if not (IO.File.Exists data.FilePath) then
            (IO.File.Create data.FilePath).Close()
        let el = Project.addFile data.ProjPath data.FilePath
        save el data.ProjPath
    | Reference data ->
        match Project.addReference data.ProjPath data.Reference with
        | Success el ->
            save el  data.ProjPath
        | Failure msg ->
            printfn "%s" msg
            1
    | _ -> 
        printfn "not implemented yet"
        1

