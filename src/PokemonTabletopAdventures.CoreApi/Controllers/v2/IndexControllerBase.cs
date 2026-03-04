using Microsoft.AspNetCore.Mvc;
using PokemonTabletopAdventures.CoreApi.DTOs;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models.Interfaces;
using PokemonTabletopAdventures.Models.Enums;

namespace PokemonTabletopAdventures.CoreApi.Controllers.v2;

public abstract class IndexControllerBase(IDexService dexService) : ControllerBase
{
    protected IDexService DexService { get; } = dexService;

    protected async Task<OkObjectResult> GetItems<TDocument, TController>(
        DexType documentType,
        ILogger<TController> logger,
        int offset,
        int limit)
        where TDocument : IDocument, IDexDocument
        where TController : IndexControllerBase
    {
        logger.LogInformation("Retrieving the first {limit} {type}. starting at {offset}", limit, documentType, offset);
        var response = await DexService.GetIndexCollectionResponse<TDocument>(documentType, offset, limit);
        return Ok(response);
    }

    protected async Task<OkObjectResult> GetItem<TDocument, TController>(
        DexType documentType,
        ILogger<TController> logger,
        string name) where TDocument : IDocument, IDexDocument
        where TController : IndexControllerBase
    {
        logger.LogInformation("Retrieving {type} {name}", typeof(TDocument).Name, name);
        var response = await DexService.GetDexEntry<TDocument>(documentType, name);
        return Ok(response);
    }
}
