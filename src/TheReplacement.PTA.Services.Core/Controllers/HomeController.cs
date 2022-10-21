using Microsoft.AspNetCore.Mvc;
using System;
using System.Reflection;

namespace TheReplacement.PTA.Services.Core.Controllers
{
    [ApiController]
    [Route("api")]
    public class HomeController : StaticControllerBase
    {
        private const string Version = "v1";

        [HttpGet]
        public object Index()
        {
            return new
            {
                Version = Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                Pokedex = $"{HostUrl}/{Version}/pokedex",
                Berrydex = $"{HostUrl}/{Version}/berrydex",
                Featuredex = new[]
                {
                    $"{HostUrl}/{Version}/featuredex/general",
                    $"{HostUrl}/{Version}/featuredex/legendary",
                    $"{HostUrl}/{Version}/featuredex/passives",
                    $"{HostUrl}/{Version}/featuredex/skills",
                },
                ItemDex = new[]
                {
                    $"{HostUrl}/{Version}/itemdex/key",
                    $"{HostUrl}/{Version}/itemdex/medical",
                    $"{HostUrl}/{Version}/itemdex/pokeball",
                    $"{HostUrl}/{Version}/itemdex/pokemon",
                    $"{HostUrl}/{Version}/itemdex/trainer",
                },
                Movedex = $"{HostUrl}/{Version}/movedex",
                Origindex = $"{HostUrl}/{Version}/origindex",
                Classdex = $"{HostUrl}/{Version}/classdex"
            };
        }
    }
}
