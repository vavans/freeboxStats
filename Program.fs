// Learn more about F# at http://fsharp.org

open System
open System.Net
open System.IO

type SessionToken = string

type SessionState =
    | Closed
    | Opened of SessionToken

let key = ""

let openSession =


let callFree =
    let req = WebRequest.Create(Uri("http://mafreebox.freebox.fr/api_version"))
    use rep = req.GetResponse()
    use repStream = rep.GetResponseStream()
    use reader = new IO.StreamReader(repStream)
    reader.ReadToEnd()

[<EntryPoint>]
let main argv =
    printfn "%s" callFree
    0 // return an integer exit code
