using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Friendplant.Data.ProfileElements {

    [Serializable]
    public class Balance {

        public int Money { get; set; }
        public int Spent { get; set; }
        public List<HistoryElement> History = new List<HistoryElement>();


        public Balance(bool radiate = false) {

            Money = 20; Spent = 0;

            AddHistory("+20", "Стартовый капитал");
        }

        public void AddHistory(string amount, string description, string date = null) {
            new HistoryElement(this, amount, description, date);
        }

        public void Transfer(int count, HistoryElement history, bool spend) {
            Money += count;
            if(spend) Spent -= count;
        }
    }
}
