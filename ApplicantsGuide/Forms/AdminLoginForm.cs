using System;
using System.Drawing;
using System.Windows.Forms;
using ApplicantsGuide.Services;

namespace ApplicantsGuide.Forms
{
    
    /// <summary>
    /// Форма авторизації представника закладу вищої освіти (ЗВО).
    /// Забезпечує автентифікацію для переходу програми в режим адміністратора.
    /// </summary>
    public class AdminLoginForm : Form
    {
       

        private readonly DatabaseManager _db;

        private TextBox _txtLogin    = null!;
        private TextBox _txtPassword = null!;
        private Button  _btnLogin    = null!;
        private Button  _btnCancel   = null!;
        private Label   _lblError    = null!;

        
        /// <summary>
        /// Ініціалізує новий екземпляр класу <see cref="AdminLoginForm"/> із посиланням на менеджер бази даних.
        /// </summary>
        public AdminLoginForm(DatabaseManager db)
        {
            _db = db;
            InitFormParameters();
            BuildUI();
        }


        /// <summary>
        /// Встановлює початкові графічні та системні параметри вікна діалогу авторизації.
        /// </summary>
        private void InitFormParameters()
        {
            Text            = "Вхід для представника ЗВО";
            Size            = new Size(420, 300);
            MinimumSize     = new Size(420, 300);
            MaximumSize     = new Size(420, 300); 
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;
            BackColor       = Color.FromArgb(245, 247, 250);
            Font            = new Font("Segoe UI", 9.5f, FontStyle.Regular, GraphicsUnit.Point);
        }


        /// <summary>
        /// Створює елементи керування графічного інтерфейсу користувача та розміщує їх у формі.
        /// </summary>
        private void BuildUI()
        {
            var lblTitle = new Label
            {
                Text      = "🔐  Авторизація представника ЗВО",
                Dock      = DockStyle.Top,
                Height    = 52,
                Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(30, 90, 160),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var layout = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                RowCount    = 5,
                ColumnCount = 2,
                BackColor   = Color.Transparent,
                Padding     = new Padding(24, 16, 24, 16)
            };

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90f));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38f)); 
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38f)); 
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28f)); 
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f)); 
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42f)); 

            layout.Controls.Add(MakeLabel("Логін:"), 0, 0);

            _txtLogin = new TextBox
            {
                Dock        = DockStyle.Fill,
                Font        = new Font("Segoe UI", 10f),
                Margin      = new Padding(0, 4, 0, 4),
                MaxLength   = 64,
                PlaceholderText = "введіть логін..."
            };
            _txtLogin.KeyDown += OnEnterPressed;
            layout.Controls.Add(_txtLogin, 1, 0);

            layout.Controls.Add(MakeLabel("Пароль:"), 0, 1);

            _txtPassword = new TextBox
            {
                Dock             = DockStyle.Fill,
                Font             = new Font("Segoe UI", 10f),
                Margin           = new Padding(0, 4, 0, 4),
                MaxLength        = 64,
                PasswordChar     = '●',
                PlaceholderText  = "введіть пароль..."
            };
            _txtPassword.KeyDown += OnEnterPressed;
            layout.Controls.Add(_txtPassword, 1, 1);

            _lblError = new Label
            {
                Text      = "⚠  Невірний логін або пароль.",
                Dock      = DockStyle.Fill,
                ForeColor = Color.FromArgb(180, 30, 30),
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Visible   = false
            };
            layout.Controls.Add(_lblError, 0, 2);
            layout.SetColumnSpan(_lblError, 2);

            var btnPanel = new FlowLayoutPanel
            {
                Dock          = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                BackColor     = Color.Transparent,
                Padding       = new Padding(0)
            };
            layout.Controls.Add(btnPanel, 0, 4);
            layout.SetColumnSpan(btnPanel, 2);

            _btnCancel = new Button
            {
                Text      = "Скасувати",
                Size      = new Size(110, 32),
                BackColor = Color.FromArgb(160, 170, 185),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand,
                Margin    = new Padding(8, 0, 0, 0)
            };
            _btnCancel.FlatAppearance.BorderSize = 0;
            _btnCancel.Click += (s, e) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };
            btnPanel.Controls.Add(_btnCancel);

            _btnLogin = new Button
            {
                Text      = "Увійти",
                Size      = new Size(110, 32),
                BackColor = Color.FromArgb(30, 90, 160),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand,
                Margin    = new Padding(0)
            };
            _btnLogin.FlatAppearance.BorderSize = 0;
            _btnLogin.Click += BtnLogin_Click;
            btnPanel.Controls.Add(_btnLogin);

            Controls.Add(layout);
            Controls.Add(lblTitle); 

            AcceptButton = _btnLogin;
        }

        /// <summary>
        /// Обробляє подію натискання кнопки автентифікації. Перевіряє коректність заповнення полів та викликає метод валідації бази даних.
        /// </summary>
        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            string login    = _txtLogin.Text.Trim();
            string password = _txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                _lblError.Text    = "⚠  Заповніть обидва поля.";
                _lblError.Visible = true;
                return;
            }

            if (_db.TryLogin(login, password))
            {
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                _lblError.Text     = "⚠  Невірний логін або пароль.";
                _lblError.Visible  = true;
                _txtPassword.Clear();
                _txtPassword.Focus();
            }
        }

        /// <summary>
        /// Обробляє подію натискання клавіш у текстових полях введення даних для автоматичного сабміту форми за натисканням Enter.
        /// </summary>
        private void OnEnterPressed(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; 
                BtnLogin_Click(sender, EventArgs.Empty);
            }
        }

        
        /// <summary>
        /// Фабричний метод для створення текстових міток інтерфейсу зі спільними графічними параметрами.
        /// </summary>
        private static Label MakeLabel(string text) => new Label
        {
            Text      = text,
            Dock      = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight,
            Font      = new Font("Segoe UI", 9.5f),
            ForeColor = Color.FromArgb(60, 70, 90),
            Padding   = new Padding(0, 0, 8, 0)
        };
    }
}