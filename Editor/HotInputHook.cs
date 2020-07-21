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
            if (keyEvent.Key == Key.CursorLeft)
            {
                EditorProgram.columnHeaders.hOffset -= 1;
            }
            else if (keyEvent.Key == Key.CursorRight)
            {
                EditorProgram.columnHeaders.hOffset += 1;
            }
            return false;
        }
    }
}