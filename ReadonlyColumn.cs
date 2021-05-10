using System;
using System.Collections.Immutable;

namespace RookDB
{
    public sealed class ReadonlyColumn
    {
        public readonly string identifier;
        public readonly ColumnType type;
        public readonly bool isOptional;
        public readonly ImmutableArray<string> metadata;
        internal uint offset;

        internal ReadonlyColumn(string ident, ColumnType type, bool optional, ImmutableArray<string> meta, uint offset)
        {
            this.identifier = ident;
            this.type = type;
            this.isOptional = optional;
            this.metadata = meta;
            this.offset = offset;
        }

        public override string ToString()
        {
            string optStr = isOptional ? "optional " : "";
            return "[ReadOnlyColumn] " + identifier + " (" + type.ToString() + ") " + optStr;
        }
    }
}