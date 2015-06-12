[<AutoOpen>]
module Projekt.Types

type Template =
    | Console
    | Library

type Direction =
    | Up
    | Down

type Repeat = | Repeat of int

type FrameworkVersion =
    | V4_0
    | V4_5
    | V4_5_1
with 
    override x.ToString () =
        match x with
        | V4_0 -> "4.0"
        | V4_5 -> "4.5"
        | V4_5_1 -> "4.5.1"

type ProjectInitData =
    { ProjPath: string
      Template: Template
      FrameworkVersion: FrameworkVersion
      Solution: string option }
with 
    static member create (path, ?template, ?fxversion, ?solution) =
        { ProjPath = path
          Template = defaultArg template Library
          FrameworkVersion = defaultArg fxversion V4_5
          Solution = solution }

type ProjectReferenceData =
    { ProjPath: string
      Reference: string }

type FileData =
    { ProjPath: string
      FilePath: string }

type Operation =
    | Init of ProjectInitData //project path 
    | Reference of ProjectReferenceData
    | NewFile of FileData
    | AddFile of FileData
    | MoveFile of FileData * Direction * Repeat
    | Error
