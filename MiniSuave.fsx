// Show your Awesomeness!

type HttpMethod = 
| Get 
| Put 
| Delete 
| Post

type Header = string * string

type Request = {
  Path : string
  Headers : Header list
  HttpMethod : HttpMethod
}

type StatusCode =
| Ok
| BadRequest
| NotFound

type Response = {
  Content : string
  Headers : Header list
  StatusCode : StatusCode
}

type Context = {
  Request : Request
  Response : Response
}

type WebPart = Context -> Async<Context option>

let response statusCode content ctx =
  let response = 
    {ctx.Response with 
      StatusCode = statusCode
      Content = content}  
  {ctx with Response = response} |> Some |> async.Return

let OK = response Ok
let BAD_REQUEST = response BadRequest
let NOT_FOUND = response NotFound

let httpMethodFilter httpMethod ctx =
  match ctx.Request.HttpMethod = httpMethod with
  | true -> Some ctx |> async.Return
  | _ -> None |> async.Return

let GET = httpMethodFilter Get 

let POST = httpMethodFilter Post

let PUT = httpMethodFilter Put

let DELETE = httpMethodFilter Delete

let Path path ctx = 
  match ctx.Request.Path = path with
  | true -> ctx |> Some |> async.Return
  | _ -> None |> async.Return

let compose w1 w2 ctx = async {
  let! result = w1 ctx
  match result with
  | Some c -> 
    let! res = w2 c
    return res
  | None -> return None 
}

let (>=>) = compose