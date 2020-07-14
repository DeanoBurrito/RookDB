using System;

namespace RookDB
{
    public static class StringHelper
    {
        public static string LimitLength(string source, int length, string hintStr = " [...]")
        {
            if (source.Length <= length)
                return source;
            source = source.Remove(length - hintStr.Length);
            return source + hintStr;
        }
    }
}