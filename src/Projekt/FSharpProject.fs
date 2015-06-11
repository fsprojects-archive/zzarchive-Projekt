namespace Projekt

open System.Xml.Linq
open XmlLinqHelpers
open System

type FSharpProject(path: string) =
  let project = XDocument.Load(path)

  let pathRelativeToPath (source: string) (target: string) =
    let source =
      if source.[source.Length - 1] = IO.Path.DirectorySeparatorChar ||
         source.[source.Length - 1] = IO.Path.AltDirectorySeparatorChar
      then
        source + (string IO.Path.DirectorySeparatorChar)
      else
        source

    (new Uri(source)).MakeRelativeUri(new Uri(target)).ToString()

  member x.AddFile(file) =
    let c = Seq.head (project.Descendants(xn (msbuildNamespace + "Compile")))
    let e = xe (msbuildNamespace + "Compile") (xa "Include" (pathRelativeToPath path file))
    c.AddBeforeSelf(e)

  member x.Flush() =
    project.Save(path)