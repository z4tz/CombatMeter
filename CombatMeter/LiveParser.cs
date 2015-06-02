using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace CombatMeter
{
    class LiveParser
    {
        string currentFile;
        long previousLength;
        FileSystemWatcher fileWatcher;
        CombatList combatList;
        TextToEntryParser textParser;

        //
        public LiveParser(CombatList _combatList)
        {
            combatList = _combatList;
            textParser = new TextToEntryParser(combatList);
            AddFileWatcherEvents();
        }


        private void AddFileWatcherEvents()
        {
            fileWatcher = new FileSystemWatcher();
            fileWatcher.Path = @"c:\SWtest\"; //todo: set to global value with path of SWTOR log folder?
            fileWatcher.NotifyFilter = NotifyFilters.FileName| NotifyFilters.Size;
            fileWatcher.Filter = "*.txt";

            fileWatcher.Changed += FileWatcher_Changed;
            fileWatcher.EnableRaisingEvents = true;
        }


        //on file update, should cover new files created since they are updated with new info after?
        //since any update appends data to file a size check is enough (NotifyFilters.LastWrite triggers twice, bug, so use size instead to avoid it)
        void FileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
             //if a new file is detected
            if (currentFile != e.Name)
            {
                currentFile = e.Name;
                previousLength = 0;
            }

            //todo: see if this can be done in a better way
            while (IsFileLocked(e.FullPath)) //wait until file is not locked, dirty version
            {
                Thread.Sleep(10);
            }
            List<string> textList = new List<string>();
            using (var fileStream = File.OpenRead(e.FullPath))
            {
                fileStream.Position = previousLength; //move stream to previous position

                previousLength = fileStream.Length; //update with current file length.

                using (var streamReader = new StreamReader(fileStream))
                {
                    while (!streamReader.EndOfStream)
                    {
                        textList.Add(streamReader.ReadLine());
                        //textParser.CreateEntry(streamReader.ReadLine()); //todo: move create-entry to after file is closed
                    }
                }
            }
            foreach (var line in textList)
            {
                textParser.CreateEntry(line);
            }

        }


        bool IsFileLocked(string filePath)
        {
            FileStream stream = null;
            try
            {
                stream = File.OpenRead(filePath);
            }
            catch (IOException)
            {
                //file locked;
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
            //file is not locked
            return false;

        }



    }
}
