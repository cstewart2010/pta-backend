using System;

namespace TheReplacement.PTA.Common.Enums
{
    /// <summary>
    /// Represents a container for Pokemon typings
    /// </summary>
    [Flags]
    public enum PokemonTypes
    {
        /// <summary>
        /// Represent no valid Type
        /// </summary>
        None = 0,

        /// <summary>
        /// Represent a Normal Type
        /// </summary>
        Normal = 1,

        /// <summary>
        /// Represent a Fire Type
        /// </summary>
        Fire = 2,

        /// <summary>
        /// Represent a Normal/Fire Type
        /// </summary>
        Normal_Fire,

        /// <summary>
        /// Represent a Water Type
        /// </summary>
        Water = 4,
        
        /// <summary>
        /// Represents a Normal/Water Type
        /// </summary>
        Normal_Water,

        /// <summary>
        /// Represents a Fire/Water Type
        /// </summary>
        Fire_Water,

        /// <summary>
        /// Represent a Grass Type
        /// </summary>
        Grass = 8,

        /// <summary>
        /// Represent a Normal/Grass Type
        /// </summary>
        Normal_Grass,

        /// <summary>
        /// Represents a Fire/Grass Type
        /// </summary>
        Fire_Grass,

        /// <summary>
        /// Represents a Water/Grass Type
        /// </summary>
        Water_Grass = Water | Grass,

        /// <summary>
        /// Represent a Electric Type
        /// </summary>
        Electric = 16,

        /// <summary>
        /// Represents a Normal/Electric
        /// </summary>
        Normal_Electric,

        /// <summary>
        /// Represents a Fire/Electric Type
        /// </summary>
        Fire_Electric,

        /// <summary>
        /// Represents a Water/Electric Type
        /// </summary>
        Water_Electric = Water | Electric,

        /// <summary>
        /// Represents a Grass/Electric Type
        /// </summary>
        Grass_Electric = Grass | Electric,

        /// <summary>
        /// Represent a Ice Type
        /// </summary>
        Ice = 32,

        /// <summary>
        /// Represents a Normal/Ice
        /// </summary>
        Normal_Ice,

        /// <summary>
        /// Represents a Fire/Ice Type
        /// </summary>
        Fire_Ice,

        /// <summary>
        /// Represents a Water/Grass Type
        /// </summary>
        Water_Ice = Water | Ice,

        /// <summary>
        /// Represents a Grass/Ice Type
        /// </summary>
        Grass_Ice = Grass | Ice,

        /// <summary>
        /// Represent a Electric/Ice Type
        /// </summary>
        Electric_Ice = Electric | Ice,

        /// <summary>
        /// Represent a Fighting Type
        /// </summary>
        Fighting = 64,

        /// <summary>
        /// Represents a Normal/Fighting
        /// </summary>
        Normal_Fighting,

        /// <summary>
        /// Represents a Fire/Fighting Type
        /// </summary>
        Fire_Fighting,

        /// <summary>
        /// Represents a Water/Fighting Type
        /// </summary>
        Water_Fighting = Water | Fighting,

        /// <summary>
        /// Represents a Grass/Fighting Type
        /// </summary>
        Grass_Fighting = Grass | Fighting,

        /// <summary>
        /// Represent a Electric/Fighting Type
        /// </summary>
        Electric_Fighting = Electric | Fighting,

        /// <summary>
        /// Represents a Ice/Fighting Type
        /// </summary>
        Ice_Fighting= Ice | Fighting,

        /// <summary>
        /// Represent a Poison Type
        /// </summary>
        Poison = 128,

        /// <summary>
        /// Represents a Normal/Poison
        /// </summary>
        Normal_Poison,

        /// <summary>
        /// Represents a Fire/Poison Type
        /// </summary>
        Fire_Poison,

        /// <summary>
        /// Represents a Water/Poison Type
        /// </summary>
        Water_Poison = Water | Poison,

        /// <summary>
        /// Represents a Grass/Poison Type
        /// </summary>
        Grass_Poison = Grass | Poison,

        /// <summary>
        /// Represent a Electric/Poison Type
        /// </summary>
        Electric_Poison = Electric | Poison,

        /// <summary>
        /// Represents a Ice/Poison Type
        /// </summary>
        Ice_Poison = Ice | Poison,

