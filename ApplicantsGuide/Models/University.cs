using System;
using System.Collections.Generic;

namespace ApplicantsGuide.Models
{
    /// <summary>
    /// Клас, що представляє заклад вищої освіти.
    /// </summary>
    public class University
    {
        /// <summary>
        /// Назва закладу вищої освіти.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Адреса закладу.
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Список спеціальностей університету.
        /// </summary>
        public List<Specialty> Specialties { get; set; }

        /// <summary>
        /// Конструктор за замовчуванням для десеріалізації JSON.
        /// </summary>
        public University()
        {
            Name = string.Empty;
            Address = string.Empty;
            Specialties = new List<Specialty>();
        }

        /// <summary>
        /// Конструктор для ініціалізації університету з валідацією даних.
        /// </summary>
        public University(string name, string address, List<Specialty>? specialties = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("University name cannot be empty.", nameof(name));

            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentException("University address cannot be empty.", nameof(address));

            Name = name;
            Address = address;
            Specialties = specialties ?? new List<Specialty>();
        }
    }
}