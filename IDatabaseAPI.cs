namespace RookDB
{
    interface IDatabaseAPI<SHEET, COLUMN, RECORD, FIELD>
    {
        bool SheetExists(RookPath path);
        bool ColumnExists(RookPath path);
        bool RecordExists(RookPath path);
        bool FieldHasValue(RookPath path);

        bool AddSheet(RookPath path, string identifier);
        bool AddColumn(RookPath path, string identifer, ColumnType type);
        bool AddRecord(RookPath path, string identifier);
        bool AddField(RookPath path, string identifier);

        void RemoveSheet(RookPath path);
        void RemoveColumn(RookPath path);
        void RemoveRecord(RookPath path);
        void RemoveFieldValue(RookPath path);

        SHEET GetSheet(RookPath path);
        COLUMN GetColumn(RookPath path);
        RECORD GetRecord(RookPath path);
        FIELD GetField(RookPath path);

        string GetVersionInfo();
    }
}