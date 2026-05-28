using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using ApplicantsGuide.Models;
using ApplicantsGuide.Services;

namespace ApplicantsGuide.Forms
{
    public partial class Form1 : Form
    {
        private readonly DatabaseManager _db;
        private List<University> _allUniversities = new();

        private ListBox  _listBoxUniversities = null!;
        private DataGridView _gridSpecialties  = null!;

        private ComboBox _cboCriteria   = null!;
        private TextBox  _txtSpecialtyQuery = null!;
        private ComboBox _cboStudyForm  = null!;
        private ComboBox _cboFunding    = null!;
        private ComboBox _cboCityFilter = null!;

        private Button _btnSearch = null!;
        private Button _btnReset  = null!;

        private StatusStrip _statusStrip       = null!;
        private ToolStripStatusLabel _statusLabel = null!;

        private bool _suppressListBoxEvent = false;

        public Form1()
        {
            InitFormParameters();
            InitializeComponent();
            BuildUI();

            _db = new DatabaseManager();
            _allUniversities = _db.Universities.ToList();

            PopulateCityFilter();
            PopulateUniversityList(_allUniversities);

            SetStatus($"Завантажено {_allUniversities.Count} ЗВО. Статистика балів актуальна за 2025 рік.");
        }

        private void InitFormParameters()
        {
            Text          = "Довідник бакалавра — Інформаційно-довідкова система ЗВО України";
            MinimumSize   = new Size(1020, 640);
            Size          = new Size(1380, 780);
            StartPosition = FormStartPosition.CenterScreen;
            WindowState   = FormWindowState.Normal;
            Font          = new Font("Segoe UI", 9.5f, FontStyle.Regular, GraphicsUnit.Point);
            BackColor     = Color.FromArgb(245, 247, 250);
        }

        private void BuildUI()
        {
            var menuStrip = new MenuStrip
            {
                BackColor = Color.FromArgb(30, 90, 160),
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold)
            };

            var menuFile = new ToolStripMenuItem("Файл") { ForeColor = Color.White };
            menuFile.DropDownItems.Add(
                new ToolStripMenuItem("Вихід", null, (s, e) => Application.Exit()));

            var menuHelp = new ToolStripMenuItem("Довідка") { ForeColor = Color.White };
            menuHelp.DropDownItems.Add(
                new ToolStripMenuItem("Про програму", null, ShowAboutDialog));

            menuStrip.Items.Add(menuFile);
            menuStrip.Items.Add(menuHelp);
            Controls.Add(menuStrip);
            MainMenuStrip = menuStrip;

            _statusStrip = new StatusStrip { BackColor = Color.FromArgb(30, 90, 160) };
            _statusLabel = new ToolStripStatusLabel("Готово")
            {
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 9f)
            };
            _statusStrip.Items.Add(_statusLabel);
            Controls.Add(_statusStrip);

            var splitMain = new SplitContainer
            {
                Dock             = DockStyle.Fill,
                SplitterWidth    = 5,
                SplitterDistance = 340,
                BackColor = Color.FromArgb(200, 215, 235)
            };
            Controls.Add(splitMain);
            splitMain.BringToFront();

