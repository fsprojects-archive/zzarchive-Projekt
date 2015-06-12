module Projekt.Tests
open System
open System.IO
open Projekt.Util
open NUnit.Framework

let removeCommon x y =
    let rec inner a b =
        match a, b with
        | [], _ | _, [] -> a, b
        | ha :: at, hb :: bt when ha = hb ->
            inner at bt
        | ha :: at, hb :: bt -> a, b
    inner x y

let sep = Path.DirectorySeparatorChar

let split (s: string) =
    s.Split sep |> Array.toList
    |> List.filter ((<>) "")

let (|FullPath|) (s: string) =
    Path.GetFullPath s

let makeRelativePath (FullPath fromPath) (FullPath toPath) =
    let fromFile = Path.GetFileName fromPath
    let fromPath = Path.GetDirectoryName fromPath
        (*
        match fromFile with
        | "" | null -> fromPath 
        | _ ->  fromPath 
        *)
    let toFile = Path.GetFileName toPath
    //let toPath = Path.GetDirectoryName toPath
    if Path.GetDirectoryName fromPath = toPath && toFile = "" then
        "."
    elif fromPath = toPath then toFile
    else
        let common =  removeCommon (split fromPath) (split toPath)
        printfn "common: %A" common
        match common with
        | [""], t ->
            String.concat (string sep) ("" :: t) //</> toFile
        | f, t when fromFile <> "" ->
            let f' = ".." :: List.map (fun _ -> "..") f
            String.concat (string sep) (f' @ t) //</> toFile
        | f, t ->
            let f' = List.map (fun _ -> "..") f
            String.concat (string sep) (f' @ t) //</> toFile


[<TestCase(@"/tst",                 @"/tst",                @"tst")>] //recursive case without trailing sep can only be considered a file
[<TestCase(@"/tst/",                @"/tst/",               @".")>]
[<TestCase(@"/tst/a/b/file.txt",    @"/tst/a/c/oth.txt",    @"../../c/oth.txt")>]
[<TestCase(@"/tst/a/file.txt",      @"/tst/oth.txt",        @"../../oth.txt")>]
[<TestCase(@"/tst/file.txt",        @"/tst/a/b/oth.txt",    @"a/b/oth.txt")>]
(*
[<TestCase(@"/tst/a/b/c/file.txt",  @"/tst/a/b/d/oth.txt",  @"../../d/oth.txt")>]
[<TestCase(@"/tst/a/b/c/",          @"/tst/a/b/d/",         @"../../d/")>]
[<TestCase(@"/tst/a/b/c",           @"/tst/a/b/d",          @"d")>]
*)
let ``makeRelativePath test cases`` (fromPath, toPath, expected) =

    let result = makeRelativePath fromPath toPath
    Assert.AreEqual(expected, result)

