using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Moughoof
{
    public class MainForm : Form
    {
        private Panel searchGroup;
        private Panel resultsGroup;

        private TextBox inputBox;
        private Label pronunciationLabel;

        private Label matchLabel;
        private RadioButton firstRadio;
        private RadioButton lastRadio;
        private NumericUpDown matchCountBox;

        private Button searchButton;
        private Label statusLabel;

        private Label sortLabel;
        private RadioButton alphabetRadio;
        private RadioButton lengthRadio;

        private ListBox resultsListBox;

        private Button exportButton;
        private Label databaseLabel;

        private readonly Color rowColor1 =
            Color.FromArgb(40, 40, 40);

        private readonly Color rowColor2 =
            Color.FromArgb(56, 56, 56);

        private readonly Color selectedColor =
            Color.FromArgb(65, 100, 145);

        private readonly List<WordEntry> dictionary =
            new List<WordEntry>();

        private List<WordEntry> currentResults =
            new List<WordEntry>();

        private string lastSearchWord = "";
        private string activeDatabaseName = "Default";
        private bool isUpdating = false;

        private class WordEntry
        {
            public string Persian { get; set; }
            public string Pronunciation { get; set; }

            public WordEntry(
                string persian,
                string pronunciation)
            {
                Persian = persian;
                Pronunciation = pronunciation;
            }
        }

        public MainForm()
        {
            InitializeForm();
            InitializeControls();
            ConnectEvents();

            LoadDefaultDictionary();
        }

        // ================================================
        // FORM
        // ================================================

        private void InitializeForm()
        {
            Text = "Moughoof";

            ClientSize = new Size(460, 650);
            MinimumSize = new Size(460, 530);
            MaximumSize = new Size(460, 2000);

            MinimizeBox = true;
            MaximizeBox = false;

            FormBorderStyle = FormBorderStyle.Sizable;

            BackColor = Color.FromArgb(32, 32, 32);
            ForeColor = Color.White;

            Font = new Font("Segoe UI", 9f);
            KeyPreview = true;
        }

        // ================================================
        // CONTROLS
        // ================================================

        private void InitializeControls()
        {
            // GROUP 1

            searchGroup = new Panel
            {
                Location = new Point(10, 10),
                Size = new Size(440, 170),
                BackColor = Color.FromArgb(32, 32, 32)
            };

            Controls.Add(searchGroup);

            // INPUT

            inputBox = new TextBox
            {
                Location = new Point(5, 5),
                Size = new Size(430, 34),

                Font = new Font(
                    "Segoe UI",
                    11.5f,
                    FontStyle.Bold),

                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = Color.Gray,
                BorderStyle = BorderStyle.FixedSingle,

                Text = "Input word",
                RightToLeft = RightToLeft.No
            };

            searchGroup.Controls.Add(inputBox);

            // PRONUNCIATION

            pronunciationLabel = new Label
            {
                Location = new Point(5, 43),
                Size = new Size(430, 20),

                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,

                ForeColor = Color.LightGray,
                BackColor = Color.Transparent
            };

            searchGroup.Controls.Add(pronunciationLabel);

            // MATCH

            matchLabel = new Label
            {
                Location = new Point(5, 73),
                Size = new Size(50, 28),

                Text = "MATCH",
                TextAlign = ContentAlignment.MiddleLeft,

                ForeColor = Color.White,
                BackColor = Color.Transparent
            };

            searchGroup.Controls.Add(matchLabel);

            firstRadio = new RadioButton
            {
                Location = new Point(62, 75),
                Size = new Size(62, 24),

                Text = "FIRST",

                ForeColor = Color.White,
                BackColor = Color.Transparent
            };

            searchGroup.Controls.Add(firstRadio);

            lastRadio = new RadioButton
            {
                Location = new Point(125, 75),
                Size = new Size(58, 24),

                Text = "LAST",

                ForeColor = Color.White,
                BackColor = Color.Transparent,

                Checked = true
            };

            searchGroup.Controls.Add(lastRadio);

            matchCountBox = new NumericUpDown
            {
                Location = new Point(188, 73),
                Size = new Size(48, 25),

                Minimum = 1,
                Maximum = 50,
                Value = 2,

                TextAlign = HorizontalAlignment.Center
            };

            searchGroup.Controls.Add(matchCountBox);

            // SEARCH

            searchButton = new Button
            {
                Location = new Point(5, 112),
                Size = new Size(100, 32),

                Text = "SEARCH",

                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(55, 55, 55),
                ForeColor = Color.White
            };

            searchButton.FlatAppearance.BorderColor =
                Color.FromArgb(90, 90, 90);

            searchGroup.Controls.Add(searchButton);

            // RESULTS COUNT

            statusLabel = new Label
            {
                Location = new Point(120, 115),
                Size = new Size(310, 25),

                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,

                ForeColor = Color.LightGray,
                BackColor = Color.Transparent,

                Text = "0 Results"
            };

            searchGroup.Controls.Add(statusLabel);

            // GROUP 2

            resultsGroup = new Panel
            {
                Location = new Point(10, 185),
                Size = new Size(440, 390),

                Anchor =
                    AnchorStyles.Top |
                    AnchorStyles.Bottom |
                    AnchorStyles.Left |
                    AnchorStyles.Right,

                BackColor = Color.FromArgb(32, 32, 32)
            };

            Controls.Add(resultsGroup);

            // SORT

            sortLabel = new Label
            {
                Location = new Point(5, 2),
                Size = new Size(42, 28),

                Text = "SORT",
                TextAlign = ContentAlignment.MiddleLeft,

                ForeColor = Color.White,
                BackColor = Color.Transparent
            };

            resultsGroup.Controls.Add(sortLabel);

            alphabetRadio = new RadioButton
            {
                Location = new Point(55, 4),
                Size = new Size(105, 24),

                Text = "ALPHABET",

                ForeColor = Color.White,
                BackColor = Color.Transparent,

                Checked = true
            };

            resultsGroup.Controls.Add(alphabetRadio);

            lengthRadio = new RadioButton
            {
                Location = new Point(165, 4),
                Size = new Size(75, 24),

                Text = "LENGTH",

                ForeColor = Color.White,
                BackColor = Color.Transparent
            };

            resultsGroup.Controls.Add(lengthRadio);

            // RESULTS LIST

            resultsListBox = new ListBox
            {
                Location = new Point(5, 35),
                Size = new Size(430, 345),

                Anchor =
                    AnchorStyles.Top |
                    AnchorStyles.Bottom |
                    AnchorStyles.Left |
                    AnchorStyles.Right,

                BackColor = rowColor1,
                ForeColor = Color.White,

                BorderStyle = BorderStyle.FixedSingle,

                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 27,
                IntegralHeight = false,

                Font = new Font("Segoe UI", 10f)
            };

            resultsGroup.Controls.Add(resultsListBox);

            // DATABASE LABEL

            databaseLabel = new Label
            {
                Location = new Point(15, 585),
                Size = new Size(330, 25),

                Anchor =
                    AnchorStyles.Bottom |
                    AnchorStyles.Left,

                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,

                Text = "Database: Default, 0 words",

                ForeColor = Color.LightSkyBlue,
                BackColor = Color.Transparent,

                Cursor = Cursors.Hand
            };

            databaseLabel.Font = new Font(
                databaseLabel.Font,
                FontStyle.Underline);

            Controls.Add(databaseLabel);

            // EXPORT

            exportButton = new Button
            {
                Location = new Point(365, 582),
                Size = new Size(80, 32),

                Anchor =
                    AnchorStyles.Bottom |
                    AnchorStyles.Right,

                Text = "EXPORT",

                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(55, 55, 55),
                ForeColor = Color.White
            };

            exportButton.FlatAppearance.BorderColor =
                Color.FromArgb(90, 90, 90);

            Controls.Add(exportButton);
        }

        // ================================================
        // EVENTS
        // ================================================

        private void ConnectEvents()
        {
            inputBox.Enter += InputBox_Enter;
            inputBox.TextChanged += InputBox_TextChanged;
            inputBox.KeyDown += InputBox_KeyDown;

            searchButton.Click += SearchButton_Click;

            firstRadio.CheckedChanged += MatchOption_Changed;
            lastRadio.CheckedChanged += MatchOption_Changed;

            matchCountBox.ValueChanged +=
                MatchCountBox_ValueChanged;

            alphabetRadio.CheckedChanged +=
                SortOption_Changed;

            lengthRadio.CheckedChanged +=
                SortOption_Changed;

            resultsListBox.DrawItem +=
                ResultsListBox_DrawItem;

            databaseLabel.Click +=
                DatabaseLabel_Click;
        }

        // ================================================
        // DEFAULT DATABASE
        // ================================================

        private void LoadDefaultDictionary()
        {
            string path = Path.Combine(
                Application.StartupPath,
                "PS.txt");

            if (!File.Exists(path))
            {
                dictionary.Clear();

                activeDatabaseName = "Default";

                UpdateDatabaseLabel();
                ClearSearchResults();

                statusLabel.Text = "PS.txt not found";
                return;
            }

            try
            {
                List<WordEntry> loaded =
                    ReadDictionaryFile(path);

                dictionary.Clear();
                dictionary.AddRange(loaded);

                activeDatabaseName = "Default";

                UpdateDatabaseLabel();
                ClearSearchResults();
                UpdatePronunciationPreview();

                statusLabel.Text = "Database loaded";
            }
            catch (Exception ex)
            {
                dictionary.Clear();

                UpdateDatabaseLabel();
                ClearSearchResults();

                statusLabel.Text = "Database error";

                MessageBox.Show(
                    "Could not load PS.txt.\r\n\r\n" +
                    ex.Message,
                    "Moughoof",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        // ================================================
        // READ TAB-SEPARATED DICTIONARY
        // ================================================

        private List<WordEntry> ReadDictionaryFile(
            string path)
        {
            List<WordEntry> loaded =
                new List<WordEntry>();

            using (StreamReader reader = new StreamReader(
                path,
                Encoding.UTF8,
                true))
            {
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    int tabIndex = line.IndexOf('\t');

                    if (tabIndex < 0)
                        continue;

                    string persian =
                        line.Substring(0, tabIndex);

                    string pronunciation =
                        line.Substring(tabIndex + 1);

                    if (string.IsNullOrWhiteSpace(persian))
                        continue;

                    if (string.IsNullOrWhiteSpace(pronunciation))
                        continue;

                    loaded.Add(
                        new WordEntry(
                            persian,
                            pronunciation));
                }
            }

            return loaded;
        }

        // ================================================
        // LOAD CUSTOM DATABASE
        // ================================================

        private void DatabaseLabel_Click(
            object sender,
            EventArgs e)
        {
            using (OpenFileDialog dialog =
                new OpenFileDialog())
            {
                dialog.Title = "Load Dictionary";

                dialog.Filter =
                    "Text files (*.txt)|*.txt|All files (*.*)|*.*";

                dialog.CheckFileExists = true;
                dialog.Multiselect = false;

                if (dialog.ShowDialog(this) !=
                    DialogResult.OK)
                {
                    return;
                }

                try
                {
                    List<WordEntry> loaded =
                        ReadDictionaryFile(dialog.FileName);

                    dictionary.Clear();
                    dictionary.AddRange(loaded);

                    activeDatabaseName =
                        Path.GetFileName(dialog.FileName);

                    UpdateDatabaseLabel();

                    ClearSearchResults();
                    UpdatePronunciationPreview();

                    statusLabel.Text = "Database loaded";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Could not load the selected dictionary.\r\n\r\n" +
                        ex.Message,
                        "Moughoof",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }
        }

        private void UpdateDatabaseLabel()
        {
            databaseLabel.Text =
                "Database: " +
                activeDatabaseName +
                ", " +
                dictionary.Count.ToString() +
                " words";
        }

        // ================================================
        // INPUT
        // ================================================

        private void InputBox_Enter(
            object sender,
            EventArgs e)
        {
            if (inputBox.Text == "Input word")
            {
                inputBox.Clear();
                inputBox.ForeColor = Color.White;
            }
        }

        private void InputBox_TextChanged(
            object sender,
            EventArgs e)
        {
            if (isUpdating)
                return;

            UpdatePronunciationPreview();
        }

        private void InputBox_KeyDown(
            object sender,
            KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                PerformSearch();
            }
        }

        private string GetInputWord()
        {
            string word = inputBox.Text.Trim();

            if (word == "Input word")
                return "";

            return word;
        }

        // ================================================
        // PERSIAN LOOKUP NORMALIZATION
        // ================================================

        private string NormalizePersianLookup(string value)
        {
            if (value == null)
                return "";

            // Treat a ZWNJ and a regular space equivalently
            // for lookup, without changing stored display text.

            return value
                .Replace('\u200C', ' ')
                .Trim();
        }

        private WordEntry FindExactWord(string word)
        {
            string normalizedWord =
                NormalizePersianLookup(word);

            return dictionary.FirstOrDefault(
                item => string.Equals(
                    NormalizePersianLookup(item.Persian),
                    normalizedWord,
                    StringComparison.Ordinal));
        }

        private void UpdatePronunciationPreview()
        {
            string word = GetInputWord();

            WordEntry entry = FindExactWord(word);

            pronunciationLabel.Text =
                entry == null
                    ? ""
                    : entry.Pronunciation;
        }

        // ================================================
        // SEARCH
        // ================================================

        private void SearchButton_Click(
            object sender,
            EventArgs e)
        {
            PerformSearch();
        }

        private void PerformSearch()
        {
            string word = GetInputWord();

            if (string.IsNullOrWhiteSpace(word))
            {
                lastSearchWord = "";

                ClearSearchResults();
                pronunciationLabel.Text = "";

                return;
            }

            WordEntry source = FindExactWord(word);

            if (source == null)
            {
                lastSearchWord = word;

                currentResults.Clear();
                RefreshResults();

                pronunciationLabel.Text = "";
                statusLabel.Text = "Word not found";

                return;
            }

            lastSearchWord = word;
            pronunciationLabel.Text = source.Pronunciation;

            string pronunciation = source.Pronunciation;

            int count = (int)matchCountBox.Value;

            if (count > pronunciation.Length)
                count = pronunciation.Length;

            string pattern;

            if (firstRadio.Checked)
            {
                pattern =
                    pronunciation.Substring(0, count);
            }
            else
            {
                pattern = pronunciation.Substring(
                    pronunciation.Length - count,
                    count);
            }

            currentResults = dictionary
                .Where(item =>
                    !string.Equals(
                        NormalizePersianLookup(item.Persian),
                        NormalizePersianLookup(source.Persian),
                        StringComparison.Ordinal))
                .Where(item =>
                {
                    if (firstRadio.Checked)
                    {
                        return item.Pronunciation.StartsWith(
                            pattern,
                            StringComparison.Ordinal);
                    }

                    return item.Pronunciation.EndsWith(
                        pattern,
                        StringComparison.Ordinal);
                })
                .ToList();

            SortCurrentResults();
            RefreshResults();
        }

        private void ClearSearchResults()
        {
            lastSearchWord = "";
            currentResults.Clear();
            RefreshResults();
        }

        // ================================================
        // MATCH OPTIONS
        // ================================================

        private void MatchOption_Changed(
            object sender,
            EventArgs e)
        {
            if (isUpdating)
                return;

            if (!string.IsNullOrEmpty(lastSearchWord))
                PerformSearch();
        }

        private void MatchCountBox_ValueChanged(
            object sender,
            EventArgs e)
        {
            if (isUpdating)
                return;

            if (!string.IsNullOrEmpty(lastSearchWord))
                PerformSearch();
        }

        // ================================================
        // SORTING
        // ================================================

        private void SortOption_Changed(
            object sender,
            EventArgs e)
        {
            if (isUpdating)
                return;

            SortCurrentResults();
            RefreshResults();
        }

        private void SortCurrentResults()
        {
            if (alphabetRadio.Checked)
            {
                currentResults = currentResults
                    .OrderBy(
                        item => item.Persian,
                        StringComparer.CurrentCulture)
                    .ToList();
            }
            else if (lengthRadio.Checked)
            {
                currentResults = currentResults
                    .OrderBy(item => item.Persian.Length)
                    .ThenBy(
                        item => item.Persian,
                        StringComparer.CurrentCulture)
                    .ToList();
            }
        }

        // ================================================
        // RESULTS
        // ================================================

        private void RefreshResults()
        {
            isUpdating = true;

            resultsListBox.BeginUpdate();

            try
            {
                resultsListBox.Items.Clear();

                foreach (WordEntry item in currentResults)
                {
                    resultsListBox.Items.Add(item);
                }

                statusLabel.Text =
                    currentResults.Count.ToString() +
                    " Results";
            }
            finally
            {
                resultsListBox.EndUpdate();
                isUpdating = false;
            }

            resultsListBox.Invalidate();
        }

        // ================================================
        // OWNER DRAW
        // ================================================

        private void ResultsListBox_DrawItem(
            object sender,
            DrawItemEventArgs e)
        {
            if (e.Index < 0 ||
                e.Index >= resultsListBox.Items.Count)
            {
                return;
            }

            WordEntry item =
                resultsListBox.Items[e.Index] as WordEntry;

            if (item == null)
                return;

            bool selected =
                (e.State & DrawItemState.Selected) ==
                DrawItemState.Selected;

            Color backgroundColor = selected
                ? selectedColor
                : (e.Index % 2 == 0
                    ? rowColor1
                    : rowColor2);

            using (SolidBrush brush =
                new SolidBrush(backgroundColor))
            {
                e.Graphics.FillRectangle(
                    brush,
                    e.Bounds);
            }

            int persianWidth =
                (int)(e.Bounds.Width * 0.6);

            Rectangle persianBounds = new Rectangle(
                e.Bounds.Left + 4,
                e.Bounds.Top,
                persianWidth - 8,
                e.Bounds.Height);

            Rectangle latinBounds = new Rectangle(
                e.Bounds.Left + persianWidth,
                e.Bounds.Top,
                e.Bounds.Width - persianWidth - 6,
                e.Bounds.Height);

            using (Font persianFont = new Font(
                "Segoe UI",
                11f,
                FontStyle.Regular))
            {
                TextRenderer.DrawText(
                    e.Graphics,
                    item.Persian,
                    persianFont,
                    persianBounds,
                    Color.White,
                    TextFormatFlags.Right |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.NoPrefix |
                    TextFormatFlags.SingleLine);
            }

            using (Font latinFont = new Font(
                "Segoe UI",
                9.5f,
                FontStyle.Regular))
            {
                TextRenderer.DrawText(
                    e.Graphics,
                    item.Pronunciation,
                    latinFont,
                    latinBounds,
                    Color.White,
                    TextFormatFlags.Left |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.NoPrefix |
                    TextFormatFlags.SingleLine);
            }

            e.DrawFocusRectangle();
        }
    }
}
