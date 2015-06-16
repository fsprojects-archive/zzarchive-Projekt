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
    | DelFile of FileData
    | MoveFile of FileData * Direction * Repeat
    | Error

type Result<'a> =
    | Success of 'a
    | Failure of string

type ResultBuilder() =
    member __.Return (x) =
        Success x
    member __.ReturnFrom (x : Result<'T>) = x
    member __.Bind(m, f) =
        match m with
        | Success m -> f m
        | Failure s -> Failure s

let result = ResultBuilder()

let atest =
    async {
        let! _ = async { return 1 }
        let! s = async { return "" }
        return s }

let test =
    result {
        let! _ = Success 1
        let!  s = Success "" 
        return s }

