namespace Projekt

open System.Xml.Linq
open Projekt.XmlLinqHelpers
open System

type FSharpProject(path: string) =
  let project = XDocument.Load(path)

  member x.AddFile(file) =
    let c = Seq.head (project.Descendants(xn (msbuildNamespace + "Compile")))
    let e = xe (msbuildNamespace + "Compile") (xa "Include" (makeRelativePath path file))
    c.AddBeforeSelf(e)

  member x.Flush() =
    project.Save(path)
