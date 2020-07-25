using System;
using System.Collections.Generic;
using System.Linq;
using Terminal.Gui;

namespace RookDB.Editor
{
    public class EditorProgram
    {
        static Window mainWin;
        static Label statusLine;
        static Label tablesLine;
        static ListView currTableView;
        internal static ColumnHeaderControl columnHeaders;
        static RookDB currDB;
        
        //https://github.com/migueldeicaza/gui.cs
        public static void RunEditor(string initialFile = null)
        {
            Application.Init();
            mainWin = new Window("RookDB Terminal Editor - empty")
            {
                X = 0,
                Y = 1, 
                Width = Dim.Fill(),
                Height = Dim.Fill(1),
            };
            Application.Top.Add(mainWin);
            currTableView = new ListView(new EditorListRenderer(null)) 
            {
                Y = 1,
                Width = Dim.Fill(),
                Height = Dim.Fill(1),

            };
            mainWin.Add(currTableView);
            columnHeaders = new ColumnHeaderControl() 
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = 1,
            };
            mainWin.Add(columnHeaders);
            tablesLine = new Label("Example Text") 
            {
                X = 1,
                Y = Pos.AnchorEnd(1),
                Width = Dim.Fill(2),
                Height = 1,
            };
            mainWin.Add(tablesLine);

            statusLine = new Label("") 
            {
                X = 0,
                Y = Pos.Bottom(Application.Top) - 1,
                Width = Dim.Fill(),
                Height = 1,
                TextColor = new Terminal.Gui.Attribute(Color.BrightYellow, Color.Black),
                Text = "Loading..."
            }; 
            Application.Top.Add(new HotInputHook()); //input hook for scrolling column headers without them being focused
            Application.Top.Add(statusLine);
            
            MenuBar mainMenu = new MenuBar(new MenuBarItem[] 
            {
                new MenuBarItem("_File", new MenuItem[] 
                {
                    new MenuItem("_New", "", CreateNew),
                    new MenuItem("_Load", "", LoadFile),
                    new MenuItem("_Save", "", SaveFile),
                    new MenuItem("S_ave As", "", SaveFileAs),
                    new MenuItem("_Quit", "", Quit),
                }),
                new MenuBarItem("_Add/Delete", new MenuItem[] 
                {
                    new MenuItem("Add Table", "", AddTable),
                    new MenuItem("Delete Table", "", DeleteTable),
                    new MenuItem("Add Column", "", AddColumn),
                    new MenuItem("Delete Column", "", DeleteColumn),
                    new MenuItem("Add Record", "", AddRecord),
                    new MenuItem("Delete Record", "", DeleteRecord),
                }),
                new MenuBarItem("_Edit", new MenuItem[] 
                {
                    new MenuItem("Rename Table", "", RenameTable),
                    new MenuItem("Edit Column", "", EditColumn),
                    new MenuItem("Change Column Order", "", ChangeColumnOrder)
                }),
                new MenuBarItem("_Options", new MenuItem[] 
                {
                    new MenuItem("_Settings", "", null),
                    new MenuItem("_Help", "", ShowHelp),
                    new MenuItem("_About", "", ShowAbout),
                }),
            });
            Application.Top.Add(mainMenu);
            if (initialFile != null && System.IO.File.Exists(initialFile))
            {
                currDB = new RookDB(System.IO.File.ReadAllText(initialFile), initialFile);
                if (!currDB.IsValid())
                    currDB = null;
            }

            PrintStatus("Editor loaded", StatusLevel.Default);
            Application.Run();
        }

        public static void SelNextTable()
        {
            if (currDB == null)
                return;
            List<DBTable> allTables = new List<DBTable>(currDB.tables.Values);
            int currIdx = allTables.IndexOf(((EditorListRenderer)currTableView.Source).currTable) + 1;
            if (currIdx >= allTables.Count)
                return; //we've reached the end of the tables
            currTableView.Source = new EditorListRenderer(allTables[currIdx]);
            columnHeaders.UpdateColumns(allTables[currIdx].columns.ToArray());
            UpdateTableDisplay(currIdx);
        }

