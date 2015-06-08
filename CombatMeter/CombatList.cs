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
        FileParser fileParser;

        public CombatList()
        {
            liveParser = new LiveParser(this);
            fileParser = new FileParser(this);
        }

        /// <summary>
        /// Parses and adds entries found in textfile to new combatlog in List, clears previous entries
        /// </summary>
        /// <param name="File">Swtor text combat log</param>
        public void ParseFile(string file)
        {
            this.Clear();
            fileParser.ParseFile(file);
        }
        /// <summary>
        /// Use to parse multiple files, will not clear in between each file.
        /// </summary>
        /// <param name="files"></param>
        public void ParseFiles(string[] files)
        {
            this.Clear();
            fileParser.ParseFiles(files);
        }
        /// <summary>
        /// Start the Liveparser, will clear previous entries
        /// </summary>
        public void StartParser()
        {
            this.Clear(); //Clear old data before starting parser
            liveParser.Start();
        }
        /// <summary>
        /// Stop the LiveParser.
        /// </summary>
        public void StopParser()
        {            
            liveParser.Stop();
        }


        
    }
}
