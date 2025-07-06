using Microsoft.AspNetCore.Mvc;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models.Interfaces;
using PokemonTabletopAdventures.Models.Enums;
using System.Threading.Tasks;

namespace PokemonTabletopAdventures.CoreApi.Controllers.v2;

public abstract class IndexControllerBase(IDexService dexService) : ControllerBase
{
    public IDexService DexService { get; } = dexService;

    public async Task<OkObjectResult> GetItems<TDocument>(
        DexType documentType,
        int offset,
        int limit) where TDocument : IDexDocument
    {
        var response = await DexService.GetIndexCollectionResponse<TDocument>(documentType, offset, limit);
        return Ok(response);
    }

    public async Task<OkObjectResult> GetItem<TDocument>(
        DexType documentType,
        string name) where TDocument : IDexDocument
    {
        var response = await DexService.GetDexEntry<TDocument>(documentType, name);
        return Ok(response);
    }
}
