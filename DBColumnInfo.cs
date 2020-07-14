using System;

namespace RookDB
{
    public sealed class DBColumnInfo
    {
        public DBTable ownerTable;
        
        public string columnIdenfier;
        public ColumnType columnType;
        internal string[] columnMeta; //specific to the columnType, might hold enum/flags values

        internal DBColumnInfo()
        {
            
        }
    }
}