namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("Projekt")>]
[<assembly: AssemblyProductAttribute("Projekt")>]
[<assembly: AssemblyDescriptionAttribute("A command line tool for creating and updating F# project files")>]
[<assembly: AssemblyVersionAttribute("1.0")>]
[<assembly: AssemblyFileVersionAttribute("1.0")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "1.0"
