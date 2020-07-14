using System;

namespace RookDB
{
    public sealed class DBRecord
    {
        public readonly DBTable ownerTable;
        
        public readonly string identifier;
        public readonly object[] values;

        internal DBRecord(string ident, object[] vals, DBTable owner)
        {
            ownerTable = owner;
            identifier = ident;
            values = vals;
        }
    }
}