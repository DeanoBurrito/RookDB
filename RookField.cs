using System;

namespace RookDB
{
    public sealed class RookField : IRookObj
    {
        internal readonly string storedValue;
        internal object cachedValue;

        public readonly RookColumn column;
        public readonly RookRecord record;
        public readonly RookSheet sheet;
        public readonly RookDB db;

        internal RookField(RookRecord record, RookColumn column, string storedValue)
        {
            this.storedValue = storedValue;
            this.cachedValue = null;
            this.column = column;
            this.record = record;
            this.sheet = record.sheet;
            this.db = record.db;
        }

        public object GetValue()
        {
            if (cachedValue == null)
                RookDB.CastFieldValue(this);
            
            return cachedValue;
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