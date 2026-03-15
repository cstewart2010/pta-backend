using PokemonTabletopAdventures.CoreApi.Constants;

namespace PokemonTabletopAdventures.CoreApi.Exceptions;

public class OutofRangeException(double left, double right) : PtaException($"Value must be bounded between {left} and {right}", PtaExceptionParts.OutOfRangeTitle, System.Net.HttpStatusCode.BadRequest)
{
    public static void CheckValue(double left, double right, double actual)
    {
        if (actual < left || actual > right)
        {
            throw new OutofRangeException(left, right);
        }
    }
}
