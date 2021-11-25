using System;

namespace TheReplacement.PTA.Common.Enums
{
    /// <summary>
    /// Container for all possible egg group combinations
    /// </summary>
    [Flags]
    public enum EggGroups
    {
        /// <summary>
        /// Represents the Undiscovered
        /// </summary>
        Undiscovered = 0,

        /// <summary>
        /// Represents the Amorphus
        /// </summary>
        Amorphus = 1,

        /// <summary>
        /// Represents the Bug
        /// </summary>
        Bug = 2,

        /// <summary>
        /// Represents the Amorphus/Bug
        /// </summary>
        Amorphus_Bug,

        /// <summary>
        /// Represents the Dragon
        /// </summary>
        Dragon = 4,

        /// <summary>
        /// Represents the Amorphus/Dragon
        /// </summary>
        Amorphus_Dragon,

        /// <summary>
        /// Represents the Bug/Dragon
        /// </summary>
        Bug_Dragon,

        /// <summary>
        /// Represents the Fairy
        /// </summary>
        Fairy = 8,

        /// <summary>
        /// Represents the Amorphus/Fairy
        /// </summary>
        Amorphus_Fairy,

        /// <summary>
        /// Represents the Bug/Fairy
        /// </summary>
        Bug_Fairy,

        /// <summary>
        /// Represents the Dragon/Fairy
        /// </summary>
        Dragon_Fairy = Dragon | Fairy,

        /// <summary>
        /// Represents the Field
        /// </summary>
        Field = 16,

        /// <summary>
        /// Represents the Amorphus/Field
        /// </summary>
        Amorphus_Field,

        /// <summary>
        /// Represents the Bug/Field
        /// </summary>
        Bug_Field,

        /// <summary>
        /// Represents the Dragon/Field
        /// </summary>
        Dragon_Field = Dragon | Field,

        /// <summary>
        /// Represents the Fairy/Field
        /// </summary>
        Fairy_Field = Fairy | Field,

        /// <summary>
        /// Represents the Flying
        /// </summary>
        Flying = 32,

        /// <summary>
        /// Represents the Amorphus/Flying
        /// </summary>
        Amorphus_Flying,

        /// <summary>
        /// Represents the Bug/Flying
        /// </summary>
        Bug_Flying,

        /// <summary>
        /// Represents the Dragon/Flying
        /// </summary>
        Dragon_Flying = Dragon | Flying,

        /// <summary>
        /// Represents the Fairy/Flying
        /// </summary>
        Fairy_Flying = Fairy | Flying,

        /// <summary>
        /// Represents the Field/Flying
        /// </summary>
        Field_Flying = Field | Flying,

        /// <summary>
        /// Represents the Grass
        /// </summary>
        Grass = 64,

        /// <summary>
        /// Represents the Amorphus/Grass
        /// </summary>
        Amorphus_Grass,

        /// <summary>
        /// Represents the Bug/Grass
        /// </summary>
        Bug_Grass,

        /// <summary>
        /// Represents the Dragon/Grass
        /// </summary>
        Dragon_Grass = Dragon | Grass,

        /// <summary>
        /// Represents the Fairy/Grass
        /// </summary>
        Fairy_Grass = Fairy | Grass,

        /// <summary>
        /// Represents the Field/Grass
        /// </summary>
        Field_Grass = Field | Grass,

        /// <summary>
        /// Represents the Flying/Grass
        /// </summary>
        Flying_Grass = Flying | Grass,

        /// <summary>
        /// Represents the Humanshape
        /// </summary>
        Humanshape = 128,

        /// <summary>
        /// Represents the Amorphus/Humanshape
        /// </summary>
        Amorphus_Humanshape,

        /// <summary>
        /// Represents the Bug/Humanshape
        /// </summary>
        Bug_Humanshape,

        /// <summary>
        /// Represents the Dragon/Humanshape
        /// </summary>
        Dragon_Humanshape = Dragon | Humanshape,

        /// <summary>
        /// Represents the Fairy/Humanshape
        /// </summary>
        Fairy_Humanshape = Fairy | Humanshape,

        /// <summary>
        /// Represents the Field/Humanshape
        /// </summary>
        Field_Humanshape = Field | Humanshape,

        /// <summary>
        /// Represents the Flying/Humanshape
        /// </summary>
        Flying_Humanshape = Flying | Humanshape,

        /// <summary>
        /// Represents the Grass/Humanshape
        /// </summary>
        Grass_Humanshape = Grass | Humanshape,

        /// <summary>
        /// Represents the Mineral
        /// </summary>
        Mineral = 256,

        /// <summary>
        /// Represents the Amorphus/Mineral
        /// </summary>
        Amorphus_Mineral,

        /// <summary>
        /// Represents the Bug/Mineral
        /// </summary>
        Bug_Mineral,

        /// <summary>
        /// Represents the Dragon/Mineral
        /// </summary>
        Dragon_Mineral = Dragon | Mineral,

        /// <summary>
        /// Represents the Fairy/Mineral
        /// </summary>
        Fairy_Mineral = Fairy | Mineral,

        /// <summary>
        /// Represents the Field/Mineral
        /// </summary>
        Field_Mineral = Field | Mineral,

        /// <summary>
        /// Represents the Flying/Mineral
        /// </summary>
        Flying_Mineral = Flying | Mineral,

        /// <summary>
        /// Represents the Grass/Mineral
        /// </summary>
        Grass_Mineral = Grass | Mineral,

        /// <summary>
        /// Represents the Humanshape/Mineral
        /// </summary>
        Humanshape_Mineral = Humanshape | Mineral,

        /// <summary>
        /// Represents the Monster
        /// </summary>
        Monster = 512,

