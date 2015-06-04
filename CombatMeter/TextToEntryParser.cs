using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Diagnostics;


namespace CombatMeter
{
    class TextToEntryParser
    {
        public static int EntryCount = 0;
        CombatList combatList;
        bool inCombat = false;
        CombatLog currentCombat;
        private string pattern = @"\[(?<timeStamp>[0-9:.]{1,}?)\] \[(?<source>[A-z0-9:.\{\}\@\-\' ]{1,}?)?\] \[(?<target>[A-z0-9:.\{\}\@\-\' ]{1,}?)\] ?\[(?<ability>[A-z0-9:.\{\}\(\) ]{1,}?)?\] ?\[(?<effect>[A-z0-9:.\{\}() ]{1,}?)?\] ?\((?<value>[A-z0-9:.\{\}\*\(\)\- ]{1,})?\) ?<?(?<threat>[0-9]{1,})?>?";
        
        /// <summary>
        /// Parses strings of text from swtor textfile to Combat-entries.
        /// As a new combat is detected, adds a new combatLog to the combatList. Skips all entries outside of combat.
        /// </summary>
        public TextToEntryParser(CombatList list)
        {
            combatList = list;            
        }

        public bool ValidEntry(string line)
        {
            return Regex.IsMatch(line,pattern);
        }

        public  void CreateEntry(string line)
        {
            CombatEntry entry;

            if (line.Contains(": Damage {"))
            {
                entry = CreateDamageEntry(line);
            }
            else if (line.Contains(": Heal {"))
            {
                entry = CreateHealEntry(line);
            }
            else if (line.Contains(": EnterCombat {")) //
            {
                entry = CreateStartEntry(line);
                inCombat = true;
                currentCombat = new CombatLog();
                combatList.Add(currentCombat);

            }
            else if (line.Contains(": ExitCombat {"))
            {
                entry = CreateExitEntry(line);
                inCombat = false;
                currentCombat.EntryList.Add(entry);
            }
            else
            {
                //todo: check if events that pass by down here is anything i want.                
                return;
            }


            if (inCombat && entry.EntryType != "CombatEntry")
            {
                currentCombat.EntryList.Add(entry);
                EntryCount++;
            }
        }

        private ExitEntry CreateExitEntry(string line)
        {
            Match match = Regex.Match(line, pattern);
            ExitEntry entry = new ExitEntry();

            entry.Timestamp = DateTime.Parse(match.Groups["timeStamp"].Value);
            entry.Source = match.Groups["source"].Value;
            entry.Target = match.Groups["target"].Value;
            return entry;
        }

        private StartEntry CreateStartEntry(string line)
        {
            Match match = Regex.Match(line, pattern);
            StartEntry entry = new StartEntry();

            entry.Timestamp = DateTime.Parse(match.Groups["timeStamp"].Value);
            entry.Source = match.Groups["source"].Value;
            entry.Target = match.Groups["target"].Value;
            return entry;
        }

        private HealEntry CreateHealEntry(string line)
        {
            Match match = Regex.Match(line, pattern);

            HealEntry healEntry = new HealEntry();
            healEntry.Timestamp = DateTime.Parse(match.Groups["timeStamp"].Value);
            healEntry.Source = match.Groups["source"].Value;
            healEntry.Target = match.Groups["target"].Value;
            healEntry.Ability = GetAbilityName(match.Groups["ability"].Value);
            healEntry.Value = GetDoubleValue(match.Groups["value"].Value);
            healEntry.Critical = match.Groups["value"].Value.Contains('*');
            if (match.Groups["threat"].Value != "")
            {
                healEntry.Threat = double.Parse(match.Groups["threat"].Value);
            }
            return healEntry;
        }

        private DamageEntry CreateDamageEntry(string line)
        {
            DamageEntry damageEntry = new DamageEntry();
            Match match = Regex.Match(line, pattern);

            damageEntry.Timestamp = DateTime.Parse(match.Groups["timeStamp"].Value);
            damageEntry.Source = match.Groups["source"].Value;
            damageEntry.Target = match.Groups["target"].Value;
            damageEntry.Ability = GetAbilityName(match.Groups["ability"].Value);
            damageEntry.Value = GetDoubleValue(match.Groups["value"].Value);
            damageEntry.Critical = match.Groups["value"].Value.Contains('*');
            if (match.Groups["threat"].Value != "")
            {
                damageEntry.Threat = double.Parse(match.Groups["threat"].Value);
            }
            
            return damageEntry;
        }


        private double GetDoubleValue(string dmgString)
        {
            string split = dmgString.Split(' ')[0];
            if (split.Contains('*'))
            {
                return double.Parse(split.Remove(split.Length-1));
            }
            else
            {
                return double.Parse(split);
            }
        }

        private string GetAbilityName(string ability)
        {
            string[] split = ability.Split(' ');
            return String.Join(" ", split, 0, split.Count() - 1);            
        }

    }
}
