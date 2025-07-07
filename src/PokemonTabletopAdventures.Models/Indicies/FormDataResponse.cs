namespace PokemonTabletopAdventures.Models.Indicies;

public class FormDataResponse
{
    public  required ICollection<FormData> Data { get; init; }
}

public class FormData
{
    public required string Name { get; init; }
    public required string Form {  get; init; }
}
