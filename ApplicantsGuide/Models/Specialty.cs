using System;

namespace ApplicantsGuide.Models
{
    /// <summary>
    /// Спеціальність ВНЗ з балами для денної та заочної форм, бюджет і контракт окремо.
    /// </summary>
    public class Specialty
    {
        public string Code { get; set; }
        
        public string Name { get; set; }

        public string Form { get; set; }

        public string Finance { get; set; }

        public double MinScore { get; set; }

        
        public decimal Price { get; set; }

        
        public int LicensedPlaces { get; set; }
        
       
       
        public Specialty()
        {
            Code    = string.Empty;
            Name    = string.Empty;
            Form    = string.Empty;
            Finance = string.Empty;
        }

        
        public Specialty(
            string code,
            string name,
            string form,
            string finance,
            double minScore,
            decimal price,
            int licensedPlaces)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Код спеціальності не може бути порожнім.", nameof(code));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Назва спеціальності не може бути порожньою.", nameof(name));
            if (form != "Денна" && form != "Заочна")
                throw new ArgumentException("Форма навчання має бути «Денна» або «Заочна».", nameof(form));
            if (finance != "Бюджет" && finance != "Контракт")
                throw new ArgumentException("Фінансування має бути «Бюджет» або «Контракт».", nameof(finance));
            if (minScore < 0)
                throw new ArgumentOutOfRangeException(nameof(minScore), "Бал не може бути від'ємним.");
            if (price < 0)
                throw new ArgumentOutOfRangeException(nameof(price), "Вартість не може бути від'ємною.");
            if (licensedPlaces < 0)
                throw new ArgumentOutOfRangeException(nameof(licensedPlaces), "Кількість місць не може бути від'ємною.");

            Code           = code;
            Name                  = name;
            Form           = form;
            Finance        = finance;
            MinScore       = minScore;
            Price          = price;
            LicensedPlaces = licensedPlaces;
        }
        public override string ToString() =>
            $"{Code} {Name} | {Form} | {Finance} | {MinScore:F2} балів | {Price:N0} грн";
    }
}