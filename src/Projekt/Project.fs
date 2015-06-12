module Projekt.Project

open System
open System.Xml.Linq

let xns s = XNamespace.Get s
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

let (|Descendant|_|) name (xe : XElement) =
    match xe.Descendants (xn (msbuildns + name)) with
    | Head h -> Some h
    | _ -> None

let (|Element|_|) name (xe : XElement) =
    match xe.Element (xn (msbuildns + name)) with
    | null -> None 
    | e -> Some e

//queries
let internal projectGuid = 
    function
    | Descendant "ProjectGuid" (Value (Guid pg)) -> 
        Some pg 
    | _ -> None

let internal projectName = 
    function
    | Descendant "Name" (Value name) -> 
        Some name 
    | _ -> None

let internal assemblyName = 
    function
    | Descendant "AssemblyName" (Value name) -> 
        Some name 
    | _ -> None

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

let internal addProjRefNode (path: string) (name: string) (guid : Guid) (el: XElement) =
    let add (el: XElement) =
        el.Add(
            xe "ProjectReference"
                [ xa "Include" path |> box
                  xe "Name" name |> box
                  xe "Project" (sprintf "{%O}" <| guid) |> box
                  xe "Private" "True" |> box ] )

    match projectReferenceItemGroup el with
    | Some prig ->
        //TODO check to ensure duplicate ProjectReferences aren't added
        add prig
        el
    | None -> 
        let ig = itemGroup el |> Option.get
        add ig
        el

let addReference (project : string) (reference : string) =
    let relPath = Projekt.Util.makeRelativePath project reference
    printfn "relpath: %s" relPath
    let proj = XElement.Load project
    let reference = XElement.Load reference
    printfn "loaded"
    let name = 
        match projectName reference with
        | Some name -> name
        | None -> assemblyName reference |> Option.get
    printfn "name: %A" name
    let guid = projectGuid reference
    printfn "guid: %A" guid
    addProjRefNode relPath name guid.Value proj
    




