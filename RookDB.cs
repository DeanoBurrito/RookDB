using System;
using System.Collections.Immutable;
using System.IO;
using System.Text.Json;

namespace RookDB
{
    public sealed class RookDB
    {
        public static RookDB LoadFile(string filename, RookDBSettings settings)
        {
            if (!File.Exists(filename))
            {
                Console.Error.WriteLine("Could not load database, file does not exist: " + filename);
                return null;
            }

            string fileData = File.ReadAllText(filename);
            return LoadString(fileData, settings);
        }

        public static RookDB LoadString(string data, RookDBSettings settings)
        {
            RookDB database = new RookDB(settings);
            try 
            {
                JsonDocument jdoc = JsonDocument.Parse(data);
                JsonElement sheetArray = jdoc.RootElement.GetProperty("sheets"); //TODO: this should be a if + try

                foreach (JsonElement sheet in sheetArray.EnumerateArray())
                {
                    LoadSheet(sheet, database);
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Error occured during database read: " + e.ToString());
                return null;
            }
            return database;
        }

        private static void LoadSheet(JsonElement root, RookDB database)
        {
            RookSheet sheet = new RookSheet();
            string name;

            if (root.TryGetProperty("name", out JsonElement propSheetName))
                name = propSheetName.GetString();

            if (root.TryGetProperty("columns", out JsonElement propColumns))
            {
                var columnsBuilder = ImmutableArray.CreateBuilder<RookColumn>();

                foreach (JsonElement column in propColumns.EnumerateArray())
                    columnsBuilder.Add(LoadColumn(column, sheet));
                
                sheet.columns = columnsBuilder.ToImmutable();
            }

            if (root.TryGetProperty("lines", out JsonElement propLines))
            {
                var recordsBuilder = ImmutableArray.CreateBuilder<RookRecord>();

                foreach (JsonElement record in propLines.EnumerateArray())
                    recordsBuilder.Add(LoadRecord(record, sheet));
                
                sheet.records = recordsBuilder.ToImmutable();
            }
        }

        private static RookColumn LoadColumn(JsonElement root, RookSheet sheet)
        {
            ColumnType columnType = ColumnType.ErrorValue;
            string columnMetadata = string.Empty;
            string name = string.Empty;
            bool isOptional = false;

            if (root.TryGetProperty("typeStr", out JsonElement propTypeStr))
            {
                string typeStr = propTypeStr.GetString();
                if (typeStr.Contains(":"))
                {
                    typeStr = typeStr.Split(':')[0]
                    columnMetadata = typeStr.Split(':')[1];
                }

                columnType = (ColumnType)int.Parse(typeStr);
            }

            if (root.TryGetProperty("name", out JsonElement propName))
                name = propName.GetString();
            
            if (root.TryGetProperty("opt", out JsonElement propOptional))
                isOptional = propOptional.GetBoolean();

            if (columnType == ColumnType.ErrorValue || name == string.Empty)
                throw new Exception("Error reading column " + name + ".");
            return new RookColumn(sheet, name, columnType, isOptional);
        }

        private static RookRecord LoadRecord(JsonElement root, RookSheet sheet)
        {
            int fieldsCount = sheet.columns.Length;
            var fieldsBiulder = ImmutableArray.CreateBuilder<RookField>(fieldsCount);
            int populatedFieldCount = 0;
            string name = string.Empty;

            if (!root.TryGetProperty(sheet.columns[0].identifier, out JsonElement nameProp))
                throw new Exception("Record has no name field/Unique identifier!");
            name = nameProp.GetString();
            RookRecord record = new RookRecord(sheet, name);

            foreach (RookColumn col in sheet.columns)
            {
                //populate the field
                populatedFieldCount++;
            }
            
            //get the uid that is the records name, so we can create the record
            //now we can go through and create all the fields, refering this record correctly
            //including the field that holds the name.
            //add fields to record, and return it all.
        }

        public static string SaveFile(RookDB db, string filename, bool overwriteInplace = false)
        { throw new NotImplementedException(); }

        public static string SaveString(RookDB db)
        { throw new NotImplementedException(); }

        public static RookDB CreateEmpty(RookDBSettings settings)
        { throw new NotImplementedException(); }

        internal string filename;
        private WriteCache writeCache;
        private ReadCache readCache;
        private RookDBSettings settings;

        private RookDB(RookDBSettings settings)
        { }

        public bool PathExists(RookPath path)
        { throw new NotImplementedException(); }

        public bool SheetExists(RookPath path)
        { throw new NotImplementedException(); }

        public bool ColumnExists(RookPath path)
        { throw new NotImplementedException(); }

        public bool RecordExists(RookPath path)
        { throw new NotImplementedException(); }

        public bool FieldExists(RookPath path)
        { throw new NotImplementedException(); }

        public RookSheet GetSheet(RookPath path)
        { throw new NotImplementedException(); }

        public RookColumn GetColumn(RookPath path)
        { throw new NotImplementedException(); }

        public RookRecord GetRecord(RookPath path)
        { throw new NotImplementedException(); }

        public RookField GetField(RookPath path)
        { throw new NotImplementedException(); }

        public void SetSheet(RookSheet sheet)
        { throw new NotImplementedException(); }

        public void SetColumn(RookColumn column)
        { throw new NotImplementedException(); }

        public void SetRecord(RookRecord record)
        { throw new NotImplementedException(); }

        public void CreateSheet(RookPath path, int index = -1)
        { throw new NotImplementedException(); }

        public void CreateColumn(RookPath path, int index = -1)
        { throw new NotImplementedException(); }

        public void CreateRecord(RookPath path, int index = -1)
        { throw new NotImplementedException(); }

        public void DeleteSheet(RookSheet sheet)
        { throw new NotImplementedException(); }

        public void DeleteColumn(RookColumn column)
        { throw new NotImplementedException(); }

        public void DeleteRecord(RookRecord record)
        { throw new NotImplementedException(); }

        internal static object CastFieldValue(RookField field)
        { throw new NotImplementedException(); }

        internal static object GetDefaultColumnValue(ColumnType type)
        { throw new NotImplementedException(); }
    }
}