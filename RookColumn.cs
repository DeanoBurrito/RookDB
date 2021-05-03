using System;

namespace RookDB
{
    public sealed class RookColumn : IRookObj
    {
        public readonly string identifier;
        public readonly ColumnType type;
        public readonly bool isOptional;

        public readonly RookSheet sheet;
        public readonly RookDB db;

        internal RookColumn(RookSheet owner, string ident, ColumnType type, bool optional)
        {
            identifier = ident;
            this.type = type;
            isOptional = optional;
            sheet = owner;
            db = owner.db;
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