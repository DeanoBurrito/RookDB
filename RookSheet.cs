using System;
using System.Collections.Immutable;

namespace RookDB
{
    public sealed class RookSheet
    {
        public readonly RookDB db;

        internal ImmutableArray<RookRecord> records;
        internal ImmutableArray<RookColumn> columns;

        internal RookSheet()
        {

        }
    }
}