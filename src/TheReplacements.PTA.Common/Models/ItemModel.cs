namespace TheReplacements.PTA.Common.Models
{
    /// <summary>
    /// Represents a Pokemon Tabletop Adventures item
    /// </summary>
    public class ItemModel
    {
        /// <summary>
        /// The name of the item
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The Amount of that item that the user is holding, greater than 0
        /// </summary>
        public int Amount { get; set; }
    }
}
