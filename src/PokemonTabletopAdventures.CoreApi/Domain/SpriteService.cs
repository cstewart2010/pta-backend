using MongoDB.Driver;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PokemonTabletopAdventures.CoreApi.Domain;

internal class SpriteService : AbstractService<SpriteModel>, ISpriteService
{
    public SpriteService() : base(MongoCollection.Sprites) { }

    public async Task<IEnumerable<SpriteModel>> GetAllSprites()
    {
        return await Task.FromResult(Collection.Find(sprite => true).ToEnumerable());
    }

    public async Task PostSprite(SpriteModel sprite)
    {
        await PostDocument(sprite);
    }
}
