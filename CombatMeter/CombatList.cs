using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Swordfish.NET.Collections;
using System.Threading;



namespace CombatMeter
{
    /// <summary>
    /// Contains a List of CombatLogs, and methods for parsing data to the CombatLogs.
    /// </summary>
    class CombatList : ConcurrentObservableCollection<CombatLog>
    {       
        LiveParser liveParser;

        public CombatList()
        {
            liveParser = new LiveParser(this);
        }

        /// <summary>
        /// Parses and adds entries found in textfile to new combatlog in List
        /// </summary>
        /// <param name="FilePath">Swtor text combat log</param>
        public void ParseFile(string FilePath)
        {

            var task = Task.Run(() =>
            {
                this.Clear();

                Stopwatch sw = new Stopwatch();
                sw.Start();

                
                TextLogReader reader = new TextLogReader(FilePath);
                List<string> textList = reader.ReadFile();
                TextToEntryParser parser = new TextToEntryParser(this);
                
                foreach (var line in textList)
                {
                    try
                    {
                        parser.CreateEntry(line);
                    }
                    catch (Exception)
                    {
                        Debug.WriteLine(line);
                        //Todo: write missed lines into a file to gather.
                        //also include exception message for better idea of why.
                    }
                }
                sw.Stop();
                Debug.WriteLine(sw.Elapsed + " For " + TextToEntryParser.EntryCount + " entries");
                TextToEntryParser.EntryCount = 0;                
            });
            
            
        }
        public void StartParser()
        {
            this.Clear(); //Clear old data before starting parser
            liveParser.Start();
        }

        public void StopParser()
        {
            liveParser.Stop();
        }


        
    }
}
