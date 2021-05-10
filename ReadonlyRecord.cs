using System;

namespace RookDB
{
    public sealed class ReadonlyRecord
    {
        public readonly string identifier;
        internal uint offset;

        internal ReadonlyRecord(string ident, uint offset)
        {
            identifier = ident;
            this.offset = offset;
        }

        public override string ToString()
        {
            return "[ReadOnlyRecod] " + identifier;
        }
    }
}