using System;
using System.Collections.Generic;
using System.Text.Json;
using CliMod;

namespace RookDB
{
    public sealed class RookDB
    {
        internal const string INVALID_DB_STR = "@@INVALID_DATABASE@@";

        internal Dictionary<string, DBTable> tables = new Dictionary<string, DBTable>();
        internal Dictionary<string, List<DBColumnInfo>> embeddedSchemas = new Dictionary<string, List<DBColumnInfo>>();
        public string filename;

        public RookDB(string jsonData, string loadedFilename)
        {
            filename = loadedFilename;
            try
            {
                JsonDocument jdoc = JsonDocument.Parse(jsonData);
                if (!jdoc.RootElement.TryGetProperty("sheets", out _))
                {
                    throw new Exception("'sheets' property does not exist under root json element. Are you sure this is a valid database?");
                }
                List<JsonElement> deferredTables = new List<JsonElement>();
                foreach (JsonElement tableElement in jdoc.RootElement.GetProperty("sheets").EnumerateArray())
                {
                    //if table is a list schema, parse it, otherwise add all regular tables to a list to be processed after.
                    if (tableElement.TryGetProperty("name", out JsonElement nameProp))
                    {
                        if (nameProp.GetString().Contains("@"))
                        {
                            string schemaName = tableElement.GetProperty("name").GetString();
                            List<DBColumnInfo> schemaDetails = DBTable.ParseColumns(tableElement.GetProperty("columns"));
                            if (embeddedSchemas.ContainsKey(schemaName))
                            {
                                Logger.Critical?.WriteLine("[RookDB] Embedded list schema has duplicate name. Name=" + schemaName);
                                continue;
                            }
                            embeddedSchemas.Add(schemaName, schemaDetails);
                        }
                        else
                            deferredTables.Add(tableElement);
                    }
                }
                foreach (JsonElement tableElement in deferredTables)
                {
                    DBTable newTable = new DBTable(tableElement, this);
                    tables.Add(newTable.identifier, newTable);
                }
            }
            catch (Exception e)
            {
                Logger.Critical?.WriteLine("[RookDB] Critical internal error occured during database parsing, details: " + e.ToString());
                filename = INVALID_DB_STR;
                return;
            }
        }

        public bool IsValid()
        {
            return filename == INVALID_DB_STR;
        }

        //API READING
        public DBTable GetTable(string path)
        { 
            string[] pathParts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (TableExists(pathParts[0]))
            {
                return tables[pathParts[0]];
            }
            return null;
        }

        public DBColumnInfo GetColumnInfo(string path)
        { 
            string[] pathParts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (pathParts.Length != 2 || !ColumnExists(path))
            {
                return null;
            }
            foreach (DBColumnInfo column in tables[pathParts[0]].columns)
            {
                if (column.columnIdenfier == pathParts[1])
                    return column;
            }
            return null;
        }

        public DBRecord GetRecord(string path)
        { 
            string[] pathParts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (pathParts.Length != 2 || !RecordExists(path))
            {
                return null;
            }
            return tables[pathParts[0]].records[pathParts[1]];
        }

        public object GetField(string path)
        { 
            string[] pathParts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (pathParts.Length != 3 || !RecordExists(pathParts[0] + "/" + pathParts[1]) || !ColumnExists(pathParts[0] + "/" + pathParts[2]))
            {
                return null;
            }
            int fieldIdx = 0;
            for (; fieldIdx < tables[pathParts[0]].columns.Count; fieldIdx++)
            {
                if (tables[pathParts[0]].columns[fieldIdx].columnIdenfier == pathParts[2])
                    return tables[pathParts[0]].records[pathParts[1]].values[fieldIdx];
            }
            return null;
        }
        public T GetField<T>(string path)
        { 
            object field = GetField(path);
            if (field != null && typeof(T) == field.GetType())
                return (T)field;
            return default(T);
        }

        //API WRITE (add/remove)
        public void AddTable(string ident, DBColumnInfo[] schema)
        { 
            if (TableExists(ident))
            {
                Logger.Info?.WriteLine("[RookDB] Unable to add table, duplicate table name already exists. Name=" + ident);
                return;
            }
            DBTable table = new DBTable(new List<DBColumnInfo>(schema), ident);
            table.ownerDB = this;
            tables.Add(ident, table);
        }
        public void UpdateTable(string ident, DBColumnInfo[] schema, DBRecord[] records)
        { 
            if (!TableExists(ident))
            {
                Logger.Info?.WriteLine("[RookDB] Unable to update table data, table does not exist. Name=" + ident);
                return;
            }
            DBTable tempTable = new DBTable(new List<DBColumnInfo>(schema), ident);
            tempTable.ownerDB = this;
            foreach (DBRecord rec in records)
            {
                if (rec.values.Length == schema.Length)
                {
                    rec.ownerTable = tempTable;
                    tempTable.records.Add(rec.identifier, rec);
                }
            }
            tables.Remove(ident);
            tables.Add(ident, tempTable);
        }
        public void RemoveTable(string ident)
        { 
            if (!TableExists(ident))
            {
                Logger.Info?.WriteLine("[RookDB] Table cannot be removed, it does not exist. Name=" + ident);
                return;
            }
            tables.Remove(ident);
        }
        public void AddColumn(string path, DBColumnInfo columnInfo)
        { 
            string[] pathParts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (pathParts.Length != 2 || columnInfo.columnIdenfier != pathParts[1])
            {
                Logger.Info?.WriteLine("[RookDB] Cannot add column, column name/format incorrect.");
                return;
            }
            if (ColumnExists(path))
            {
                Logger.Info?.WriteLine("[RookDB] Column already exists in this name by that name. Please check entered details.");
                return;
            }
            tables[pathParts[0]].columns.Add(columnInfo);
            foreach (DBRecord record in tables[pathParts[0]].records.Values)
            {
                object[] newVals = new object[record.values.Length + 1];
                Array.Copy(record.values, 0, newVals, 0, record.values.Length);
                newVals[record.values.Length] = GetDefaultFieldValue(columnInfo);
                record.values = newVals;
            }
        }
        public void UpdateColumn(string path, DBColumnInfo newColumnInfo)
        { throw new NotImplementedException(); }
        public void RemoveColumn(string path)
        { throw new NotImplementedException(); }
        public void AddRecord(string path, DBRecord record)
        { throw new NotImplementedException(); }
        public void UpdateRecord(string path, object[] newValues)
        { throw new NotImplementedException(); }
        public void RemoveRecord(string path)
        { throw new NotImplementedException(); }

