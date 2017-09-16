module FreeBoxAPI

open System
open System.Net
open Newtonsoft.Json

type AuthResult = {
    app_token : string
    track_id : int
}

type AuthResponse = {
    success : bool
    result : AuthResult
}

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
