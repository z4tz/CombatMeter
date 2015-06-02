using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CombatMeter
{
    class CombatEntry
    {
        public string Source { get; set; }
        public string Target { get; set; }
        public string Ability { get; set; }
        public DateTime Timestamp { get; set; }
        public double Value { get; set; }
        public double Threat { get; set; }
        public bool Critical { get; set; }
        public string EntryType {
            get 
            {
                return this.GetType().ToString().Split('.')[1];
            }
        }

    }
}
