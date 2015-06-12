#load "Util.fs"
#load "Types.fs"
#r "System.Xml"
#r "System.Xml.Linq"

open System
open System.Xml.Linq

let xns s = XNamespace.Get s
let msbuildns = "http://schemas.microsoft.com/developer/msbuild/2003"
let xn s = XName.Get(s, msbuildns) 
let xname s = XName.Get s

let p = XElement.Load("Projekt.fsproj")
let pt = XElement.Load("../../tests/Projekt.Tests/Projekt.Tests.fsproj")

let (|Head|_|) =
    Seq.tryFind (fun _ -> true)

let (|Value|) (xe: XElement) =
    xe.Value

let (|Guid|_|) s =
    match Guid.TryParse s with
    | true, g -> Some g
    | _ -> None

let (|Descendant|_|) name (xe : XElement) =
    match xe.Descendants (xn name) with
    | Head h -> Some h
    | _ -> None

let (|Element|_|) name (xe : XElement) =
    match xe.Element (xn name) with
    | null -> None 
    | e -> Some e

//getters
let projectGuid = 
    function
    | Descendant "ProjectGuid" (Value (Guid pg)) -> 
        Some pg 
    | _ -> None

let projectReferenceItemGroup =
    function
    | Descendant "ProjectReference" e -> 
        e.Parent |> Some
    | _ -> None

let projRefT = """
    <ProjectReference Include="..\..\src\Projekt\Projekt.fsproj">
      <Name>Projekt</Name>
      <Project>{165a6853-05ed-4f03-a7b1-1c84d4f01bf5}</Project>
      <Private>True</Private>
    </ProjectReference>
    """
projectReferenceItemGroup pt
p.Name.LocalName
p.Element (xn "Import")
p.Value
projectGuid p
open System.IO

let removeCommon x y =
    let rec inner a b =
        match a, b with
        | [], _ | _, [] -> a, b
        | ha :: at, hb :: bt when ha = hb ->
            inner at bt
        | ha :: at, hb :: bt -> a, b
    inner x y

let f = @"/tst/a/b/c/file.txt".Split(Path.DirectorySeparatorChar) |> Array.toList
let t = @"/tst/a/b/d/e/oth.txt".Split(Path.DirectorySeparatorChar) |> Array.toList
let f', t' =removeCommon f t

let f'' = List.map (fun _ -> "..") f'
let sep = string Path.DirectorySeparatorChar
String.concat sep (f'' @ t')

Path.GetDirectoryName "/test/"
Path.GetFileName "/test/"







