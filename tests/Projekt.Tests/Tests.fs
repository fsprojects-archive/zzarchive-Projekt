module Projekt.Tests

open System
open System.Xml.Linq
open System.IO
open NUnit.Framework
open Projekt.Util
open FsUnit

let baseXml = """<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="4.0" DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <PropertyGroup>
    <ProjectGuid>165a6853-05ed-4f03-a7b1-1c84d4f01bf5</ProjectGuid>
    <AssemblyName>Test</AssemblyName>
    <Name>Test</Name>
  </PropertyGroup>
</Project>
"""

let singleIGandTestRef = """<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="4.0" DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <PropertyGroup>
    <ProjectGuid>165a6853-05ed-4f03-a7b1-1c84d4f01bf5</ProjectGuid>
    <AssemblyName>Test</AssemblyName>
    <Name>Test</Name>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="../../src/Test/Test.fsproj">
      <Name>Test</Name>
      <Project>{165a6853-05ed-4f03-a7b1-1c84d4f01bf5}</Project>
      <Private>True</Private>
    </ProjectReference>
  </ItemGroup>
</Project>
""" 
[<Test>]
let ``hasProjectReferenceWithInclude should find`` () =
    let sut = XElement.Parse singleIGandTestRef
    Project.hasProjectReferenceWithInclude "../../src/Test/Test.fsproj" sut
    |> Assert.IsTrue

[<Test>] 
let ``addProjRefNode should append an ItemGroup if none found`` () =
    let expected = XElement.Parse singleIGandTestRef 
    let guid = Guid.Parse "{165a6853-05ed-4f03-a7b1-1c84d4f01bf5}"
    let sut = XElement.Parse baseXml
    let (Success result) = Project.addProjRefNode "../../src/Test/Test.fsproj" "Test" guid sut
    XNode.DeepEquals(expected, result)
    |> Assert.IsTrue

[<Test>] 
let ``addProjRefNode should append a ProjectReference if one already exists`` () =
    let expected = """<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="4.0" DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <PropertyGroup>
    <ProjectGuid>165a6853-05ed-4f03-a7b1-1c84d4f01bf5</ProjectGuid>
    <AssemblyName>Test</AssemblyName>
    <Name>Test</Name>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="../../src/Test/Test.fsproj">
      <Name>Test</Name>
      <Project>{165a6853-05ed-4f03-a7b1-1c84d4f01bf5}</Project>
      <Private>True</Private>
    </ProjectReference>
    <ProjectReference Include="../../src/Test2/Test2.fsproj">
      <Name>Test2</Name>
      <Project>{ceb3e6b3-c06f-4a24-82e6-9e70ba4adfe8}</Project>
      <Private>True</Private>
    </ProjectReference>
  </ItemGroup>
</Project>
""" 
    let expected = XElement.Parse expected
    let guid = Guid.Parse "{ceb3e6b3-c06f-4a24-82e6-9e70ba4adfe8}"
    let sut = XElement.Parse singleIGandTestRef
    let (Success result) = Project.addProjRefNode "../../src/Test2/Test2.fsproj" "Test2" guid sut
    XNode.DeepEquals(expected, result)
    |> Assert.IsTrue

[<Test>] 
let ``addProjRefNode should be idempotent`` () =
    let expected = XElement.Parse singleIGandTestRef 
    let sut = XElement.Parse singleIGandTestRef 
    let guid = Guid.Parse "{165a6853-05ed-4f03-a7b1-1c84d4f01bf5}"
    let (Success result) = Project.addProjRefNode "../../src/Test/Test.fsproj" "Test" guid sut
    XNode.DeepEquals(expected, result)
    |> Assert.IsTrue

[<Test>]
let ``addFile should add the file if not present`` () =
    let projFile = __SOURCE_DIRECTORY__ + "/data/AddFile-input.fsproj"
    let srcFile = __SOURCE_DIRECTORY__ + "/data/Tests.fs"
    match Project.addFile projFile srcFile None with
    | Failure s -> Assert.Fail s
    | Success result ->
        let expected = XElement.Load (__SOURCE_DIRECTORY__ + "/data/AddFile-expected.fsproj")
        XNode.DeepEquals(expected, result)
        |> Assert.IsTrue

[<Test>]
let ``addFile should create the file if it doesn't exist`` () =
    let projFile = __SOURCE_DIRECTORY__ + "/data/AddFile-input.fsproj"
    let srcFile = __SOURCE_DIRECTORY__ + "/data/Newfile.fs"
    match Project.addFile projFile srcFile None with
    | Failure s -> Assert.Fail s
    | Success result ->
        Assert.That(File.Exists srcFile, "File should exist: " + srcFile)
        File.Delete srcFile
    
