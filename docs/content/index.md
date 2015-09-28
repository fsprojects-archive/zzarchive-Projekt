# What is Projekt?

Projekt is a tool for generating and managing F# project (`.fsproj`)
files, designed to be used with lightweight text editors. There are
two reasons for using `.fsproj` files in your F# development:

1. Visual Studio and Xamarin Studio/Monodevelop both use this project
   format. If you are interested in working with others, or making
   your project easy to approach, then this is the way to go.

2. All intellisense tools for F# currently require an `.fsproj` file
   as a basis for describing the project. This isn't a fundamental
   limitation, but this is how
   [FSharp.Compiler.Service](http://github.com/fsharp/FSharp.Compiler.Service)
   currently works.

## How to get Projekt

Projekt is currently available as a [download from GitHub.com][download].

## Getting Started

As this tool is designed to be integrated with editors, this
documentation is intended for developers. To start using the tool,
either:

1. Clone [the source][gh] and build with `build.cmd` (Windows) or
`build.sh` (Unix), or
2. Download the latest [release archive][download].

Projekt is intended to be invoked from the command line, and has a
number of 'subcommands', much like Git or
[Paket](https://github.com/fsprojects/Paket). All commands except
`version` expect an `.fsproj` file to be passed as the first argument,
and any that manipulate the F# source files and project file
referenced by the `.fsproj` file also require a second argument. The
help text below lists the commands and the various command-specific
arguments.

    [lang=batch]
    projekt (init|reference|movefile|addfile|delfile|listfiles|version) /path/to/project [/path/to/(file|project)]

            --template <string>: init -- specify the template (library|console) [default: library]
            --frameworkversion [-fxv] <string>: init -- specify the framework version (4.0|4.5|4.5.1) [default: 4.5]
            --organisation <string>: init -- specify the organisation
            --direction <string>: movefile -- specify the direction (down|up)
            --repeat <int>: movefile -- specify the distance [default: 1]
            --link <string>: addfile -- specify an optional Link attribute
            --compile <bool>: addfile -- should the file be compiled or not  [default: true]
            --help [-h|/h|/help|/?]: display this list of options.

## Example Usage

#### Generate a new project

Use the `init` subcommand. One possible usage is:

    [lang=batch]
    <path to projekt>/projekt init MyProject.fsproj --template console --organisation MyOrganisation

#### Add a file to a project

To add the file `MyFile.fs` to the project `MyProject.fsproj` (where
both are in the current directory), the following command should be
used:

    [lang=batch]
    <path to projekt>/projekt addfile MyProject.fsproj MyFile.fs

#### Change order of files in a project

The order of compilation is important in F# projects. The previous
command would add the file `MyFile.fs` as the last file in the
project. To move the file `MyFile.fs` up by two, use:

    [lang=batch]
    <path to projekt>/projekt movefile MyProject.fsproj MyFile.fs --direction up --repeat 2

#### List the order of files in a project

The order of compilation is important in F# projects. This command lets you
see the current ordering of files in the project. To see the order of files in
`MyProject.fsproj`, use:

    [lang=batch]
    <path to projekt>/projekt listfiles MyProject.fsproj

#### Reference another project

Use the `reference` command:

    [lang=batch]
    <path to projekt>/projekt reference MyProject.fsproj ../AnotherProject/AnotherProject.fsproj

## Contributing and copyright

The project is hosted on [GitHub][gh] where you can [report issues][issues], fork the project and submit pull requests.

Please see the [Quick contributing guide in the README][readme] for contribution guidelines.

The library is available under MIT license, which allows modification and redistribution for both commercial and non-commercial purposes.
For more information see the [License file][license].

  [content]: https://github.com/fsprojects/Projekt/tree/master/docs/content
  [gh]: https://github.com/fsprojects/Projekt
  [issues]: https://github.com/fsprojects/Projekt/issues
  [readme]: https://github.com/fsprojects/Projekt/blob/master/README.md
  [license]: http://fsprojects.github.io/Projekt/license.html
  [download]: https://github.com/fsprojects/Projekt/releases/latest
