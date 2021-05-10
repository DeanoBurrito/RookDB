using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text.Json;

namespace RookDB
{
    public sealed class ReadonlyDB : IDatabaseAPI<ReadonlySheet, ReadonlyColumn, ReadonlyRecord, object>
    {
        public static ReadonlyDB LoadString(string data)
        {
            try
            {
                JsonDocument jdoc = JsonDocument.Parse(data);
                JsonElement sheetsElement = jdoc.RootElement.GetProperty("sheets");

                var sheetsBuilder = ImmutableArray.CreateBuilder<ReadonlySheet>();
                foreach (JsonElement sheet in sheetsElement.EnumerateArray())
                {
                    sheetsBuilder.Add(LoadSheet(sheet));
                }

                return new ReadonlyDB(sheetsBuilder.ToImmutable());
            }
            catch (Exception e)
            {
                Console.WriteLine("RookDB Error: " + e.ToString());
                return new ReadonlyDB(ImmutableArray.Create<ReadonlySheet>());
            }
        }

        public static ReadonlyDB LoadFile(string filename)
        {
            if (!File.Exists(filename))
                return new ReadonlyDB(ImmutableArray.Create<ReadonlySheet>());
            
            string text = File.ReadAllText(filename);
            return LoadString(text);
        }

        private static ReadonlySheet LoadSheet(JsonElement root)
        {
            string sheetName = root.GetProperty("name").GetString();

            var columnsBuilder = ImmutableArray.CreateBuilder<ReadonlyColumn>();
            var recordsBuilder = ImmutableArray.CreateBuilder<ReadonlyRecord>();
            var fieldsBuilder = ImmutableArray.CreateBuilder<object>();

            uint columnIndex = 0;
            foreach (JsonElement column in root.GetProperty("columns").EnumerateArray())
            {
                columnsBuilder.Add(LoadColumn(column, columnIndex));
                columnIndex++;
            }

            uint recordIndex = 0;
            foreach (JsonElement record in root.GetProperty("lines").EnumerateArray())
            {
                var loadedRecord = LoadRecord(record, recordIndex, columnsBuilder);
                recordsBuilder.Add(loadedRecord.record);
                recordIndex++;

                fieldsBuilder.AddRange(loadedRecord.fields);
            }
            
            bool visible = true;
            if (root.TryGetProperty("props", out JsonElement propProps))
            {
                //check sheet properties (currently just visibility)
                if (propProps.TryGetProperty("hide", out JsonElement propHide))
                    visible = !propHide.GetBoolean();
            }

            return new ReadonlySheet(sheetName, columnsBuilder.ToImmutable(), recordsBuilder.ToImmutable(), fieldsBuilder.ToImmutable(), visible);
        }

        private static ReadonlyColumn LoadColumn(JsonElement root, uint columnOffset)
        {
            if (!root.TryGetProperty("typeStr", out JsonElement propTypeStr))
                throw new Exception("Column missing type string.");
            if (!root.TryGetProperty("name", out JsonElement propName))
                throw new Exception("Column missing name.");
            
            bool isOptional = false;
            if (root.TryGetProperty("opt", out JsonElement propOpt))
                isOptional = propOpt.GetBoolean();
            
            ColumnType type = ColumnType.ErrorValue;
            string columnMeta = propTypeStr.GetString();
            var metadataBuilder = ImmutableArray.CreateBuilder<string>();

            if (columnMeta.Contains(':'))
            {
                string[] split = columnMeta.Split(':');
                type = (ColumnType)int.Parse(split[0]);
                columnMeta = split[1];

                split = columnMeta.Split(',');
                Array.Copy(split, 1, split, 0, split.Length - 1); //trim first element
                metadataBuilder.AddRange(split);
            }
            else
                type = (ColumnType)int.Parse(columnMeta);

            return new ReadonlyColumn(propName.GetString(), type, isOptional, metadataBuilder.ToImmutable(), columnOffset);
        }

