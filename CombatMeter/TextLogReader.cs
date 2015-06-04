using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;


namespace CombatMeter
{
    //todo: remake to a static class? 
    static class TextLogReader
    {
        public static List<string> ReadFile(string filePath)
        {
            List<string> textList = new List<string>();
            
            try
            {
                using (StreamReader sr = new StreamReader(filePath))
                {
                    while (sr.Peek() >=0)
                    {
                        textList.Add(sr.ReadLine());
                    }                    
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Failed to read file {0}", filePath);
            }
            return textList;
        }
    }
}
