using PokemonTabletopAdventures.Models.Enums;

namespace PokemonTabletopAdventures.Models.Pokemons
{
    public class PokemonForm
    {
        /// <inheritdoc/>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The Pokemon species' dex number
        /// </summary>
        public int DexNo { get; set; }

        /// <summary>
        /// The Pokemon species' form name
        /// </summary>
        public string Form { get; set; } = string.Empty;

        /// <summary>
        /// The Pokemon species' normal form
        /// </summary>
        public string NormalPortrait { get; set; } = string.Empty;

        /// <summary>
        /// The Pokemon species' shiny form
        /// </summary>
        public string ShinyPortrait { get; set; } = string.Empty;

        /// <summary>
        /// Collection of Pokemon stats
        /// </summary>
        public Stats PokemonStats { get; set; } = new();

        /// <summary>
        /// The Pokemon species' type positioning
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// The Pokemon species' <see cref="Size"/>
        /// </summary>
        public Size Size { get; set; }

        /// <summary>
        /// The Pokemon species' <see cref="Weight"/>
        /// </summary>
        public Weight Weight { get; set; }

        /// <summary>
        /// The Pokemon species' move list
        /// </summary>
        public IEnumerable<string> Moves { get; set; } = [];

        /// <summary>
        /// The Pokemon species' Skills
        /// </summary>
        public IEnumerable<string> Skills { get; set; } = [];

        /// <summary>
        /// The Pokemon species' Passives
        /// </summary>
        public IEnumerable<string> Passives { get; set; } = [];

        /// <summary>
        /// The Pokemon species' Proficiencies
        /// </summary>
        public IEnumerable<string> Proficiencies { get; set; } = [];

        /// <summary>
        /// The Pokemon species' <see cref="EggGroups"/>
        /// </summary>
        public IEnumerable<EggGroups> EggGroups { get; set; } = [];

        /// <summary>
        /// The Pokemon species' hatch rate
        /// </summary>
        public string EggHatchRate { get; set; } = string.Empty;

        /// <summary>
        /// The Pokemon species' habitats
        /// </summary>
        public IEnumerable<string> Habitats { get; set; } = [];

        /// <summary>
        /// The Pokemon species' diet
        /// </summary>
        public string Diet { get; set; } = string.Empty;

        /// <summary>
        /// The Pokemon species' <see cref="Rarity"/>
        /// </summary>
        public string Rarity { get; set; } = string.Empty;

        /// <summary>
        /// The Pokemon species' evolution stage
        /// </summary>
        public int Stage { get; set; }

        /// <summary>
        /// The Pokemon species' form name, if any
        /// </summary>
        public string SpecialFormName { get; set; } = string.Empty;

        /// <summary>
        /// The Pokemon species' base form, if any
        /// </summary>
        public string BaseFormName { get; set; } = string.Empty;

        /// <summary>
        /// The Pokemon species' GMax move, if any
        /// </summary>
        public string GMaxMove { get; set; } = string.Empty;

        /// <summary>
        /// The Pokemon species' previous evolution, if any
        /// </summary>
        public string EvolvesFrom { get; set; } = string.Empty;

        /// <summary>
        /// The Pokemon species' legendary stats, if applicable
        /// </summary>
        public LegendaryStats? LegendaryStats { get; set; }
    }
}
