namespace PokemonTabletopAdventures.CoreApi.Constants;

public static class EnvironmentVariableNames
{
    public const string Database = "Database";
#if DEBUG
    public const string MongoDBConnectionString = "MainConnectionString";
#else
    public const string MongoDBConnectionString = "MongoDBConnectionString";
#endif
}