        /// <summary>
        /// Represents a Fighting/Poison Type
        /// </summary>
        Fighting_Poison = Fighting | Poison,

        /// <summary>
        /// Represent a Ground Type
        /// </summary>
        Ground = 256,

        /// <summary>
        /// Represents a Normal/Ground
        /// </summary>
        Normal_Ground,

        /// <summary>
        /// Represents a Fire/Ground Type
        /// </summary>
        Fire_Ground,

        /// <summary>
        /// Represents a Water/Ground Type
        /// </summary>
        Water_Ground = Water | Ground,

        /// <summary>
        /// Represents a Grass/Ground Type
        /// </summary>
        Grass_Ground = Grass | Ground,

        /// <summary>
        /// Represent a Electric/Ground Type
        /// </summary>
        Electric_Ground = Electric | Ground,

        /// <summary>
        /// Represents a Ice/Ground Type
        /// </summary>
        Ice_Ground = Ice | Ground,

        /// <summary>
        /// Represents a Fighting/Ground Type
        /// </summary>
        Fighting_Ground = Fighting | Ground,

        /// <summary>
        /// Represents a Poison/Ground Type
        /// </summary>
        Poison_Ground = Poison | Ground,

        /// <summary>
        /// Represent a Flying Type
        /// </summary>
        Flying = 512,

        /// <summary>
        /// Represents a Normal/Flying
        /// </summary>
        Normal_Flying,

        /// <summary>
        /// Represents a Fire/Flying Type
        /// </summary>
        Fire_Flying,

        /// <summary>
        /// Represents a Water/Flying Type
        /// </summary>
        Water_Flying = Water | Flying,

        /// <summary>
        /// Represents a Grass/Flying Type
        /// </summary>
        Grass_Flying = Grass | Flying,

        /// <summary>
        /// Represent a Electric/Flying Type
        /// </summary>
        Electric_Flying = Electric | Flying,

        /// <summary>
        /// Represents a Ice/Flying Type
        /// </summary>
        Ice_Flying = Ice | Flying,

        /// <summary>
        /// Represents a Fighting/Flying Type
        /// </summary>
        Fighting_Flying = Fighting | Flying,

        /// <summary>
        /// Represents a Poison/Flying Type
        /// </summary>
        Poison_Flying = Poison | Flying,

        /// <summary>
        /// Represents a Ground/Flying Type
        /// </summary>
        Ground_Flying = Ground | Flying,

        /// <summary>
        /// Represent a Psychic Type
        /// </summary>
        Psychic = 1024,

        /// <summary>
        /// Represents a Normal/Psychic
        /// </summary>
        Normal_Psychic,

        /// <summary>
        /// Represents a Fire/Psychic Type
        /// </summary>
        Fire_Psychic,

        /// <summary>
        /// Represents a Water/Psychic Type
        /// </summary>
        Water_Psychic = Water | Psychic,

        /// <summary>
        /// Represents a Grass/Psychic Type
        /// </summary>
        Grass_Psychic = Grass | Psychic,

        /// <summary>
        /// Represent a Electric/Psychic Type
        /// </summary>
        Electric_Psychic = Electric | Psychic,

        /// <summary>
        /// Represents a Ice/Psychic Type
        /// </summary>
        Ice_Psychic = Ice | Psychic,

        /// <summary>
        /// Represents a Fighting/Psychic Type
        /// </summary>
        Fighting_Psychic = Fighting | Psychic,

        /// <summary>
        /// Represents a Poison/Psychic Type
        /// </summary>
        Poison_Psychic = Poison | Psychic,

        /// <summary>
        /// Represents a Ground/Psychic Type
        /// </summary>
        Ground_Psychic = Ground | Psychic,

        /// <summary>
        /// Represents a Flying/Psychic Type
        /// </summary>
        Flying_Psychic = Flying | Psychic,

        /// <summary>
        /// Represent a Bug Type
        /// </summary>
        Bug = 2048,

        /// <summary>
        /// Represents a Normal/Bug
        /// </summary>
        Normal_Bug,

        /// <summary>
        /// Represents a Fire/Bug Type
        /// </summary>
        Fire_Bug,

        /// <summary>
        /// Represents a Water/Bug Type
        /// </summary>
        Water_Bug = Water | Bug,

