module Projekt.Main

(*
projekt (init|reference|newfile|addfile/addfile) /path/to/project {--template=(library|console)} {--solution=path/to/sln} --direction=(down|up) --repeat={int} --frameworkversion=(4.0|4.5|4.5.1)

--solution - when specificed add remove 
--direction - for move operations
--frameworkversion
--n - repeat action n times - ony works for move operations
--compile=(true|false) - when adding file this flag allows you to exclude it from compilation

examples:
projekt init /path/to/new/fsproj --template=library //creates a new project but does not add it to a solution

projekt reference /path/to/target {thingtoreference} //references project or binary

projekt newfile /path/to/project {FileName} //add file to project - could template files?

projekt addfile /path/to/project /path/to/file //add file to project - could template files?

projekt movefile /path/to/project --direction=up --n=3 //add file to project - could template files?

 *)

open System

[<EntryPoint>]
let main argv =
    printfn "pre %A" argv
    let op = Args.parse argv

    match op with
    | Init data ->
        Template.init "templates" data
    | NewFile data ->
        let p = new FSharpProject(data.ProjPath)
        p.AddFile data.FilePath
        p.Flush()
    | Reference data ->
        let el = Project.addReference data.ProjPath data.Reference
        el.Save(data.ProjPath)
    | _ -> failwith "not implemented yet"
    printfn "operation: %A parseArgs" op
    0
