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

        public static string SquishArray(string[] array)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < array.Length; i++)
            {
                if (i > 0)
                    sb.Append(", ");
                sb.Append(array[i]);
            }

            return sb.ToString();
        }
    }
}