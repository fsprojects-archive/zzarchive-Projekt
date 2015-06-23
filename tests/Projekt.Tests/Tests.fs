module Projekt.Tests

open System
open System.Xml.Linq
open System.IO
open NUnit.Framework
open Projekt.Util

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

