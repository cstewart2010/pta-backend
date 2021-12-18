using System;
using System.Reflection;
using TheReplacement.PTA.Common.Attributes;
using TheReplacement.PTA.Common.Enums;

namespace TheReplacement.PTA.Common.Utilities
{
    /// <summary>
    /// Provides a set of extensions for retrieving enum attributes
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Returns the nature's <see cref="NatureModifierAttribute"/>
        /// </summary>
        /// <param name="nature">The provided nature's modifier</param>
        public static NatureModifierAttribute GetNatureModifier(this Nature nature)
        {
            var field = typeof(Nature).GetField(nature.ToString());
            return GetAttribute<NatureModifierAttribute>(field);
        }

        private static T GetAttribute<T>(FieldInfo field) where T : Attribute
        {
            var attribute = field.GetCustomAttribute<T>();
            if (attribute is T result)
            {
                return result;
            }

            return null;
        }
    }
}
