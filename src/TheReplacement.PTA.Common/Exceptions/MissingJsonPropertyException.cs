using System;

namespace TheReplacement.PTA.Common.Exceptions
{
    public class MissingJsonPropertyException : Exception
    {
        public MissingJsonPropertyException(string property)
            : base($"Missing Json property {property}")
        {
            PropertyName = property;
        }

        public string PropertyName { get; }
    }
}