        /// <summary>
        /// Represents a Grass/Bug Type
        /// </summary>
        Grass_Bug = Grass | Bug,

        /// <summary>
        /// Represent a Electric/Bug Type
        /// </summary>
        Electric_Bug = Electric | Bug,

        /// <summary>
        /// Represents a Ice/Bug Type
        /// </summary>
        Ice_Bug = Ice | Bug,

        /// <summary>
        /// Represents a Fighting/Bug Type
        /// </summary>
        Fighting_Bug = Fighting | Bug,

        /// <summary>
        /// Represents a Poison/Bug Type
        /// </summary>
        Poison_Bug = Poison | Bug,

        /// <summary>
        /// Represents a Ground/Bug Type
        /// </summary>
        Ground_Bug = Ground | Bug,

        /// <summary>
        /// Represents a Flying/Bug Type
        /// </summary>
        Flying_Bug = Flying | Bug,

        /// <summary>
        /// Represents a Psychic/Bug Type
        /// </summary>
        Psychic_Bug = Psychic | Bug,

        /// <summary>
        /// Represent a Rock Type
        /// </summary>
        Rock = 4096,

        /// <summary>
        /// Represents a Normal/Rock
        /// </summary>
        Normal_Rock,

        /// <summary>
        /// Represents a Fire/Rock Type
        /// </summary>
        Fire_Rock,

        /// <summary>
        /// Represents a Water/Rock Type
        /// </summary>
        Water_Rock = Water | Rock,

        /// <summary>
        /// Represents a Grass/Rock Type
        /// </summary>
        Grass_Rock = Grass | Rock,

        /// <summary>
        /// Represent a Electric/Rock Type
        /// </summary>
        Electric_Rock = Electric | Rock,

        /// <summary>
        /// Represents a Ice/Rock Type
        /// </summary>
        Ice_Rock = Ice | Rock,

        /// <summary>
        /// Represents a Fighting/Rock Type
        /// </summary>
        Fighting_Rock = Fighting | Rock,

        /// <summary>
        /// Represents a Poison/Rock Type
        /// </summary>
        Poison_Rock = Poison | Rock,

        /// <summary>
        /// Represents a Ground/Rock Type
        /// </summary>
        Ground_Rock = Ground | Rock,

        /// <summary>
        /// Represents a Flying/Rock Type
        /// </summary>
        Flying_Rock = Flying | Rock,

        /// <summary>
        /// Represents a Psychic/Rock Type
        /// </summary>
        Psychic_Rock = Psychic | Rock,

        /// <summary>
        /// Represents a Bug/Rock Type
        /// </summary>
        Bug_Rock = Bug | Rock,

        /// <summary>
        /// Represent a Ghost Type
        /// </summary>
        Ghost = 8192,

        /// <summary>
        /// Represents a Normal/Ghost
        /// </summary>
        Normal_Ghost,

        /// <summary>
        /// Represents a Fire/Ghost Type
        /// </summary>
        Fire_Ghost,

        /// <summary>
        /// Represents a Water/Ghost Type
        /// </summary>
        Water_Ghost = Water | Ghost,

        /// <summary>
        /// Represents a Grass/Rock Type
        /// </summary>
        Grass_Ghost = Grass | Ghost,

        /// <summary>
        /// Represent a Electric/Ghost Type
        /// </summary>
        Electric_Ghost = Electric | Ghost,

        /// <summary>
        /// Represents a Ice/Ghost Type
        /// </summary>
        Ice_Ghost = Ice | Ghost,

        /// <summary>
        /// Represents a Fighting/Ghost Type
        /// </summary>
        Fighting_Ghost = Fighting | Ghost,

        /// <summary>
        /// Represents a Poison/Ghost Type
        /// </summary>
        Poison_Ghost = Poison | Ghost,

        /// <summary>
        /// Represents a Ground/Ghost Type
        /// </summary>
        Ground_Ghost = Ground | Ghost,

        /// <summary>
        /// Represents a Flying/Ghost Type
        /// </summary>
        Flying_Ghost = Flying | Ghost,

        /// <summary>
        /// Represents a Psychic/Ghost Type
        /// </summary>
        Psychic_Ghost = Psychic | Ghost,

        /// <summary>
        /// Represents a Bug/Ghost Type
        /// </summary>
        Bug_Ghost = Bug | Ghost,

