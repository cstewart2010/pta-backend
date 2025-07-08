using MongoDB.Driver;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.Domain.Handlers;
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

public class DexService(ILogger<DexService> logger) : AbstractMongoService<BasePokemonDto>(MongoCollection.BasePokemon), IDexService
{
    private readonly ILogger<DexService> _logger = logger;

    public async Task<IEnumerable<TDocument>> GetDexEntries<TDocument>(DexType documentType) where TDocument : IDexDocument
    {
        var collection = MongoCollectionHelper.GetMongoCollection<TDocument>(documentType.ToString());
        return await Task.FromResult(collection.Find(document => true).ToEnumerable());
    }

    public async Task<IndexResponse<TDocument>> GetDexEntry<TDocument>(DexType documentType, string name) where TDocument : IDexDocument
    {
        var collection = MongoCollectionHelper.GetMongoCollection<TDocument>(documentType.ToString());
        var item = collection.Find(document => document.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
        return item == null ? throw new ItemNotFoundException(name) : await Task.FromResult(new IndexResponse<TDocument> { Data = item });
    }

    public async Task<Pokemon> GetNewPokemon(string name, string nickname, string form)
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
        return await Task.FromResult(GetPokemonFromBase(entry.Pokemon, nature, gender, status, nickname, entry.AlternateForms));
    }

    public async Task<PokemonAndForms> GetPokedexEntry(string name, string form)
    {
        var allForms = Collection.Find(document => document.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase)).ToEnumerable();
        var model = allForms.First(document => document.Form.Equals(form, StringComparison.CurrentCultureIgnoreCase));
        var alternateForms = allForms.Where(document => !document.Form.Equals(form, StringComparison.CurrentCultureIgnoreCase))
            .Select(document => document.Form);
        return await Task.FromResult(new PokemonAndForms
        {
            Pokemon = DtoHandler.ParseFromDto(model),
            AlternateForms = [.. alternateForms]
        });
    }

    public async Task<IEnumerable<PokemonForm>> GetPossibleEvolutions(Pokemon pokemon)
    {
        var allEvolutions = Collection.Find(document => document.EvolvesFrom.Equals(pokemon.SpeciesName, StringComparison.CurrentCultureIgnoreCase)).ToEnumerable();
        var forms = allEvolutions.Where(evolution => evolution.Form.Equals(pokemon.Form, StringComparison.CurrentCultureIgnoreCase)).Select(DtoHandler.ParseFromDto);
        return await Task.FromResult(forms);
    }

    public async Task<IndexCollectionResponse> GetIndexCollectionResponse<TDocument>(
        DexType documentType,
        int offset,
        int limit) where TDocument : IDexDocument
    {
        var collection = MongoCollectionHelper.GetMongoCollection<TDocument>(documentType.ToString());
        var documents = collection.Find(document => true).Skip(offset).Limit(limit).ToEnumerable();
        var count = documents.Count();
        var results = documents.Select(x => x.Name);

        return await Task.FromResult(new IndexCollectionResponse
        {
            Count = count,
            Results = results
        });
    }

    public async Task PostDexEntries<TDocument>(string collectionName, IEnumerable<TDocument> documents) where TDocument : IDexDocument
    {
        var collection = MongoCollectionHelper.GetMongoCollection<TDocument>(typeof(TDocument).Name);
        foreach (var document in documents)
        {
            if (collection.Find(currentDocument => document.Name == currentDocument.Name).Any())
            {
                continue;
            }

            AddDexEntry(() => collection.InsertOne(document));
        }

        await Task.CompletedTask;
    }

    public async Task PostPokedexEntries(IEnumerable<PokemonForm> documents)
    {
        foreach (var document in documents)
        {
            if (Collection.Find(currentDocument => document.Name == currentDocument.Name && document.Form == currentDocument.Form).Any())
            {
                continue;
            }

            var dto = DtoHandler.ParseFromModel(document);
            AddDexEntry(() => Collection.InsertOne(dto));
        }

        await Task.CompletedTask;
    }

    public async Task<Pokemon> GetEvolved(
        Pokemon pokemon,
        IEnumerable<string> keptMoves,
        string evolvedName,
        IEnumerable<string> newMoves)
    {
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

        return await Task.FromResult(new Pokemon
        {
            PokemonId = pokemon.PokemonId,
            DexNo = basePokemon.DexNo,
            SpeciesName = basePokemon.Name,
            Nickname = pokemon.Nickname,
            Gender = pokemon.Gender,
            PokemonStatus = pokemon.PokemonStatus,
            Moves = keptMoves.Union(newMoves),
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
            EggGroups = basePokemon.EggGroups.Select(x => x.ToString()),
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
        });
    }

    private static Pokemon GetPokemonFromBase(
        PokemonForm basePokemon,
        Nature nature,
        Gender gender,
        Status status,
        string? nickname,
        IEnumerable<string> altForms)
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
            EggGroups = basePokemon.EggGroups.Select(x => x.ToString()),
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
            Pokeball = Pokeball.Basic_Ball.ToString(),
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

    private static void AddDexEntry(Action action)
    {
        try
        {
            action();
        }
        catch (MongoWriteException exception)
        {
            throw new PtaMongoException(exception);
        }
    }
}
