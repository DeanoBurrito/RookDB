using System;

namespace RookDB
{
    public sealed class DBRecord
    {
        public DBTable ownerTable;
        
        public string identifier;
        public object[] values;

        internal DBRecord(string ident, object[] vals)
        {
            identifier = ident;
            values = vals;
        }
    }
}