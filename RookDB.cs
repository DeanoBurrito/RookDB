using System;

namespace RookDB
{
    public sealed class RookDB
    {
        internal static object GetColumnDefaultValue(ColumnType type)
        {
            switch (type)
            {
                case ColumnType.UniqueIdentifier:
                    return "";
                case ColumnType.Text:
                    return "";
                case ColumnType.Boolean:
                    return false;
                case ColumnType.Integer:
                    return 0;
                case ColumnType.Float:
                    return 0f;
                case ColumnType.Enumeration:
                    return 0;
                case ColumnType.Reference:
                    return "";
                case ColumnType.List:
                    return new object[0];
                case ColumnType.Flags:
                    return 0;
                case ColumnType.Color:
                    return 0;
            }
            
            throw new Exception("Unexpected type for column: " + (int)type);
        }

        internal static object ParseField(ColumnType type, object rawValue)
        {
            switch (type)
            {
                case ColumnType.UniqueIdentifier:
                case ColumnType.Text:
                case ColumnType.Boolean:
                case ColumnType.Integer:
                case ColumnType.Float:
                case ColumnType.Enumeration:
                case ColumnType.Reference:
                case ColumnType.List:
                case ColumnType.Flags:
                case ColumnType.Color:
                    return null;
            }

            throw new Exception("Unexpected type for column: " + (int)type);
        }
    }
}