        /// <summary>
        /// Represents a Rock/Ghost Type
        /// </summary>
        Rock_Ghost = Rock | Ghost,

        /// <summary>
        /// Represent a Dark Type
        /// </summary>
        Dark = 16384,

        /// <summary>
        /// Represents a Normal/Dark
        /// </summary>
        Normal_Dark,

        /// <summary>
        /// Represents a Fire/Dark Type
        /// </summary>
        Fire_Dark,

        /// <summary>
        /// Represents a Water/Dark Type
        /// </summary>
        Water_Dark = Water | Dark,

        /// <summary>
        /// Represents a Grass/Dark Type
        /// </summary>
        Grass_Dark = Grass | Dark,

        /// <summary>
        /// Represent a Electric/Dark Type
        /// </summary>
        Electric_Dark = Electric | Dark,

        /// <summary>
        /// Represents a Ice/Dark Type
        /// </summary>
        Ice_Dark = Ice | Dark,

        /// <summary>
        /// Represents a Fighting/Dark Type
        /// </summary>
        Fighting_Dark = Fighting | Dark,

        /// <summary>
        /// Represents a Poison/Dark Type
        /// </summary>
        Poison_Dark = Poison | Dark,

        /// <summary>
        /// Represents a Ground/Dark Type
        /// </summary>
        Ground_Dark = Ground | Dark,

        /// <summary>
        /// Represents a Flying/Dark Type
        /// </summary>
        Flying_Dark = Flying | Dark,

        /// <summary>
        /// Represents a Psychic/Dark Type
        /// </summary>
        Psychic_Dark = Psychic | Dark,

        /// <summary>
        /// Represents a Bug/Dark Type
        /// </summary>
        Bug_Dark = Bug | Dark,

        /// <summary>
        /// Represents a Rock/Dark Type
        /// </summary>
        Rock_Dark = Rock | Dark,

        /// <summary>
        /// Represents a Ghost/Dark Type
        /// </summary>
        Ghost_Dark = Ghost | Dark,

        /// <summary>
        /// Represent a Dragon Type
        /// </summary>
        Dragon = 32768,

        /// <summary>
        /// Represents a Normal/Dragon
        /// </summary>
        Normal_Dragon,

        /// <summary>
        /// Represents a Fire/Dragon Type
        /// </summary>
        Fire_Dragon,

        /// <summary>
        /// Represents a Water/Dragon Type
        /// </summary>
        Water_Dragon = Water | Dragon,

        /// <summary>
        /// Represents a Grass/Dragon Type
        /// </summary>
        Grass_Dragon = Grass | Dragon,

        /// <summary>
        /// Represent a Electric/Dragon Type
        /// </summary>
        Electric_Dragon = Electric | Dragon,

        /// <summary>
        /// Represents a Ice/Dragon Type
        /// </summary>
        Ice_Dragon = Ice | Dragon,

        /// <summary>
        /// Represents a Fighting/Dragon Type
        /// </summary>
        Fighting_Dragon = Fighting | Dragon,

        /// <summary>
        /// Represents a Poison/Dragon Type
        /// </summary>
        Poison_Dragon = Poison | Dragon,

        /// <summary>
        /// Represents a Ground/Dragon Type
        /// </summary>
        Ground_Dragon = Ground | Dragon,

        /// <summary>
        /// Represents a Flying/Dragon Type
        /// </summary>
        Flying_Dragon = Flying | Dragon,

        /// <summary>
        /// Represents a Psychic/Dragon Type
        /// </summary>
        Psychic_Dragon = Psychic | Dragon,

        /// <summary>
        /// Represents a Bug/Dragon Type
        /// </summary>
        Bug_Dragon = Bug | Dragon,

        /// <summary>
        /// Represents a Rock/Dragon Type
        /// </summary>
        Rock_Dragon = Rock | Dragon,

        /// <summary>
        /// Represents a Ghost/Dragon Type
        /// </summary>
        Ghost_Dragon = Ghost | Dragon,

        /// <summary>
        /// Represents a Dark/Dragon Type
        /// </summary>
        Dark_Dragon = Dark | Dragon,

        /// <summary>
        /// Represent a Steel Type
        /// </summary>
        Steel = 65536,

        /// <summary>
        /// Represents a Normal/Steel
        /// </summary>
        Normal_Steel,

