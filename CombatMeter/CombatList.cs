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
        private Task liveParserTask;

        public CombatList()
        {
            
        }

        /// <summary>
        /// Parses and adds entries found in textfile to new combatlog in List
        /// </summary>
        /// <param name="FileName">Swtor text combat log</param>
        public void ParseFile(string FileName)
        {

            var task = Task.Run(() =>
            {
                this.Clear();

                Stopwatch sw = new Stopwatch();
                sw.Start();

                
                TextLogReader reader = new TextLogReader(FileName);
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

        /// <summary>
        /// 
        /// </summary>
        public void StartParser()
        {
            liveParserTask = Task.Run(() =>
            {
                LiveParser liveParser = new LiveParser(this);                
            });
        }

        public void StopParser()
        {
            //todo: cancel parser if active
            if (liveParserTask != null && liveParserTask.Status == TaskStatus.Running)
            {
                
            }
        }


        
    }
}
