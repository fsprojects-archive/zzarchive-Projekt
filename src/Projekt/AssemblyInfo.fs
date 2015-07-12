namespace System
open System.Reflection
open System.Runtime.CompilerServices

[<assembly: AssemblyTitleAttribute("Projekt")>]
[<assembly: AssemblyProductAttribute("Projekt")>]
[<assembly: AssemblyDescriptionAttribute("A command line tool for creating and updating F# project files")>]
[<assembly: AssemblyVersionAttribute("0.0.2")>]
[<assembly: AssemblyFileVersionAttribute("0.0.2")>]
[<assembly: InternalsVisibleToAttribute("Projekt.Tests")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.0.2"
