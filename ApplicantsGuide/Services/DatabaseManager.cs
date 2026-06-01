using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using ApplicantsGuide.Models;

namespace ApplicantsGuide.Services
{

    /// <summary>
    /// Менеджер бази даних проєкту. Забезпечує збереження, завантаження, пошук та редагування інформації про ЗВО та їхні спеціальності.
    /// </summary>
    public class DatabaseManager
    {

        public const string FormFullTime = "Денна";
        public const string FormPartTime = "Заочна";
        public const string FinanceBudget   = "Бюджет";
        public const string FinanceContract = "Контракт";

        private readonly string _filePath;
        private List<University> _universities;

        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Поточна активна сесія авторизованого університету (null, якщо користувач — абітурієнт).
        /// </summary>
        public University? CurrentSession { get; private set; } = null;

        /// <summary>
        /// Повертає значення, яке вказує, чи авторизовано представника ЗВО в системі.
        /// </summary>
         public bool IsAdminLoggedIn => CurrentSession != null;

        /// <summary>
        /// Ініціалізує новий екземпляр класу <see cref="DatabaseManager"/> та завантажує дані.
        /// </summary>
        public DatabaseManager(string? filePath = null)
        {
            _filePath = filePath ?? Path.Combine(AppContext.BaseDirectory, "universities.json");
            _universities = new List<University>();
            LoadOrSeedDatabase();
        }

        /// <summary>
        /// Список усіх університетів, доступний лише для читання з метою захисту цілісності колекції.
        /// </summary>
        public IReadOnlyList<University> Universities => _universities.AsReadOnly();

        /// <summary>
        /// Виконує спробу автентифікації представника ЗВО за логіном та паролем.
        /// </summary>
         public bool TryLogin(string login, string password)
        {
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
                return false;
 
            var university = _universities.FirstOrDefault(u =>
                u.Login    == login.Trim() &&
                u.Password == password.Trim());
 
            if (university is null)
                return false;
 
            CurrentSession = university;
            return true;
        }

        /// <summary>
        /// Завершує поточну адміністративну сесію ЗВО.
        /// </summary>
        public void Logout()
        {
            CurrentSession = null;
        }

