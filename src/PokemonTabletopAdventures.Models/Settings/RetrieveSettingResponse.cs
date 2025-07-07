namespace PokemonTabletopAdventures.Models.Settings;

public class RetrieveSettingResponse
{
    public required ICollection<Setting> Settings { get; set; }
}