        public static void SelPrevTable()
        {
            if (currDB == null)
                return;
            List<DBTable> allTables = new List<DBTable>(currDB.tables.Values);
            int currIdx = allTables.IndexOf(((EditorListRenderer)currTableView.Source).currTable) - 1;
            if (currIdx < 0)
                return; //no more tables that way
            currTableView.Source = new EditorListRenderer(allTables[currIdx]);
            columnHeaders.UpdateColumns(allTables[currIdx].columns.ToArray());
            UpdateTableDisplay(currIdx);
        }

        static void UpdateTableDisplay(int currIdx = -1)
        {
            List<DBTable> allTables = new List<DBTable>(currDB.tables.Values);
            if (currIdx == -1)
                currIdx = allTables.IndexOf(((EditorListRenderer)currTableView.Source).currTable);

            string newTL = "";
            for (int i = 0; i < allTables.Count; i++)
            {
                if (i == currIdx)
                    newTL += " [" + allTables[i].identifier + "] ";
                else
                    newTL += " " + allTables[i].identifier + " ";
            }
            tablesLine.Text = newTL;
        }

        static void CreateNew()
        {
            string dbName = "temp.cdb";
            Dialog getNameDialog = new Dialog("Enter new database name", 60, 10);
            TextField nameField = new TextField("") { X = 1, Y = 1, Width = Dim.Fill(), Height = 1 };
            Button confirmBtn = new Button("Confirm")
            {
                Clicked = () => 
                {
                    if (nameField.Text.Length > 5 && nameField.Text.EndsWith(".cdb"))
                    {
                        dbName = dbName = nameField.Text.ToString();
                        Application.RequestStop();
                    }
                }
            };
            getNameDialog.AddButton(confirmBtn);
            getNameDialog.Add(nameField);
            Application.Run(getNameDialog);

            currDB = new RookDB(dbName);
            PrintStatus("Created new database, unsaved.", StatusLevel.Warning);
            UpdateWindowName();
        }

        static void LoadFile()
        {
            OpenDialog openFileDialog = new OpenDialog("Select File", "Select a *.cdb file to open");
            openFileDialog.Height = Dim.Percent(70f);
            openFileDialog.Width = Dim.Percent(70f);
            openFileDialog.AllowedFileTypes = new string[] {".cdb"};
            openFileDialog.AllowsMultipleSelection = false;
            openFileDialog.CanChooseDirectories = false;
            openFileDialog.CanChooseFiles = true;
            Application.Run(openFileDialog);

            List<string> files = new List<string>(openFileDialog.FilePaths);
            if (files.Count < 1)
            {
                PrintStatus("No file was selected to open.", StatusLevel.Error);
                return;
            }
            RookDB loadedDB = new RookDB(System.IO.File.ReadAllText(files[0]), files[0]);
            if (loadedDB.IsValid())
            {
                currDB = loadedDB;
                currTableView.Source = new EditorListRenderer(currDB.tables.First().Value);
                columnHeaders.UpdateColumns(currDB.tables.First().Value.columns.ToArray());
                UpdateWindowName();
                UpdateTableDisplay();
                PrintStatus("Loaded database: " + loadedDB.filename, StatusLevel.Default);
            }
            else
            {
                PrintStatus("Could not load database, file was invalid.", StatusLevel.Error);
            }
        }

        static void SaveFile()
        {
            if (currDB == null)
            {
                PrintStatus("Couldn't save database, nothing loaded in memory.", StatusLevel.Error);
                return;
            }
            RookDB.WriteFile(currDB, currDB.filename, true);
            PrintStatus("Database saved successfully.", StatusLevel.Default);
        }

