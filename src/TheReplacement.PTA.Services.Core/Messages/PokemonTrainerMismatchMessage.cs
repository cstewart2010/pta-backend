namespace TheReplacement.PTA.Services.Core.Messages
{
    public class PokemonTrainerMismatchMessage : AbstractMessage
    {
        internal PokemonTrainerMismatchMessage(
            string pokemonTrainerId,
            string trainerId)
        {
            Message = "Invalid trainerId";
            ExpectedTrainerId = trainerId;
            PokemonTrainerId = pokemonTrainerId;
        }

        public override string Message { get; }
        public string ExpectedTrainerId { get; }
        public string PokemonTrainerId { get; }
    }
}
