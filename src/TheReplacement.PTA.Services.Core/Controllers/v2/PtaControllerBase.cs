using Microsoft.AspNetCore.Mvc;
using PokemonTabletopAdventures.CoreApi.Domain;
using PokemonTabletopAdventures.CoreApi.DTOs;
using PokemonTabletopAdventures.CoreApi.Exceptions;
using PokemonTabletopAdventures.CoreApi.Extensions;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models;
using PokemonTabletopAdventures.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PokemonTabletopAdventures.CoreApi.Controllers.v2
{
    public abstract class PtaControllerBase(
        IUserService userService,
        ITrainerService trainerService,
        IPokemonService pokemonService,
        IGameService gameService,
        IDexService dexService,
        IPokedexService pokedexService,
        IEncryptionService encryptionService) : ControllerBase
    {
        private readonly IEncryptionService _encryptionService = encryptionService;
        public IUserService UserService { get; } = userService;
        public ITrainerService TrainerService { get; } = trainerService;
        public IPokemonService PokemonService { get; } = pokemonService;
        public IGameService GameService { get; } = gameService;
        public IDexService DexService { get; } = dexService;
        public IPokedexService PokedexService { get; } = pokedexService;

        protected async Task<PokemonModel> BuildPokemon(
            Guid trainerId,
            Guid gameId,
            WildPokemon wild)
        {
            var pokemon = await BuildDefaultPokemon(wild);
            pokemon.TrainerId = trainerId;
            pokemon.OriginalTrainerId = trainerId;
            pokemon.GameId = gameId;
            return pokemon;
        }

        protected async Task<IEnumerable<LogModel>> AddItemsToTrainer(TrainerModel trainer, IEnumerable<ItemModel> items)
        {
            var itemList = trainer.Items;
            foreach (var item in items)
            {
                trainer.Items = UpdateAllItemsWithAddition
                (
                    itemList,
                    item,
                    trainer
                );
            }

            await TrainerService.UpdateTrainer(trainer);
            return items.Select(item => new LogModel
            (
                user: trainer.TrainerName,
                action: $"added ({item.Amount}) {item.Name}"
            ));
        }

        protected async Task VerifyIdentity(
            string accessToken,
            string sessionAuth,
            Guid id)
        {
            var user = await UserService.GetUserById(id);
            Request.VerifyIdentity(user, _encryptionService, accessToken, sessionAuth);
        }

        protected async Task IsUserGM(
            Guid id,
            Guid gameId,
            string accessToken,
            string sessionAuth)
        {
            var trainer = await TrainerService.GetTrainerById(id, gameId);
            var user = await UserService.GetUserById(id);
            Request.IsUserGM(_encryptionService, accessToken, sessionAuth, user, trainer);
        }

        protected async Task AssignAuthAndToken(Guid id)
        {
            await Response.AssignAuthAndToken(_encryptionService, UserService, id);
        }

        protected async Task RefreshToken(Guid id)
        {
            await Response.RefreshToken(_encryptionService, UserService, id);
        }

        protected async Task<IEnumerable<LogModel>> RemoveItemsFromTrainer(TrainerModel trainer, IEnumerable<ItemModel> items)
        {
            var itemList = trainer.Items;
            foreach (var item in items)
            {
                itemList = UpdateAllItemsWithReduction
                (
                    itemList,
                    item,
                    trainer
                );
            }

            await TrainerService.UpdateTrainerItemList(trainer.TrainerId,trainer.GameId,itemList);
            return items.Select(item => new LogModel
            (
                user: trainer.TrainerName,
                action: $"removed ({item.Amount}) {item.Name}"
            ));
        }

        protected async Task IsGameAuthenticated(
            string gamePassword,
            GameModel game)
        {
            await _encryptionService.VerifySecret(gamePassword, game.PasswordHash);
        }

        protected async Task IsUserAuthenticated(PutLoginRequest request)
        {
            var user = await UserService.GetUserByUsername(request.Username) ?? throw new PtaUnauthorizedException("No username found with provided");
            await _encryptionService.VerifySecret(request.Password, user.PasswordHash);
            await UserService.UpdateUserOnlineStatus(user.UserId, true);
        }

        private static List<ItemModel> UpdateAllItemsWithAddition(
            List<ItemModel> itemList,
            ItemModel itemToken,
            TrainerModel trainer)
        {
            var item = trainer.Items.FirstOrDefault(item => item.Name.Equals(itemToken.Name, StringComparison.CurrentCultureIgnoreCase));
            if (item == null)
            {
                itemList.Add(itemToken);
            }
            else
            {
                itemList = [.. trainer.Items.Select(item => UpdateItemWithAddition(item, itemToken))];
            }

            return itemList;
        }

        private static ItemModel UpdateItemWithAddition(
            ItemModel item,
            ItemModel newItem)
        {
            if (item.Name == newItem.Name)
            {
                item.Amount = item.Amount + newItem.Amount > 100
                        ? 100
                        : item.Amount + newItem.Amount;
            }

            return item;
        }

        private static List<ItemModel> UpdateAllItemsWithReduction(
            List<ItemModel> itemList,
            ItemModel itemToken,
            TrainerModel trainer)
        {
            var item = trainer.Items.FirstOrDefault(item => item.Name.Equals(itemToken.Name, StringComparison.CurrentCultureIgnoreCase));
            if ((item?.Amount ?? 0) >= itemToken.Amount)
            {
                itemList = [.. trainer.Items
                .Select(item => UpdateItemWithReduction(item, itemToken))
                .Where(item => item.Amount > 0)];
            }

            return itemList;
        }

        private static ItemModel UpdateItemWithReduction(
            ItemModel item,
            ItemModel newItem)
        {
            if (item.Name == newItem.Name)
            {
                item.Amount -= newItem.Amount;
            }

            return item;
        }

        private async Task<PokemonModel> BuildDefaultPokemon(WildPokemon wild)
        {
            var random = new Random();
            if (!Enum.TryParse(wild.Gender, true, out Gender gender))
            {
                var genders = Enum.GetValues<Gender>();
                gender = genders[random.Next(genders.Length)];
            }

            if (!Enum.TryParse(wild.Nature, true, out Nature nature))
            {
                var natures = Enum.GetValues<Nature>();
                nature = natures[random.Next(1, natures.Length)];
            }

            if (!Enum.TryParse(wild.Status, true, out Status status))
            {
                status = Status.Normal;
            }

            var form = wild.Form.Replace('_', '/');
            var pokemon = await DexService.GetNewPokemon
            (
                wild.Pokemon,
                nature,
                gender,
                status,
                null,
                form
            );

            if (wild.ForceShiny)
            {
                pokemon.IsShiny = true;
            }
            pokemon.Pokeball = Pokeball.Basic_Ball.ToString().Replace("_", "");
            return pokemon;
        }
    }
}
