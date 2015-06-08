using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CombatMeter
{
    class FileParser
    {
        CombatList combatList;

        public FileParser(CombatList _combatList)
        {
            combatList = _combatList;
        }

        public void ParseFile(string file)
        {
            var task = Task.Run(() =>
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();



                List<string> textList = TextLogReader.ReadFile(file);
                TextToEntryParser parser = new TextToEntryParser(combatList);

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

        public void ParseFiles(string[] files)
        {
            foreach (var file in files)
            {
                ParseFile(file);
            }
        }
    }
}
