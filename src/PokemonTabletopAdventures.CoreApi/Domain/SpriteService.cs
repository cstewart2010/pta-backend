using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Exceptions;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models.Games;

namespace PokemonTabletopAdventures.CoreApi.Domain;

public class SpriteService(
    IRepositoryService repositoryService,
    ILogger<SpriteService> logger) : AbstractMongoService<SpriteDto>(repositoryService, MongoCollection.Sprites), ISpriteService
{
    public async Task<ICollection<Sprite>> GetAllSprites()
    {
        logger.LogInformation("Retrieving sprites");
        var dtos = await Collection.GetManyAsync(sprite => true, logger);
        logger.LogInformation("Successfully retrieved sprites");
        return [..dtos.Select(dto => new Sprite
        {
            FriendlyText = dto.FriendlyText,
            Value = dto.Value,
        })];
    }

    public async Task PostSprite(Sprite sprite)
    {
        logger.LogInformation("Add sprite {friendlyText} with filename {value}", sprite.FriendlyText, sprite.Value);
        if (string.IsNullOrEmpty(sprite.FriendlyText) || string.IsNullOrEmpty(sprite.Value))
        {
            throw new PtaException("Cannot add sprite with empty field", "Invalid Sprite", System.Net.HttpStatusCode.BadRequest);
        }
        var dto = new SpriteDto
        {
            FriendlyText = sprite.FriendlyText,
            Value = sprite.Value
        };
        await PostDocument(dto, logger);
        logger.LogInformation("{friendlyText} sprite added", sprite.FriendlyText);
    }
}