        //API EXPLORE
        public bool TableExists(string ident)
        { 
            string[] pathParts = ident.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (pathParts.Length != 1)
            {
                return false; //no funky inputs here, table ID or gtfo
            }
            return tables.ContainsKey(pathParts[0]);
        }

        public bool ColumnExists(string path)
        { 
            string[] pathParts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (pathParts.Length != 2 || !TableExists(pathParts[0]))
            {
                return false;
            }
            foreach (DBColumnInfo column in tables[pathParts[0]].columns)
            {
                if (column.columnIdenfier == pathParts[1])
                    return true;
            }
            return false;
        }

        public bool RecordExists(string path)
        { 
            string[] pathParts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (pathParts.Length != 2 || !TableExists(pathParts[0]))
            {
                return false; //must be formatted as TABLE_ID/RECORD_ID and table must exist
            }
            return tables[pathParts[0]].records.ContainsKey(pathParts[1]);
        }

        public List<DBTable> GetTables()
        { 
            return new List<DBTable>(tables.Values); //thanks CLR runtime
        }

        public List<DBColumnInfo> GetColumns(string table)
        { 
            if (!TableExists(table))
                return new List<DBColumnInfo>(); //empty list
            return new List<DBColumnInfo>(tables[table].columns); //new list because we want to return a COPY, not a ref.
        }

        public List<DBRecord> GetRecords(string table)
        { 
            if (!TableExists(table))
                return new List<DBRecord>();
            return new List<DBRecord>(tables[table].records.Values);
        }

        public static string PrettyPrintColumn(DBColumnInfo column)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("identifier=");
            sb.Append(column.columnIdenfier);
            sb.Append(", type=");
            sb.Append(column.columnType.ToString());
            if (column.columnType == ColumnType.Enumeration || column.columnType == ColumnType.Flags)
            {
                //also display all available options
                sb.Append(", values=");
                for (int i = 0; i < column.columnMeta.Length; i++)
                {
                    if (i > 0)
                        sb.Append("|");
                    sb.Append((string)column.columnMeta[i]);
                }
            }
            else if (column.columnType == ColumnType.Reference)
            {
                sb.Append(", baseTable="); //this is the table that references will be directed to (cant know exact records, as those are stored on the records themselves)
                sb.Append((string)column.columnMeta[0]);
            }
            return sb.ToString();
        }

        public static string PrettyPrintRecord(DBRecord record)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("ID=");
            sb.Append(record.identifier);
            for (int i = 0; i < record.ownerTable.columns.Count; i++)
            {
                sb.Append(", ");
                sb.Append(PrettyPrintField(record, i));
            }
            return sb.ToString();
        }

        public static string PrettyPrintField(DBRecord record, int fieldIdx)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            DBColumnInfo columnInfo = record.ownerTable.columns[fieldIdx];
            sb.Append(columnInfo.columnIdenfier);
            sb.Append(" (");
            sb.Append(columnInfo.columnType);
            sb.Append(")=");
            sb.Append(PrettyPrintFieldValue(record, fieldIdx));
            return sb.ToString();
        }

        public static string PrettyPrintFieldValue(DBRecord record, int fieldIdx)
        {
            string rtnString = "";
            switch (record.ownerTable.columns[fieldIdx].columnType)
            {
                case ColumnType.Flags:
                    string[] flags = (string[])record.values[fieldIdx];
                    for (int i = 0; i < flags.Length; i++)
                    {
                        if (i > 0)
                            rtnString += " + ";
                        rtnString += flags[i];
                    }
                    return rtnString;
                case ColumnType.List:
                    return "{ListData}";
                case ColumnType.Color:
                    byte[] colArr = (byte[])record.values[fieldIdx];
                    rtnString += $"r{colArr[0]} g{colArr[1]} b{colArr[2]}";
                    if (colArr.Length == 4)
                        rtnString += " a{colArr[3]}";
                    return rtnString;
                case ColumnType.Reference:
                    return "{ " + (string)record.values[fieldIdx] + " }";
                default:
                    return record.values[fieldIdx].ToString();
            }
        }
        
        internal static object GetDefaultFieldValue(DBColumnInfo colInfo)
        {
            switch (colInfo.columnType)
            {
                case ColumnType.Boolean:
                    return false;
                case ColumnType.Color:
                    return new byte[] {0, 0, 0};
                case ColumnType.Enumeration:
                    return colInfo.columnMeta[0];
                case ColumnType.Flags:
                    return new string[0];
                case ColumnType.Float:
                    return 0f;
                case ColumnType.Integer:
                    return 0;
                case ColumnType.List:
                    return null;
                default:
                    return "";
            }
        }
    }
}