            BuildLeftPanel(splitMain.Panel1);
            BuildRightPanel(splitMain.Panel2);
        }

        private void BuildLeftPanel(SplitterPanel panel)
        {
            panel.BackColor = Color.FromArgb(240, 244, 250);

            var lblTitle = new Label
            {
                Text      = "📋  Заклади вищої освіти",
                Dock      = DockStyle.Top,
                Height    = 36,
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 90, 160),
                BackColor = Color.FromArgb(220, 232, 248),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(10, 0, 0, 0)
            };

            var filterPanel = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 36,
                BackColor = Color.FromArgb(230, 238, 252),
                Padding   = new Padding(6, 4, 6, 4)
            };

            var lblCity = new Label
            {
                Text      = "Місто:",
                AutoSize  = false,
                Width     = 45,
                Dock      = DockStyle.Left,
                TextAlign = ContentAlignment.MiddleLeft,
                Font      = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(60, 70, 90)
            };

            _cboCityFilter = new ComboBox
            {
                Dock          = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font          = new Font("Segoe UI", 9f)
            };
            _cboCityFilter.SelectedIndexChanged += CboCityFilter_SelectedIndexChanged;

            filterPanel.Controls.Add(_cboCityFilter);
            filterPanel.Controls.Add(lblCity);

            _listBoxUniversities = new ListBox
            {
                Dock                = DockStyle.Fill,
                Font                = new Font("Segoe UI", 9.5f),
                BorderStyle         = BorderStyle.None,
                BackColor           = Color.White,
                ForeColor           = Color.FromArgb(30, 40, 60),
                HorizontalScrollbar = true,
                SelectionMode       = SelectionMode.One,
                DrawMode            = DrawMode.OwnerDrawVariable
            };
            _listBoxUniversities.MeasureItem          += ListBox_MeasureItem;
            _listBoxUniversities.DrawItem              += ListBox_DrawItem;
            _listBoxUniversities.SelectedIndexChanged  += ListBoxUniversities_SelectedIndexChanged;

            panel.Controls.Add(_listBoxUniversities);
            panel.Controls.Add(filterPanel);
            panel.Controls.Add(lblTitle);
        }

        private void BuildRightPanel(SplitterPanel panel)
        {
            panel.BackColor = Color.FromArgb(245, 247, 250);

            var layout = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                RowCount    = 2,
                ColumnCount = 1,
                BackColor   = Color.Transparent
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 200f));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            panel.Controls.Add(layout);

            var groupSearch = new GroupBox
            {
                Text      = "  🔍  Аналітичний пошук за критеріями",
                Dock      = DockStyle.Fill,
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 90, 160),
                BackColor = Color.White,
                Margin    = new Padding(6, 4, 6, 4),
                Padding   = new Padding(10, 5, 10, 8)
            };
            layout.Controls.Add(groupSearch, 0, 0);
            BuildSearchPanel(groupSearch);

            _gridSpecialties = new DataGridView
            {
                Dock                        = DockStyle.Fill,
                Margin                      = new Padding(6, 0, 6, 6),
                ReadOnly                    = true,
                AllowUserToAddRows          = false,
                AllowUserToDeleteRows       = false,
                AllowUserToResizeRows       = false,
                MultiSelect                 = false,
                SelectionMode               = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode         = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                BackgroundColor             = Color.White,
                BorderStyle                 = BorderStyle.None,
                GridColor                   = Color.FromArgb(220, 228, 240),
                RowHeadersVisible           = false,
                Font                        = new Font("Segoe UI", 9.5f),
                AutoSizeRowsMode            = DataGridViewAutoSizeRowsMode.None
            };

            typeof(DataGridView)
                .GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(_gridSpecialties, true, null);

            _gridSpecialties.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(30, 90, 160),
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Padding   = new Padding(3)
            };
            _gridSpecialties.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(240, 245, 255)
            };
            _gridSpecialties.DefaultCellStyle.SelectionBackColor = Color.FromArgb(100, 160, 230);
            _gridSpecialties.DefaultCellStyle.SelectionForeColor = Color.White;

            InitializeGridColumns();
            layout.Controls.Add(_gridSpecialties, 0, 1);
        }

        private void BuildSearchPanel(GroupBox groupBox)
        {
            var tbl = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                RowCount    = 4,
                ColumnCount = 4,
                BackColor   = Color.Transparent
            };

            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 115f));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,  55f));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 115f));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,  45f));

            tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 25f));
            tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 25f));
            tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 25f));
            tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 25f));

            tbl.Controls.Add(MakeLabel("Критерій:"), 0, 0);

            _cboCriteria = new ComboBox
            {
                Dock          = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font          = new Font("Segoe UI", 9.5f),
                Margin        = new Padding(3, 4, 3, 4)
            };
            _cboCriteria.Items.AddRange(new object[]
            {
                "1. Все щодо обраного ВНЗ",
                "2. Все щодо обраної спеціальності",
                "3. Пошук мінімального конкурсу"
            });
            _cboCriteria.SelectedIndex = 0;
            _cboCriteria.SelectedIndexChanged += CboCriteria_SelectedIndexChanged;
            tbl.Controls.Add(_cboCriteria, 1, 0);
            tbl.SetColumnSpan(_cboCriteria, 3);

            tbl.Controls.Add(MakeLabel("Спеціальність:"), 0, 1);

            _txtSpecialtyQuery = new TextBox
            {
                Dock            = DockStyle.Fill,
                Font            = new Font("Segoe UI", 9.5f),
                Margin          = new Padding(3, 4, 3, 4),
                PlaceholderText = "Введіть код або назву (наприклад: 121 або Право)...",
                Enabled         = false
            };
            tbl.Controls.Add(_txtSpecialtyQuery, 1, 1);
            tbl.SetColumnSpan(_txtSpecialtyQuery, 3);

            tbl.Controls.Add(MakeLabel("Форма навчання:"), 0, 2);

            _cboStudyForm = new ComboBox
            {
                Dock          = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font          = new Font("Segoe UI", 9.5f),
                Margin        = new Padding(3, 4, 3, 4),
                Enabled       = false
            };
            _cboStudyForm.Items.AddRange(new object[] { "Денна", "Заочна" });
            _cboStudyForm.SelectedIndex = 0;
            tbl.Controls.Add(_cboStudyForm, 1, 2);

            tbl.Controls.Add(MakeLabel("Фінансування:"), 2, 2);

            _cboFunding = new ComboBox
            {
                Dock          = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font          = new Font("Segoe UI", 9.5f),
                Margin        = new Padding(3, 4, 3, 4),
                Enabled       = false
            };
            _cboFunding.Items.AddRange(new object[] { "Бюджет", "Контракт" });
            _cboFunding.SelectedIndex = 0;
            tbl.Controls.Add(_cboFunding, 3, 2);

            var btnPanel = new FlowLayoutPanel
            {
                Dock          = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor     = Color.Transparent,
                Padding       = new Padding(0)
            };
            tbl.Controls.Add(btnPanel, 0, 3);
            tbl.SetColumnSpan(btnPanel, 4);

            _btnSearch = new Button
            {
                Text      = "🔍  Пошук",
                Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Size = new Size(140, 30),
                BackColor = Color.FromArgb(30, 90, 160),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand,
                Margin    = new Padding(0, 3, 12, 3)
            };
            _btnSearch.FlatAppearance.BorderSize = 0;
            _btnSearch.Click += BtnSearch_Click;
            btnPanel.Controls.Add(_btnSearch);

            _btnReset = new Button
            {
                Text      = "↺  Скинути",
                Font      = new Font("Segoe UI", 9.5f),
                Size = new Size(140, 30),
                BackColor = Color.FromArgb(160, 170, 185),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand,
                Margin    = new Padding(0, 3, 0, 3)
            };
            _btnReset.FlatAppearance.BorderSize = 0;
            _btnReset.Click += BtnReset_Click;
            btnPanel.Controls.Add(_btnReset);

            groupBox.Controls.Add(tbl);
        }

        private void InitializeGridColumns()
        {
            _gridSpecialties.Columns.Clear();

            AddColumn("ColUniversity",        "Університет",           1, hidden: true);
            AddColumn("ColName",              "Спеціальність",          3);
            AddColumn("ColFullTimeBudget",    "Денна (Бюджет)",         1, align: DataGridViewContentAlignment.MiddleCenter);
            AddColumn("ColFullTimeContract",  "Денна (Контракт)",       1, align: DataGridViewContentAlignment.MiddleCenter);
            AddColumn("ColPartTimeBudget",    "Заочна (Бюджет)",        1, align: DataGridViewContentAlignment.MiddleCenter);
            AddColumn("ColPartTimeContract",  "Заочна (Контракт)",      1, align: DataGridViewContentAlignment.MiddleCenter);
            AddColumn("ColContractCost",      "Вартість (грн/рік)",     1, align: DataGridViewContentAlignment.MiddleRight);
        }

        private void AddColumn(
            string name, string header, int fillWeight,
            bool hidden = false,
            DataGridViewContentAlignment align = DataGridViewContentAlignment.MiddleLeft)
        {
            _gridSpecialties.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name             = name,
                HeaderText       = header,
                FillWeight       = fillWeight,
                SortMode         = DataGridViewColumnSortMode.Automatic,
                Visible          = !hidden,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = align }
            });
        }

        private static Label MakeLabel(string text) => new Label
        {
            Text      = text,
            Dock      = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight,
            Font      = new Font("Segoe UI", 9f),
            ForeColor = Color.FromArgb(60, 70, 90),
            Padding   = new Padding(0, 0, 6, 0)
        };

        private void ListBox_MeasureItem(object? sender, MeasureItemEventArgs e)
        {
            e.ItemHeight = 36;
        }

        private void ListBox_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= _listBoxUniversities.Items.Count) return;

            Graphics  g    = e.Graphics;
            Rectangle rect = e.Bounds;

            bool  isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            Color backColor  = isSelected
                ? Color.FromArgb(210, 230, 255)
                : (e.Index % 2 == 0 ? Color.White : Color.FromArgb(248, 250, 255));
            Color foreColor  = isSelected ? Color.FromArgb(10, 60, 130) : Color.FromArgb(30, 40, 60);

            using (var brush = new SolidBrush(backColor))
                g.FillRectangle(brush, rect);

            if (isSelected)
            {
                using var accent = new SolidBrush(Color.FromArgb(30, 90, 160));
                g.FillRectangle(accent, rect.X, rect.Y, 4, rect.Height);
            }

            string text     = _listBoxUniversities.Items[e.Index]?.ToString() ?? string.Empty;
            var    textRect = new Rectangle(rect.X + 10, rect.Y, rect.Width - 14, rect.Height);
            var    flags    = TextFormatFlags.Left | TextFormatFlags.VerticalCenter
                              | TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine;

            TextRenderer.DrawText(g, text, e.Font ?? _listBoxUniversities.Font, textRect, foreColor, flags);

            using var pen = new Pen(Color.FromArgb(220, 228, 240), 1);
            g.DrawLine(pen, rect.Left, rect.Bottom - 1, rect.Right, rect.Bottom - 1);

            e.DrawFocusRectangle();
        }

        private static string ExtractCity(string address)
        {
            int idx = address.IndexOf("м. ", StringComparison.Ordinal);
            if (idx < 0) return "Інше";
            string rest  = address.Substring(idx + 3).Trim();
            int    comma = rest.IndexOf(',');
            return comma >= 0 ? rest.Substring(0, comma).Trim() : rest;
        }

        private void PopulateCityFilter()
        {
            var cities = _allUniversities
                .Select(u => ExtractCity(u.Address))
                .Distinct().OrderBy(c => c).ToList();

            _cboCityFilter.Items.Clear();
            _cboCityFilter.Items.Add("Усі міста");
            foreach (var city in cities)
                _cboCityFilter.Items.Add(city);

            _suppressListBoxEvent = true;
            _cboCityFilter.SelectedIndex = 0;
            _suppressListBoxEvent = false;
        }

        private List<University> GetFilteredUniversities()
        {
            string city = _cboCityFilter.SelectedItem?.ToString() ?? "Усі міста";
            return city == "Усі міста"
                ? _allUniversities
                : _allUniversities.Where(u => ExtractCity(u.Address) == city).ToList();
        }

        private string? GetSelectedCity()
        {
            string city = _cboCityFilter.SelectedItem?.ToString() ?? "Усі міста";
            return city == "Усі міста" ? null : city;
        }

        private void PopulateUniversityList(List<University> universities)
        {
            _suppressListBoxEvent = true;
            _listBoxUniversities.BeginUpdate();
            _listBoxUniversities.Items.Clear();
            foreach (var uni in universities)
                _listBoxUniversities.Items.Add(uni.Name);
            _listBoxUniversities.EndUpdate();
            _suppressListBoxEvent = false;

            if (_listBoxUniversities.Items.Count > 0)
                _listBoxUniversities.SelectedIndex = 0;
            else
            {
                _gridSpecialties.Rows.Clear();
                SetStatus("За обраним фільтром університетів не знайдено.");
            }
        }

        private static string ScoreText(double score) => score > 0 ? score.ToString("F1") : "—";

        private void DisplayUniversitySpecialties(University university)
        {
            _gridSpecialties.Columns["ColUniversity"]!.Visible = false;
            _gridSpecialties.SuspendLayout();
            _gridSpecialties.Rows.Clear();

            foreach (var spec in university.Specialties)
            {
                _gridSpecialties.Rows.Add(
                    university.Name,
                    spec.Name,
                    ScoreText(spec.FullTimeBudgetScore),
                    ScoreText(spec.FullTimeContractScore),
                    ScoreText(spec.PartTimeBudgetScore),
                    ScoreText(spec.PartTimeContractScore),
                    spec.ContractCost.ToString("N0") + " грн");
            }

            _gridSpecialties.ResumeLayout();
            SetStatus($"{university.Name} — {university.Specialties.Count} спеціальностей.");
        }

        private void DisplaySearchResults(List<(University University, Specialty Specialty)> results)
        {
            _gridSpecialties.Columns["ColUniversity"]!.Visible = true;
            _gridSpecialties.SuspendLayout();
            _gridSpecialties.Rows.Clear();

            foreach (var (uni, spec) in results)
            {
                _gridSpecialties.Rows.Add(
                    uni.Name,
                    spec.Name,
                    ScoreText(spec.FullTimeBudgetScore),
                    ScoreText(spec.FullTimeContractScore),
                    ScoreText(spec.PartTimeBudgetScore),
                    ScoreText(spec.PartTimeContractScore),
                    spec.ContractCost.ToString("N0") + " грн");
            }

            _gridSpecialties.ResumeLayout();
            SetStatus($"Знайдено {results.Count} записів за вашим запитом.");
        }

        private void CboCityFilter_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_suppressListBoxEvent) return;
            var filtered = GetFilteredUniversities();
            PopulateUniversityList(filtered);
            string city = _cboCityFilter.SelectedItem?.ToString() ?? "Усі міста";
            SetStatus(city == "Усі міста"
                ? $"Показано всі {filtered.Count} ВНЗ."
                : $"Місто «{city}» — знайдено {filtered.Count} ВНЗ.");
        }

        private void ListBoxUniversities_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_suppressListBoxEvent) return;
            if (_listBoxUniversities.SelectedIndex < 0) return;

            string? selectedName = _listBoxUniversities.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedName)) return;

            var university = _allUniversities.FirstOrDefault(u => u.Name == selectedName);
            if (university is null) return;

            DisplayUniversitySpecialties(university);
        }

        private void CboCriteria_SelectedIndexChanged(object? sender, EventArgs e)
        {
            int criterion = _cboCriteria.SelectedIndex;

            bool queryEnabled = criterion == 1 || criterion == 2;
            bool formEnabled  = criterion == 2;

            _txtSpecialtyQuery.Enabled = queryEnabled;
            _cboStudyForm.Enabled      = formEnabled;
            _cboFunding.Enabled        = formEnabled;

            if (!queryEnabled)
                _txtSpecialtyQuery.Clear();
        }

        private void BtnSearch_Click(object? sender, EventArgs e)
        {
            switch (_cboCriteria.SelectedIndex)
            {
                case 0: ExecuteCriterion1(); break;
                case 1: ExecuteCriterion2(); break;
                case 2: ExecuteCriterion3(); break;
            }
        }

        private void BtnReset_Click(object? sender, EventArgs e)
        {
            if (_cboCriteria.Items.Count  > 0) _cboCriteria.SelectedIndex  = 0;
            if (_cboStudyForm.Items.Count > 0) _cboStudyForm.SelectedIndex = 0;
            if (_cboFunding.Items.Count   > 0) _cboFunding.SelectedIndex   = 0;
            _txtSpecialtyQuery.Clear();
            if (_cboCityFilter.Items.Count > 0) _cboCityFilter.SelectedIndex = 0;
            SetStatus("Пошук скинуто.");
        }

        private void ShowAboutDialog(object? sender, EventArgs e)
        {
            MessageBox.Show(
                "Інформаційно-довідкова система «Довідник бакалавра»\n\n" +
                "Версія: 2.0\n" +
                "База даних: ТОП-100 ЗВО України (за статистикою вступу 2025 року)\n" +
                "Технологія: Windows Forms, .NET 8.0\n" +
                "Мова: C# 12\n\n" +
                "Курсова робота — ХНУРЕ",
                "Про програму",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void ExecuteCriterion1()
        {
            string? selectedName = _listBoxUniversities.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedName))
            {
                ShowWarning("Оберіть університет зі списку ліворуч.", "Підказка");
                return;
            }

            var university = _allUniversities.FirstOrDefault(u => u.Name == selectedName);
            if (university is null) return;

            DisplayUniversitySpecialties(university);

            string sep  = new string('═', 48);
            string card =
                $"🏛️  ІНФОРМАЦІЯ ПРО ЗАКЛАД\n{sep}\n\n" +
                $"  📌  Назва:                 {university.Name}\n\n" +
                $"  📍  Адреса:                {university.Address}\n\n" +
                $"  📚  Спеціальностей у базі: {university.Specialties.Count}";

            MessageBox.Show(card, "Картка закладу", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ExecuteCriterion2()
        {
            string query = _txtSpecialtyQuery.Text.Trim();
            if (string.IsNullOrEmpty(query))
            {
                ShowWarning("Введіть код або назву спеціальності.", "Порожній запит");
                _txtSpecialtyQuery.Focus();
                return;
            }

            var results     = _db.FindSpecialtiesByName(query);
            string? selCity = GetSelectedCity();

            if (selCity != null)
                results = results
                    .Where(r => ExtractCity(r.University.Address) == selCity)
                    .ToList();

            if (results.Count == 0)
            {
                string note = selCity != null ? $" у місті «{selCity}»" : string.Empty;
                ShowInfo(
                    $"За запитом «{query}»{note} нічого не знайдено.\n\n" +
                    "Перевірте код або назву, або змініть фільтр міста.",
                    "Результати пошуку");
                SetStatus($"«{query}» — нічого не знайдено.");
                return;
            }

            DisplaySearchResults(results);
            string cityStr = selCity != null ? $" ({selCity})" : string.Empty;
            SetStatus($"«{query}»{cityStr} — знайдено {results.Count} записів.");
        }

        private void ExecuteCriterion3()
        {
            string query      = _txtSpecialtyQuery.Text.Trim();
            string studyForm  = _cboStudyForm.SelectedItem?.ToString() ?? "Денна";
            string fundingType = _cboFunding.SelectedItem?.ToString() ?? "Бюджет";

            if (string.IsNullOrEmpty(query))
            {
                ShowWarning("Введіть код або назву спеціальності.", "Порожній запит");
                _txtSpecialtyQuery.Focus();
                return;
            }

            string? selCity = GetSelectedCity();

            var candidates = _db.FindSpecialtiesByName(query);
            if (selCity != null)
                candidates = candidates
                    .Where(r => ExtractCity(r.University.Address) == selCity)
                    .ToList();

            (University University, Specialty Specialty, double Score)? best = null;
            double minScore = double.MaxValue;

            foreach (var (uni, spec) in candidates)
            {
                double score = (studyForm, fundingType) switch
                {
                    ("Денна",  "Бюджет")   => spec.FullTimeBudgetScore,
                    ("Денна",  "Контракт") => spec.FullTimeContractScore,
                    ("Заочна", "Бюджет")   => spec.PartTimeBudgetScore,
                    ("Заочна", "Контракт") => spec.PartTimeContractScore,
                    _                       => 0.0
                };

                if (score > 0 && score < minScore)
                {
                    minScore = score;
                    best     = (uni, spec, score);
                }
            }

            if (best is null)
            {
                string note = selCity != null ? $" у місті «{selCity}»" : string.Empty;
                ShowInfo(
                    $"За запитом «{query}» ({studyForm}, {fundingType}){note} мінімального балу не знайдено.\n\n" +
                    "Спробуйте інший тип фінансування або змініть фільтр міста.",
                    "Результати пошуку");
                SetStatus($"«{query}» ({studyForm}, {fundingType}) — не знайдено.");
                return;
            }

            var (resUni, resSpec, resScore) = best.Value;

            DisplaySearchResults(new List<(University, Specialty)> { (resUni, resSpec) });

            string sep1 = new string('═', 48);
            string sep2 = new string('─', 48);

            string card =
                $"🏛️  КАРТКА ЗАКЛАДУ ВИЩОЇ ОСВІТИ\n{sep1}\n\n" +
                $"  📌  Назва:              {resUni.Name}\n\n" +
                $"  📍  Адреса:             {resUni.Address}\n\n" +
                $"  📚  Спеціальність:      {resSpec.Name}\n\n" +
                $"  🎓  Форма навчання:     {studyForm}\n\n" +
                $"  💳  Тип фінансування:   {fundingType}\n\n" +
                $"  📊  Прохідний бал:      {resScore:F1} (НМТ)\n\n" +
                $"  💰  Вартість контракту: {resSpec.ContractCost:N0} грн / рік\n\n" +
                $"{sep2}\n" +
                $"  ✅  Цей заклад має найнижчий бал\n" +
                $"      серед усіх ВНЗ бази для обраної спеціальності.";

            MessageBox.Show(
                card,
                $"Мінімальний конкурс — {studyForm}, {fundingType}",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            SetStatus(
                $"Мінімум: {resUni.Name} — {resScore:F1} балів ({studyForm}, {fundingType}, {resSpec.Name}).");
        }

        private void SetStatus(string message) => _statusLabel.Text = message;

        private static void ShowInfo(string message, string title) =>
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);

        private static void ShowWarning(string message, string title) =>
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
}