        /// <summary>
        /// Represents the Amorphus/Monster
        /// </summary>
        Amorphus_Monster,

        /// <summary>
        /// Represents the Bug/Monster
        /// </summary>
        Bug_Monster,

        /// <summary>
        /// Represents the Dragon/Monster
        /// </summary>
        Dragon_Monster = Dragon | Monster,

        /// <summary>
        /// Represents the Fairy/Monster
        /// </summary>
        Fairy_Monster = Fairy | Monster,

        /// <summary>
        /// Represents the Field/Monster
        /// </summary>
        Field_Monster = Field | Monster,

        /// <summary>
        /// Represents the Flying/Monster
        /// </summary>
        Flying_Monster = Flying | Monster,

        /// <summary>
        /// Represents the Grass/Monster
        /// </summary>
        Grass_Monster = Grass | Monster,

        /// <summary>
        /// Represents the Humanshape/Monster
        /// </summary>
        Humanshape_Monster = Humanshape | Monster,

        /// <summary>
        /// Represents the Mineral/Monster
        /// </summary>
        Mineral_Monster = Mineral | Monster,

        /// <summary>
        /// Represents the Water 1
        /// </summary>
        Water1 = 1024,

        /// <summary>
        /// Represents the Amorphus/Water 1
        /// </summary>
        Amorphus_Water1,

        /// <summary>
        /// Represents the Bug/Water 1
        /// </summary>
        Bug_Water1,

        /// <summary>
        /// Represents the Dragon/Water 1
        /// </summary>
        Dragon_Water1 = Dragon | Water1,

        /// <summary>
        /// Represents the Fairy/Water 1
        /// </summary>
        Fairy_Water1 = Fairy | Water1,

        /// <summary>
        /// Represents the Field/Water 1
        /// </summary>
        Field_Water1 = Field | Water1,

        /// <summary>
        /// Represents the Flying/Water 1
        /// </summary>
        Flying_Water1 = Flying | Water1,

        /// <summary>
        /// Represents the Grass/Water 1
        /// </summary>
        Grass_Water1 = Grass | Water1,

        /// <summary>
        /// Represents the Humanshape/Water 1
        /// </summary>
        Humanshape_Water1 = Humanshape | Water1,

        /// <summary>
        /// Represents the Mineral/Water 1
        /// </summary>
        Mineral_Water1 = Mineral | Water1,

        /// <summary>
        /// Represents the Monster/Water 1
        /// </summary>
        Monster_Water1 = Monster | Water1,

        /// <summary>
        /// Represents the Water 2
        /// </summary>
        Water2 = 2048,

        /// <summary>
        /// Represents the Amorphus/Water 2
        /// </summary>
        Amorphus_Water2,

        /// <summary>
        /// Represents the Bug/Water 2
        /// </summary>
        Bug_Water2,

        /// <summary>
        /// Represents the Dragon/Water 2
        /// </summary>
        Dragon_Water2 = Dragon | Water2,

        /// <summary>
        /// Represents the Fairy/Water 2
        /// </summary>
        Fairy_Water2 = Fairy | Water2,

        /// <summary>
        /// Represents the Field/Water 2
        /// </summary>
        Field_Water2 = Field | Water2,

        /// <summary>
        /// Represents the Flying/Water 2
        /// </summary>
        Flying_Water2 = Flying | Water2,

        /// <summary>
        /// Represents the Grass/Water 2
        /// </summary>
        Grass_Water2 = Grass | Water2,

        /// <summary>
        /// Represents the Humanshape/Water 2
        /// </summary>
        Humanshape_Water2 = Humanshape | Water2,

        /// <summary>
        /// Represents the Mineral/Water 2
        /// </summary>
        Mineral_Water2 = Mineral | Water2,

        /// <summary>
        /// Represents the Monster/Water 2
        /// </summary>
        Monster_Water2 = Monster | Water2,

        /// <summary>
        /// Represents the Water 1/Water 2
        /// </summary>
        Water1_Water2 = Water1 | Water2,

        /// <summary>
        /// Represents the Water 3
        /// </summary>
        Water3 = 4096,

        /// <summary>
        /// Represents the Amorphus/Water 3
        /// </summary>
        Amorphus_Water3,

        /// <summary>
        /// Represents the Bug/Water 3
        /// </summary>
        Bug_Water3,

        /// <summary>
        /// Represents the Dragon/Water 3
        /// </summary>
        Dragon_Water3 = Dragon | Water3,

        /// <summary>
        /// Represents the Fairy/Water 3
        /// </summary>
        Fairy_Water3 = Fairy | Water3,

        /// <summary>
        /// Represents the Field/Water 3
        /// </summary>
        Field_Water3 = Field | Water3,

        /// <summary>
        /// Represents the Flying/Water 3
        /// </summary>
        Flying_Water3 = Flying | Water3,

        /// <summary>
        /// Represents the Grass/Water 3
        /// </summary>
        Grass_Water3 = Grass | Water3,

        /// <summary>
        /// Represents the Humanshape/Water 3
        /// </summary>
        Humanshape_Water3 = Humanshape | Water3,

        /// <summary>
        /// Represents the Mineral/Water 3
        /// </summary>
        Mineral_Water3 = Mineral | Water3,

        /// <summary>
        /// Represents the Monster/Water 3
        /// </summary>
        Monster_Water3 = Monster | Water3,

        /// <summary>
        /// Represents the Water 1/Water 3
        /// </summary>
        Water1_Water3 = Water1 | Water3,

        /// <summary>
        /// Represents the Water 2/Water 3
        /// </summary>
        Water2_Water3 = Water2 | Water3,

        /// <summary>
        /// Represents the Ditto Egg Group
        /// </summary>
        Ditto = 8191
    }
}
