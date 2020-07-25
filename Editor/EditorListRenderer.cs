using System;
using System.Collections.Generic;
using Terminal.Gui;

namespace RookDB.Editor
{
    public class EditorListRenderer : IListDataSource
    {
        internal DBTable currTable;
        public int hOffset = 0;

        public EditorListRenderer(DBTable table)
        {
            currTable = table;
        }
        
        public bool IsMarked(int item)
        { return false; }

        public void SetMark(int item, bool value)
        {}

        public void Render(ListView container, ConsoleDriver driver, bool selected, int item, int col, int line, int width)
        {
            List<DBRecord> records = new List<DBRecord>(currTable.records.Values);
            int hOffset = EditorProgram.columnHeaders.hOffset;
            if (hOffset > currTable.columns.Count)
                hOffset = currTable.columns.Count - 1;
            if (hOffset < 0)
                hOffset = 0;

            string dispStr = records[item].identifier.PadRight(ColumnHeaderControl.COLUMN_WIDTH);
            for (int i = hOffset; i < currTable.columns.Count; i++)
            {
                if (dispStr.Length + ColumnHeaderControl.COLUMN_WIDTH >= width)
                    break;
                dispStr += StringHelper.LimitLength(
                    RookDB.PrettyPrintFieldValue(records[item], i), 
                    ColumnHeaderControl.COLUMN_WIDTH, 
                    " .. ").PadRight(ColumnHeaderControl.COLUMN_WIDTH);
            }

            driver.AddStr(dispStr);
        }

        public int Count
        {
            get { return currTable == null ? 0 : currTable.records.Count; }
        }
    }
}