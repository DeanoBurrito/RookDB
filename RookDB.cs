using System;
using System.Collections.Generic;

namespace RookDB
{
    public sealed class RookDB
    {
        public static RookDB LoadFile(string filename, RookDBSettings settings)
        { throw new NotImplementedException(); }

        public static RookDB LoadString(string data, RookDBSettings settings)
        { throw new NotImplementedException(); }

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