        /// <summary>
        /// Виконує пошук конкретного університету за його назвою або її фрагментом.
        /// </summary>
        public University? FindUniversityByName(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return null;
            return _universities.FirstOrDefault(u =>
                u.Name.Contains(query.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Здійснює аналітичний пошук спеціальностей за кодом/назвою та встановленими фільтрами.
        /// </summary>
        public List<(University University, Specialty Specialty)> FindSpecialties(
            string query,
            string? form    = null,
            string? finance = null,
            string? city    = null)
        {
            var results = new List<(University, Specialty)>();
            if (string.IsNullOrWhiteSpace(query)) return results;
 
            string q = query.Trim();
 
            foreach (var uni in _universities)
            {
                if (city != null && ExtractCity(uni.Address) != city)
                    continue;
 
                foreach (var spec in uni.Specialties)
                {
                    bool nameMatch =
                        spec.Code.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                        spec.Name.Contains(q, StringComparison.OrdinalIgnoreCase);
 
                    if (!nameMatch) continue;
 
                    if (form    != null && spec.Form    != form)    continue;
                    if (finance != null && spec.Finance != finance)  continue;
 
                    results.Add((uni, spec));
                }
            }
 
            return results;
        }
 
        /// <summary>
        /// Знаходить спеціальність із мінімальним прохідним балом серед тих, що відповідають критеріям.
        /// </summary>
        public (University University, Specialty Specialty, double Score)?
            FindMinimumScore(
                string query,
                string form,
                string finance,
                string? city = null)
        {
            var candidates = FindSpecialties(query, form, finance, city);
            if (candidates.Count == 0) return null;
 
            var valid = candidates.Where(c => c.Specialty.MinScore > 0).ToList();
            if (valid.Count == 0) return null;
 
            var best = valid.MinBy(c => c.Specialty.MinScore);
            return (best.University, best.Specialty, best.Specialty.MinScore);
        }
 
        /// <summary>
        /// Додає нову спеціальність до колекції обраного ЗВО з верифікацією прав доступу.
        /// </summary>
        public bool AddSpecialty(string universityId, Specialty specialty)
        {
            var uni = GetOwnUniversity(universityId);
            if (uni is null) return false;
 
            uni.Specialties.Add(specialty);
            SaveDatabase();
            return true;
        }
 
        /// <summary>
        /// Видаляє спеціальність за індексом з колекції обраного ЗВО.
        /// </summary>
        public bool RemoveSpecialty(string universityId, int specialtyIndex)
        {
            var uni = GetOwnUniversity(universityId);
            if (uni is null) return false;
            if (specialtyIndex < 0 || specialtyIndex >= uni.Specialties.Count)
                return false;
 
            uni.Specialties.RemoveAt(specialtyIndex);
            SaveDatabase();
            return true;
        }
 
        /// <summary>
        /// Оновлює вартість навчання для вказаної спеціальності ЗВО.
        /// </summary>
        public bool UpdatePrice(string universityId, int specialtyIndex, decimal newPrice)
        {
            var uni = GetOwnUniversity(universityId);
            if (uni is null) return false;
            if (specialtyIndex < 0 || specialtyIndex >= uni.Specialties.Count)
                return false;
            if (newPrice < 0) return false;
 
            uni.Specialties[specialtyIndex].Price = newPrice;
            SaveDatabase();
            return true;
        }
 
        /// <summary>
        /// Оновлює мінімальний прохідний балл для вказаної спеціальності ЗВО.
        /// </summary>
        public bool UpdateMinScore(string universityId, int specialtyIndex, double newScore)
        {
            var uni = GetOwnUniversity(universityId);
            if (uni is null) return false;
            if (specialtyIndex < 0 || specialtyIndex >= uni.Specialties.Count)
                return false;
            if (newScore < 0) return false;
 
            uni.Specialties[specialtyIndex].MinScore = newScore;
            SaveDatabase();
            return true;
        }
 
        /// <summary>
        /// Примусово зберігає всі незбережені зміни поточної сесії адміністратора до сховища.
        /// </summary>
        public bool SaveChanges()
        {
            if (!IsAdminLoggedIn) return false;
            SaveDatabase();
            return true;
        }
 
        
        private University? GetOwnUniversity(string universityId)
        {
            if (CurrentSession is null) return null;
 
            
            if (CurrentSession.Id != universityId) return null;
 
            
            return _universities.FirstOrDefault(u => u.Id == universityId);
        }
 
        /// <summary>
        /// Допоміжний метод парсингу рядка адреси для відокремлення назви міста.
        /// </summary>
        public static string ExtractCity(string address)
        {
            int idx = address.IndexOf("м. ", StringComparison.Ordinal);
            if (idx < 0) return "Інше";
            string rest  = address.Substring(idx + 3).Trim();
            int    comma = rest.IndexOf(',');
            return comma >= 0 ? rest.Substring(0, comma).Trim() : rest;
        }
 
       
        private void LoadOrSeedDatabase()
        {
            if (File.Exists(_filePath))
            {
                // try-catch захищає додаток від критичного падіння у випадку пошкодження файлу JSON
                try
                {
                    using var fs = new FileStream(
                        _filePath,
                        FileMode.Open, FileAccess.Read, FileShare.Read,
                        bufferSize: 65536, useAsync: false);
 
                    var loaded = JsonSerializer.Deserialize<List<University>>(fs, _jsonOptions);
                    if (loaded != null && loaded.Count > 0)
                    {
                        _universities = loaded;
                        return;
                    }
                }
                catch (JsonException) { }
                catch (IOException)   { }
            }
 
            _universities = CreateSeedData();
            SaveDatabase();
        }
 
        
        private void SaveDatabase()
        {
            try
            {
                string? dir = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
 
                // Транзакційний метод збереження через тимчасовий файл .tmp захищає базу від затирання та руйнування структури при раптовому вимкненні чи збої програми.
                string tmpPath = _filePath + ".tmp";
 
                using (var fs = new FileStream(
                    tmpPath,
                    FileMode.Create, FileAccess.Write, FileShare.None,
                    bufferSize: 65536, useAsync: false))
                {
                    JsonSerializer.Serialize(fs, _universities, _jsonOptions);
                }
 
                
                File.Move(tmpPath, _filePath, overwrite: true);
            }
            catch (IOException) { }
        }
 
       
        private static List<University> CreateSeedData()
        {
            return new List<University>
            {
                new University(
                    id:       "nure",
                    login:    "nure_admin",
                    password: "nure2026",
                    name:     "ХНУРЕ — Харківський національний університет радіоелектроніки",
                    address:  "просп. Науки, 14, м. Харків, 61166",
                    specialties: new List<Specialty>
                    {
                        new Specialty("121", "Інженерія програмного забезпечення", FormFullTime, FinanceBudget,   163.04, 34900, 120),
                        new Specialty("121", "Інженерія програмного забезпечення", FormFullTime, FinanceContract, 142.07, 34900, 180),
                        new Specialty("121", "Інженерія програмного забезпечення", FormPartTime, FinanceBudget,   160.33, 34900,  30),
                        new Specialty("121", "Інженерія програмного забезпечення", FormPartTime, FinanceContract, 137.78, 34900,  50),
                        new Specialty("122", "Комп'ютерні науки",                  FormFullTime, FinanceBudget,   160.30, 34900, 150),
                        new Specialty("122", "Комп'ютерні науки",                  FormFullTime, FinanceContract, 138.31, 34900, 200),
                        new Specialty("125", "Кібербезпека та захист інформації",  FormFullTime, FinanceBudget,   161.00, 34900, 100),
                        new Specialty("125", "Кібербезпека та захист інформації",  FormFullTime, FinanceContract, 141.94, 34900, 130),
                    }),
 
                new University(
                    id:       "kpi",
                    login:    "kpi_admin",
                    password: "kpi2026",
                    name:     "КПІ ім. Ігоря Сікорського — Київський політехнічний інститут",
                    address:  "просп. Берестейський, 37, м. Київ, 03056",
                    specialties: new List<Specialty>
                    {
                        new Specialty("121", "Інженерія програмного забезпечення", FormFullTime, FinanceBudget,   168.19, 38000, 150),
                        new Specialty("121", "Інженерія програмного забезпечення", FormFullTime, FinanceContract, 151.47, 38000, 200),
                        new Specialty("121", "Інженерія програмного забезпечення", FormPartTime, FinanceContract, 148.47, 38000,  50),
                        new Specialty("122", "Комп'ютерні науки",                  FormFullTime, FinanceBudget,   164.19, 35000, 180),
                        new Specialty("122", "Комп'ютерні науки",                  FormFullTime, FinanceContract, 148.47, 35000, 250),
                        new Specialty("131", "Прикладна механіка",                 FormFullTime, FinanceBudget,   150.19, 22000,  90),
                        new Specialty("131", "Прикладна механіка",                 FormFullTime, FinanceContract, 141.47, 22000, 120),
                    }),
            };
        }
    }
}