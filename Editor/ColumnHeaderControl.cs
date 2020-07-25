using System;
using System.Collections.Generic;
using Terminal.Gui;

namespace RookDB.Editor
{
    public class ColumnHeaderControl : View
    {
        public const int COLUMN_WIDTH = 20;
        public const int COL_NAME_WIDTH = 13;
        
        public int hOffset = 0;
        DBColumnInfo[] columns;
        
        public ColumnHeaderControl()
        {

        }

        public void UpdateColumns(DBColumnInfo[] cols)
        {
            columns = cols;
        }

        public override void Redraw(Rect region)
        {
            if (columns == null)
                return;
            if (hOffset >= columns.Length)
                hOffset = columns.Length - 1;
            if (hOffset < 0)
                hOffset = 0;
            
            PositionCursor();
            int lineLen = 0;
            for (int i = hOffset - 1; i < columns.Length; i++)
            {
                if (lineLen + COLUMN_WIDTH >= region.Width)
                    break; //if drawing another line would overrun the current one, dont draw any more.
                
                lineLen += COLUMN_WIDTH;
                if (i == hOffset - 1)
                {
                    Driver.AddStr("Record ID".PadRight(COLUMN_WIDTH));
                }
                else
                {
                    string printStr = StringHelper.LimitLength(columns[i].columnIdenfier, COL_NAME_WIDTH, "") + " (" + GetShortColumnName(columns[i].columnType) + ")";
                    printStr = printStr.PadRight(COLUMN_WIDTH);
                    Driver.AddStr(printStr);
                }
            }
        }

        private string GetShortColumnName(ColumnType type)
        {
            switch (type)
            {
                case ColumnType.Boolean:
                    return "Bool";
                case ColumnType.Color:
                    return "Colr";
                case ColumnType.Enumeration:
                    return "Enum";
                case ColumnType.Flags:
                    return "Flag";
                case ColumnType.Float:
                    return "FltP";
                case ColumnType.Integer:
                    return "Int";
                case ColumnType.List:
                    return "List";
                case ColumnType.Reference:
                    return "Ref";
                case ColumnType.Text:
                    return "Text";
                case ColumnType.UniqueIdentifier:
                    return "UID";
            }
            throw new Exception("Wat.");
        }
    }
}