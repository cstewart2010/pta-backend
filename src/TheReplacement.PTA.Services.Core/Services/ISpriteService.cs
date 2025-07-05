using System.Collections.Generic;
using System.Threading.Tasks;
using PokemonTabletopAdventures.Models;

namespace PokemonTabletopAdventures.CoreApi.Services;

public interface ISpriteService
{
    /// <summary>
    /// Returns all sprites
    /// </summary>
    public Task<IEnumerable<SpriteModel>> GetAllSprites();

    /// <summary>
    /// Attempts to add a sprite using the provided document
    /// </summary>
    /// <param name="sprite">The document to add</param>
    public Task PostSprite(SpriteModel sprite);
}