        /// <summary>
        /// Represents a Fire/Steel Type
        /// </summary>
        Fire_Steel,

        /// <summary>
        /// Represents a Water/Steel Type
        /// </summary>
        Water_Steel = Water | Steel,

        /// <summary>
        /// Represents a Grass/Steel Type
        /// </summary>
        Grass_Steel = Grass | Steel,

        /// <summary>
        /// Represent a Electric/Steel Type
        /// </summary>
        Electric_Steel = Electric | Steel,

        /// <summary>
        /// Represents a Ice/Steel Type
        /// </summary>
        Ice_Steel = Ice | Steel,

        /// <summary>
        /// Represents a Fighting/Steel Type
        /// </summary>
        Fighting_Steel = Fighting | Steel,

        /// <summary>
        /// Represents a Poison/Steel Type
        /// </summary>
        Poison_Steel = Poison | Steel,

        /// <summary>
        /// Represents a Ground/Steel Type
        /// </summary>
        Ground_Steel = Ground | Steel,

        /// <summary>
        /// Represents a Flying/Steel Type
        /// </summary>
        Flying_Steel = Flying | Steel,

        /// <summary>
        /// Represents a Psychic/Steel Type
        /// </summary>
        Psychic_Steel = Psychic | Steel,

        /// <summary>
        /// Represents a Bug/Steel Type
        /// </summary>
        Bug_Steel = Bug | Steel,

        /// <summary>
        /// Represents a Rock/Steel Type
        /// </summary>
        Rock_Steel = Rock | Steel,

        /// <summary>
        /// Represents a Ghost/Steel Type
        /// </summary>
        Ghost_Steel = Ghost | Steel,

        /// <summary>
        /// Represents a Dark/Steel Type
        /// </summary>
        Dark_Steel = Dark | Steel,

        /// <summary>
        /// Represents a Dragon/Steel Type
        /// </summary>
        Dragon_Steel = Dragon | Steel,

        /// <summary>
        /// Represent a Fairy Type
        /// </summary>
        Fairy = 131072,

        /// <summary>
        /// Represents a Normal/Fairy
        /// </summary>
        Normal_Fairy,

        /// <summary>
        /// Represents a Fire/Fairy Type
        /// </summary>
        Fire_Fairy,

        /// <summary>
        /// Represents a Water/Fairy Type
        /// </summary>
        Water_Fairy = Water | Fairy,

        /// <summary>
        /// Represents a Grass/Fairy Type
        /// </summary>
        Grass_Fairy = Grass | Fairy,

        /// <summary>
        /// Represent a Electric/Fairy Type
        /// </summary>
        Electric_Fairy = Electric | Fairy,

        /// <summary>
        /// Represents a Ice/Fairy Type
        /// </summary>
        Ice_Fairy = Ice | Fairy,

        /// <summary>
        /// Represents a Fighting/Fairy Type
        /// </summary>
        Fighting_Fairy = Fighting | Fairy,

        /// <summary>
        /// Represents a Poison/Steel Type
        /// </summary>
        Poison_Fairy = Poison | Fairy,

        /// <summary>
        /// Represents a Ground/Fairy Type
        /// </summary>
        Ground_Fairy = Ground | Fairy,

        /// <summary>
        /// Represents a Flying/Fairy Type
        /// </summary>
        Flying_Fairy = Flying | Fairy,

        /// <summary>
        /// Represents a Psychic/Fairy Type
        /// </summary>
        Psychic_Fairy = Psychic | Fairy,

        /// <summary>
        /// Represents a Bug/Fairy Type
        /// </summary>
        Bug_Fairy = Bug | Fairy,

        /// <summary>
        /// Represents a Rock/Fairy Type
        /// </summary>
        Rock_Fairy = Rock | Fairy,

        /// <summary>
        /// Represents a Ghost/Fairy Type
        /// </summary>
        Ghost_Fairy = Ghost | Fairy,

        /// <summary>
        /// Represents a Dark/Fairy Type
        /// </summary>
        Dark_Fairy = Dark | Fairy,

        /// <summary>
        /// Represents a Dragon/Fairy Type
        /// </summary>
        Dragon_Fairy = Dragon | Fairy,

        /// <summary>
        /// Represents a Steel/Fairy Type
        /// </summary>
        Steel_Fairy = Steel | Fairy,
    }
}
