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

        internal RookRecord(RookSheet sheet, string ident, ImmutableArray<RookField> fields)
        {
            identifier = ident;
            this.sheet = sheet;
            this.db = sheet.db;
            this.fields = fields;
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