using System;

namespace RookDB
{
    public enum ColumnType : byte
    {
        UniqueIdentifier = 0,
        Text = 1,
        Boolean = 2,
        Integer = 3,
        Float = 4,
        Enumeration = 5,
        Reference = 6,
        List = 8,
        Flags = 10,
        Color = 11,
    }
}