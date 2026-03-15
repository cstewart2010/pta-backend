namespace PokemonTabletopAdventures.CoreApi.IntegrationTests;

public class HttpClientHelper : IDisposable
{
    private readonly HttpClient _httpClient = new HttpClient
    {
        BaseAddress = new Uri(Constants.ApiRootUrl)
    };

    public async Task<Response> SendRequestAsync(HttpRequestMessageBuilder builder)
    {
        var request = builder.Build();
        var (response, content) =  await SendAsync(request);
        return new Response(response, content);
    }

    public async Task<Response<T>> SendRequestAsync<T>(HttpRequestMessageBuilder builder)
    {
        var request = builder.Build();
        var (response, content) =  await SendAsync(request);
        return new Response<T>(response, content);
    }

    private async Task<(HttpResponseMessage httpResponse, string content)> SendAsync(HttpRequestMessage request)
    {
        await TestContext.Out.WriteLineAsync($"Sending request to {request.RequestUri}");
        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        return (response, content);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}