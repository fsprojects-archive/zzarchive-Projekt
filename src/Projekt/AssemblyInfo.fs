namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("Projekt")>]
[<assembly: AssemblyProductAttribute("Projekt")>]
[<assembly: AssemblyDescriptionAttribute("A command line tool for creating and updating F# project files")>]
[<assembly: AssemblyVersionAttribute("0.0.1")>]
[<assembly: AssemblyFileVersionAttribute("0.0.1")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.0.1"
