using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CombatMeter
{
    class CombatListViewModel
    {
        private CombatList combatList;
        public object DataContext { get { return combatList; } }


        public CombatListViewModel()
        {
            combatList = new CombatList();
        }

        public void ParseFile(string FilePath)
        {
            combatList.ParseFile(FilePath);
        }

        public void StartLiveParser()
        {
            combatList.StartParser();
        }

        public void StopLiveParser()
        {
            combatList.StopParser();
        }

        

        
    }
}
