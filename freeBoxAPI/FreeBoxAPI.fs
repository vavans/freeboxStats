module FreeBoxAPI

open System
open System.Net
open System.IO
open System.Text
open Newtonsoft.Json

type AuthResult = {
    app_token : string
    track_id : int
}

type AuthResponse = {
    success : bool
    result : AuthResult
}

type ChallengeResult = {
    logged_in : bool
    challenge : string
    password_salt : string
}

type ChallengeResponse = {
    success : bool
    result : ChallengeResult
}

type OpenSessionResult= {
    session_token : string
    challenge : string
}

type OpenSessionResponse = {
    success : bool
    result : OpenSessionResult
}

type CheckStatusResult = {
    status : string
}

type CheckStatusResponse = {
    success : bool
    result : CheckStatusResult
}

type InternetStatusResult = {
    rate_down : float
    rate_up : float
}

type InternetStatusResponse = {
    success : bool
    result : InternetStatusResult
}

let sendOpenSessionRequest (app_token : string) (password_salt : string) (challenge : string) : OpenSessionResponse =
    printfn "app_token : <%s>  salt : <%s>   challenge : <%s>" app_token password_salt challenge
    let hmac = new System.Security.Cryptography.HMACSHA1(Encoding.UTF8.GetBytes(app_token))
    let pass = hmac.ComputeHash(Encoding.UTF8.GetBytes(challenge))
                |> Seq.map (fun b -> b.ToString("x2"))
                |> String.Concat
    let client = new Http.HttpClient()
    let contentPosted = (sprintf """{
   "app_id": "fr.freebox.testapp",
   "app_version": "0.0.7",
   "password" : "%s"
}""" pass)
    let content = new Http.StringContent(contentPosted)
    printfn "post data : %s" contentPosted
    let rep = client.PostAsync(Uri("http://mafreebox.freebox.fr/api/v4/login/session"), content)
    let json = rep.Result.Content.ReadAsStringAsync().Result
    JsonConvert.DeserializeObject<OpenSessionResponse>(json);

let sendChallengeRequest : ChallengeResponse =
    let client = new Http.HttpClient()
    let rep = client.GetAsync(Uri("http://mafreebox.freebox.fr/api/v4/login"))
    let json = rep.Result.Content.ReadAsStringAsync().Result
    JsonConvert.DeserializeObject<ChallengeResponse>(json);

let sendCheckStatus track_id : CheckStatusResponse =
    let client = new Http.HttpClient()
    let rep = client.GetAsync(Uri(sprintf "http://mafreebox.freebox.fr/api/v4/login/authorize/%d" track_id))
    let json = rep.Result.Content.ReadAsStringAsync().Result
    JsonConvert.DeserializeObject<CheckStatusResponse>(json);

let sendAuthorizeRequest (app_id : string) (app_name : string) (app_version : string) (device_name : string) : AuthResponse =
    let client = new Http.HttpClient()
    let content = new Http.StringContent("""{
   "app_id": "fr.freebox.testapp",
   "app_name": "Test App",
   "app_version": "0.0.7",
   "device_name": "Pc de Xavier"
}""")
    let rep = client.PostAsync(Uri("http://mafreebox.freebox.fr/api/v4/login/authorize"), content)
    let json = rep.Result.Content.ReadAsStringAsync().Result
    JsonConvert.DeserializeObject<AuthResponse>(json);


let sendInternetStatusRequest (session_token : string) : InternetStatusResponse =
    let client = new Http.HttpClient()
    client.DefaultRequestHeaders.Add("X-Fbx-App-Auth", session_token)
    let rep = client.GetAsync(Uri("http://mafreebox.freebox.fr/api/v4/connection"))
    let json = rep.Result.Content.ReadAsStringAsync().Result
    JsonConvert.DeserializeObject<InternetStatusResponse>(json);