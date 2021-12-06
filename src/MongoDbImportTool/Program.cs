using MongoDbImportTool.Builders;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MongoDbImportTool
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new TaskFactory();
            var tasks = new List<Task>
            {
                factory.StartNew(BasePokemonBuilder.AddBasePokemon),
                factory.StartNew(BerryBuilder.AddBerries),
                factory.StartNew(BaseItemBuilder.AddItems),
                factory.StartNew(FeatureBuilder.AddFeatures),
                factory.StartNew(MoveBuilder.AddMoves),
                factory.StartNew(OriginBuilder.AddOrigins),
                factory.StartNew(TrainerClassBuilder.AddClasses)
            };
            foreach (var task in tasks)
            {
                if (!(task.IsCompleted || task.IsFaulted))
                {
                    task.Wait();
                }
            }
        }
    }
}
