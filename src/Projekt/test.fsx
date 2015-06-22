
#r "System.Xml.dll"
#r "System.Xml.Linq.dll"
open System
open System.Xml
open System.Xml.Linq

#load "Types.fs"
open Projekt
#load "Util.fs"
open Projekt
#load "Project.fs"
open Projekt

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
let proj = XElement.Parse delInput
let e = 
  match proj with
  | (Project.Descendant "Compile" ((Project.Attribute "Include" a) as e)) -> e
  | _ -> failwith ""

e.Name
e.Remove()
proj
let x = Project.removeFileIfPresent proj "Tests.fs"

