namespace PokemonTabletopAdventures.CoreApi.Constants;

public static class PtaExceptionParts
{
    public const string EmptyImportJsonMessage = "Import json was empty";
    public const string UnauthorizedPokemonUseMessage = "This pokemon can only be accessed by it's trainer or the game master";
    public const string MissingUsernameMessage = "No username found with provided";
    public const string AlreadyCaughtPokemonMessage = "You cannot catch previously caught pokemon.";
    public const string PlayNotOnlineMessage = "Player is not currently online";
    public const string BrokeNeighborMessage = "Not enough money to purchase all items on list";
    public const string EmptyTokenMessage = "Empty token";
    public const string ImproperTokenMessage = "Improper token";
    public const string ExpiredTokenMessage = "Expired token";
    public const string InvalidSecretMessage = "Invalid secret";
    public const string SelfTradeMessage = "Cannot trade with oneself";
    public const string NullDataMessage = "EvolvePokemonData property was null";
    public const string TooManyParticipantsMessage = "Request should contain exactly one participant";
    public const string TooManyShopsMessage = "Request should contain exactly one shop";
    public const string TooManyShopsTrainers = "Request should contain exactly one trainers";
    public const string UnauthorizedTrainerCompletionMessage = "User is not authorized for this action";
    public const string NoUserFoundMessage = "No username found with provided";

    public const string InvalidCatchTitle = "Invalid Catch Attempt";
    public const string InvalidSettingTitle = "Invalid Setting Request";
    public const string InvalidShopTitle = "Invalid Shop Request";
    public const string InvalidTrainerTitle = "Invalid Trainer Request";
    public const string InvalidPokemonTitle = "Invalid Pokemon Retrieval";
    public const string DeletionErrorTitle = "Deletion Fail";
    public const string UpdateErrorTitle = "Failure At Update";
    public const string EvolutionErrorTitle = "Invalid Evolution";
    public const string DuplicateEntryTitle = "Duplicate Entry";
    public const string OneOrMoreTitle = "One Or More Errors oOcurs";
    public const string InvalidTradeTitle = "Invalid Trade Request";
    public const string ItemNotFoundTitle = "Item Was Not Found";
    public const string OutOfRangeTitle = "Input Out Of Range";
    public const string MongoDbErrorTitle = "MongoDB Exception";
    public const string AuthenticationErrorTitle = "Authentication Failed";
    public const string UnknownEntityTitle = "Unknown Entity";
    public const string UserNotFoundTitle = "User was not found";
    public const string InvalidFormTitle = "Invalid Form";
}
