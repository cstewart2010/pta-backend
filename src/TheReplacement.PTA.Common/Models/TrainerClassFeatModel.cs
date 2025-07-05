namespace TheReplacement.PTA.Common.Models
{
    /// <summary>
    /// Represents a <see cref="FeatureModelv1"/> learned as part of a <see cref="TrainerClassModelv1"/>
    /// </summary>
    public class TrainerClassFeatModelv1
    {
        /// <summary>
        /// The naem of the feature
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The level that the feature is learned
        /// </summary>
        public int LevelLearned { get; set; }
    }
}
