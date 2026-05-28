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
    public class DatabaseManager
    {
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

        public DatabaseManager(string? filePath = null)
        {
            _filePath = filePath ?? Path.Combine(AppContext.BaseDirectory, "universities.json");
            _universities = new List<University>();
            LoadOrSeedDatabase();
        }

        public IReadOnlyList<University> Universities => _universities.AsReadOnly();

        public University? FindUniversityByName(string universityName)
        {
            if (string.IsNullOrWhiteSpace(universityName)) return null;
            return _universities.FirstOrDefault(u =>
                u.Name.Contains(universityName.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        public List<(University University, Specialty Specialty)> FindSpecialtiesByName(string specialtyQuery)
        {
            var results = new List<(University, Specialty)>();
            if (string.IsNullOrWhiteSpace(specialtyQuery)) return results;

            string query = specialtyQuery.Trim();
            foreach (var university in _universities)
                foreach (var specialty in university.Specialties)
                    if (specialty.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
                        results.Add((university, specialty));

            return results;
        }

        public (University University, Specialty Specialty, double Score)? FindMinimumScore(
            string specialtyQuery,
            string studyForm,
            string fundingType)
        {
            if (string.IsNullOrWhiteSpace(specialtyQuery)) return null;

            string query = specialtyQuery.Trim();
            var candidates = new List<(University University, Specialty Specialty, double Score)>();

            foreach (var university in _universities)
            {
                foreach (var specialty in university.Specialties)
                {
                    if (!specialty.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
                        continue;

                    double score = (studyForm, fundingType) switch
                    {
                        ("Денна", "Бюджет") => specialty.FullTimeBudgetScore,
                        ("Денна", "Контракт") => specialty.FullTimeContractScore,
                        ("Заочна", "Бюджет") => specialty.PartTimeBudgetScore,
                        ("Заочна", "Контракт") => specialty.PartTimeContractScore,
                        _ => 0.0
                    };

                    if (score > 0)
                        candidates.Add((university, specialty, score));
                }
            }

            if (candidates.Count == 0) return null;

            return candidates.MinBy(c => c.Score);
        }

        private void LoadOrSeedDatabase()
        {
            if (File.Exists(_filePath))
            {
                try
                {
                    using var fs = new FileStream(
                        _filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 65536, false);

                    var loaded = JsonSerializer.Deserialize<List<University>>(fs, _jsonOptions);
                    if (loaded != null && loaded.Count > 0)
                    {
                        _universities = loaded;
                        return;
                    }
                }
                catch (JsonException) { }
                catch (IOException) { }
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

                using var fs = new FileStream(
                    _filePath, FileMode.Create, FileAccess.Write, FileShare.None, 65536, false);

                JsonSerializer.Serialize(fs, _universities, _jsonOptions);
            }
            catch (IOException) { }
        }

        private static List<University> CreateSeedData()
        {
            return new List<University>
            {
                new University(
                    "ХНУРЕ — Харківський національний університет радіоелектроніки",
                    "просп. Науки, 14, м. Харків, 61166",
                    new List<Specialty>
                    {
                        new Specialty("121 Інженерія програмного забезпечення", 172.5, 158.0, 155.0, 140.0, 38500),
                        new Specialty("122 Комп'ютерні науки",                  168.0, 154.0, 151.0, 136.0, 37000),
                        new Specialty("123 Комп'ютерна інженерія",              165.5, 151.5, 148.0, 133.0, 36000),
                        new Specialty("124 Системний аналіз",                   162.0, 148.5, 145.0, 130.0, 35000),
                        new Specialty("125 Кібербезпека",                       170.0, 156.0, 153.0, 138.0, 40000),
                        new Specialty("126 Інформаційні системи та технології", 160.0, 147.0, 143.0, 128.0, 34500),
                    }),

                new University(
                    "НТУ «ХПІ» — Національний технічний університет «Харківський політехнічний інститут»",
                    "вул. Кирпичова, 2, м. Харків, 61002",
                    new List<Specialty>
                    {
                        new Specialty("121 Інженерія програмного забезпечення", 169.0, 155.0, 152.0, 137.0, 36500),
                        new Specialty("122 Комп'ютерні науки",                  165.0, 152.0, 148.5, 133.5, 35500),
                        new Specialty("123 Комп'ютерна інженерія",              163.0, 150.0, 146.0, 131.0, 35000),
                        new Specialty("124 Системний аналіз",                   159.5, 146.0, 142.0, 127.5, 33500),
                        new Specialty("125 Кібербезпека",                       167.5, 153.5, 150.0, 135.0, 38500),
                        new Specialty("126 Інформаційні системи та технології", 158.0, 145.0, 141.0, 126.5, 33000),
                    }),

                new University(
                    "КНУ ім. Тараса Шевченка — Київський національний університет імені Тараса Шевченка",
                    "вул. Володимирська, 60, м. Київ, 01601",
                    new List<Specialty>
                    {
                        new Specialty("121 Інженерія програмного забезпечення", 180.0, 165.0, 162.0, 147.0, 48000),
                        new Specialty("122 Комп'ютерні науки",                  177.5, 162.5, 159.0, 144.0, 46500),
                        new Specialty("123 Комп'ютерна інженерія",              174.0, 159.0, 156.0, 141.0, 45000),
                        new Specialty("124 Системний аналіз",                   171.0, 157.0, 153.0, 138.0, 44000),
                        new Specialty("125 Кібербезпека",                       178.5, 164.0, 160.5, 145.5, 49500),
                        new Specialty("126 Інформаційні системи та технології", 169.5, 155.5, 151.5, 136.5, 43000),
                    }),
            };
        }
    }
}