#r "packages/UnionArgParser/lib/net40/UnionArgParser.dll"
open Nessos.UnionArgParser

type Args =
    | Project
    | Template of string
with
    interface IArgParserTemplate with
        member s.Usage = 
            match s with
            | Template t -> "template"
            | Project -> "project"

let args = [|"project"; "--template"; "console" |]

let parser = UnionArgParser.Create<Args>()

let results = parser.Parse(args)


results.GetAllResults()

open System.IO
Path.GetDirectoryName "/test/file"
(*
projekt (project|file) (create|add|remove|reference|move) {Name} {--path=/path} {--template=(library|console)} {--target=/path/hello.fsproj} {--solution=path/to/sln} --direction=(down|up)

--path default is current directory
--target is required for file operations and project references - always points to an fsproj
--solution - when specificed add remove 
--direction - for move operations
--n - repeat action n times - ony works for move operations


examples:
projekt project create MyProject --path=src --template=library //creates a new project but does not add it to a solution

projekt project reference MyProject --path=src --target=/path/to/fsproj //adds MyProject as a reference to --target 

projekt project add MyProject --path=src --solution=/path/to/sln //adds MyProject to the solution

projekt file create MyFile.fs --target=/path/to/fsproj //creates a new file in the target project

projekt file move SomeFile.fs --direction=down --n=3 --target=/path/to.fsproj //moves a file down three locations


//utilise defaults
projekt MyProject //creats a new library project in the current directory
 *)







