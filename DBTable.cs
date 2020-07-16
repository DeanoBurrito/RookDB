using System;
using System.Collections.Generic;
using System.Text.Json;
using CliMod;

namespace RookDB
{
    public sealed class DBTable
    {
        public RookDB ownerDB;

        public string identifier;
        internal List<DBColumnInfo> columns = new List<DBColumnInfo>();
        internal Dictionary<string, DBRecord> records = new Dictionary<string, DBRecord>();

        internal DBTable(JsonElement tableBase)
        {
            ParseWholeText(tableBase);
        }
        
        internal DBTable(List<DBColumnInfo> schema, string name)
        {
            columns = schema;
            identifier = name;
        }

        private void ParseWholeText(JsonElement baseElement)
        {
            if (baseElement.TryGetProperty("name", out JsonElement nameProp))
            {
                identifier = nameProp.GetString();
            }
            else
            {
                Logger.Debug?.WriteLine("[RookDB] database table has no name property!");
                identifier = "_unnamed_" + new Random().Next().ToString() + "_";
            }

            if (!baseElement.TryGetProperty("columns", out _))
            {
                Logger.Critical?.WriteLine("[RookDB] Database table has no column information. Cannot load without schema.");
                return;
            }
            if (!baseElement.TryGetProperty("lines", out _))
            {
                Logger.Critical?.WriteLine("[RookDB] Database table has no records (not even a node), an error must have occured during export/write. Cannot load without schema.");
                return;
            }

            List<DBColumnInfo> columnsInfos = ParseColumns(baseElement.GetProperty("columns"));
            foreach (DBColumnInfo col in columnsInfos)
            {
                col.ownerTable = this;
                columns.Add(col);
            }
            foreach (JsonElement recordElement in baseElement.GetProperty("lines").EnumerateArray())
            {
                DBRecord rec = ParseRecord(recordElement, columns);
                if (records.ContainsKey(rec.identifier))
                {
                    Logger.Critical?.WriteLine("[RookDB] Record has duplicated unique identifier, ignoring current record. UID=" + rec.identifier);
                    continue;
                }
                rec.ownerTable = this;
                records.Add(rec.identifier, rec);
            }
            
            Logger.Debug?.WriteLine("[RookDB] Loaded table with " + columns.Count + " columns.");
        }

        internal static List<DBColumnInfo> ParseColumns(JsonElement baseElement)
        {
            List<DBColumnInfo> rtnInfos = new List<DBColumnInfo>();
            foreach (JsonElement columnElement in baseElement.EnumerateArray())
            {
                DBColumnInfo cInfo = new DBColumnInfo();
                cInfo.columnIdenfier = columnElement.GetProperty("name").GetString();
                string typeStr = columnElement.GetProperty("typeStr").GetString();
                if (typeStr.Contains(":"))
                {
                    //extract and parse meta, process new typeStr as wells
                    string[] splitStr = typeStr.Split(":");
                    typeStr = splitStr[0];
                    cInfo.columnMeta = splitStr[1].Split(',');
                }
                byte typeNum = byte.Parse(typeStr);
                if (!Enum.IsDefined(typeof(ColumnType), typeNum))
                {
                    Logger.Critical?.WriteLine("[RookDB] Attempted to parsed column with type " + typeStr + ". This value is not defined, ignoring column.");
                    continue;
                }
                cInfo.columnType = (ColumnType)typeNum;
                rtnInfos.Add(cInfo);
            }
            return rtnInfos;
        }

        internal static DBRecord ParseRecord(JsonElement baseElement, List<DBColumnInfo> columns)
        {
            string recordIdent = "";
            List<object> values = new List<object>();
            foreach (DBColumnInfo column in columns)
            {
                switch (column.columnType)
                {
                    case ColumnType.UniqueIdentifier:
                        if (baseElement.TryGetProperty(column.columnIdenfier, out JsonElement uidChild))
                        { 
                            recordIdent = uidChild.GetString(); 
                            values.Add(recordIdent);
                        }
                        else
                        { throw new Exception("[RookDB] Cannot have a UniqueIdentifier with no value! (JSON property not found via column name.)"); }
                        break;
                    case ColumnType.Boolean:
                        if (baseElement.TryGetProperty(column.columnIdenfier, out JsonElement boolChild))
                        { values.Add(boolChild.GetBoolean()); }
                        else
                        { values.Add(RookDB.GetDefaultFieldValue(column)); }
                        break;
                    case ColumnType.Color:
                        if (baseElement.TryGetProperty(column.columnIdenfier, out JsonElement colorChild))
                        {
                            //read int, force to 6 digit wide hex representation, parse pairs of characters as bytes (r/g/b values).
                            string cHex = colorChild.GetInt32().ToString("X6");
                            values.Add(new byte[] 
                            {
                                Convert.ToByte(cHex.Substring(0, 2), 16),
                                Convert.ToByte(cHex.Substring(2, 2), 16),
                                Convert.ToByte(cHex.Substring(4, 2), 16)
                            });
                        }
                        else
                        { values.Add(RookDB.GetDefaultFieldValue(column)); }
                        break;
                    case ColumnType.Enumeration:
                        if (baseElement.TryGetProperty(column.columnIdenfier, out JsonElement enumChild) || enumChild.GetInt32() >= column.columnMeta.Length)
                        { values.Add(column.columnMeta[enumChild.GetInt32()]); }
                        else
                        { values.Add(RookDB.GetDefaultFieldValue(column)); }
                        break;
                    case ColumnType.Flags:
                        if (baseElement.TryGetProperty(column.columnIdenfier, out JsonElement flagsChild))
                        { 
                            List<string> claimed = new List<string>();
                            uint flagData = flagsChild.GetUInt32();
                            for (int shiftCount = 0; shiftCount < 32 && shiftCount < column.columnMeta.Length; shiftCount++)
                            {
                                if ((flagData & (1 << shiftCount)) != 0)
                                { claimed.Add(column.columnMeta[shiftCount]); }
                            }
                            values.Add(claimed.ToArray());
                        }
                        else
                        { values.Add(RookDB.GetDefaultFieldValue(column)); }
                        break;
                    case ColumnType.Float:
                        if (baseElement.TryGetProperty(column.columnIdenfier, out JsonElement floatChild))
                        { values.Add(floatChild.GetSingle()); }
                        else
                        { values.Add(RookDB.GetDefaultFieldValue(column)); }
                        break;
                    case ColumnType.Integer:
                        if (baseElement.TryGetProperty(column.columnIdenfier, out JsonElement intChild))
                        { values.Add(intChild.GetInt32()); }
                        else
                        { values.Add(RookDB.GetDefaultFieldValue(column)); }
                        break;
                    case ColumnType.List:
                        if (baseElement.TryGetProperty(column.columnIdenfier, out JsonElement listElement))
                        { values.Add(""); }
                        else
                        { values.Add(RookDB.GetDefaultFieldValue(column)); }
                        break;
                    case ColumnType.Reference:
                        if (baseElement.TryGetProperty(column.columnIdenfier, out JsonElement refChild))
                        { values.Add(column.columnMeta[0] + "/" + refChild.GetString()); }
                        else
                        { values.Add(RookDB.GetDefaultFieldValue(column)); }
                        break;
                    case ColumnType.Text:
                        if (baseElement.TryGetProperty(column.columnIdenfier, out JsonElement textChild))
                        { values.Add(textChild.GetString()); }
                        else
                        { values.Add(RookDB.GetDefaultFieldValue(column)); }
                        break;
                }
            }
            return new DBRecord(recordIdent, values.ToArray());
        }
    }
}