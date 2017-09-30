open System
open System.IO
open FreeBoxAPI

type SessionToken = string
type Key = string

type UnknownState = UnknownCon

type KnownState = {
    App_token : Key
}

type ConnectedState = {
    Token : SessionToken
}

type FreeDevice =
    | Unknown of UnknownState
    | Known of KnownState
    | Connected of ConnectedState

let getKey : string option =
    if File.Exists("./key.txt") then
        Some (File.OpenText("./key.txt").ReadToEnd())
    else
        None

let setKey (k : string) : unit =
    if File.Exists("./key.txt") then
        File.Delete("./key.txt")
    use f = new StreamWriter("./key.txt")
    f.Write(k)

let initDevice =
    let key = getKey
    match key with
    | Some k -> FreeDevice.Known { App_token = k }
    | None   -> FreeDevice.Unknown UnknownCon

type UnknownState with
    member this.Auth =
        let rep = FreeBoxAPI.sendAuthorizeRequest "" "" "" ""
        if rep.success then
            printfn "Authorisation en cours... Validez physiquement sur la freebox"
            printfn "Token reçu: %s" rep.result.app_token
            printfn "Appuyer sur entrée après avoir validé..."
            Console.ReadLine() |> ignore
            printfn "Vérification authorization"
            let r = FreeBoxAPI.sendCheckStatus rep.result.track_id
            if r.success then
                printfn "Authorization status : %s" r.result.status
                setKey rep.result.app_token
                Some (FreeDevice.Known { App_token = rep.result.app_token })
            else
                None
        else
            None


let startDevice =
    let d = initDevice
    match d with
    | Unknown state -> state.Auth
    | Known state   -> Some d
    | _ -> None

let openSession (state : KnownState) =
    let app_token = state.App_token
    let rep = FreeBoxAPI.sendChallengeRequest
    if rep.success then
        printfn "Challenge reçu : %s" rep.result.challenge
        printfn "Ouverture session..."
        let session = FreeBoxAPI.sendOpenSessionRequest app_token rep.result.password_salt rep.result.challenge
        if session.success then
            printfn "Session ouverte, token : %s" session.result.session_token
            Some (FreeDevice.Connected { Token = session.result.session_token })
        else
            None
    else
        None


type KnownState with
    member this.OpenSession = openSession this

let connectionStatus (c : ConnectedState) =
    let rep = FreeBoxAPI.sendInternetStatusRequest c.Token
    if rep.success then
        Some (String.Format("d: {0:###0}ko - u: {1:###0}ko" , rep.result.rate_down / 1000.0, rep.result.rate_up / 1000.0))
    else
        None

[<EntryPoint>]
let main argv =
    let device = startDevice
    match device with
    | Some (Known s) ->
        printfn "une clef %s" s.App_token
        match s.OpenSession with
        | Some (Connected s) ->
            printfn "Connecté youhou !"
            match connectionStatus s with
            | Some d -> printfn "débits : %s" d
            | _ -> printfn "aie"
        | _ -> printfn "impossible de se connecter"
    | _   -> printfn "Impossible de se connecter à la freebox"


    //let x = device.Register
    // match device.Register with
    // | Some k -> printfn "une clef %s" k
    // | None   -> printfn "pas de clef"
    //let rep = sendAuthorizeRequest "appid" "appname" "appversion" "appowner"

    //printfn "%O" rep
    0 // return an integer exit code