[<Test>]
let ``addFile should create an item group if not present`` () =
    let projFile = __SOURCE_DIRECTORY__ + "/data/AddFile-NoItemGroup-input.fsproj"
    let srcFile = __SOURCE_DIRECTORY__ + "/data/Tests.fs"
    match Project.addFile projFile srcFile None with
    | Failure s -> Assert.Fail s
    | Success result ->
        let expected = XElement.Load (__SOURCE_DIRECTORY__ + "/data/AddFile-NoItemGroup-expected.fsproj")
        XNode.DeepEquals(expected, result)
        |> Assert.IsTrue

[<Test>]
let ``addFile should fail if file in project`` () =
    let projFile = __SOURCE_DIRECTORY__ + "/data/AddFile-AlreadyPresent-input.fsproj"
    let srcFile = __SOURCE_DIRECTORY__ + "/data/Tests.fs"
    match Project.addFile projFile srcFile None with
    | Failure s -> s |> should endWith "already exists in project."
    | Success result -> Assert.Fail()

[<Test>]
let ``addFile should fail if trying to copy and already on disk`` () =
    let projFile = __SOURCE_DIRECTORY__ + "/data/AddFile-input.fsproj"
    let srcFile = __SOURCE_DIRECTORY__ + "/Tests.fs"
    match Project.addFile projFile srcFile None with
    | Failure s -> s |> should endWith "already present."
    | Success result -> Assert.Fail()

[<Test>]
let ``addFile should fail if trying to copy and doesn't exist`` () =
    let projFile = __SOURCE_DIRECTORY__ + "/data/AddFile-input.fsproj"
    let srcFile = __SOURCE_DIRECTORY__ + "/FileThatWillHopefullyNeverExist"
    match Project.addFile projFile srcFile None with
    | Failure s -> s |> should endWith "not found."
    | Success result -> Assert.Fail()

[<Test>]
let ``addFile should add next to a None Include if no Compile Include`` () =
    let projFile = __SOURCE_DIRECTORY__ + "/data/AddFile-OnlyNone-input.fsproj"
    let srcFile = __SOURCE_DIRECTORY__ + "/data/Tests.fs"
    match Project.addFile projFile srcFile None with
    | Failure s -> Assert.Fail s
    | Success result ->
        let expected = XElement.Load (__SOURCE_DIRECTORY__ + "/data/AddFile-OnlyNone-expected.fsproj")
        XNode.DeepEquals(expected, result)
        |> Assert.IsTrue

[<Test>]
let ``addFile should insert the link`` () =
    let projFile = __SOURCE_DIRECTORY__ + "/data/AddFile-Link-input.fsproj"
    let srcFile = __SOURCE_DIRECTORY__ + "/Tests.fs"
    match Project.addFile projFile srcFile (Some "foldername/Tests.fs") with
    | Failure s -> Assert.Fail s
    | Success result ->
        let expected = XElement.Load (__SOURCE_DIRECTORY__ + "/data/AddFile-Link-expected.fsproj")
        XNode.DeepEquals(expected, result)
        |> Assert.IsTrue
    
[<Test>]
let ``addFile followed by delFile should be identity`` () =
    let projFile = __SOURCE_DIRECTORY__ + "/data/AddFile-input.fsproj"
    let srcFile = __SOURCE_DIRECTORY__ + "/data/Tests.fs"
    match Project.addFile projFile srcFile None with
    | Failure s -> Assert.Fail s
    | Success p ->
        let tmpFile = __SOURCE_DIRECTORY__ + "/data/" + Path.GetRandomFileName()
        try
            p.Save(tmpFile)
            match Project.delFile tmpFile srcFile with
            | Failure s -> Assert.Fail s
            | Success result ->
                let expected = XElement.Load (__SOURCE_DIRECTORY__ + "/data/AddFile-input.fsproj")
                XNode.DeepEquals(expected, result)
                |> Assert.IsTrue
        finally
            File.Delete tmpFile

let delInput = """<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="4.0" DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <PropertyGroup>
    <ProjectGuid>165a6853-05ed-4f03-a7b1-1c84d4f01bf5</ProjectGuid>
    <AssemblyName>Test</AssemblyName>
    <Name>Test</Name>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include="Tests.fs" />
  </ItemGroup>
</Project>
"""

let delExpected = """<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="4.0" DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <PropertyGroup>
    <ProjectGuid>165a6853-05ed-4f03-a7b1-1c84d4f01bf5</ProjectGuid>
    <AssemblyName>Test</AssemblyName>
    <Name>Test</Name>
  </PropertyGroup>
  <ItemGroup>
  </ItemGroup>
</Project>
"""

[<Test>]
let ``delFile should remove file if present`` () =
    let proj = XElement.Parse delInput
    match Project.removeFileIfPresent proj "Tests.fs" with
    | Failure s -> Assert.Fail s
    | Success result ->
        let expected = XElement.Parse delExpected
        XNode.DeepEquals(expected, result)
        |> Assert.IsTrue
    
