using System;
using System.Collections.Immutable;

namespace RookDB
{
    public sealed class RookRecord : IRookObj
    {
        public readonly string identifier;

        public readonly RookSheet sheet;
        public readonly RookDB db;

        internal ImmutableArray<RookField> fields;

        internal RookRecord(RookSheet sheet, string ident)
        {
            identifier = ident;
            this.sheet = sheet;
            this.db = sheet.db;
        }

        public void FlushWriteCache()
        {
            throw new NotImplementedException();
        }

        public string GetFullPath()
        {
            throw new NotImplementedException();
        }
    }
}