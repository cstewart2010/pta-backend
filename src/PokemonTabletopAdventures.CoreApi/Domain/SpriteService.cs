using MongoDB.Driver;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models.Games;

namespace PokemonTabletopAdventures.CoreApi.Domain;

internal class SpriteService : AbstractMongoService<SpriteDto>, ISpriteService
{
    public SpriteService() : base(MongoCollection.Sprites) { }

    public async Task<IEnumerable<Sprite>> GetAllSprites()
    {
        var dtos = await Task.FromResult(Collection.Find(sprite => true).ToEnumerable());
        return dtos.Select(dto => new Sprite
        {
            FriendlyText = dto.FriendlyText,
            Value = dto.Value,
        });
    }

    public async Task PostSprite(Sprite sprite)
    {
        var dto = new SpriteDto
        {
            FriendlyText = sprite.FriendlyText,
            Value = sprite.Value
        };
        await PostDocument(dto);
    }
}
