using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PokemonTabletopAdventures.CoreApi.Constants;
using System.Text.Json;

namespace PokemonTabletopAdventures.CoreApi.IntegrationTests;


public class Response
{
    private static readonly StringEnumConverter Options = new StringEnumConverter();

    public Response(HttpResponseMessage response, string content)
    {
        Content = content;
        IsSuccessful = response.IsSuccessStatusCode;        
        if (!response.IsSuccessStatusCode)
        {
            ProblemDetails = JsonConvert.DeserializeObject<ProblemDetails>(content, Options);
        }
        if (response.Headers.TryGetValues(HeaderNames.AccessToken, out var tokens))
        {
            ActivityToken = tokens.FirstOrDefault();
        }
        if (response.Headers.TryGetValues(HeaderNames.SessionAuth, out var auths))
        {
            SessionAuth = auths.FirstOrDefault();
        }
    }

    public string? Content { get; }
    public string? ActivityToken { get; }
    public string? SessionAuth { get; }
    public bool IsSuccessful { get; }
    public ProblemDetails? ProblemDetails { get; }
}
public class Response<T>
{
    private static readonly StringEnumConverter Options = new StringEnumConverter();

    public Response(HttpResponseMessage response, string content)
    {
        Content = content;
        IsSuccessful = response.IsSuccessStatusCode;
        if (response.IsSuccessStatusCode)
        {
            Data = JsonConvert.DeserializeObject<T>(content, Options);
        }
        else
        {
            ProblemDetails = JsonConvert.DeserializeObject<ProblemDetails>(content, Options);
        }
        if (response.Headers.TryGetValues(HeaderNames.AccessToken, out var tokens))
        {
            ActivityToken = tokens.FirstOrDefault();
        }
        if (response.Headers.TryGetValues(HeaderNames.SessionAuth, out var auths))
        {
            SessionAuth = auths.FirstOrDefault();
        }
    }

    public string? Content { get; }
    public T? Data { get; }
    public string? ActivityToken { get; }
    public string? SessionAuth { get; }
    public bool IsSuccessful { get; }
    public ProblemDetails? ProblemDetails { get; set; }
}
