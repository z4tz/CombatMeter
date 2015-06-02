using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Specialized;
using Swordfish.NET.Collections;
using System.Diagnostics;

namespace CombatMeter 
{
    class DamageSumation
    {
        public string Ability { get; set; }
        public double AbilityDamage { get; set; }
        public double AbilityDPS { get; set; }
    }


    /// <summary>
    /// Contains a list of CombatEntries and various statistic from one combat, to be put in the CombatList
    /// </summary>
    class CombatLog : INotifyPropertyChanged
    {
        
        public ConcurrentObservableCollection<CombatEntry> EntryList { get; set; }
        public IEnumerable<DamageSumation> DamageList { get; set; }

        #region Properties
        //both start end endtime has todays date as a basis to avoid some problems with running liveparser on a datechange hopefully (if swtor uses local time for logs)
        //problems could occur if a separate file is parsed during a datechange, but much more unlikely and can just be reopened for correct result.
        public DateTime StartTime { get; set; }
        private DateTime endTime;
        public DateTime EndTime
        {
            get
            {
                //If endtime is not set yet
                if (endTime == DateTime.MinValue)
                {                    
                    return EntryList[EntryList.Count - 1].Timestamp.AddTicks(StartTime.Ticks);
                }
                else
                {
                    return endTime;
                }
            }
            set { endTime = value; }
        }


        public string Player { get; set; }

        private double damage;
        public double Damage
        {
            get { return damage; }
            set
            {
                if (value != damage)
                {
                    damage = value;
                    OnPropertyChanged("Damage");
                }
            }
        }
        
        private double dps;
        public double DPS
        {
            get { return dps; }
            set
            {
                if (value != dps)
                {
                    dps = value;
                    OnPropertyChanged("DPS");
                }                
            }
        }

        private double healing;
        public double Healing
        {
            get { return healing; }
            set
            {
                if (value != healing)
                {
                    healing = value;
                    OnPropertyChanged("Healing");
                }
            }
        }

        private double hps;
        public double HPS
        {
            get { return hps; }
            set
            {
                if (value != hps)
                {
                    hps = value;
                    OnPropertyChanged("HPS");
                }
            }
        }

        private double threat;

        public double Threat
        {
            get { return threat; }
            set
            {
                if (value != threat)
                {
                    threat = value;
                    OnPropertyChanged("Threat");
                }
            }
        }
        
        private double tps;
        public double TPS
        {
            get { return tps; }
            set
            {
                if (value != tps)
                {
                    tps = value;
                    OnPropertyChanged("TPS");
                }
            }
        }

        private double damageTaken;
        public double DamageTaken
        {
            get { return damageTaken; }
            set
            {
                if (value != damageTaken)
                {
                    damageTaken = value;
                    OnPropertyChanged("DamageTaken");
                }
            }
        }

        private double dtps;
        public double DTPS
        {
            get { return dtps; }
            set
            {
                if (value != dtps)
                {
                     dtps = value;
                     OnPropertyChanged("DTPS");
                }
            }
        }

        private double healingTaken;
        public double HealingTaken
        {
            get { return healingTaken; }
            set
            {
                if (value != healingTaken)
                {
                     healingTaken = value;
                     OnPropertyChanged("HealingTaken");
                }
            }
        }

        private double htps;
        public double HTPS
        {
            get { return htps; }
            set
            {
                if (value != htps)
                {
                     htps = value;
                     OnPropertyChanged("HTPS");
                }
            }
        }

        public double CombatTime
        {
            get
            {
                return (EndTime - StartTime).TotalSeconds;
            }
        }

        #endregion Properties



        public CombatLog ()
        {
            EntryList = new ConcurrentObservableCollection<CombatEntry>();
            EntryList.CollectionChanged += EntryList_CollectionChanged;
            StartTime = DateTime.Now;
            
        }

        void EntryList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                UpdateStatistics(e);
                UpdatePerSecondStats();
                UpdateDamageDone();
            }

        }


        private void UpdateStatistics(NotifyCollectionChangedEventArgs e)
        {
            foreach (CombatEntry entry in e.NewItems)
            {
                if (entry is DamageEntry)
                {
                    var damageEntry = (DamageEntry)entry;

                    if (damageEntry.Source == Player)
                    {
                        Damage += damageEntry.Value;
                        Threat += damageEntry.Threat;  
                    }
                    if (damageEntry.Target == Player)
                    {
                        DamageTaken += damageEntry.Value;
                    }
                }

                else if (entry is HealEntry)
                {
                    var healEntry = (HealEntry)entry;

                    if (healEntry.Source == Player)
                    {
                        Healing += healEntry.Value;
                        Threat += healEntry.Threat;
                    }
                    if (healEntry.Target == Player)
                    {
                        HealingTaken += healEntry.Value;
                    }
                }

                else if (entry is StartEntry)
                {
                    var startEntry = (StartEntry)entry;

                    StartTime = startEntry.Timestamp;
                    Player = startEntry.Source;
                }

                else if (entry is ExitEntry)
                {
                    var endEntry = (ExitEntry)entry;

                    EndTime = endEntry.Timestamp;                 
                }

                
                entry.Timestamp = new DateTime()+(entry.Timestamp - StartTime);
            }            
        }


        private void UpdatePerSecondStats()
        {

            double seconds = CombatTime; //call once to save calculations.

            if (seconds != 0)
            {
                DPS = Damage / seconds;
                HPS = Healing / seconds;
                TPS = Threat / seconds;
                DTPS = DamageTaken / seconds;
                HTPS = HealingTaken / seconds;
                var test = new { ability = "hello", value = 100 };
            }
        }
        

        private void UpdateDamageDone()
        {

            DamageList =
                from entry in EntryList
                where entry.EntryType == "DamageEntry"
                where entry.Source == Player
                group entry by entry.Ability into Abilities
                select new DamageSumation { Ability = Abilities.Key, AbilityDamage = Abilities.Sum(entry => entry.Value), AbilityDPS = Abilities.Sum(entry => entry.Value) / CombatTime };

            OnPropertyChanged("DamageList");
            
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }



    }
}
