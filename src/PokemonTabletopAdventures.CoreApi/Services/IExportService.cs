using PokemonTabletopAdventures.Models.Games;

namespace PokemonTabletopAdventures.CoreApi.Services;

public interface IExportService
{
    /// <summary>
    /// Returns a file stream for the a json file of the game session
    /// </summary>
    /// <param name="game">The game session to export</param>
    public Task<FileStream> GetExportStream(Game game);

    /// <summary>
    /// Return true if the game session was successfully imported
    /// </summary>
    /// <param name="json">The stringified json object to parse</param>
    /// <param name="errors">The errors found while attempting to import the game session</param>
    /// <returns></returns>
    public Task<Game> ParseImport(string json);
}
