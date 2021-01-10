using System;
using System.Collections.Generic;
using System.Text;

namespace Friendplant.Data.ProfileElements {

    [Serializable]
    public class HistoryElement {
        public string Date { get; private set; }
        public string Amount { get; private set; }
        public string Description { get; private set; }

        public HistoryElement(Balance balance, string amount, string description, string date = null) {

            // Set Date
            if(date == null) {
                Date = DateTimeOffset.Now.ToString("dd.MM.yy - H:mm:ss");
            }
            else Date = date;

            Amount = amount;            // Set Amount of sparkles
            Description = description;  // Set Description

            // Extra history remove
            if(balance.History.Count == 10) {
                balance.History.RemoveAt(9);
            }

            // Release History Element
            balance.History.Insert(0, this);
        }
    }
}
