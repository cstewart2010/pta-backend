using PokemonTabletopAdventures.Models.Games;

namespace PokemonTabletopAdventures.CoreApi.Services;

public interface ISpriteService
{
    /// <summary>
    /// Returns all sprites
    /// </summary>
    public Task<ICollection<Sprite>> GetAllSprites();

    /// <summary>
    /// Attempts to add a sprite using the provided document
    /// </summary>
    /// <param name="sprite">The document to add</param>
    public Task PostSprite(Sprite sprite);
}
