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
        /// Унікальний рядковий ідентифікатор ЗВО в межах бази даних
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Логін представника ЗВО для входу в кабінет адміністратора.
        /// </summary>
        public string Login { get; set; }

        /// <summary>
        /// Пароль представника ЗВО.
        /// </summary>
        public string Password { get; set; }

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
            Id         = string.Empty;
            Login      = string.Empty;
            Password   = string.Empty;
            Name = string.Empty;
            Address = string.Empty;
            Specialties = new List<Specialty>();
        }

        /// <summary>
        /// Конструктор для ініціалізації університету з валідацією даних.
        /// </summary>
        public University(
            string id,
            string login,
            string password,
            string name,
            string address,
            List<Specialty>? specialties = null)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Id університету не може бути порожнім.", nameof(id));
            if (string.IsNullOrWhiteSpace(login))
                throw new ArgumentException("Логін не може бути порожнім.", nameof(login));
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Пароль не може бути порожнім.", nameof(password));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Назва університету не може бути порожньою.", nameof(name));
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentException("Адреса університету не може бути порожньою.", nameof(address));

            Id          = id;
            Login       = login;
            Password    = password;
            Name = name;
            Address = address;
            Specialties = specialties ?? new List<Specialty>();
        }
    }
}