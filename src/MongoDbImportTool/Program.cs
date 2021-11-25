using MongoDbImportTool.Builders;

namespace MongoDbImportTool
{
    class Program
    {
        static void Main(string[] args)
        {
            BasePokemonBuilder.AddBasePokemon();
            BerryBuilder.AddBerries();
            BaseItemBuilder.AddItems();
            FeatureBuilder.AddFeatures();
            MoveBuilder.AddMoves();
            OriginBuilder.AddOrigins();
            TrainerClassBuilder.AddClasses();
        }
    }
}
