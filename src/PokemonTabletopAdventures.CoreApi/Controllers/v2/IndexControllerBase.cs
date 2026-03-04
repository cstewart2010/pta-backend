using Microsoft.AspNetCore.Mvc;
using PokemonTabletopAdventures.CoreApi.DTOs;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models.Interfaces;
using PokemonTabletopAdventures.Models.Enums;

namespace PokemonTabletopAdventures.CoreApi.Controllers.v2;

public abstract class IndexControllerBase(IDexService dexService) : ControllerBase
{
    protected IDexService DexService { get; } = dexService;

    protected async Task<OkObjectResult> GetItems<TDocument>(
        DexType documentType,
        int offset,
        int limit) where TDocument : IDocument, IDexDocument
    {
        var response = await DexService.GetIndexCollectionResponse<TDocument>(documentType, offset, limit);
        return Ok(response);
    }

    protected async Task<OkObjectResult> GetItem<TDocument>(
        DexType documentType,
        string name) where TDocument : IDocument, IDexDocument
    {
        var response = await DexService.GetDexEntry<TDocument>(documentType, name);
        return Ok(response);
    }
}
