using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.DTOs;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Exceptions;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models;
using PokemonTabletopAdventures.Models.Enums;
using PokemonTabletopAdventures.Models.Extensions;
using PokemonTabletopAdventures.Models.Indicies;
using PokemonTabletopAdventures.Models.Interfaces;
using PokemonTabletopAdventures.Models.Pokemons;

namespace PokemonTabletopAdventures.CoreApi.Domain;

public class DexService(
    IRepositoryService repositoryService,
    IDtoToModelMapper dtoToModelMapper,
    IModelToDtoMapper modelToDtoMapper,
    ILogger<DexService> logger) : AbstractMongoService<BasePokemonDto>(repositoryService, MongoCollection.BasePokemon), IDexService
{
    private readonly IRepositoryService _repositoryService = repositoryService;

    public async Task<ICollection<TDocument>> GetDexEntries<TDocument>(DexType documentType) where TDocument : IDocument, IDexDocument
    {
        logger.LogInformation("Getting Dex entries for document type {documentType}", documentType);
        var collection = _repositoryService.GetCollection<TDocument>(documentType.ToString());
        return await collection.GetManyAsync(document => true, logger);
    }

    public async Task<IndexResponse<TDocument>> GetDexEntry<TDocument>(DexType documentType, string name) where TDocument : IDocument, IDexDocument
    {
        var collection = _repositoryService.GetCollection<TDocument>(documentType.ToString());
        var item = await collection.GetOneAsync(document => document.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase), logger);
        return item == null ? throw new ItemNotFoundException(name) : new IndexResponse<TDocument> { Data = item };
    }

    public async Task<Pokemon> GetNewPokemon(string name, string? nickname, string form)
    {
        var random = new Random();
        var nature = (Nature)random.Next(1, 21);
        var gender = (Gender)random.Next(3);
        var status = Status.Normal;
        return await GetNewPokemon(name, nature, gender, status, nickname, form);
    }

    public async Task<Pokemon> GetNewPokemon(string name, Nature nature, Gender gender, Status status, string? nickname, string form)
    {
        var entry = await GetPokedexEntry(name, form);
        return GetPokemonFromBase(entry.Pokemon, nature, gender, status, nickname, entry.AlternateForms);
    }

    public async Task<PokemonAndForms> GetPokedexEntry(string name, string form)
    {
        var allForms = await Collection.GetManyAsync(document => document.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase), logger);
        if (allForms.Count == 0)
        {
            throw new ItemNotFoundException(name);
        }
        var model = allForms.FirstOrDefault(document => document.Form.Equals(form, StringComparison.CurrentCultureIgnoreCase)) ?? throw new ItemNotFoundException(form);
        var alternateForms = allForms.Where(document => !document.Form.Equals(form, StringComparison.CurrentCultureIgnoreCase))
            .Select(document => document.Form);
        return new PokemonAndForms
        {
            Pokemon = await dtoToModelMapper.ParseFromDto(model),
            AlternateForms = [.. alternateForms]
        };
    }

    public async Task<ICollection<PokemonForm>> GetPossibleEvolutions(Pokemon pokemon)
    {
        var allEvolutions = await Collection.GetManyAsync(document => document.EvolvesFrom.Equals(pokemon.SpeciesName, StringComparison.CurrentCultureIgnoreCase), logger);
        var forms = await Task.WhenAll(allEvolutions.Where(evolution => evolution.Form.Equals(pokemon.Form, StringComparison.CurrentCultureIgnoreCase)).Select(dtoToModelMapper.ParseFromDto));
        return forms;
    }

    public async Task<IndexCollectionResponse> GetIndexCollectionResponse<TDocument>(
        DexType documentType,
        int offset,
        int limit) where TDocument : IDocument, IDexDocument
    {
        var collection = _repositoryService.GetCollection<TDocument>(documentType.ToString());
        var documents = await collection.GetManyAsync(document => true, offset, limit, logger);
        var count = documents.Count;
        var results = documents.Select(x => x.Name).ToList();

        return new IndexCollectionResponse
        {
            Count = count,
            Results = results
        };
    }

    public async Task<IndexCollectionResponse> GetOrderedIndexCollectionResponse()
    {
        var documents = await Collection.GetManyAsync(document => true, logger);
        var results = documents.OrderBy(pokemon => pokemon.DexNo).GroupBy(pokemon => pokemon.Name).Select(x => x.Key).ToArray();

        return new IndexCollectionResponse
        {
            Count = results.Length,
            Results = results
        };
    }

    public async Task PostDexEntries<TDocument>(string collectionName, ICollection<TDocument> documents) where TDocument : IDocument, IDexDocument
    {
        var collection = _repositoryService.GetCollection<TDocument>(typeof(TDocument).Name);
        foreach (var document in documents)
        {
            var items = await collection.GetManyAsync(currentDocument => document.Name == currentDocument.Name, logger);
            if (items.Count != 0)
            {
                continue;
            }

            await PostDocument(collection, document, logger);
        }
    }

    public async Task PostPokedexEntries(ICollection<PokemonForm> documents)
    {
        foreach (var document in documents)
        {
            var items = await Collection.GetManyAsync(currentDocument => document.Name == currentDocument.Name && document.Form == currentDocument.Form, logger);
            if (items.Count != 0)
            {
                continue;
            }

            var dto = await modelToDtoMapper.ParseFromModel(document);
            await PostDocument(dto, logger);
        }
    }

    public async Task<Pokemon> GetEvolved(
        Pokemon pokemon,
        ICollection<string> keptMoves,
        string evolvedName,
        ICollection<string> newMoves)
    {
        var invalidKeptMoves = keptMoves.Where(x => !pokemon.Moves.Contains(x)).ToArray();
        if (invalidKeptMoves.Length != 0)
        {
            throw new InvalidEvolutionException($"{pokemon.SpeciesName} does not know {string.Join(", ", invalidKeptMoves)}");
        }
        var forms = await GetPokedexEntry(evolvedName, pokemon.Form);
        var basePokemon = forms.Pokemon;
        if (!string.Equals(basePokemon.EvolvesFrom, pokemon.SpeciesName, StringComparison.CurrentCultureIgnoreCase))
        {
            throw new InvalidEvolutionException($"{pokemon.SpeciesName} cannot evolve into {evolvedName}");
        }

        var evolvedMoves = basePokemon.Moves.Select(move => move.ToLower());
        var invalidMoves = newMoves.Where(move => !evolvedMoves.Contains(move.ToLower(System.Globalization.CultureInfo.CurrentCulture))).ToArray();
        if (invalidMoves.Length != 0)
        {
            throw new InvalidEvolutionException($"{basePokemon.Name} cannot learn {string.Join(", ", invalidMoves)}");
        }

        return new Pokemon
        {
            PokemonId = pokemon.PokemonId,
            DexNo = basePokemon.DexNo,
            SpeciesName = basePokemon.Name,
            Nickname = pokemon.Nickname,
            Gender = pokemon.Gender,
            PokemonStatus = pokemon.PokemonStatus,
            Moves = keptMoves.Union(newMoves).ToList(),
            Type = basePokemon.Type,
            CatchRate = GetCatchRate(basePokemon),
            Nature = pokemon.Nature,
            IsShiny = pokemon.IsShiny,
            PokemonStats = basePokemon.PokemonStats,
            Size = basePokemon.Size,
            Weight = basePokemon.Weight,
            Skills = basePokemon.Skills,
            Passives = basePokemon.Passives,
            Proficiencies = basePokemon.Proficiencies,
            EggGroups = basePokemon.EggGroups.Select(x => x.ToString()).ToList(),
            EggHatchRate = basePokemon.EggHatchRate,
            Habitats = basePokemon.Habitats,
            Diet = basePokemon.Diet,
            Rarity = basePokemon.Rarity,
            GMaxMove = basePokemon.GMaxMove,
            EvolvedFrom = basePokemon.EvolvesFrom,
            LegendaryStats = basePokemon.LegendaryStats,
            IsOnActiveTeam = pokemon.IsOnActiveTeam,
            AlternateForms = forms.AlternateForms,
            NormalPortrait = basePokemon.NormalPortrait,
            ShinyPortrait = basePokemon.ShinyPortrait,
            TrainerId = pokemon.TrainerId,
            OriginalTrainerId = pokemon.OriginalTrainerId,
            Form = pokemon.Form,
            CanEvolve = false,
            CurrentHP = pokemon.CurrentHP,
            GameId = pokemon.GameId,
            Pokeball = pokemon.Pokeball
        };
    }

    private static Pokemon GetPokemonFromBase(
        PokemonForm basePokemon,
        Nature nature,
        Gender gender,
        Status status,
        string? nickname,
        ICollection<string> altForms)
    {
        var updatedNickname = string.IsNullOrWhiteSpace(nickname)
            ? basePokemon.Name
            : nickname;

        var modifier = nature.GetNatureModifier();
        var stats = new Stats
        {
            HP = basePokemon.PokemonStats.HP,
            Attack = basePokemon.PokemonStats.Attack + modifier.AttackModifier,
            Defense = basePokemon.PokemonStats.Defense + modifier.DefenseModifier,
            SpecialAttack = basePokemon.PokemonStats.SpecialAttack + modifier.SpecialAttackModifier,
            SpecialDefense = basePokemon.PokemonStats.SpecialDefense + modifier.SpecialDefenseModifier,
            Speed = basePokemon.PokemonStats.Speed + modifier.SpeedModifier,
        };

        return new Pokemon
        {
            PokemonId = Guid.NewGuid(),
            DexNo = basePokemon.DexNo,
            SpeciesName = basePokemon.Name,
            Nickname = updatedNickname,
            Gender = gender,
            PokemonStatus = status,
            Moves = basePokemon.Moves,
            Type = basePokemon.Type,
            CatchRate = GetCatchRate(basePokemon),
            Nature = nature,
            IsShiny = new Random().Next(420) == 69,
            PokemonStats = stats,
            CurrentHP = stats.HP,
            Size = basePokemon.Size,
            Weight = basePokemon.Weight,
            Skills = basePokemon.Skills,
            Passives = basePokemon.Passives,
            Proficiencies = basePokemon.Proficiencies,
            EggGroups = basePokemon.EggGroups.Select(x => x.ToString()).ToList(),
            EggHatchRate = basePokemon.EggHatchRate,
            Habitats = basePokemon.Habitats,
            Diet = basePokemon.Diet,
            Rarity = basePokemon.Rarity,
            GMaxMove = basePokemon.GMaxMove,
            EvolvedFrom = basePokemon.EvolvesFrom,
            LegendaryStats = basePokemon.LegendaryStats,
            Form = basePokemon.Form,
            AlternateForms = altForms,
            NormalPortrait = basePokemon.NormalPortrait,
            ShinyPortrait = basePokemon.ShinyPortrait,
            IsOnActiveTeam = false,
            CanEvolve = false,
            GameId = Guid.Empty,
            OriginalTrainerId = Guid.Empty,
            Pokeball = nameof(Pokeball.Basic_Ball),
            TrainerId = Guid.Empty,
        };
    }

    private static int GetCatchRate(PokemonForm basePokemon)
    {
        Enum.TryParse(basePokemon.Rarity, true, out Rarity rarity);
        return rarity switch
        {
            Rarity.Common => 50,
            Rarity.Uncommon => 40,
            _ => 30,
        } - (15 * (basePokemon.Stage - 1));
    }
}
