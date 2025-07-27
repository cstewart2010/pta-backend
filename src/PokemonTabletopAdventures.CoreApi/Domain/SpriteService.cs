using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models.Games;

namespace PokemonTabletopAdventures.CoreApi.Domain;

public class SpriteService(IRepositoryService repositoryService) : AbstractMongoService<SpriteDto>(repositoryService, MongoCollection.Sprites), ISpriteService
{
    public async Task<ICollection<Sprite>> GetAllSprites()
    {
        var dtos = await Collection.GetManyAsync(sprite => true);
        return [..dtos.Select(dto => new Sprite
        {
            FriendlyText = dto.FriendlyText,
            Value = dto.Value,
        })];
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
