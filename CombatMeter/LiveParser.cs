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
    //todo: add feature to skip previous combats in current logfile until latest if it's not complete.
    class LiveParser
    {
        string logDirectory; //where
        string currentFile="";
        long previousPosition;
        FileSystemWatcher fileWatcher;
        CombatList combatList;
        TextToEntryParser textParser;
        CancellationTokenSource LiveParserTokenSource;
        int badMatchCount = 0;
        int maxBadMatch = 10; //how many times in a row a bad match can be made before skipping past

        //
        public LiveParser(CombatList _combatList)
        {
            logDirectory = Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\Documents\Star Wars - The Old Republic\CombatLogs\");
            combatList = _combatList;
            textParser = new TextToEntryParser(combatList);
            FindLatestFile();            
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
        
        private void RemoveFileWatcherEvents()
        {
            fileWatcher.Created -= fileWatcher_Created;
            fileWatcher.EnableRaisingEvents = false;
        } 

        //On new file created in watched folder
        private void fileWatcher_Created(object sender, FileSystemEventArgs e)
        {
            currentFile = e.FullPath;            
            previousPosition = 0; //if a new file is detected move to start of file for reading
        }

        /// <summary>
        /// Start periodically look for updates in logfile
        /// </summary>
        /// <param name="period">How often it should look, in milliseconds.</param>
        public async void Start(double period=100)
        {
            LiveParserTokenSource = new CancellationTokenSource();
            var CancellationToken = LiveParserTokenSource.Token;

            AddFileWatcherEvents();

            try
            {
                await Task.Run(async () =>
                {
                    await PeriodicTask.Run(ReadChanges, TimeSpan.FromMilliseconds(period), CancellationToken);

                }, CancellationToken
                );
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
            finally
            {
                LiveParserTokenSource.Dispose();
            }
        }


       public void Stop()
        {
            if (LiveParserTokenSource.Token.CanBeCanceled)
            {
                LiveParserTokenSource.Cancel();                
            }
            RemoveFileWatcherEvents();
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
                    fileStream.Position = previousPosition; //move stream to previous position                                       

                    using (var streamReader = new StreamReader(fileStream))
                    {
                        while (!streamReader.EndOfStream)
                        {
                            
                            string line = streamReader.ReadLine();
                            if (textParser.ValidEntry(line))
                            {                                
                                textList.Add(line);
                            }
                            else if (badMatchCount>=maxBadMatch) // if to many, skip line and just move on.
                            {          
                                Debug.WriteLine(line);
                                continue;
                            }
                            else // retry on next read.
                            {
                                badMatchCount++;                                
                                return;                                
                            }                            
                        }
                        previousPosition = fileStream.Length;
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
                badMatchCount = 0; //when written
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
