using System;
using System.Collections.Generic;
using System.Text;

namespace Friendplant.Data.ProfileElements {

    [Serializable]
    public class HistoryElement {
        public string Date { get; private set; } // Дата транзакции
        public string Amount { get; private set; } // Изменение баланса
        public string Description { get; private set; } // Описание транзакции

        public HistoryElement(Balance balance, string amount, string description, string date = null) {

            // Автоматическое установление даты
            if(date == null) {
                Date = DateTimeOffset.Now.ToString("dd.MM.yy - H:mm:ss");
            }
            else Date = date;

            Amount = amount;            // Устанавливаем на сколько изменился баланс
            Description = description;  // Устанавливаем описание

        }
    }
}
