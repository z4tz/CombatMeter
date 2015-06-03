using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace CombatMeter
{

    //todo: possibly rewrite so that file is kept open (should be safe with fileshare.readwrite) and simply polled.
    class LiveParser
    {
        string logDirectory;
        string currentFile="";
        long previousLength;
        FileSystemWatcher fileWatcher;
        CombatList combatList;
        TextToEntryParser textParser;
        CancellationTokenSource LiveParserTokenSource;

        //
        public LiveParser(CombatList _combatList)
        {
            logDirectory = Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\Documents\Star Wars - The Old Republic\CombatLogs\");
            combatList = _combatList;
            textParser = new TextToEntryParser(combatList);
            FindLatestFile();

            AddFileWatcherEvents();
        }


        private void FindLatestFile()
        {
            var directory = new DirectoryInfo(logDirectory);
            currentFile = directory.GetFiles().OrderByDescending(file => file.LastWriteTime).First().FullName;
        }


        //Events for finding new files in logDirectory
        private void AddFileWatcherEvents()
        {
            fileWatcher = new FileSystemWatcher();
            fileWatcher.Path = logDirectory; //todo: set to global value with path of SWTOR log folder?
            fileWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime;
            fileWatcher.Filter = "*.txt";

            fileWatcher.Created += fileWatcher_Created;
            fileWatcher.EnableRaisingEvents = true;
        }

        void fileWatcher_Created(object sender, FileSystemEventArgs e)
        {
            currentFile = e.FullPath;            
            previousLength = 0; //if a new file is detected move to start of file for reading
        }


        public async void Start()
        {
            LiveParserTokenSource = new CancellationTokenSource();
            var LiveParserToken = LiveParserTokenSource.Token;
            
            await Task.Run(async () =>
                {
                    await PeriodicTask.Run(ReadChanges, TimeSpan.FromMilliseconds(1000.0), LiveParserToken);
                });
            //await PeriodicTask.Run(ReadChanges, TimeSpan.FromMilliseconds(1000.0), LiveParserToken);     
        }

        //todo: check that a a cancelation-token can be used when it's active in another thread.
       public void Stop()
        {
            if (LiveParserTokenSource.Token.CanBeCanceled)
            {
                LiveParserTokenSource.Cancel();
            }
            
        }               


        private void ReadChanges()
        {
            if (currentFile != "")
            {
                //todo: see if this can be done in a better way
                while (IsFileLocked(currentFile)) //if file is unavalable, wait.  dirty version
                {
                    Thread.Sleep(100);
                }
                List<string> textList = new List<string>();
                using (var fileStream = File.Open(currentFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
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
                    try
                    {
                        textParser.CreateEntry(line);
                    }
                    catch (Exception)
                    {

                        Debug.WriteLine(line);
                    }
                } 
            }
        }


        bool IsFileLocked(string filePath)
        {
            FileStream stream = null;
            try
            {
                stream = File.Open(filePath,FileMode.Open,FileAccess.Read,FileShare.ReadWrite);
            }
            catch (IOException)
            {
                Debug.WriteLine("file locked");
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
