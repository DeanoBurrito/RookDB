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
                foreach (JsonElement tableElement in jdoc.RootElement.GetProperty("sheets").EnumerateArray())
                {
                    DBTable newTable = new DBTable(tableElement);
                    newTable.ownerDB = this;
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

        //API READING
        //Regular methods throw exceptions on error. Might make the API bloated, but it provides options for just priting the error or handling the specific case.
        public DBTable GetTable(string path)
        { throw new NotImplementedException(); }
        public DBTable GetTableOrNull(string path)
        { throw new NotImplementedException(); }
        public DBColumnInfo GetColumnInfo(string path)
        { throw new NotImplementedException(); }
        public DBColumnInfo GetColumnInfoOrNull(string path)
        { throw new NotImplementedException(); }
        public DBRecord GetRecord(string path)
        { throw new NotImplementedException(); }
        public DBRecord GetRecordOrNull(string path)
        { throw new NotImplementedException(); }
        public T GetRecord<T>(string path)
        { throw new NotImplementedException(); }
        public T GetRecordOrNull<T>(string path)
        { throw new NotImplementedException(); }

        //API WRITE (add/remove)
        public void AddTable(string ident, DBColumnInfo[] schema)
        { throw new NotImplementedException(); }
        public void RemoveTable(string ident)
        { throw new NotImplementedException(); }
        public void AddColumn(string path, DBColumnInfo columnInfo)
        { throw new NotImplementedException(); }
        public void RemoveColumn(string path)
        { throw new NotImplementedException(); }
        public void AddRecord(string path, DBRecord record)
        { throw new NotImplementedException(); }
        public void RemoveRecord(string path)
        { throw new NotImplementedException(); }

        //API EXPLORE
        public bool TableExists(string ident)
        { throw new NotImplementedException(); }
        public bool ColumnExists(string path)
        { throw new NotImplementedException(); }
        public bool RecordExists(string path)
        { throw new NotImplementedException(); }
        public List<DBTable> GetTables()
        { throw new NotImplementedException(); }
        public List<DBColumnInfo> GetColumns(string table)
        { throw new NotImplementedException(); }
        public List<DBRecord> GetRecords(string path)
        { throw new NotImplementedException(); }

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
            sb.Append("identifier=");
            sb.Append(record.identifier);
            for (int i = 0; i < record.ownerTable.columns.Count;)
            {
                sb.Append(",");
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
                    rtnString += "r{colArr[0]} g{colArr[1]} b{colArr[2]}";
                    if (colArr.Length == 4)
                        rtnString += " a{colArr[3]}";
                    return rtnString;
                default:
                    return record.values[fieldIdx].ToString();
            }
        }
    }
}