using PokemonTabletopAdventures.Models;
using System.Collections.Generic;

namespace PokemonTabletopAdventures.CoreApi.DTOs.Indicies;

public class PokemonAndForms
{
    public required BasePokemonModel Pokemon { get; init; }
    public required ICollection<string> AlternateForms { get; init; }
}
