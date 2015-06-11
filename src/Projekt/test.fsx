
#load "Main.fs"
__SOURCE_DIRECTORY__

open Projekt
Args.split ["project"; "create"; "MyName"; "--solution=src/Mysolution"]
