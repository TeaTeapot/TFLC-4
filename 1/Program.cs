using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace TextEditor
{
    public partial class MainForm : Form
    {
        private RichTextBox textBoxEditor;
        private TextBox textBoxOutput;
        private MenuStrip menuStrip;
        private ToolStrip toolStrip;
        private SplitContainer splitContainer;

        private ComboBox comboBoxSearchType;
        private Button buttonSearch;
        private Label labelMatchCount;
        private DataGridView dataGridViewResults;
        private Panel searchPanel;

        private readonly Regex[] regexes = new Regex[]
        {
            new Regex(@"^\d{16}$", RegexOptions.Multiline),
            new Regex(@"^[a-zA-Zа-яА-Я0-9!@#$%^&()+{};,_.-]{1,255}\.(docx|png|cs|txt|md|jpg|cpp|py|exe|pptx)$", RegexOptions.Multiline),
            new Regex(@"^10\.\d{4,}\/[a-zA-Z0-9_.\-~+()\/,;:=]+$", RegexOptions.Multiline)
        };

        private readonly string[] searchTypeNames = new string[]
        {
            "16 цифр подряд",
            "Имя файла (1-255 символов + расширение)",
            "DOI 10.xxxx/..."
        };

        public MainForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Текстовый редактор - Языковой процессор";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(500, 400);

            splitContainer = new SplitContainer();
            splitContainer.Dock = DockStyle.Fill;
            splitContainer.Orientation = Orientation.Horizontal;
            splitContainer.SplitterDistance = this.ClientSize.Height * 2 / 3;
            splitContainer.SplitterWidth = 5;
            splitContainer.Panel1MinSize = 100;
            splitContainer.Panel2MinSize = 100;

            textBoxEditor = new RichTextBox();
            textBoxEditor.Multiline = true;
            textBoxEditor.ScrollBars = RichTextBoxScrollBars.Both;
            textBoxEditor.Dock = DockStyle.Fill;
            textBoxEditor.Font = new Font("Consolas", 10);
            textBoxEditor.AcceptsTab = true;
            textBoxEditor.WordWrap = false;

            textBoxOutput = new TextBox();
            textBoxOutput.Multiline = true;
            textBoxOutput.ScrollBars = ScrollBars.Both;
            textBoxOutput.Dock = DockStyle.Fill;
            textBoxOutput.ReadOnly = true;
            textBoxOutput.Font = new Font("Consolas", 10);
            textBoxOutput.BackColor = Color.Lavender;
            textBoxOutput.ForeColor = Color.DarkBlue;

            splitContainer.Panel1.Controls.Add(textBoxEditor);
            splitContainer.Panel2.Controls.Add(textBoxOutput);

            menuStrip = new MenuStrip();
            menuStrip.Dock = DockStyle.Top;

            ToolStripMenuItem fileMenu = new ToolStripMenuItem("Файл");
            fileMenu.DropDownItems.Add("Создать", null, OnFileNew);
            fileMenu.DropDownItems.Add("Открыть", null, OnFileOpen);
            fileMenu.DropDownItems.Add("Сохранить", null, OnFileSave);
            fileMenu.DropDownItems.Add("Сохранить как", null, OnFileSaveAs);
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add("Выход", null, OnFileExit);

            ToolStripMenuItem editMenu = new ToolStripMenuItem("Правка");
            editMenu.DropDownItems.Add("Отменить", null, OnEditUndo);
            editMenu.DropDownItems.Add(new ToolStripSeparator());
            editMenu.DropDownItems.Add("Вырезать", null, OnEditCut);
            editMenu.DropDownItems.Add("Копировать", null, OnEditCopy);
            editMenu.DropDownItems.Add("Вставить", null, OnEditPaste);
            editMenu.DropDownItems.Add("Удалить", null, OnEditDelete);
            editMenu.DropDownItems.Add(new ToolStripSeparator());
            editMenu.DropDownItems.Add("Выделить все", null, OnEditSelectAll);

            ToolStripMenuItem textMenu = new ToolStripMenuItem("Текст");
            ToolStripMenuItem startMenu = new ToolStripMenuItem("Пуск");
            ToolStripMenuItem helpMenu = new ToolStripMenuItem("Справка");
            helpMenu.DropDownItems.Add("Вызов справки", null, OnHelp);
            helpMenu.DropDownItems.Add("О программе", null, OnAbout);

            menuStrip.Items.AddRange(new ToolStripItem[] { fileMenu, editMenu, textMenu, startMenu, helpMenu });

            toolStrip = new ToolStrip();
            toolStrip.Dock = DockStyle.Top;
            toolStrip.ImageScalingSize = new Size(20, 20);

            toolStrip.Items.Add(new ToolStripButton("📄", null, OnFileNew, "New"));
            toolStrip.Items.Add(new ToolStripButton("📂", null, OnFileOpen, "Open"));
            toolStrip.Items.Add(new ToolStripButton("💾", null, OnFileSave, "Save"));
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(new ToolStripButton("↶", null, OnEditUndo, "Undo"));
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(new ToolStripButton("✂", null, OnEditCut, "Cut"));
            toolStrip.Items.Add(new ToolStripButton("📋", null, OnEditCopy, "Copy"));
            toolStrip.Items.Add(new ToolStripButton("📝", null, OnEditPaste, "Paste"));
            toolStrip.Items.Add(new ToolStripButton("❌", null, OnEditDelete, "Delete"));
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(new ToolStripButton("🔍", null, OnHelp, "Help"));
            toolStrip.Items.Add(new ToolStripButton("ℹ", null, OnAbout, "About"));

            SetupSearchPanel();

            this.Controls.Add(splitContainer);
            this.Controls.Add(toolStrip);
            this.Controls.Add(menuStrip);
            this.Controls.Add(searchPanel);

            this.MainMenuStrip = menuStrip;
            this.Resize += MainForm_Resize;
        }

        private void SetupSearchPanel()
        {
            searchPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 220,
                BackColor = Color.FromArgb(240, 240, 240)
            };

            comboBoxSearchType = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(10, 10),
                Width = 220,
                Font = new Font("Segoe UI", 9)
            };
            comboBoxSearchType.Items.AddRange(searchTypeNames);
            comboBoxSearchType.SelectedIndex = 0;

            buttonSearch = new Button
            {
                Text = "Найти",
                Location = new Point(240, 9),
                Width = 80,
                Height = 25,
                BackColor = Color.SteelBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            buttonSearch.Click += ButtonSearch_Click;

            labelMatchCount = new Label
            {
                Text = "Совпадений: 0",
                Location = new Point(335, 12),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            dataGridViewResults = new DataGridView
            {
                Location = new Point(10, 45),
                Width = searchPanel.Width - 20,
                Height = 165,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                Font = new Font("Consolas", 9)
            };
            dataGridViewResults.Columns.Add("Match", "Найденная подстрока");
            dataGridViewResults.Columns.Add("Position", "Позиция (строка, символ)");
            dataGridViewResults.Columns.Add("Length", "Длина");
            dataGridViewResults.Columns[0].Width = 300;
            dataGridViewResults.Columns[1].Width = 140;
            dataGridViewResults.Columns[2].Width = 60;

            dataGridViewResults.SelectionChanged += DataGridViewResults_SelectionChanged;

            searchPanel.Controls.Add(comboBoxSearchType);
            searchPanel.Controls.Add(buttonSearch);
            searchPanel.Controls.Add(labelMatchCount);
            searchPanel.Controls.Add(dataGridViewResults);
        }

        private void ButtonSearch_Click(object sender, EventArgs e)
        {
            string text = textBoxEditor.Text;
            if (string.IsNullOrEmpty(text))
            {
                MessageBox.Show("Нет текста для поиска.\nВведите текст в верхней области.",
                    "Поиск", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int selectedIndex = comboBoxSearchType.SelectedIndex;
            Regex regex = regexes[selectedIndex];

            var matches = regex.Matches(text);
            dataGridViewResults.Rows.Clear();

            if (matches.Count == 0)
            {
                labelMatchCount.Text = "Совпадений: 0";
                MessageBox.Show("Совпадений не найдено.", "Поиск",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            foreach (Match match in matches)
            {
                int lineNumber = 1;
                int charIndex = 0;
                int currentPos = 0;

                for (int i = 0; i < text.Length && currentPos < match.Index; i++)
                {
                    if (text[i] == '\n')
                    {
                        lineNumber++;
                        charIndex = 0;
                    }
                    else
                    {
                        charIndex++;
                    }
                    currentPos++;
                }

                dataGridViewResults.Rows.Add(
                    match.Value,
                    $"Строка {lineNumber}, символ {charIndex + 1}",
                    match.Length
                );
            }

            labelMatchCount.Text = $"Совпадений: {matches.Count}";

            ResetHighlighting();
        }

        private void DataGridViewResults_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridViewResults.SelectedRows.Count == 0)
                return;

            string selectedMatch = dataGridViewResults.SelectedRows[0].Cells[0].Value?.ToString();
            if (string.IsNullOrEmpty(selectedMatch))
                return;

            string text = textBoxEditor.Text;
            int index = text.IndexOf(selectedMatch);

            if (index >= 0)
            {
                ResetHighlighting();

                textBoxEditor.Select(index, selectedMatch.Length);
                textBoxEditor.Focus();
                textBoxEditor.SelectionBackColor = Color.Yellow;
            }
        }

        private void ResetHighlighting()
        {
            int selectionStart = textBoxEditor.SelectionStart;
            int selectionLength = textBoxEditor.SelectionLength;

            textBoxEditor.SelectAll();
            textBoxEditor.SelectionBackColor = textBoxEditor.BackColor;

            textBoxEditor.Select(selectionStart, selectionLength);
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (this.WindowState != FormWindowState.Minimized)
            {
                splitContainer.SplitterDistance = Math.Max(
                    100,
                    Math.Min(
                        splitContainer.Height - 100,
                        this.ClientSize.Height * 2 / 3
                    )
                );
            }
        }

        private void OnFileNew(object sender, EventArgs e)
        {
            if (ConfirmSaveChanges())
            {
                textBoxEditor.Clear();
                textBoxOutput.Clear();
                dataGridViewResults.Rows.Clear();
                labelMatchCount.Text = "Совпадений: 0";
                ResetHighlighting();
                this.Text = "Текстовый редактор - Языковой процессор [Новый файл]";
            }
        }

        private void OnFileOpen(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (ConfirmSaveChanges())
                {
                    textBoxEditor.Text = System.IO.File.ReadAllText(dialog.FileName);
                    dataGridViewResults.Rows.Clear();
                    labelMatchCount.Text = "Совпадений: 0";
                    ResetHighlighting();
                    this.Text = $"Текстовый редактор - Языковой процессор [{dialog.FileName}]";
                }
            }
        }

        private void OnFileSave(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                System.IO.File.WriteAllText(dialog.FileName, textBoxEditor.Text);
                this.Text = $"Текстовый редактор - Языковой процессор [{dialog.FileName}]";
                MessageBox.Show("Файл сохранен успешно!", "Сохранение",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void OnFileSaveAs(object sender, EventArgs e) => OnFileSave(sender, e);

        private void OnFileExit(object sender, EventArgs e)
        {
            if (ConfirmSaveChanges())
            {
                this.Close();
            }
        }

        private bool ConfirmSaveChanges()
        {
            if (!string.IsNullOrEmpty(textBoxEditor.Text))
            {
                DialogResult result = MessageBox.Show(
                    "Сохранить изменения в текущем файле?",
                    "Подтверждение",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    OnFileSave(this, EventArgs.Empty);
                    return true;
                }
                else if (result == DialogResult.No)
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        private void OnEditUndo(object sender, EventArgs e)
        {
            if (textBoxEditor.CanUndo)
            {
                textBoxEditor.Undo();
            }
        }

        private void OnEditCut(object sender, EventArgs e) => textBoxEditor.Cut();
        private void OnEditCopy(object sender, EventArgs e) => textBoxEditor.Copy();
        private void OnEditPaste(object sender, EventArgs e) => textBoxEditor.Paste();
        private void OnEditDelete(object sender, EventArgs e) => textBoxEditor.SelectedText = "";
        private void OnEditSelectAll(object sender, EventArgs e) => textBoxEditor.SelectAll();

        private void OnHelp(object sender, EventArgs e)
        {
            string helpText = @"ТЕКСТОВЫЙ РЕДАКТОР - ЯЗЫКОВОЙ ПРОЦЕССОР

СПРАВКА ПО ФУНКЦИЯМ ПРОГРАММЫ

===========================================
МЕНЮ ""ФАЙЛ""
===========================================
• Создать - создает новый пустой документ
  Если текущий документ содержит несохраненные изменения,
  программа предложит сохранить их перед созданием нового.

• Открыть - открывает существующий текстовый файл
  Поддерживаемые форматы: .txt и любые другие текстовые файлы.
  Перед открытием проверяет наличие несохраненных изменений.

• Сохранить - сохраняет текущий документ
  Если файл еще не был сохранен, открывается диалог
  для указания имени и расположения файла.

• Сохранить как - сохраняет документ с новым именем
  Всегда открывает диалог выбора файла для сохранения.

• Выход - закрывает программу
  При наличии несохраненных изменений предлагает их сохранить.

===========================================
МЕНЮ ""ПРАВКА""
===========================================
• Отменить - отменяет последнее действие
  Работает для операций ввода, удаления, вставки текста.

• Вырезать - удаляет выделенный текст и копирует его в буфер обмена
• Копировать - копирует выделенный текст в буфер обмена
• Вставить - вставляет текст из буфера обмена в текущую позицию курсора
• Удалить - удаляет выделенный текст без копирования в буфер обмена
• Выделить все - выделяет весь текст в области редактирования

===========================================
ПОИСК ПО РЕГУЛЯРНЫМ ВЫРАЖЕНИЯМ
===========================================
В нижней панели доступны три типа поиска:
1. 16 цифр подряд - находит строки, состоящие ровно из 16 цифр
2. Имя файла - находит имена файлов с допустимыми расширениями
3. DOI 10.xxxx/... - находит идентификаторы DOI формата 10.xxxx/...

Результаты отображаются в таблице с указанием позиции и длины.
При выборе строки в таблице соответствующая подстрока подсвечивается желтым.

===========================================
МЕНЮ ""СПРАВКА""
===========================================
• Вызов справки - открывает данное окно справки
• О программе - показывает информацию о версии и авторе

===========================================
ПАНЕЛЬ ИНСТРУМЕНТОВ
===========================================
Панель инструментов содержит кнопки для быстрого доступа
к наиболее часто используемым функциям.

===========================================
ИНТЕРФЕЙС ПРОГРАММЫ
===========================================
Программа разделена на три основные области:
1. Верхняя область - редактирование текста (RichTextBox)
2. Средняя область - вывод результатов языкового процессора
3. Нижняя область - поиск по регулярным выражениям

Размер областей можно изменять, перетаскивая разделитель.";

            MessageBox.Show(helpText, "Справка - Реализованные функции",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnAbout(object sender, EventArgs e)
        {
            MessageBox.Show(
                "Текстовый редактор v2.0\n" +
                "Языковой процессор (будущая версия)\n" +
                "Добавлен поиск по регулярным выражениям\n" +
                "Подсветка найденных фрагментов\n" +
                "(c) 2026",
                "О программе",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}