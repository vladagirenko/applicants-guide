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

    /// <summary>
    /// Головна форма інформаційно-довідкової системи «Довідник бакалавра».
    /// Забезпечує користувацький інтерфейс для аналітичного пошуку та редагування спеціальностей ЗВО.
    /// </summary>
    public partial class Form1 : Form
    {
        private readonly DatabaseManager _db;
        private List<University> _allUniversities = new();

        private ListBox  _listBoxUniversities = null!;

        private ComboBox _cboCityFilter       = null!;
        private DataGridView _gridSpecialties  = null!;

        private ComboBox _cboCriteria   = null!;
        private TextBox  _txtSpecialtyQuery = null!;
        private ComboBox _cboStudyForm  = null!;
        private ComboBox _cboFunding    = null!;
        

        private Button _btnSearch = null!;
        private Button _btnReset  = null!;

         private StatusStrip          _statusStrip = null!;
        private ToolStripStatusLabel _statusLabel = null!;

        private Panel  _adminPanel       = null!;
        private Label  _lblAdminSession  = null!;
        private Button _btnAddSpecialty  = null!;
        private Button _btnEditPrice     = null!;
        private Button _btnDeleteRow     = null!;
        private Button _btnAdminLogout   = null!;

        
        private ToolStripMenuItem _menuAdminLogin = null!;
        private bool _suppressListBoxEvent = false;

        /// <summary>
        /// Ініціалізує новий екземпляр класу <see cref="Form1"/>, налаштовує компоненти інтерфейсу та завантажує базу даних.
        /// </summary>
        public Form1()
        {
            InitFormParameters();
            try { InitializeComponent(); } catch { }
            BuildUI();

            _db = new DatabaseManager();
            _allUniversities = _db.Universities.ToList();

            PopulateCityFilter();
            PopulateUniversityList(_allUniversities);

            SetStatus($"Завантажено {_allUniversities.Count} ЗВО. Статистика актуальна на 2025 рік.");
        }

        /// <summary>
        /// Встановлює початкові візуальні параметри, розміри та стилі головного вікна програми.
        /// </summary>
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

        /// <summary>
        /// Послідовно конструює всі основні контейнери та панелі графічного інтерфейсу додатка.
        /// </summary>
        private void BuildUI()
       {
            BuildMenuStrip();
            BuildStatusStrip();
            BuildMainLayout();
        }
 
        /// <summary>
        /// Створює та налаштовує головне меню програми (MenuStrip).
        /// </summary>
        private void BuildMenuStrip()
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
 
            _menuAdminLogin = new ToolStripMenuItem("🔐  Кабінет ЗВО")
            {
                ForeColor = Color.FromArgb(255, 215, 80),
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold)
            };
            _menuAdminLogin.Click += MenuAdminLogin_Click;
 
            var menuHelp = new ToolStripMenuItem("Довідка") { ForeColor = Color.White };
            menuHelp.DropDownItems.Add(
                new ToolStripMenuItem("Про програму", null, ShowAboutDialog));
 
            menuStrip.Items.Add(menuFile);
            menuStrip.Items.Add(_menuAdminLogin);
            menuStrip.Items.Add(menuHelp);
            Controls.Add(menuStrip);
            MainMenuStrip = menuStrip;
        }
 
        /// <summary>
        /// Ініціалізує та додає нижню панель стану програми (StatusStrip).
        /// </summary>
        private void BuildStatusStrip()
        {
            _statusStrip = new StatusStrip { BackColor = Color.FromArgb(30, 90, 160) };
            _statusLabel = new ToolStripStatusLabel("Готово")
            {
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 9f)
            };
            _statusStrip.Items.Add(_statusLabel);
            Controls.Add(_statusStrip);
        }
 
        /// <summary>
        /// Створює розділювальний контейнер (SplitContainer) та ініціалізує бічні робочі панелі додатка.
        /// </summary>
        private void BuildMainLayout()
        {
            var splitMain = new SplitContainer
            {
                Dock             = DockStyle.Fill,
                SplitterWidth    = 5,
                SplitterDistance = 340,
                BackColor        = Color.FromArgb(200, 215, 235)
            };
            Controls.Add(splitMain);
            splitMain.BringToFront();
 
            BuildLeftPanel(splitMain.Panel1);
            BuildRightPanel(splitMain.Panel2);
        }
 
        /// <summary>
        /// Конструює ліву панель, що містить заголовок, адаптивний фільтр міст та список університетів.
        /// </summary>
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
            var filterLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 1,
                ColumnCount = 2,
                BackColor = Color.Transparent
            };
            filterLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50f)); // Фіксована ширина для слова "Місто:"
            filterLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f)); // Комбобокс забере весь залишок папки

            var lblCity = new Label
            {
                Text      = "Місто:",
                Dock      = DockStyle.Fill,
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

            filterLayout.Controls.Add(lblCity, 0, 0);
            filterLayout.Controls.Add(_cboCityFilter, 1, 0);
            filterPanel.Controls.Add(filterLayout);

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
            _listBoxUniversities.MeasureItem         += ListBox_MeasureItem;
            _listBoxUniversities.DrawItem             += ListBox_DrawItem;
            _listBoxUniversities.SelectedIndexChanged += ListBoxUniversities_SelectedIndexChanged;
 
            panel.Controls.Add(_listBoxUniversities);
            panel.Controls.Add(filterPanel);
            panel.Controls.Add(lblTitle);
        }
 
        /// <summary>
        /// Конструює праву панель, яка поєднує блок аналітичного пошуку, таблицю виведення результатів та панель адміністрування.
        /// </summary>
        private void BuildRightPanel(SplitterPanel panel)
        {
            panel.BackColor = Color.FromArgb(245, 247, 250);
 
            var layout = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                RowCount    = 3,
                ColumnCount = 1,
                BackColor   = Color.Transparent
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 200f)); // пошук
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));  // таблиця
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 0f));   // адмін (схована)
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
            _gridSpecialties.AlternatingRowsDefaultCellStyle =
                new DataGridViewCellStyle { BackColor = Color.FromArgb(240, 245, 255) };
            _gridSpecialties.DefaultCellStyle.SelectionBackColor = Color.FromArgb(100, 160, 230);
            _gridSpecialties.DefaultCellStyle.SelectionForeColor = Color.White;
 
            InitializeGridColumns();
            layout.Controls.Add(_gridSpecialties, 0, 1);
 
            BuildAdminPanel(layout);
        }
 
        /// <summary>
        /// Створює елементи керування вибору критеріїв аналітичного пошуку всередині компонента GroupBox.
        /// </summary>
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
            for (int i = 0; i < 4; i++)
                tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 25f));
 
            tbl.Controls.Add(MakeLabel("Критерій:"), 0, 0);
            _cboCriteria = new ComboBox
            {
                Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9.5f), Margin = new Padding(3, 4, 3, 4)
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
                Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9.5f),
                Margin = new Padding(3, 4, 3, 4),
                PlaceholderText = "Код або назва (напр.: 121 або Право)...",
                Enabled = false
            };
            tbl.Controls.Add(_txtSpecialtyQuery, 1, 1);
            tbl.SetColumnSpan(_txtSpecialtyQuery, 3);
 
            tbl.Controls.Add(MakeLabel("Форма навчання:"), 0, 2);
            _cboStudyForm = new ComboBox
            {
                Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9.5f), Margin = new Padding(3, 4, 3, 4),
                Enabled = false
            };
            _cboStudyForm.Items.AddRange(new object[]
                { DatabaseManager.FormFullTime, DatabaseManager.FormPartTime });
            _cboStudyForm.SelectedIndex = 0;
            tbl.Controls.Add(_cboStudyForm, 1, 2);
 
            tbl.Controls.Add(MakeLabel("Фінансування:"), 2, 2);
            _cboFunding = new ComboBox
            {
                Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9.5f), Margin = new Padding(3, 4, 3, 4),
                Enabled = false
            };
            _cboFunding.Items.AddRange(new object[]
                { DatabaseManager.FinanceBudget, DatabaseManager.FinanceContract });
            _cboFunding.SelectedIndex = 0;
            tbl.Controls.Add(_cboFunding, 3, 2);
 
            var btnPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent, Padding = new Padding(0)
            };
            tbl.Controls.Add(btnPanel, 0, 3);
            tbl.SetColumnSpan(btnPanel, 4);
 
            _btnSearch = MakeButton("🔍  Пошук", Color.FromArgb(30, 90, 160), bold: true);
            _btnSearch.Click += BtnSearch_Click;
            btnPanel.Controls.Add(_btnSearch);
 
            _btnReset = MakeButton("↺  Скинути", Color.FromArgb(160, 170, 185));
            _btnReset.Click += BtnReset_Click;
            btnPanel.Controls.Add(_btnReset);
 
            groupBox.Controls.Add(tbl);
        }
 
        /// <summary>
        /// Конструює нижню панель адміністрування (CRUD-операції представника університету).
        /// </summary>
        private void BuildAdminPanel(TableLayoutPanel parentLayout)
        {
            _adminPanel = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = Color.FromArgb(225, 235, 252),
                Padding   = new Padding(10, 6, 10, 6),
                Visible   = false
            };
 
            _lblAdminSession = new Label
            {
                Text      = string.Empty,
                Dock      = DockStyle.Left,
                AutoSize  = false,
                Width     = 220,
                Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(20, 70, 140),
                TextAlign = ContentAlignment.MiddleLeft
            };
 
            _btnAddSpecialty = MakeAdminButton("➕  Додати рядок");
            _btnAddSpecialty.Click += BtnAddSpecialty_Click;
 
            _btnEditPrice = MakeAdminButton("✏  Змінити вартість");
            _btnEditPrice.Click += BtnEditPrice_Click;
 
            _btnDeleteRow = MakeAdminButton("🗑  Видалити рядок", danger: true);
            _btnDeleteRow.Click += BtnDeleteRow_Click;
 
            _btnAdminLogout = MakeAdminButton("🔓  Вийти");
            _btnAdminLogout.BackColor = Color.FromArgb(120, 130, 145);
            _btnAdminLogout.Click += BtnAdminLogout_Click;
 
            var btnFlow = new FlowLayoutPanel
            {
                Dock          = DockStyle.Right,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize      = true,
                AutoSizeMode  = AutoSizeMode.GrowAndShrink, 
                WrapContents  = false,
                BackColor     = Color.Transparent,
                Padding       = new Padding(0)
            };
            btnFlow.Controls.Add(_btnAddSpecialty);
            btnFlow.Controls.Add(_btnEditPrice);
            btnFlow.Controls.Add(_btnDeleteRow);
            btnFlow.Controls.Add(_btnAdminLogout);
 
            _adminPanel.Controls.Add(btnFlow);
            _adminPanel.Controls.Add(_lblAdminSession);
 
            parentLayout.Controls.Add(_adminPanel, 0, 2);
        }
 
       /// <summary>
        /// Ініціалізує та створює структуру колонок таблиці відображення спеціальностей DataGridView.
        /// </summary>
        private void InitializeGridColumns()
        {
            _gridSpecialties.Columns.Clear();
 
            AddColumn("ColUniId",    "",                   1, hidden: true);
            AddColumn("ColRowIndex", "",                   1, hidden: true);
            AddColumn("ColUniName",  "Університет",        3, hidden: true);
            AddColumn("ColCode",     "Код",                1, align: DataGridViewContentAlignment.MiddleCenter);
            AddColumn("ColName",     "Спеціальність",      4);
            AddColumn("ColForm",     "Форма",              1, align: DataGridViewContentAlignment.MiddleCenter);
            AddColumn("ColFinance",  "Фінансування",       1, align: DataGridViewContentAlignment.MiddleCenter);
            AddColumn("ColScore",    "Сер. бал",           1, align: DataGridViewContentAlignment.MiddleCenter);
            AddColumn("ColPrice",    "Вартість (грн/рік)", 1, align: DataGridViewContentAlignment.MiddleRight);
            AddColumn("ColPlaces",   "Місць",              1, align: DataGridViewContentAlignment.MiddleCenter);
        }
 
        /// <summary>
        /// Додає одну кастомізовану колонку заданого типу до таблиці DataGridView.
        /// </summary>
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
 
       
        /// <summary>
        /// Обробляє подію визначення висоти елементів списку університетів для реалізації стилю OwnerDraw.
        /// </summary>
        private void ListBox_MeasureItem(object? sender, MeasureItemEventArgs e) =>
            e.ItemHeight = 36;
 
        /// <summary>
        /// Виконує кастомне графічне відтворення (рендеринг) елементів списку університетів з акцентною смугою виділення.
        /// </summary>
        private void ListBox_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= _listBoxUniversities.Items.Count) return;
 
            Graphics  g    = e.Graphics;
            Rectangle rect = e.Bounds;
            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
 
            Color back = isSelected
                ? Color.FromArgb(210, 230, 255)
                : (e.Index % 2 == 0 ? Color.White : Color.FromArgb(248, 250, 255));
            Color fore = isSelected ? Color.FromArgb(10, 60, 130) : Color.FromArgb(30, 40, 60);
 
            using (var b = new SolidBrush(back)) g.FillRectangle(b, rect);
            if (isSelected)
                using (var b = new SolidBrush(Color.FromArgb(30, 90, 160)))
                    g.FillRectangle(b, rect.X, rect.Y, 4, rect.Height);
 
            string text = _listBoxUniversities.Items[e.Index]?.ToString() ?? string.Empty;
            var textRect = new Rectangle(rect.X + 10, rect.Y, rect.Width - 14, rect.Height);
            TextRenderer.DrawText(g, text, e.Font ?? _listBoxUniversities.Font, textRect, fore,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine);
 
            using var pen = new Pen(Color.FromArgb(220, 228, 240), 1);
            g.DrawLine(pen, rect.Left, rect.Bottom - 1, rect.Right, rect.Bottom - 1);
            e.DrawFocusRectangle();
        }
 
       
        /// <summary>
        /// Отримує унікальний перелік міст із адрес ЗВО та заповнює комбобокс фільтрації.
        /// </summary>
        private void PopulateCityFilter()
        {
            var cities = _allUniversities
                .Select(u => DatabaseManager.ExtractCity(u.Address))
                .Distinct().OrderBy(c => c).ToList();
 
            _cboCityFilter.Items.Clear();
            _cboCityFilter.Items.Add("Усі міста");
            foreach (var c in cities) _cboCityFilter.Items.Add(c);
 
            _suppressListBoxEvent = true;
            _cboCityFilter.SelectedIndex = 0;
            _suppressListBoxEvent = false;
        }
 
        /// <summary>
        /// Повертає відфільтрований за містом список університетів.
        /// </summary>
        private List<University> GetFilteredUniversities()
        {
            string city = _cboCityFilter.SelectedItem?.ToString() ?? "Усі міста";
            return city == "Усі міста"
                ? _allUniversities
                : _allUniversities
                    .Where(u => DatabaseManager.ExtractCity(u.Address) == city)
                    .ToList();
        }
 
        /// <summary>
        /// Повертає назву обраного в поточний момент міста (або null, якщо обрано "Усі міста").
        /// </summary>
        private string? GetSelectedCity()
        {
            string city = _cboCityFilter.SelectedItem?.ToString() ?? "Усі міста";
            return city == "Усі міста" ? null : city;
        }
 

        /// <summary>
        /// Оновлює вміст графічного компонента ListBox списком переданих ЗВО.
        /// </summary>
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
 
       /// <summary>
        /// Виводить у таблицю DataGridView повний список спеціальностей конкретного закладу вищої освіти.
        /// </summary>
        private void DisplayUniversitySpecialties(University university)
        {
            _gridSpecialties.Columns["ColUniName"]!.Visible = false;
 
            _gridSpecialties.SuspendLayout();
            _gridSpecialties.Rows.Clear();
 
            for (int i = 0; i < university.Specialties.Count; i++)
            {
                var spec = university.Specialties[i];
                _gridSpecialties.Rows.Add(
                    university.Id,   
                    i,               
                    university.Name,
                    spec.Code,
                    spec.Name,
                    spec.Form,
                    spec.Finance,
                    spec.MinScore > 0 ? spec.MinScore.ToString("F2") : "—",
                    spec.Price.ToString("N0") + " грн",
                    spec.LicensedPlaces > 0 ? spec.LicensedPlaces.ToString() : "—");
            }
 
            _gridSpecialties.ResumeLayout();
            SetStatus($"{university.Name} — {university.Specialties.Count} рядків.");
        }
 
        /// <summary>
        /// Заповнює таблицю DataGridView зведеними результатами багатокритеріального аналітичного пошуку.
        /// </summary>
        private void DisplaySearchResults(
            List<(University University, Specialty Specialty)> results)
        {
            _gridSpecialties.Columns["ColUniName"]!.Visible = true;
 
            _gridSpecialties.SuspendLayout();
            _gridSpecialties.Rows.Clear();
 
            foreach (var (uni, spec) in results)
            {
                int idx = uni.Specialties.IndexOf(spec);
                _gridSpecialties.Rows.Add(
                    uni.Id,
                    idx,
                    uni.Name,
                    spec.Code,
                    spec.Name,
                    spec.Form,
                    spec.Finance,
                    spec.MinScore > 0 ? spec.MinScore.ToString("F2") : "—",
                    spec.Price.ToString("N0") + " грн",
                    spec.LicensedPlaces > 0 ? spec.LicensedPlaces.ToString() : "—");
            }
 
            _gridSpecialties.ResumeLayout();
            SetStatus($"Знайдено {results.Count} рядків за вашим запитом.");
        }
 
       
        /// <summary>
        /// Обробляє зміну обраного міста у фільтрі для миттєвого оновлення дерева ЗВО.
        /// </summary>
        private void CboCityFilter_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_suppressListBoxEvent) return;
            var filtered = GetFilteredUniversities();
            PopulateUniversityList(filtered);
            string city = _cboCityFilter.SelectedItem?.ToString() ?? "Усі міста";
            SetStatus(city == "Усі міста"
                ? $"Показано всі {filtered.Count} ВНЗ."
                : $"Місто «{city}» — {filtered.Count} ВНЗ.");
        }
 
        /// <summary>
        /// Обробляє клік по університету у списку: завантажує таблицю та регулює доступ адмін-кнопок.
        /// </summary>
        private void ListBoxUniversities_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_suppressListBoxEvent) return;
            if (_listBoxUniversities.SelectedIndex < 0) return;
 
            string? name = _listBoxUniversities.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(name)) return;
 
            var uni = _allUniversities.FirstOrDefault(u => u.Name == name);
            if (uni is null) return;
 
            DisplayUniversitySpecialties(uni);
 
            // Розрахунок безпеки доступу: кнопки активні тільки якщо адмін дивіться свій власний університет
            bool isOwnUni = _db.IsAdminLoggedIn && _db.CurrentSession?.Id == uni.Id;
            _btnAddSpecialty.Enabled = isOwnUni;
            _btnEditPrice.Enabled    = isOwnUni;
            _btnDeleteRow.Enabled    = isOwnUni;
        }
 
        /// <summary>
        /// Обробляє зміну критерію пошуку, динамічно вмикаючи або вимикаючи відповідні фільтри.
        /// </summary
        private void CboCriteria_SelectedIndexChanged(object? sender, EventArgs e)
        {
            int c = _cboCriteria.SelectedIndex;
            _txtSpecialtyQuery.Enabled = c == 1 || c == 2;
            _cboStudyForm.Enabled      = c == 2;
            _cboFunding.Enabled        = c == 2;
            if (!_txtSpecialtyQuery.Enabled) _txtSpecialtyQuery.Clear();
        }
        
        /// <summary>
        /// Точка маршрутизації події пошуку відповідно до обраного користувачем індексу критерію.
        /// </summary>
        private void BtnSearch_Click(object? sender, EventArgs e)
        {
            switch (_cboCriteria.SelectedIndex)
            {
                case 0: ExecuteCriterion1(); break;
                case 1: ExecuteCriterion2(); break;
                case 2: ExecuteCriterion3(); break;
            }
        }
 
        /// <summary>
        /// Скидає всі елементи панелі аналітичного пошуку та текстові фільтри до початкових значень.
        /// </summary>
        private void BtnReset_Click(object? sender, EventArgs e)
        {
            if (_cboCriteria.Items.Count  > 0) _cboCriteria.SelectedIndex  = 0;
            if (_cboStudyForm.Items.Count > 0) _cboStudyForm.SelectedIndex = 0;
            if (_cboFunding.Items.Count   > 0) _cboFunding.SelectedIndex   = 0;
            _txtSpecialtyQuery.Clear();
            if (_cboCityFilter.Items.Count > 0) _cboCityFilter.SelectedIndex = 0;
            SetStatus("Пошук скинуто.");
        }
 
        /// <summary>
        /// Виконує пошук за критерієм 1: виводить картку та загальні відомості про обраний університет.
        /// </summary>
        private void ExecuteCriterion1()
        {
            string? name = _listBoxUniversities.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(name))
            {
                ShowWarning("Оберіть університет зі списку ліворуч.", "Підказка");
                return;
            }
            var uni = _allUniversities.FirstOrDefault(u => u.Name == name);
            if (uni is null) return;
 
            DisplayUniversitySpecialties(uni);
 
            string sep = new string('═', 48);
            MessageBox.Show(
                $"🏛️  ІНФОРМАЦІЯ ПРО ЗАКЛАД\n{sep}\n\n" +
                $"  📌  Назва:   {uni.Name}\n\n" +
                $"  📍  Адреса:  {uni.Address}\n\n" +
                $"  📚  Рядків у базі: {uni.Specialties.Count}",
                "Картка закладу", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
 
        /// <summary>
        /// Виконує пошук за критерієм 2: знаходить обрану спеціальність (за кодом або фрагментом назви).
        /// </summary>
        private void ExecuteCriterion2()
        {
            string query = _txtSpecialtyQuery.Text.Trim();
            if (string.IsNullOrEmpty(query))
            {
                ShowWarning("Введіть код або назву спеціальності.", "Порожній запит");
                _txtSpecialtyQuery.Focus();
                return;
            }
 
            var results = _db.FindSpecialties(query, city: GetSelectedCity());
            if (results.Count == 0)
            {
                ShowInfo($"За запитом «{query}» нічого не знайдено.", "Результати пошуку");
                SetStatus($"«{query}» — нічого не знайдено.");
                return;
            }
            DisplaySearchResults(results);
        }
 
        /// <summary>
        /// Виконує пошук за критерієм 3: знаходить заклад із мінімальним прохідним конкурсом (балом) НМТ.
        /// </summary>
        private void ExecuteCriterion3()
        {
            string query      = _txtSpecialtyQuery.Text.Trim();
            string studyForm  = _cboStudyForm.SelectedItem?.ToString() ?? DatabaseManager.FormFullTime;
            string funding    = _cboFunding.SelectedItem?.ToString()   ?? DatabaseManager.FinanceBudget;
 
            if (string.IsNullOrEmpty(query))
            {
                ShowWarning("Введіть код або назву спеціальності.", "Порожній запит");
                _txtSpecialtyQuery.Focus();
                return;
            }
 
            var best = _db.FindMinimumScore(query, studyForm, funding, GetSelectedCity());
            if (best is null)
            {
                ShowInfo(
                    $"За запитом «{query}» ({studyForm}, {funding}) нічого не знайдено.",
                    "Результати пошуку");
                return;
            }
 
            var (uni, spec, score) = best.Value;
            DisplaySearchResults(new List<(University, Specialty)> { (uni, spec) });
 
            string sep1 = new string('═', 48);
            string sep2 = new string('─', 48);
            MessageBox.Show(
                $"🏛️  КАРТКА ЗАКЛАДУ ВИЩОЇ ОСВІТИ\n{sep1}\n\n" +
                $"  📌  Назва:             {uni.Name}\n\n" +
                $"  📍  Адреса:            {uni.Address}\n\n" +
                $"  📚  Спеціальність:     {spec.Code} {spec.Name}\n\n" +
                $"  🎓  Форма навчання:    {studyForm}\n\n" +
                $"  💳  Фінансування:      {funding}\n\n" +
                $"  📊  Сер. бал НМТ:     {score:F2}\n\n" +
                $"  💰  Вартість:          {spec.Price:N0} грн / рік\n\n" +
                $"  🪑  Ліцензованих місць: {spec.LicensedPlaces}\n\n" +
                $"{sep2}\n  ✅  Найнижчий конкурс серед усіх ВНЗ бази.",
                $"Мінімальний конкурс — {studyForm}, {funding}",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
 
            SetStatus($"Мінімум: {uni.Name} — {score:F2} ({studyForm}, {funding}, {spec.Code}).");
        }
 
        /// <summary>
        /// Керує логікою кліку по кнопці кабінету: відкриває форму авторизації або ініціює вихід із сесії.
        /// </summary>
        private void MenuAdminLogin_Click(object? sender, EventArgs e)
        {
            if (_db.IsAdminLoggedIn)
            {
                if (MessageBox.Show(
                    $"Ви авторизовані як:\n{_db.CurrentSession!.Name}\n\nВийти з кабінету?",
                    "Кабінет ЗВО", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                    == DialogResult.Yes)
                {
                    LogoutAdmin();
                }
                return;
            }
 
            using var loginForm = new AdminLoginForm(_db);
            if (loginForm.ShowDialog(this) == DialogResult.OK)
                ActivateAdminSession();
        }
 
        /// <summary>
        /// Перемикає графічний інтерфейс програми в режим модератора, розгортаючи нижню адмін-панель.
        /// </summary>
        private void ActivateAdminSession()
        {
            var session = _db.CurrentSession!;
 
            _lblAdminSession.Text = $"🏛️  Авторизовано: {session.Name}";
            _menuAdminLogin.Text  = "✅  Кабінет ЗВО (активний)";
 
            var layout = (TableLayoutPanel)_adminPanel.Parent!;
            layout.RowStyles[2] = new RowStyle(SizeType.Absolute, 50f);
            _adminPanel.Visible = true;
 
            _btnAddSpecialty.Enabled = false;
            _btnEditPrice.Enabled    = false;
            _btnDeleteRow.Enabled    = false;
 
            int idx = _listBoxUniversities.FindStringExact(session.Name);
            if (idx >= 0)
                _listBoxUniversities.SelectedIndex = idx;
 
            SetStatus($"Авторизовано: {session.Name}. Доступне редагування своїх спеціальностей.");
        }
 
        /// <summary>
        /// Обробляє клік представника ЗВО на кнопку виходу з адмін-панелі.
        /// </summary>
        private void BtnAdminLogout_Click(object? sender, EventArgs e) => LogoutAdmin();
 
        /// <summary>
        /// Згортає панель адміністрування та повертає додаток у безпечний режим перегляду абітурієнта.
        /// </summary>
        private void LogoutAdmin()
        {
            _db.Logout();
 
            _lblAdminSession.Text = string.Empty;
            _menuAdminLogin.Text  = "🔐  Кабінет ЗВО";
 
            // Ховаємо адмін-панель
            var layout = (TableLayoutPanel)_adminPanel.Parent!;
            layout.RowStyles[2] = new RowStyle(SizeType.Absolute, 0f);
            _adminPanel.Visible = false;
 
            SetStatus("Сесію завершено. Режим: абітурієнт.");
        }
 
        /// <summary>
        /// Відкриває діалогове вікно додавання та вносить новий рядок спеціальності у JSON-базу.
        /// </summary>
        private void BtnAddSpecialty_Click(object? sender, EventArgs e)
        {
            if (!CheckAdminOnOwnUni()) return;
            var session = _db.CurrentSession!;
 
            using var dlg = new AddSpecialtyDialog();
            if (dlg.ShowDialog(this) != DialogResult.OK) return;
 
            bool ok = _db.AddSpecialty(session.Id, dlg.CreatedSpecialty!);
            if (!ok)
            {
                ShowWarning("Не вдалося додати рядок. Перевірте сесію.", "Помилка");
                return;
            }
 
            RefreshCurrentUniDisplay();
            SetStatus("Рядок спеціальності додано і збережено.");
        }
 
        /// <summary>
        /// Обробляє зміну ціни контракту для виділеного рядка спеціальності з валідацією прав доступу.
        /// </summary>
        private void BtnEditPrice_Click(object? sender, EventArgs e)
        {
            if (!CheckAdminOnOwnUni()) return;
            if (!TryGetSelectedRowInfo(out string uniId, out int rowIdx)) return;
 
            var session = _db.CurrentSession!;
            if (uniId != session.Id)
            {
                ShowWarning("Редагування можливе лише для власного ЗВО.", "Немає доступу");
                return;
            }
 
            string input = Microsoft.VisualBasic.Interaction.InputBox(
                "Введіть нову річну вартість навчання (грн):",
                "Змінити вартість", string.Empty);
 
            if (string.IsNullOrWhiteSpace(input)) return;
 
            if (!decimal.TryParse(input.Trim(), out decimal newPrice) || newPrice < 0)
            {
                ShowWarning("Некоректне значення вартості.", "Помилка введення");
                return;
            }
 
            bool ok = _db.UpdatePrice(session.Id, rowIdx, newPrice);
            if (!ok) { ShowWarning("Не вдалося оновити вартість.", "Помилка"); return; }
 
            RefreshCurrentUniDisplay();
            SetStatus($"Вартість оновлено: {newPrice:N0} грн.");
        }
 
        /// <summary>
        /// Видаляє виділений рядок спеціальності з колекції ЗВО після незворотного підтвердження користувачем.
        /// </summary>
        private void BtnDeleteRow_Click(object? sender, EventArgs e)
        {
            if (!CheckAdminOnOwnUni()) return;
            if (!TryGetSelectedRowInfo(out string uniId, out int rowIdx)) return;
 
            var session = _db.CurrentSession!;
            if (uniId != session.Id)
            {
                ShowWarning("Видалення можливе лише для власного ЗВО.", "Немає доступу");
                return;
            }
 
            if (MessageBox.Show(
                "Видалити виділений рядок спеціальності?",
                "Підтвердження", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
                != DialogResult.Yes) return;
 
            bool ok = _db.RemoveSpecialty(session.Id, rowIdx);
            if (!ok) { ShowWarning("Не вдалося видалити рядок.", "Помилка"); return; }
 
            RefreshCurrentUniDisplay();
            SetStatus("Рядок видалено і зміни збережено.");
        }
 
        
        private bool CheckAdminOnOwnUni()
        {
            if (!_db.IsAdminLoggedIn)
            {
                ShowWarning("Потрібна авторизація.", "Немає доступу");
                return false;
            }
            return true;
        }
 
        private bool TryGetSelectedRowInfo(out string uniId, out int rowIdx)
        {
            uniId  = string.Empty;
            rowIdx = -1;
 
            if (_gridSpecialties.SelectedRows.Count == 0)
            {
                ShowWarning("Виберіть рядок у таблиці.", "Підказка");
                return false;
            }
 
            var row = _gridSpecialties.SelectedRows[0];
            uniId  = row.Cells["ColUniId"].Value?.ToString() ?? string.Empty;
            rowIdx = row.Cells["ColRowIndex"].Value is int i ? i : -1;
 
            if (rowIdx < 0)
            {
                ShowWarning("Не вдалося визначити індекс рядка.", "Помилка");
                return false;
            }
            return true;
        }
 
        private void RefreshCurrentUniDisplay()
        {
            _allUniversities = _db.Universities.ToList();
 
            string? name = _listBoxUniversities.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(name)) return;
 
            var uni = _allUniversities.FirstOrDefault(u => u.Name == name);
            if (uni != null) DisplayUniversitySpecialties(uni);
        }
 
        /// <summary>
        /// Відображає системне модальне вікно відомостей про розробників та технологічний стек проєкту.
        /// </summary>
        private void ShowAboutDialog(object? sender, EventArgs e)
        {
            MessageBox.Show(
                "Довідник бакалавра — Інформаційно-довідкова система ЗВО України\n\n" +
                "Версія: 3.0\n" +
                "Технологія: Windows Forms, .NET 8.0\n\n" +
                "Курсова робота — ХНУРЕ",
                "Про програму", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
 
        private void SetStatus(string message) => _statusLabel.Text = message;
 
        private static void ShowInfo(string message, string title) =>
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
 
        private static void ShowWarning(string message, string title) =>
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
 
        private static Label MakeLabel(string text) => new Label
        {
            Text      = text,
            Dock      = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight,
            Font      = new Font("Segoe UI", 9f),
            ForeColor = Color.FromArgb(60, 70, 90),
            Padding   = new Padding(0, 0, 6, 0)
        };
 
        private static Button MakeButton(string text, Color back, bool bold = false)
        {
            var btn = new Button
            {
                Text      = text,
                Font      = new Font("Segoe UI", 9.5f, bold ? FontStyle.Bold : FontStyle.Regular),
                Size      = new Size(150, 30),
                BackColor = back,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand,
                Margin    = new Padding(0, 3, 12, 3)
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }
 
        private static Button MakeAdminButton(string text, bool danger = false)
        {
            var btn = new Button
            {
                Text      = text,
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Size      = new Size(135, 28),
                BackColor = danger
                    ? Color.FromArgb(180, 50, 50)
                    : Color.FromArgb(40, 110, 190),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand,
                Margin    = new Padding(0, 0, 4, 0)
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }
    }
}