        private static (ReadonlyRecord record, List<object> fields) LoadRecord(JsonElement root, uint recordOffset, ImmutableArray<ReadonlyColumn>.Builder columns)
        {
            List<object> fields = new List<object>();
            string name = null;
            for (int colIndex = 0; colIndex < columns.Count; colIndex++)
            {
                object fieldValue = null;
                if (!root.TryGetProperty(columns[colIndex].identifier, out JsonElement propField))
                {
                    if (columns[colIndex].isOptional)
                    {
                        object fieldIntermediateValue = RookDB.GetColumnDefaultValue(columns[colIndex].type);
                        fieldValue = RookDB.ParseField(columns[colIndex].type, fieldIntermediateValue);
                    }
                    else
                        throw new Exception("Record did not contain a required field.");
                }
                else
                {
                    object fieldRaw;
                    switch (columns[colIndex].type)
                    {
                        case ColumnType.UniqueIdentifier:
                            fieldRaw = propField.GetString();
                            name = (string)fieldRaw;
                            break;
                        case ColumnType.Text:
                            fieldRaw = propField.GetString(); break;
                        case ColumnType.Boolean:
                            fieldRaw = propField.GetBoolean(); break;
                        case ColumnType.Integer:
                            fieldRaw = propField.GetInt32(); break;
                        case ColumnType.Float:
                            fieldRaw = propField.GetSingle(); break;
                        case ColumnType.Enumeration:
                            fieldRaw = propField.GetUInt32(); break;
                        case ColumnType.Reference:
                            fieldRaw = propField.GetString(); break;
                        case ColumnType.List:
                            throw new NotImplementedException(); //BOOOOOOOM
                        case ColumnType.Flags:
                            fieldRaw = propField.GetUInt32(); break;
                        case ColumnType.Color:
                            fieldRaw = propField.GetUInt32(); break;
                    }
                }
                
                fields.Add(fieldValue);
            }

            if (name == null)
                throw new Exception("Record did not have a uniqueidentifier field to use at its key.");

            return (new ReadonlyRecord(name, recordOffset), fields);
        }

        private readonly Dictionary<string, ulong> userLookups = new Dictionary<string, ulong>();
        private readonly List<object> userMetadata = new List<object>();

        private readonly ImmutableArray<ReadonlySheet> sheets;

        private ReadonlyDB(ImmutableArray<ReadonlySheet> sheets)
        { 
            this.sheets = sheets;
        }

        public bool MetaExists(RookPath path)
        {
            if (path.id.HasValue 
                && path.id < (ulong)userMetadata.Count 
                && path.isMeta)
                return true;

            //NOTE: userlookups is never checked, nor is it checked if the value stored is null.
            return false;
        }

        public void SetMeta(ref RookPath path, object meta)
        {
            if (MetaExists(path))
                userMetadata[(int)path.id.Value] = meta;
            else
            {
                //modify the path so it contains the data required to access the stored user object
                path.isMeta = true;
                path.id = (uint)userMetadata.Count;

                userMetadata.Add(meta);
                userLookups.Add(path.path, path.id.Value);
            }
        }

        public object GetMeta(RookPath path)
        {
            if (!MetaExists(path))
                return null;
            
            if (!path.id.HasValue)
            {
                try
                {
                    return userMetadata[(int)userLookups[path.path]];
                }
                catch (Exception)
                {
                    return null;
                }
            }
            else if (path.isMeta)
            {
                return userMetadata[(int)path.id.Value];
            }
            
            return null;
        }

        public string GetVersionInfo()
        {
            return "RookDB 2.0.0 dev-branch";
        }

        public bool SheetExists(RookPath path)
        { throw new NotImplementedException(); }

        public bool ColumnExists(RookPath path)
        { throw new NotImplementedException(); }

        public bool RecordExists(RookPath path)
        { throw new NotImplementedException(); }

        public bool FieldHasValue(RookPath path)
        { throw new NotImplementedException(); }

        public ReadonlySheet GetSheet(RookPath path)
        { throw new NotImplementedException(); }
        public ReadonlyColumn GetColumn(RookPath path)
        { throw new NotImplementedException(); }
        public ReadonlyRecord GetRecord(RookPath path)
        { throw new NotImplementedException(); }
        public object GetField(RookPath path)
        { throw new NotImplementedException(); }


        public bool AddSheet(RookPath path, string identifier)
        { throw new NotImplementedException("This feature is not implemented in a readonly database."); }
        public bool AddColumn(RookPath path, string identifer, ColumnType type)
        { throw new NotImplementedException("This feature is not implemented in a readonly database."); }
        public bool AddRecord(RookPath path, string identifier)
        { throw new NotImplementedException("This feature is not implemented in a readonly database."); }
        public bool AddField(RookPath path, string identifier)
        { throw new NotImplementedException("This feature is not implemented in a readonly database."); }
        public void RemoveSheet(RookPath path)
        { throw new NotImplementedException("This feature is not implemented in a readonly database."); }
        public void RemoveColumn(RookPath path)
        { throw new NotImplementedException("This feature is not implemented in a readonly database."); }
        public void RemoveRecord(RookPath path)
        { throw new NotImplementedException("This feature is not implemented in a readonly database."); }
        public void RemoveFieldValue(RookPath path)
        { throw new NotImplementedException("This feature is not implemented in a readonly database."); }
    }
}