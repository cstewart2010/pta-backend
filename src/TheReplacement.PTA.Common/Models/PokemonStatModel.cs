namespace TheReplacement.PTA.Common.Models
{
    /// <summary>
    /// Represent the parts of a Pokemon's stat
    /// </summary>
    public class PokemonStatModel
    {
        /// <summary>
        /// The Base species value of the stat
        /// </summary>
        public int Base { get; set; }

        /// <summary>
        /// That Nature modifier
        /// </summary>
        public int Nature { get; set; }

        /// <summary>
        /// Any additional modifiers
        /// </summary>
        public int Modifier { get; set; }

        /// <summary>
        /// The points gain from levelling up
        /// </summary>
        public int Added { get; set; }

        /// <summary>
        /// The sum of all of the parts
        /// </summary>
        public int Total { get; set; }

        internal void AggregateStatValue()
        {
            Total = Base + Nature + Modifier + Added;
        }
    }
}
