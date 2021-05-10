using System;
using System.Collections.Immutable;

namespace RookDB
{
    public sealed class ReadonlySheet
    {
        public readonly string identifier;
        
        internal readonly ImmutableArray<ReadonlyColumn> columns;
        internal readonly ImmutableArray<ReadonlyRecord> records;
        internal readonly ImmutableArray<object> fields;
        internal readonly bool visible;

        internal ReadonlySheet(string ident, ImmutableArray<ReadonlyColumn> columns, ImmutableArray<ReadonlyRecord> records, ImmutableArray<object> fields, bool visible)
        {
            identifier = ident;

            this.columns = columns;
            this.records = records;
            this.fields = fields;
            this.visible = visible;
        }

        public ReadonlyColumn GetColumn(RookPath path)
        {
            //Unsupported naming schemes:
            //"sheet/column_name/record_name"
            string columnName = null;
            switch (path.segments.Length)
            {
                case 1:
                    columnName = path.segments[0]; //"column_name"
                    break;
                case 2:
                    columnName = path.segments[1]; //"sheet/column_name" OR "record_name/column_name"
                    break;
                case 3:
                    columnName = path.segments[2]; //"sheet/record_name/column_name"
                    break;
            }
            if (columnName == null)
                return null; //unable to find column

            foreach (ReadonlyColumn col in columns)
            {
                if (col.identifier == columnName)
                    return col;
            }
            return null; //column not found
        }
        
        public ReadonlyRecord GetRecord(RookPath path)
        {
            //Unsupported naming schemes:
            //"sheet/column_name/record_name"
            string recordName = null;
            switch (path.segments.Length)
            {
                case 1:
                    recordName = path.segments[0]; //"record_name"
                    break;
                case 2:
                    if (path.segments[0] == identifier)
                        recordName = path.segments[1]; //"sheet/record_name"
                    else
                        recordName = path.segments[0]; //"record_name/column_name"
                    break;
                case 3:
                    recordName = path.segments[1]; //"sheet/record_name/column_name"
                    break;
            }
            if (recordName == null)
                return null;
            
            foreach (ReadonlyRecord record in records)
            {
                if (record.identifier == recordName)
                    return record;
            }
            return null;
        }
        
        public object GetField(RookPath path)
        {
            if (path.id.HasValue && !path.isMeta && path.id < (ulong)fields.Length)
                return fields[(int)path.id];
            
            //need to locate field manually, and then populate path with new id
            ReadonlyColumn column = GetColumn(path);
            ReadonlyRecord record = GetRecord(path);
            if (column == null || record == null)
                return null;
            
            uint index = record.offset + column.offset;
            path.isMeta = false;
            path.id = index;
            return fields[(int)index];
        }

        public override string ToString()
        {
            return "[ReadOnlySheet] " + identifier + ", " + columns.Length + " columns, " + records.Length + " records.";
        }
    }
}