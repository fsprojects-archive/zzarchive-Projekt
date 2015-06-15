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

let (|Attribute|_|) name (xe : XElement) =
    match xe.Attribute (xn name) with
    | null -> None 
    | a -> Some a

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

let internal compileItemGroup =
    function
    | Descendant "Compile" el ->
        Some el.Parent
    | _ -> None  

let parentOfDescendant name el =
    match el with
    | Descendant name el -> Some el.Parent
    | _ -> None

let hasCompileWithInclude incl =
    function
    | Descendant "Compile" (Attribute "Include" a) when a.Value = incl -> true
    | _ -> false

let hasProjectReferenceWithInclude incl =
    function
    | Descendant "ProjectReference" (Attribute "Include" a) when a.Value = incl -> 
        true
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
        ()
    | Some prig ->
        prig.Add projRef
    | None -> 
        let ig = xe "ItemGroup" projRef
        match itemGroup el with
        | Some first ->
            first.AddAfterSelf ig
        | None -> //no ItemGroups!
            el.Add ig
    el

let addReference (project : string) (reference : string) =
    let relPath = Projekt.Util.makeRelativePath project reference
    let proj = XElement.Load project
    let reference = XElement.Load reference
    let name = 
        match projectName reference with
        | Some name -> name
        | None -> assemblyName reference |> Option.get
    let guid = projectGuid reference
    addProjRefNode relPath name guid.Value proj

let addFile (project: string) (file: string) =
    let proj = XElement.Load project
    let relpath = makeRelativePath project file
    if hasCompileWithInclude relpath proj then
      proj
    else
      let fileEntry = xe ("Compile") (xa "Include" relpath)
      let insertionPoint =
        List.pick (fun f -> f proj)
          [ parentOfDescendant "Compile"
            parentOfDescendant "None"
            Some ]
      insertionPoint.Add (xe "ItemGroup" fileEntry)
      proj

let delFile (project: string) (file: string) =
    let proj = XElement.Load project
    let relpath = makeRelativePath project file
    match proj with
    | (Descendant "Compile" (Attribute "Include" a) as e)
        when a.Value = relpath -> e.Remove()
    | _ -> ()
    proj