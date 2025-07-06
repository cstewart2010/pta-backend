using MongoDB.Driver;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PokemonTabletopAdventures.CoreApi.Domain;

internal class NpcService : AbstractService<NpcModel>, INpcService
{
    public NpcService() : base(MongoCollection.NPCs) { }

    public async Task DeleteNpc(Guid id)
    {
        await ThrowIfNull(
            id,
            npcId => Collection.FindOneAndDelete(npc => npc.NPCId == npcId),
            PropertyNames.NpcId);
    }

    public async Task DeleteNpcByGameId(Guid gameId)
    {
        await ThrowIfNull(
            gameId,
            gameId => Collection.FindOneAndDelete(npc => npc.GameId == gameId),
            PropertyNames.GameId);
    }

    public async Task<NpcModel> GetNpc(Guid id)
    {
        return await ThrowIfNull(
            id,
            id => Collection.Find(npc => npc.NPCId == id).SingleOrDefault(),
            PropertyNames.NpcId);
    }

    public async Task<IEnumerable<NpcModel>> GetNpcs(IEnumerable<Guid> npcIds)
    {
        return await ThrowIfNull(
            npcIds,
            id => Collection.Find(npc => npcIds.Contains(npc.NPCId)).ToEnumerable(),
            PropertyNames.NpcId);
    }

    public async Task<IEnumerable<NpcModel>> GetNpcsByGameId(Guid gameId)
    {
        return await ThrowIfNull(
            gameId,
            id => Collection.Find(npc => npc.GameId == id).ToEnumerable(),
            PropertyNames.GameId);
    }

    public async Task PostNpc(NpcModel npc)
    {
        await PostDocument(npc);
    }

    public async Task<NpcModel> UpdateNpc(NpcModel updatedNpc)
    {
        await UpsertDocument(
            Builders<NpcModel>.Filter.Eq(npc => npc.NPCId, updatedNpc.NPCId),
            updatedNpc);

        return updatedNpc;
    }
}
