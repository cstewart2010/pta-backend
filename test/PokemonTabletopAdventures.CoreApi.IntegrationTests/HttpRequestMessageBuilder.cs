using System.Text;
using System.Text.Json;

namespace PokemonTabletopAdventures.CoreApi.IntegrationTests;

public class HttpRequestMessageBuilder(HttpMethod httpMethod, string endpoint)
{
    private readonly HttpRequestMessage _httpRequestMessage = new(httpMethod, endpoint);
    private readonly Dictionary<string, string> _headers = new();
    private string? _jsonPayload;
    private const string ContentType = "application/json";

    public HttpRequestMessageBuilder WithPayload<T>(T payload)
    {
        _jsonPayload = JsonSerializer.Serialize(payload);
        return this;
    }

    public HttpRequestMessageBuilder WithHeader(string name, string value)
    {
        _headers.Add(name, value);
        return this;
    }

    public HttpRequestMessage Build()
    {
        TestContext.WriteLine($"Building request for {_httpRequestMessage.Method} {_httpRequestMessage.RequestUri}");
        if (_jsonPayload != null)
        {
            TestContext.WriteLine($"Payload: {_jsonPayload}");
            _httpRequestMessage.Content = new StringContent(_jsonPayload, Encoding.UTF8, ContentType);
        }

        foreach (var header in _headers)
        {
            TestContext.WriteLine($"{header.Key}: {header.Value}");
            _httpRequestMessage.Headers.Add(header.Key, header.Value);
        }
        TestContext.WriteLine("Request built");
        return _httpRequestMessage;
    }
}
