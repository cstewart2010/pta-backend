using System.Collections;

namespace PokemonTabletopAdventures.CoreApi.Exceptions
{
    public class GeneralErrorsException : PtaException
    {
        public GeneralErrorsException(IEnumerable errors) : base(string.Join("\n", errors), "One or more errors occurs", System.Net.HttpStatusCode.BadRequest) { }
    }
}
