using System.Net;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PokemonTabletopAdventures.CoreApi.Constants;

namespace PokemonTabletopAdventures.CoreApi.IntegrationTests;

public abstract class BaseResponse
{
    protected static readonly StringEnumConverter Options = new StringEnumConverter();

    protected BaseResponse(HttpResponseMessage response, string content)
    {
        TestContext.WriteLine("Parsing response");
        TestContext.WriteLine($"Status Code: {response.StatusCode}");
        TestContext.WriteLine($"Content: {content}");
        Content = content;
        IsSuccessful = response.IsSuccessStatusCode;
        StatusCode = response.StatusCode;
        if (!response.IsSuccessStatusCode)
        {
            ProblemDetails = JsonConvert.DeserializeObject<ProblemDetails>(content, Options);
        }
        if (response.Headers.TryGetValues(HeaderNames.SessionAuth, out var auths))
        {
            SessionAuth = auths.FirstOrDefault();
        }
    }

    public string? Content { get; }
    public string? SessionAuth { get; }
    public bool IsSuccessful { get; }
    public ProblemDetails? ProblemDetails { get; }
    public HttpStatusCode StatusCode { get; }
}

public class Response(HttpResponseMessage response, string content) : BaseResponse(response, content) { }

public class Response<T> : BaseResponse
{
    public Response(HttpResponseMessage response, string content)
        : base(response, content)
    {
        if (response.IsSuccessStatusCode)
        {
            Data = JsonConvert.DeserializeObject<T>(content, Options);
        }
    }

    public T? Data { get; }
}
