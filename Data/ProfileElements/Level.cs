using System;
using System.Collections.Generic;
using System.Text;

namespace Friendplant.Data.ProfileElements {

    [Serializable]
    public class Level {
        public int Amount { get; set; }
        public int Experience { get; set; }
        public int ExpNeeding { get; set; }

        public bool Radiate { get; set; }

        public Level() {       
            Amount = 1;
            Experience = 0;
            ExpNeeding = 10;
            Radiate = false;
        }

        public bool AddExp(int count = 1) {
            Experience += count;
            if(Experience >= ExpNeeding) {
                Experience = 0;
                Amount++;
                ExpNeeding = Convert.ToInt32(ExpNeeding * 1.25);
                return true;
            }
            else return false;
        }

    }
}
