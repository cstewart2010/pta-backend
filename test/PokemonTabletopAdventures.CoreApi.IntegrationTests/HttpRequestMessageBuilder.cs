using System.Text;
using System.Text.Json;

namespace PokemonTabletopAdventures.CoreApi.IntegrationTests;

public class HttpRequestMessageBuilder(string endpoint)
{
    public Dictionary<string, string> Headers { get; } = new();
    
    public string? JsonPayload { get; set; }
    
    public HttpMethod HttpMethod { get; set; } = HttpMethod.Get;

    public string Endpoint { get; set; } = endpoint;
    
    private const string ContentType = "application/json";

    public HttpRequestMessage Build()
    {
        var httpRequestMessage = new HttpRequestMessage(HttpMethod, Endpoint);
        TestContext.Out.WriteLine($"Building request for {httpRequestMessage.Method} {httpRequestMessage.RequestUri}");
        if (JsonPayload != null)
        {
            TestContext.Out.WriteLine($"Payload: {JsonPayload}");
            httpRequestMessage.Content = new StringContent(JsonPayload, Encoding.UTF8, ContentType);
        }

        foreach (var header in Headers)
        {
            TestContext.Out.WriteLine($"{header.Key}: {header.Value}");
            httpRequestMessage.Headers.Add(header.Key, header.Value);
        }
        TestContext.Out.WriteLine("Request built");
        return httpRequestMessage;
    }
}
