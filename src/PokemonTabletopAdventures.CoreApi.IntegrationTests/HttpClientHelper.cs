namespace PokemonTabletopAdventures.CoreApi.IntegrationTests;

public class HttpClientHelper : IDisposable
{
    private readonly HttpClient _httpClient = new HttpClient
    {
        BaseAddress = new Uri(Constants.ApiRootUrl)
    };

    public async Task<Response> SendRequestAsync(HttpRequestMessage request)
    {
        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        return new Response(response, content);
    }

    public async Task<Response<T>> SendRequestAsync<T>(HttpRequestMessage request)
    {
        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        return new Response<T>(response, content);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}