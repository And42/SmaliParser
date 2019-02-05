using System;

namespace SmaliParser.Logic
{
    internal static class Utils
    {
        public static bool IsPrimitive(string type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (type.Length > 0 && type[0] == '[')
                type = type.TrimStart('[');

            return type.Length == 1 && GlobalConstants.Primitives.Contains(type);
        }
    }
}
