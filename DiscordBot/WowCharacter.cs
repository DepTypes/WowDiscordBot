using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WowBot
{
    class WowCharacter
    {
        public string Name { get; private set; } = "Unknown";
        public int Race { get; private set; } = -1;
        public int WowClass { get; private set; } = -1;
        public int Gender { get; private set; } = -1;
        public int Level { get; private set; } = -1;
        public int MapId { get; private set; } = -1;

        public WowCharacter(string name, int race, int wowClass, int gender, int level, int mapId)
        {
            this.Name = name;
            this.Race = race;
            this.WowClass = wowClass;
            this.Gender = gender;
            this.Level = level;
            this.MapId = mapId;
        }

        public enum RaceName
        {
            Error = -1,
            Undefined = 0,
            Human = 1,
            Orc = 2,
            Dwarf = 3,
            Nightelf = 4,
            Undead = 5,
            Tauren = 6,
            Gnome = 7,
            Troll = 8,
            Bloodelf = 10,
            Draenei = 11
        }

        public enum ClassName
        {
            Error = -1,
            Undefined = 0,
            Warrior = 1,
            Paldin = 2,
            Hunter = 3,
            Rogue = 4,
            Priest = 5,
            DeathKnight = 6,
            Shaman = 7,
            Mage = 8,
            Warlock = 9,
            Unused = 10,
            Druid = 11
        }

        public enum GenderName
        {
            Error = -1,
            Male = 0,
            Female = 1
        }

        public string GetRaceString()
            => ((RaceName)Race).ToString();

        public string GetClassString()
            => ((ClassName)WowClass).ToString();

        public string GetGenderString()
            => ((GenderName)Gender).ToString();
    }
}
