namespace PokemonTabletopAdventures.CoreApi.IntegrationTests;

internal class Constants
{
    #if !DEBUG
    public const string ApiRootUrl = "https://localhost:5001";
    #else
    public const string ApiRootUrl = "https://pta-2-htemh0ddc0eghka8.eastus-01.azurewebsites.net";
    #endif
}
