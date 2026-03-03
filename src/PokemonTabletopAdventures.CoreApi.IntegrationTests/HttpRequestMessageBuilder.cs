using System.Text;
using System.Text.Json;

namespace PokemonTabletopAdventures.CoreApi.IntegrationTests;

public class HttpRequestMessageBuilder
{
    private readonly HttpRequestMessage _httpRequestMessage;

    public HttpRequestMessageBuilder(HttpMethod httpMethod, string endpoint)
    {
        _httpRequestMessage = new HttpRequestMessage(httpMethod, endpoint);
    }

    public HttpRequestMessageBuilder WithPayload<T>(string contentType, T payload)
    {
        var json = JsonSerializer.Serialize(payload);
        _httpRequestMessage.Content = new StringContent(json, Encoding.UTF8, contentType);
        return this;
    }

    public HttpRequestMessageBuilder WithHeader(string name, string value)
    {
        _httpRequestMessage.Headers.Add(name, value);
        return this;
    }

    public HttpRequestMessage Build()
    {
        return _httpRequestMessage;
    }
}