        static void SaveFileAs()
        {
            SaveDialog saveDialog = new SaveDialog("Select save location", "Select location and filename to save database as.");
            saveDialog.IsExtensionHidden = false;
            saveDialog.AllowedFileTypes = new string[] {"*.cdb"};
            Application.Run(saveDialog);
            if (saveDialog.FileName == null)
            {
                PrintStatus("Save As was cancelled by user.", StatusLevel.Warning);
                return;
            }
            string selPath = saveDialog.FilePath.ToString();
            if (!selPath.EndsWith(".cdb"))
                selPath += ".cdb";
            if (System.IO.File.Exists(selPath))
            {
                bool overwriteConfirmed = false;
                Dialog overwriteDialog = new Dialog("Confirm Overwrite", 20, 25);
                Label overwriteLabel = new Label("You are about to overwrite an existing file.\nContinue?");
                Button okBtn = new Button("Overwrite", true) { Clicked = () => { overwriteConfirmed = true; Application.RequestStop(); }};
                Button cancelBtn = new Button("Cancel") { Clicked = () => { Application.RequestStop(); }};
                overwriteDialog.AddButton(okBtn);
                overwriteDialog.AddButton(cancelBtn);
                overwriteDialog.Add(overwriteLabel);
                Application.Run(overwriteDialog);
                if (!overwriteConfirmed)
                    return;
            }
            RookDB.WriteFile(currDB, selPath, true);
            PrintStatus("Saved under alt file name: " + StringHelper.LimitLengthInverse(selPath, 50), StatusLevel.Default);
        }

        static void Quit()
        {
            Application.RequestStop();
        }

        static void AddTable()
        {

        }

        static void DeleteTable()
        {}

        static void AddColumn()
        {}

        static void DeleteColumn()
        {}

        static void AddRecord()
        {}

        static void DeleteRecord()
        {}

        static void RenameTable()
        {}

        static void EditColumn()
        {}

        static void ChangeColumnOrder()
        {}

        static void ShowAbout()
        {
            Dialog aboutWin = new Dialog("About RookDB", 80, 20);
            Label aboutLabel = new Label(@" 
RookDB is based off CastleDB and is a static database.
Files are stored in human readable JSON. RookDB makes a few changes,
for details please view the README.txt in the github repo.

RookDB is written in C# (dotnet core 3) by Dean T.
This editor is written using the Terminal.Gui toolkit by Miguel Deicaza.
All source is available at https://github.com/DeanoBurrito/RookDB.git

[ESC] to close this window.
                ")
            {
                X = 1,
                Y = 1,
                Width = Dim.Fill(1),
                Height = Dim.Fill(1),
            };
            aboutWin.Add(aboutLabel);
            Application.Run(aboutWin);
        }

        public static void ShowHelp()
        {
            Dialog aboutWin = new Dialog("Help", 80, 20);
            Label aboutLabel = new Label(@" 
All menu commands can be activated by hitting ALT + [highlighted_key].
If for some reason Alt refuses to work, you can activate the top level menu with F9.
Browsing tables is down with the left/right/up/down keys.
PageUp/PageDown switch between tables.
Hitting enter will edit the currently centered column and selected record.
Space will show a menu of columns on the currently selected record.

Adding/Removing columns, tables and records can all be done via menu commands.
Renaming and changing column order are supported as well, moving records is currently not.

[ESC] to close this window.
                ")
            {
                X = 1,
                Y = 1,
                Width = Dim.Fill(1),
                Height = Dim.Fill(1),
            };
            aboutWin.Add(aboutLabel);
            Application.Run(aboutWin);
        }

        static void UpdateWindowName()
        {
            if (currDB == null)
                return;
            mainWin.Title = "RookDB Editor - " + StringHelper.LimitLengthInverse(currDB.filename, 45);
        }

        static void PrintStatus(string status, StatusLevel level)
        {
            statusLine.Text = status;
            switch (level)
            {
                case StatusLevel.CriticalError:
                    statusLine.TextColor = new Terminal.Gui.Attribute(Color.BrightRed, Color.DarkGray);
                    break;
                case StatusLevel.Error:
                    statusLine.TextColor = new Terminal.Gui.Attribute(Color.Red, Color.Black);
                    break;
                case StatusLevel.Warning:
                    statusLine.TextColor = new Terminal.Gui.Attribute(Color.BrightYellow, Color.Black);
                    break;
                case StatusLevel.Default:
                default:
                    statusLine.TextColor = new Terminal.Gui.Attribute(Color.White, Color.Black);
                    break;
            }
        }
    }
}