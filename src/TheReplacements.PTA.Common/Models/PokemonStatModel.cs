namespace TheReplacements.PTA.Common.Models
{
    public class PokemonStatModel
    {
        public int Base { get; set; }
        public int Nature { get; set; }
        public int Modifier { get; set; }
        public int Added { get; set; }
        public int Total { get; set; }

        internal void AggregateStatValue()
        {
            Total = Base + Nature + Modifier + Added;
        }
    }
}
