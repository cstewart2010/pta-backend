﻿using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheReplacement.PTA.Common.Internal;

namespace TheReplacement.PTA.Common.Models
{
    public class MinifiedGameModel
    {
        internal MinifiedGameModel(GameModel game)
        {
            GameId = game.GameId;
            Nickname = game.Nickname;
            GameMasters = MongoCollectionHelper.Trainers
                .Find(trainer => trainer.GameId == game.GameId && trainer.IsGM)
                .ToEnumerable()
                .Select(trainer => trainer.TrainerName);
        }

        /// <summary>
        /// The PTA game session id
        /// </summary>
        public string GameId { get; }

        /// <summary>
        /// The PTA game masters
        /// </summary>
        public IEnumerable<string> GameMasters{ get; }

        /// <summary>
        /// A user-friendly nickname for the game session
        /// </summary>
        public string Nickname { get; }
    }
}
