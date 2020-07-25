using System;
using Terminal.Gui;

namespace RookDB.Editor
{
    public class HotInputHook : View
    {
        public HotInputHook()
        {}

        public override bool ProcessHotKey(KeyEvent keyEvent)
        {
            //for horizontal DBColumn scrolling
            if (keyEvent.Key == Key.CursorLeft)
            {
                EditorProgram.columnHeaders.hOffset -= 1;
            }
            else if (keyEvent.Key == Key.CursorRight)
            {
                EditorProgram.columnHeaders.hOffset += 1;
            }

            //for switching tables
            if (keyEvent.Key == Key.PageDown)
            {
                EditorProgram.SelNextTable();
            }
            if (keyEvent.Key == Key.PageUp)
            {
                EditorProgram.SelPrevTable();
            }

            //for editing selected values
            if (keyEvent.Key == Key.Enter)
            {
                
            }
            if (keyEvent.Key == Key.Space)
            {
                //select which field to edit on current record
            }

            if (keyEvent.Key == Key.F1)
            {
                EditorProgram.ShowHelp();
            }

            return false;
        }
    }
}