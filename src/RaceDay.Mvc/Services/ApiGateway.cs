using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
namespace RaceDay.Mvc.Services;
public sealed record ApiReply<T>(bool Ok,T? Value,string Error,int StatusCode);
public sealed class ApiGateway(IHttpClientFactory factory,IHttpContextAccessor accessor) {
    private static readonly JsonSerializerOptions JsonOptions=new(JsonSerializerDefaults.Web);
    private HttpClient Client()=>factory.CreateClient("RaceDayApi");
    private HttpRequestMessage Request(HttpMethod method,string route,HttpContent? body=null) {
        var req=new HttpRequestMessage(method,route){Content=body};
        var cookie=accessor.HttpContext?.Session.GetString("ApiCookie");
        if(!string.IsNullOrEmpty(cookie))req.Headers.TryAddWithoutValidation("Cookie",cookie);
        return req;
    }
    public async Task<HttpResponseMessage> RawAsync(HttpMethod method,string route,HttpContent? content=null) {
        using var req=Request(method,route,content);
        return await Client().SendAsync(req,HttpCompletionOption.ResponseHeadersRead);
    }
    private static async Task<ApiReply<T>> Parse<T>(HttpResponseMessage r) {
        using(r) {
            if(r.IsSuccessStatusCode) {
                if(r.StatusCode==System.Net.HttpStatusCode.NoContent) return new(true,default,"",(int)r.StatusCode);
                var obj=await r.Content.ReadFromJsonAsync<T>(JsonOptions);return new(true,obj,"",(int)r.StatusCode);
            }
            var error="Request could not be completed. Please check your details.";
            try {
                using var doc=JsonDocument.Parse(await r.Content.ReadAsStringAsync());
                if(doc.RootElement.TryGetProperty("message",out var m))error=m.GetString()??error;
                else if(doc.RootElement.TryGetProperty("errors",out var errs)&&errs.ValueKind==JsonValueKind.Object) {
                    var first=errs.EnumerateObject().FirstOrDefault();if(first.Value.ValueKind==JsonValueKind.Array)error=first.Value.EnumerateArray().FirstOrDefault().GetString()??error;
                }
            } catch(JsonException) { }
            return new(false,default,error,(int)r.StatusCode);
        }
    }
    public async Task<ApiReply<T>> GetAsync<T>(string route) {try {return await Parse<T>(await RawAsync(HttpMethod.Get,route));}catch(HttpRequestException){return new(false,default,"Cannot reach RaceDay API. Start the API service.",503);} }
    public async Task<ApiReply<T>> SendAsync<T>(HttpMethod method,string route,object? data=null) {
        try {return await Parse<T>(await RawAsync(method,route,data is null?null:JsonContent.Create(data)));}
        catch(HttpRequestException){return new(false,default,"Cannot reach RaceDay API. Start the API service.",503);}
    }
    public async Task<(ApiReply<T> Reply,string? Cookie)> LoginAsync<T>(object data) {
        try {
            using var response=await RawAsync(HttpMethod.Post,"api/auth/login",JsonContent.Create(data));
            string? cookie=null;
            if(response.Headers.TryGetValues("Set-Cookie",out var values)) cookie=values.Select(v=>v.Split(';')[0]).FirstOrDefault(v=>v.StartsWith("RaceDay.Api.Session=",StringComparison.Ordinal));
            // Copy response for Parse (Parse disposes it). Retain cookie value separately.
            var reply=await Parse<T>(response);
            return (reply,cookie);
        } catch(HttpRequestException) {return(new(false,default,"Cannot reach RaceDay API.",503),null);}
    }
    public async Task<ApiReply<object>> UploadAsync(string route,IFormFile file) {
        try {
            using var form=new MultipartFormDataContent();
            using var stream=file.OpenReadStream();
            using var data=new StreamContent(stream);data.Headers.ContentType=new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
            form.Add(data,"file",Path.GetFileName(file.FileName));
            return await Parse<object>(await RawAsync(HttpMethod.Post,route,form));
        } catch(HttpRequestException){return new(false,null,"Cannot reach RaceDay API.",503);}
    }
}
