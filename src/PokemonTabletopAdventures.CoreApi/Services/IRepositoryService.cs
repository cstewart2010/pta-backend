namespace PokemonTabletopAdventures.CoreApi.Services;

public interface IRepositoryService
{
    public ICollectionService<T> GetCollection<T>(string collectionName);
}
