using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PokemonTabletopAdventures.CoreApi.DTOs.Npcs;

public class Npc
{
    internal Npc() { }

    internal static async Task<Npc> ParseFromModel(NpcModel npc, IPokemonService pokemonService)
    {
        var npcPokemon = await  pokemonService.GetPokemonByTrainerId(npc.NPCId);
        return new Npc
        {
            NpcId = npc.NPCId,
            GameId = npc.GameId,
            TrainerName = npc.TrainerName,
            Feats = npc.Feats,
            TrainerClasses = npc.TrainerClasses,
            TrainerStats = npc.TrainerStats,
            PokemonTeam = npcPokemon.Where(pokemon => pokemon.IsOnActiveTeam),
            Level = npc.Level,
            TrainerSkills = npc.TrainerSkills,
            Gender = npc.Gender,
            Height = npc.Height,
            Weight = npc.Weight,
            Description = npc.Description,
            Personality = npc.Personality,
            Background = npc.Background,
            Goals = npc.Goals,
            Species = npc.Species,
            Sprite = npc.Sprite,
        };
    }

    internal async Task<NpcModel> ParseBackToModel(INpcService npcService)
    {
        var npc = await npcService.GetNpc(NpcId);
        npc.TrainerName = TrainerName;
        npc.Feats = Feats;
        npc.TrainerClasses = TrainerClasses;
        npc.TrainerStats = TrainerStats;
        npc.Level = Level;
        npc.TrainerSkills = TrainerSkills;
        npc.Gender = Gender;
        npc.Height = Height;
        npc.Weight = Weight;
        npc.Description = Description;
        npc.Personality = Personality;
        npc.Background = Background;
        npc.Goals = Goals;
        npc.Species = Species;
        npc.Sprite = Sprite;
        return npc;
    }

    public required Guid NpcId { get; set; }

    public required Guid GameId { get; set; }

    public required string TrainerName { get; set; }

    public required IEnumerable<string> Feats { get; set; }

    public required IEnumerable<string> TrainerClasses { get; set; }

    public required StatsModel TrainerStats { get; set; }

    public required IEnumerable<PokemonModel> PokemonTeam { get; set; }

    public required int Level { get; set; }

    public required IEnumerable<TrainerSkill> TrainerSkills { get; set; }

    public required string Gender { get; set; }

    public required int Height { get; set; }

    public required int Weight { get; set; }

    public required string Description { get; set; }

    public required string Personality { get; set; }

    public required string Background { get; set; }

    public required string Goals { get; set; }

    public required string Species { get; set; }

    public required string Sprite { get; set; }
}
