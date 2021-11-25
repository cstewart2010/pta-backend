using Newtonsoft.Json.Linq;
using System;

namespace TheReplacement.PTA.Common.Exceptions
{
    public class InvalidJsonPropertyException : Exception
    {
        public InvalidJsonPropertyException(JProperty property, Array expectedValues)
            : base($"Invalid value for Json property {property.Name}: {property.Value}.\n" +
                  $"Expected: {string.Join(',', expectedValues)}")
        {
            PropertyName = property.Name;
            PropertyValue = property.Value;
        }
        public InvalidJsonPropertyException(JProperty property, Type expectedType)
            : base($"Invalid value for Json property {property.Name}: {property.Value}.\n" +
                  $"Expected typeof({expectedType.Name})")
        {
            PropertyName = property.Name;
            PropertyValue = property.Value;
        }

        public string PropertyName { get; }
        public object PropertyValue { get; }
    }
}
