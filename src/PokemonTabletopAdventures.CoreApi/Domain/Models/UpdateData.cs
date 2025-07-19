namespace PokemonTabletopAdventures.CoreApi.Domain.Models;

public class UpdateData(string field, object value)
{
    public string Field { get; } = field;
    public object Value { get; } = value;
}
