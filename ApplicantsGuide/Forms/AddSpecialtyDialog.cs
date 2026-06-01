using System;
using System.Drawing;
using System.Windows.Forms;
using ApplicantsGuide.Models;

namespace ApplicantsGuide.Forms
{

    /// <summary>
    /// Модальне діалогове вікно для введення даних нової спеціальності.
    /// Використовується представниками ЗВО в режимі адміністратора додатка.
    /// </summary>
    public class AddSpecialtyDialog : Form
    {
        private TextBox _txtCode = null!;
        private TextBox _txtName = null!;
        private ComboBox _cboForm = null!;
        private ComboBox _cboFinance = null!;
        private TextBox _txtScore = null!;
        private TextBox _txtPrice = null!;

        private Button _btnSave = null!;
        private Button _btnCancel = null!;

        /// <summary>
        /// Об'єкт створеної спеціальності, доступний після успішного закриття форми.
        /// </summary>
        public Specialty? CreatedSpecialty { get; private set; }

        /// <summary>
        /// Ініціалізує новий екземпляр класу <see cref="AddSpecialtyDialog"/>.
        /// </summary>
        public AddSpecialtyDialog()
        {
            InitFormParameters();
            BuildUI();
        }

        private void InitFormParameters()
        {
            Text = "Додавання нового рядка спеціальності";
            Size = new Size(460, 360);
            MinimumSize = new Size(460, 360);
            MaximumSize = new Size(460, 360);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.FromArgb(245, 247, 250);
            Font = new Font("Segoe UI", 9.5f, FontStyle.Regular, GraphicsUnit.Point);
        }

        private void BuildUI()
        {
            
            // Основна таблиця (займає весь простір, що залишився над кнопками)
            var tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 7,
                ColumnCount = 2,
                Padding = new Padding(20, 15, 20, 15),
                BackColor = Color.Transparent
            };

            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130f));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

            for (int i = 0; i < 6; i++)
                tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 36f));
            tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            // Заповнюємо полями
            tbl.Controls.Add(MakeLabel("Код спеціальності:"), 0, 0);
            _txtCode = new TextBox { Dock = DockStyle.Fill, PlaceholderText = "наприклад: 121" };
            tbl.Controls.Add(_txtCode, 1, 0);

            tbl.Controls.Add(MakeLabel("Назва напряму:"), 0, 1);
            _txtName = new TextBox { Dock = DockStyle.Fill, PlaceholderText = "наприклад: Комп'ютерні науки" };
            tbl.Controls.Add(_txtName, 1, 1);

            tbl.Controls.Add(MakeLabel("Форма навчання:"), 0, 2);
            _cboForm = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            _cboForm.Items.AddRange(new object[] { "Денна", "Заочна" });
            _cboForm.SelectedIndex = 0;
            tbl.Controls.Add(_cboForm, 1, 2);

            tbl.Controls.Add(MakeLabel("Фінансування:"), 0, 3);
            _cboFinance = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            _cboFinance.Items.AddRange(new object[] { "Бюджет", "Контракт" });
            _cboFinance.SelectedIndex = 0;
            tbl.Controls.Add(_cboFinance, 1, 3);

            tbl.Controls.Add(MakeLabel("Прохідний бал (НМТ):"), 0, 4);
            _txtScore = new TextBox { Dock = DockStyle.Fill, PlaceholderText = "наприклад: 154.5 (або 0 якщо немає)" };
            tbl.Controls.Add(_txtScore, 1, 4);

            tbl.Controls.Add(MakeLabel("Вартість (грн/рік):"), 0, 5);
            _txtPrice = new TextBox { Dock = DockStyle.Fill, PlaceholderText = "наприклад: 36000" };
            tbl.Controls.Add(_txtPrice, 1, 5);

            var btnPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 45,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 5, 20, 0),
                BackColor = Color.FromArgb(230, 235, 245)
            };

            _btnCancel = new Button { Text = "Скасувати", Size = new Size(100, 30), BackColor = Color.FromArgb(160, 170, 185), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            _btnCancel.FlatAppearance.BorderSize = 0;
            _btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            _btnSave = new Button { Text = "Зберегти", Size = new Size(100, 30), BackColor = Color.FromArgb(30, 90, 160), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            _btnSave.FlatAppearance.BorderSize = 0;
            _btnSave.Click += BtnSave_Click;

            btnPanel.Controls.Add(_btnCancel);
            btnPanel.Controls.Add(_btnSave);

            // Додаємо елементи на форму в правильному порядку контейнеризації
            Controls.Add(tbl);
            Controls.Add(btnPanel);
            AcceptButton = _btnSave;
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtCode.Text) || string.IsNullOrWhiteSpace(_txtName.Text))
            {
                MessageBox.Show("Будь ласка, заповніть код та назву спеціальності.", "Помилка валідації", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            double.TryParse(_txtScore.Text.Replace(',', '.').Trim(), out double score);
            int.TryParse(_txtPrice.Text.Trim(), out int price);

            CreatedSpecialty = new Specialty
            {
                Code = _txtCode.Text.Trim(),
                Name = _txtName.Text.Trim(),
                Form = _cboForm.SelectedItem?.ToString() ?? "Денна",
                Finance = _cboFinance.SelectedItem?.ToString() ?? "Бюджет",
                MinScore = score,
                Price = price
            };

            DialogResult = DialogResult.OK;
            Close();
        }

        private static Label MakeLabel(string text) => new Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight,
            Font = new Font("Segoe UI", 9f),
            ForeColor = Color.FromArgb(60, 70, 90),
            Padding = new Padding(0, 0, 6, 0)
        };
    }
}