using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Friendplant.Data.ProfileElements {

    /// <summary>
    /// Баланс пользователя
    /// </summary>
    [Serializable]
    public class Balance {

        public int Money { get; set; } // Сколько денег
        public int Spent { get; set; } // Потраченные деньги
        public List<HistoryElement> History = new List<HistoryElement>(); // История транзакций


        public Balance() {

            Money = 20; Spent = 0;

            AddHistory("+20", "Стартовый капитал");
        }

        /// <summary>
        /// Добавляет пользователю элемент Истории.
        /// </summary>
        /// <param name="amount">Количество блестяшек.</param>
        /// <param name="description">Описание транзакции.</param>
        /// <param name="date">Дата транзакции. Null - для автоматической генерации</param>
        public void AddHistory(string amount, string description, string date = null) {

            // Удаление "лишней" Истории
            if(History.Count == 10) {
                History.RemoveAt(9);
            }

            // Добавление элемента Истории
            History.Insert(0, new HistoryElement(this, amount, description, date));
        }

        /// <summary>
        /// Делает транзакцию на увелечение/уменьшение счета.
        /// </summary>
        /// <param name="count">Количество блестяшек для транзакции. Отрицательное, чтобы снять блестяшки с счета.</param>
        /// <param name="spend">Будет ли увеличиватся количество потраченных блестяшек.</param>
        /// <param name="amount">Количество блестяшек для Истории.</param>
        /// <param name="description">Описание транзакции для Истории.</param>
        /// <param name="date">Дата транзакции для Истории. Null - для автоматической генерации</param>
        public void Transfer(int count, bool spend, string amount, string description, string date = null ) {
            Money += count; // Изменение количества денег
            if(spend) Spent -= count; // Увелечение потраченных денег
            AddHistory(amount, description, date); // Добавление Истории
        }
    }
}
