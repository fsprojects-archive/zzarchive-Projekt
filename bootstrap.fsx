open System
Environment.Version
match Uri.TryCreate("http://test", UriKind.Absolute) with
| true, uri -> ()
| false, _ -> ()
