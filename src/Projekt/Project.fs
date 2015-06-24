module Projekt.Project

open System
open System.IO
open System.Xml.Linq

let msbuildns = "{http://schemas.microsoft.com/developer/msbuild/2003}"
let xname s = XName.Get s

let xn s = XName.Get s
let xe n (v: obj) = new XElement(xn (msbuildns + n), v)
let xa n (v: obj) = new XAttribute(xn n, v)

let (|Head|_|) =
    Seq.tryFind (fun _ -> true)

let (|Value|) (xe: XElement) =
    xe.Value

let (|Guid|_|) s =
    match Guid.TryParse s with
    | true, g -> Some g
    | _ -> None

let (|Descendants|_|) name (xe : XElement) =
    match xe.Descendants (xn (msbuildns + name)) with
    | null -> None
    | desc when not (Seq.isEmpty desc) -> 
        Some (desc |> Seq.toList)
    | _ -> None

let (|Descendant|_|) name (xe : XElement) =
    match xe.Descendants (xn (msbuildns + name)) with
    | null -> None
    | Head h -> Some h
    | _ -> None

let (|Element|_|) name (xe : XElement) =
    match xe.Element (xn (msbuildns + name)) with
    | null -> None 
    | e -> Some e

let (|Attribute|_|) name (xe : XElement) =
    match xe.Attribute (xn name) with
    | null -> None 
    | a -> Some a

//queries
let internal projectGuid = 
    function
    | Descendant "ProjectGuid" (Value (Guid pg)) -> 
        Success pg 
    | _ -> Failure "err: failed to read project guid."

let internal projectName = 
    function
    | Descendant "Name" (Value name) -> 
        Success name 
    | _ -> Failure "err: failed to read project name."

let internal assemblyName = 
    function
    | Descendant "AssemblyName" (Value name) -> 
        Success name 
    | _ -> Failure "err: failed to read assembly name."

let internal itemGroup = 
    function
    | Descendant "ItemGroup" el -> 
        Some el 
    | _ -> None

let internal projectReferenceItemGroup =
    function
    | Descendant "ProjectReference" e -> 
        e.Parent |> Some
    | _ -> None

let internal compileItemGroup =
    function
    | Descendant "Compile" el ->
        Some el.Parent
    | _ -> None  

let parentOfDescendant name el =
    match el with
    | Descendant name el -> Some el.Parent
    | _ -> None

let hasCompileWithInclude file (xe: XElement) =
    let hasInclude = function
      | Attribute "Include" a when a.Value = file -> true
      | _ -> false
    xe.Descendants (xn (msbuildns + "Compile"))
    |> Seq.exists hasInclude

let hasProjectReferenceWithInclude incl =
    function
    | Descendants "ProjectReference" descs -> 
        Seq.exists (
            function
            | Attribute "Include" a -> a.Value = incl
            | _ -> false) descs
    | _ -> false

let internal addProjRefNode (path: string) (name: string) (guid : Guid) (el: XElement) =
    let projRef =
        xe "ProjectReference"
            [ xa "Include" path |> box
              xe "Name" name |> box
              xe "Project" (sprintf "{%O}" <| guid) |> box
              xe "Private" "True" |> box ]

    match projectReferenceItemGroup el with
    | Some _ when hasProjectReferenceWithInclude path el -> 
        Success el
    | Some prig ->
        prig.Add projRef 
        Success el
    | None -> 
        let ig = xe "ItemGroup" projRef
        match itemGroup el with
        | Some first ->
            first.AddAfterSelf ig
        | None -> //no ItemGroups!
            el.Add ig
        Success el

let private load (path : string) =
    try XElement.Load path |> Success
    with
    | ex -> Failure (sprintf "err: failed to load %s as XElement. Message: %s" path ex.Message)

let addReference project reference =
    result {
        let relPath = Util.makeRelativePath project reference
        let! proj = load project
        let! reference = load reference
        let! name = 
            match projectName reference with
            | Success name -> Success name
            | _ -> assemblyName reference
        let! guid = projectGuid reference
        return! addProjRefNode relPath name guid proj }

let addFile (project: string) (file: string) (link: Option<string>) : Result<XElement> =
    match load project with
    | Failure x -> Failure x
    | Success proj ->

    let addFileToProject relpath =
        if hasCompileWithInclude relpath proj then
            Failure (sprintf "File '%s' already exists in project." relpath)
        else
            let linkOpt = match link with
                          | None -> []
                          | Some l -> [xe "Link" l :> obj]
            let fileRef = xa "Include" relpath :> obj :: linkOpt
            let fileEntry = xe ("Compile") fileRef
            match List.tryPick (fun f -> f proj)
                               [ parentOfDescendant "Compile"
                                 parentOfDescendant "None" ] with
            | Some ig -> ig.Add fileEntry
            | None -> proj.Add (xe "ItemGroup" fileEntry)
            Success proj

    if link.IsNone then
        if Path.GetDirectoryName project = Path.GetDirectoryName file then
            if not (File.Exists file) then
                (File.Create file).Close()
            addFileToProject (Path.GetFileName file)
        else
            // We would copy 'file' to adjacent to the project
            let target = Path.GetDirectoryName project </> Path.GetFileName file
            if not (File.Exists file) then
                Failure (sprintf "File '%s' not found." file)
            else // File.Exists file
                if File.Exists target then
                    Failure (sprintf "Target file '%s' already present." target)
                else
                    File.Copy(file, target)
                    addFileToProject (Path.GetFileName file)
    else
        addFileToProject (makeRelativePath project file)

let internal removeFileIfPresent (proj: XElement) relpath =
    let hasInclude = function
        | Attribute "Include" a when a.Value = relpath -> true
        | _ -> false
    let eopt =
        proj.Descendants (xn (msbuildns + "Compile"))
        |> Seq.tryFind hasInclude
    match eopt with
    | Some e -> e.Remove(); Success proj
    | None -> Failure (sprintf "File '%s' not found in project." relpath)

let delFile (project: string) (file: string) =
    let proj = XElement.Load project
    let relpath = makeRelativePath project file
    removeFileIfPresent proj relpath

