open System

open System.IO
open FreeBoxAPI

type SessionToken = string
type Key = string

type UnknownState = UnknownCon

type KnownState = {
    Key : Key
}

type ConnectedState = {
    Token : SessionToken
}

type FreeDevice =
    | Unknown of UnknownState
    | Known of KnownState
    | Connected of ConnectedState

let getKey =
    if File.Exists("./key.txt") then
        Some (File.OpenText("./key.txt").ReadToEnd())
    else
        None

let initDevice =
    let key = getKey
    match key with
    | Some k -> FreeDevice.Known { Key = k }
    | None   -> FreeDevice.Unknown UnknownCon

type UnknownState with
    member this.Auth =
        let rep = FreeBoxAPI.sendAuthorizeRequest "" "" "" ""
        if rep.success then
            printfn "Authorisation en cours... Validez physiquement sur la freebox"
            printfn "Token reçu: %s" rep.result.app_token
            printfn "Appuyer sur entrée après avoir validé..."
            Console.ReadLine() |> ignore
            Some rep.result.app_token
        else
            None


let startDevice =
    let d = initDevice
    match d with
    | Unknown state -> state.Auth
    | Known state   -> None
    | _ -> None

let openSession (state : KnownState) =
    let k = state.Key
    FreeDevice.Connected { Token = "test" }


type KnownState with
    member this.OpenSession = openSession this

// type NewState with
//     member this.Register =

// type ConnectedState with
//     member this.GetFreeboxStats = "freebox stats"

// let rec getFreeboxStats device =
//     match device with
//     | UnknownDevice state -> getFreeboxStats state.Start
//     | NewDevice state     ->


// type FreeDevice with
//     member this.GetStats =

// let callFree =
//     let req = WebRequest.Create(Uri("http://mafreebox.freebox.fr/api_version"))
//     use rep = req.GetResponse()
//     use repStream = rep.GetResponseStream()
//     use reader = new IO.StreamReader(repStream)
//     reader.ReadToEnd()

[<EntryPoint>]
let main argv =
    let device = startDevice
    // match device with
    // | Some d -> printfn "une clef %s" d
    // | None   -> printfn "pas de clef"


    //let x = device.Register
    // match device.Register with
    // | Some k -> printfn "une clef %s" k
    // | None   -> printfn "pas de clef"
    let rep = sendAuthorizeRequest "appid" "appname" "appversion" "appowner"

    printfn "%O" rep
    0 // return an integer exit code
