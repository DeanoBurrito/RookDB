using System;
using System.Collections.Generic;
using Terminal.Gui;

namespace RookDB.Editor
{
    public class EditorProgram
    {
        static Window mainWin;
        static Label statusLine;
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
            statusLine = new Label("") 
            {
                X = 0,
                Y = Pos.Bottom(Application.Top) - 1,
                Width = Dim.Fill(),
                Height = 1,
                TextColor = new Terminal.Gui.Attribute(Color.BrightYellow, Color.Black),
                Text = "Loading..."
            };
            Application.Top.Add(statusLine);
            Application.Top.Add(mainWin);
            
            MenuBar mainMenu = new MenuBar(new MenuBarItem[] 
            {
                new MenuBarItem("_File", new MenuItem[] 
                {
                    new MenuItem("_New", "", CreateNew),
                    new MenuItem("_Load", "", LoadFile),
                    new MenuItem("_Save", "", SaveFile),
                    new MenuItem("S_aveAs", "", SaveFileAs),
                    new MenuItem("_Quit", "", Quit),
                }),
                new MenuBarItem("_Edit", new MenuItem[] 
                {
                    new MenuItem("_Copy", "", null),
                    new MenuItem("C_ut", "", null),
                    new MenuItem("_Paste", "", null),
                }),
                new MenuBarItem("_Options", new MenuItem[] 
                {
                    new MenuItem("_Settings", "", null),
                    new MenuItem("_About", "", null),
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

        static void RenderFull()
        {
            if (currDB == null)
                return;
            mainWin.Title = StringHelper.LimitLengthInverse("RookDB Terminal Editor - " + currDB.filename, 54);
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
            RenderFull();
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
                RenderFull();
                PrintStatus("Loaded database: " + loadedDB.filename, StatusLevel.Default);
            }
            else
            {
                PrintStatus("Could not load database, file was invalid.", StatusLevel.Error);
            }
        }

        static void SaveFile()
        {}

        static void SaveFileAs()
        {}

        static void Quit()
        {
            Application.RequestStop();
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