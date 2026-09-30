using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Moughoof
{
    public class WordEntry
    {
        public string Word { get; set; }
        public string Pronunciation { get; set; }

        public WordEntry(string word, string pronunciation)
        {
            Word = word;
            Pronunciation = pronunciation;
        }
    }

    public class MainForm : Form
    {
        private TextBox inputBox;
        private Label pronunciationLabel;

        private Panel searchGroup;
        private Label matchLabel;
        private RadioButton firstRadio;
        private RadioButton lastRadio;
        private NumericUpDown matchCountBox;
        private Button searchButton;
        private Label statusLabel;

        private Panel resultsGroup;
        private Label sortLabel;
        private RadioButton alphabetRadio;
        private RadioButton lengthRadio;
        private ListBox resultList;

        private Button exportButton;
        private Label databaseLabel;

        private List<WordEntry> entries =
            new List<WordEntry>();

        private List<WordEntry> currentResults =
            new List<WordEntry>();

        private string currentDatabaseName = "Default";
        private int currentDatabaseLineCount = 0;

        private bool placeholderActive = true;

        private ContextMenuStrip resultContextMenu;

        public MainForm()
        {
            InitializeForm();
            InitializeControls();
            LoadDefaultDictionary();
        }

        private void InitializeForm()
        {
            Text = "Moughoof";

            ClientSize = new Size(460, 650);

            MinimumSize =
                new Size(460, 530);

            MaximumSize =
                new Size(460, 2000);

            MinimizeBox = true;
            MaximizeBox = false;

            FormBorderStyle =
                FormBorderStyle.Sizable;

            BackColor =
                Color.FromArgb(32, 32, 32);

            ForeColor = Color.White;

            Font =
                new Font("Segoe UI", 9f);

            KeyPreview = true;

            KeyDown +=
                MainForm_KeyDown;
        }

        private void InitializeControls()
        {
            // =================================================
            // GROUP 1
            // INPUT / MATCH / SEARCH
            // =================================================

            searchGroup = new Panel
            {
                Location = new Point(10, 10),
                Size = new Size(440, 170),
                BackColor =
                    Color.FromArgb(32, 32, 32)
            };

            Controls.Add(searchGroup);

            // Input

            inputBox = new TextBox
            {
                Location = new Point(5, 5),
                Size = new Size(430, 34),

                Font =
                    new Font(
                        "Segoe UI",
                        11.5f,
                        FontStyle.Bold),

                BackColor =
                    Color.FromArgb(45, 45, 45),

                ForeColor = Color.Gray,

                BorderStyle =
                    BorderStyle.FixedSingle,

                Text = "Input word",

                RightToLeft =
                    RightToLeft.No
            };

            inputBox.GotFocus +=
                InputBox_GotFocus;

            inputBox.LostFocus +=
                InputBox_LostFocus;

            inputBox.TextChanged +=
                InputBox_TextChanged;

            searchGroup.Controls.Add(inputBox);

            // Pronunciation

            pronunciationLabel = new Label
            {
                Location = new Point(5, 43),
                Size = new Size(430, 20),

                AutoSize = false,

                TextAlign =
                    ContentAlignment.MiddleLeft,

                ForeColor =
                    Color.LightGray,

                BackColor =
                    Color.Transparent
            };

            searchGroup.Controls.Add(
                pronunciationLabel);

            // MATCH label

            matchLabel = new Label
            {
                Location = new Point(5, 73),
                Size = new Size(50, 28),

                Text = "MATCH",

                TextAlign =
                    ContentAlignment.MiddleLeft,

                ForeColor = Color.White,

                BackColor =
                    Color.Transparent
            };

            searchGroup.Controls.Add(
                matchLabel);

            // FIRST

            firstRadio = new RadioButton
            {
                Location = new Point(62, 75),
                Size = new Size(62, 24),

                Text = "FIRST",

                ForeColor = Color.White,

                BackColor =
                    Color.Transparent
            };

            searchGroup.Controls.Add(
                firstRadio);

            // LAST

            lastRadio = new RadioButton
            {
                Location = new Point(125, 75),
                Size = new Size(58, 24),

                Text = "LAST",

                ForeColor = Color.White,

                BackColor =
                    Color.Transparent,

                Checked = true
            };

            searchGroup.Controls.Add(
                lastRadio);

            // Number

            matchCountBox = new NumericUpDown
            {
                Location = new Point(188, 73),
                Size = new Size(48, 25),

                Minimum = 1,
                Maximum = 50,

                Value = 2,

                TextAlign =
                    HorizontalAlignment.Center
            };

            searchGroup.Controls.Add(
                matchCountBox);

            // SEARCH

            searchButton = new Button
            {
                Location = new Point(5, 112),
                Size = new Size(100, 32),

                Text = "SEARCH",

                FlatStyle =
                    FlatStyle.Flat,

                BackColor =
                    Color.FromArgb(55, 55, 55),

                ForeColor = Color.White
            };

            searchButton.FlatAppearance.BorderColor =
                Color.FromArgb(90, 90, 90);

            searchButton.Click +=
                SearchButton_Click;

            searchGroup.Controls.Add(
                searchButton);

            // Results count

            statusLabel = new Label
            {
                Location = new Point(120, 115),
                Size = new Size(310, 25),

                AutoSize = false,

                TextAlign =
                    ContentAlignment.MiddleLeft,

                ForeColor =
                    Color.LightGray,

                BackColor =
                    Color.Transparent,

                Text = "0 Results"
            };

            searchGroup.Controls.Add(
                statusLabel);


            // =================================================
            // GROUP 2
            // SORT / RESULTS LIST
            // =================================================

            resultsGroup = new Panel
            {
                Location = new Point(10, 185),

                Size = new Size(440, 390),

                Anchor =
                    AnchorStyles.Top |
                    AnchorStyles.Bottom |
                    AnchorStyles.Left |
                    AnchorStyles.Right,

                BackColor =
                    Color.FromArgb(32, 32, 32)
            };

            Controls.Add(resultsGroup);

            // SORT label

            sortLabel = new Label
            {
                Location = new Point(5, 2),
                Size = new Size(42, 28),

                Text = "SORT",

                TextAlign =
                    ContentAlignment.MiddleLeft,

                ForeColor = Color.White,

                BackColor =
                    Color.Transparent
            };

            resultsGroup.Controls.Add(
                sortLabel);

            // ALPHABET

            alphabetRadio = new RadioButton
            {
                Location = new Point(55, 4),
                Size = new Size(105, 24),

                Text = "ALPHABET",

                ForeColor = Color.White,

                BackColor =
                    Color.Transparent,

                Checked = true
            };

            resultsGroup.Controls.Add(
                alphabetRadio);

            // LENGTH

            lengthRadio = new RadioButton
            {
                Location = new Point(165, 4),
                Size = new Size(75, 24),

                Text = "LENGTH",

                ForeColor = Color.White,

                BackColor =
                    Color.Transparent
            };

            resultsGroup.Controls.Add(
                lengthRadio);

            alphabetRadio.CheckedChanged +=
                SortRadio_CheckedChanged;

            lengthRadio.CheckedChanged +=
                SortRadio_CheckedChanged;

            // ListBox

            resultList = new ListBox
            {
                Location = new Point(5, 32),

                Size = new Size(430, 350),

                Anchor =
                    AnchorStyles.Top |
                    AnchorStyles.Bottom |
                    AnchorStyles.Left |
                    AnchorStyles.Right,

                BackColor =
                    Color.FromArgb(40, 40, 40),

                ForeColor = Color.White,

                BorderStyle =
                    BorderStyle.FixedSingle,

                DrawMode =
                    DrawMode.OwnerDrawFixed,

                ItemHeight = 28,

                SelectionMode =
                    SelectionMode.One,

                IntegralHeight = false
            };

            resultList.DrawItem +=
                ResultList_DrawItem;

            resultList.MouseDown +=
                ResultList_MouseDown;

            resultList.KeyDown +=
                ResultList_KeyDown;

            resultList.DoubleClick +=
                ResultList_DoubleClick;

            resultsGroup.Controls.Add(
                resultList);

            InitializeContextMenu();


            // =================================================
            // FREE CONTROLS
            // DATABASE / EXPORT
            // =================================================

            databaseLabel = new Label
            {
                Location = new Point(15, 585),

                Size = new Size(300, 25),

                Anchor =
                    AnchorStyles.Bottom |
                    AnchorStyles.Left,

                AutoSize = false,

                TextAlign =
                    ContentAlignment.MiddleLeft,

                Text =
                    "Database: Default, 0 words",

                ForeColor =
                    Color.LightSkyBlue,

                BackColor =
                    Color.Transparent,

                Cursor =
                    Cursors.Hand
            };

            databaseLabel.Font =
                new Font(
                    databaseLabel.Font,
                    FontStyle.Underline);

            databaseLabel.Click +=
                DatabaseLabel_Click;

            Controls.Add(databaseLabel);

            exportButton = new Button
            {
                Location = new Point(365, 582),

                Size = new Size(80, 32),

                Anchor =
                    AnchorStyles.Bottom |
                    AnchorStyles.Right,

                Text = "EXPORT",

                FlatStyle =
                    FlatStyle.Flat,

                BackColor =
                    Color.FromArgb(55, 55, 55),

                ForeColor = Color.White
            };

            exportButton.FlatAppearance.BorderColor =
                Color.FromArgb(90, 90, 90);

            exportButton.Click +=
                ExportButton_Click;

            Controls.Add(exportButton);

            UpdateBottomControls();
        }

        protected override void OnResize(
            EventArgs e)
        {
            base.OnResize(e);

            UpdateBottomControls();
        }

        private void UpdateBottomControls()
        {
            if (databaseLabel == null ||
                exportButton == null)
            {
                return;
            }

            int bottomY =
                ClientSize.Height - 43;

            databaseLabel.Location =
                new Point(
                    15,
                    bottomY);

            exportButton.Location =
                new Point(
                    ClientSize.Width - 95,
                    bottomY - 3);
        }

        // =====================================================
        // GLOBAL ENTER
        // =====================================================

        private void MainForm_KeyDown(
            object sender,
            KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Search();

                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        // =====================================================
        // INPUT
        // =====================================================

        private void InputBox_GotFocus(
            object sender,
            EventArgs e)
        {
            if (placeholderActive)
            {
                placeholderActive = false;

                inputBox.Clear();

                inputBox.ForeColor =
                    Color.White;

                inputBox.RightToLeft =
                    RightToLeft.Yes;
            }
        }

        private void InputBox_LostFocus(
            object sender,
            EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(
                inputBox.Text))
            {
                placeholderActive = true;

                inputBox.Text =
                    "Input word";

                inputBox.ForeColor =
                    Color.Gray;

                inputBox.RightToLeft =
                    RightToLeft.No;
            }
        }

        private void InputBox_TextChanged(
            object sender,
            EventArgs e)
        {
            if (placeholderActive)
            {
                pronunciationLabel.Text = "";
                return;
            }

            string word =
                inputBox.Text.Trim();

            if (word.Length == 0)
            {
                pronunciationLabel.Text = "";
                return;
            }

            pronunciationLabel.Text =
                FindPronunciation(word);
        }

        // =====================================================
        // SEARCH
        // =====================================================

        private void SearchButton_Click(
            object sender,
            EventArgs e)
        {
            Search();
        }

        private void Search()
        {
            if (placeholderActive)
                return;

            string inputWord =
                inputBox.Text.Trim();

            if (inputWord.Length == 0)
                return;

            string pronunciation =
                FindPronunciation(inputWord);

            pronunciationLabel.Text =
                pronunciation;

            currentResults.Clear();

            if (pronunciation.Length == 0)
            {
                DisplayResults();
                return;
            }

            pronunciation =
                NormalizePronunciation(
                    pronunciation);

            int count =
                (int)matchCountBox.Value;

            string target;

            if (firstRadio.Checked)
            {
                target =
                    pronunciation.Length <= count
                        ? pronunciation
                        : pronunciation.Substring(
                            0,
                            count);
            }
            else
            {
                target =
                    pronunciation.Length <= count
                        ? pronunciation
                        : pronunciation.Substring(
                            pronunciation.Length - count,
                            count);
            }

            string normalizedInput =
                NormalizePersianWord(
                    inputWord);

            foreach (WordEntry entry
                in entries)
            {
                if (string.Equals(
                    NormalizePersianWord(
                        entry.Word),
                    normalizedInput,
                    StringComparison.Ordinal))
                {
                    continue;
                }

                string entryPronunciation =
                    NormalizePronunciation(
                        entry.Pronunciation);

                if (entryPronunciation.Length == 0)
                    continue;

                string candidate;

                if (firstRadio.Checked)
                {
                    candidate =
                        entryPronunciation.Length <= count
                            ? entryPronunciation
                            : entryPronunciation.Substring(
                                0,
                                count);
                }
                else
                {
                    candidate =
                        entryPronunciation.Length <= count
                            ? entryPronunciation
                            : entryPronunciation.Substring(
                                entryPronunciation.Length - count,
                                count);
                }

                if (string.Equals(
                    candidate,
                    target,
                    StringComparison.Ordinal))
                {
                    currentResults.Add(entry);
                }
            }

            SortCurrentResults();
            DisplayResults();
        }

        // =====================================================
        // PRONUNCIATION LOOKUP
        // =====================================================

        private string FindPronunciation(
            string inputWord)
        {
            string normalizedInput =
                NormalizePersianWord(
                    inputWord);

            foreach (WordEntry entry
                in entries)
            {
                string normalizedEntry =
                    NormalizePersianWord(
                        entry.Word);

                if (string.Equals(
                    normalizedEntry,
                    normalizedInput,
                    StringComparison.Ordinal))
                {
                    return entry.Pronunciation;
                }
            }

            return "";
        }

        // =====================================================
        // PERSIAN NORMALIZATION
        // =====================================================

        private string NormalizePersianWord(
            string text)
        {
            if (string.IsNullOrEmpty(text))
                return "";

            return text
                .Replace(" ", "")
                .Replace("\u200C", "");
        }

        // =====================================================
        // PRONUNCIATION NORMALIZATION
        // =====================================================

        private string NormalizePronunciation(
            string text)
        {
            if (string.IsNullOrEmpty(text))
                return "";

            StringBuilder result =
                new StringBuilder();

            char previous = '\0';

            foreach (char c in text)
            {
                if (c == previous)
                    continue;

                result.Append(c);
                previous = c;
            }

            return result.ToString();
        }

        // =====================================================
        // SORT
        // =====================================================

        private void SortRadio_CheckedChanged(
            object sender,
            EventArgs e)
        {
            RadioButton radio =
                sender as RadioButton;

            if (radio == null ||
                !radio.Checked)
            {
                return;
            }

            SortCurrentResults();
            DisplayResults();
        }

        private void SortCurrentResults()
        {
            if (currentResults == null)
                return;

            if (alphabetRadio != null &&
                alphabetRadio.Checked)
            {
                currentResults.Sort(
                    CompareAlphabet);
            }
            else
            {
                currentResults.Sort(
                    CompareLength);
            }
        }

        private int CompareAlphabet(
            WordEntry a,
            WordEntry b)
        {
            return CultureInfo
                .GetCultureInfo("fa-IR")
                .CompareInfo
                .Compare(
                    a.Word,
                    b.Word,
                    CompareOptions.StringSort);
        }

        private int CompareLength(
            WordEntry a,
            WordEntry b)
        {
            int result =
                a.Word.Length.CompareTo(
                    b.Word.Length);

            if (result != 0)
                return result;

            return CompareAlphabet(
                a,
                b);
        }

        // =====================================================
        // DISPLAY RESULTS
        // =====================================================

        private void DisplayResults()
        {
            if (resultList == null ||
                statusLabel == null)
            {
                return;
            }

            resultList.BeginUpdate();

            try
            {
                resultList.Items.Clear();

                foreach (WordEntry entry
                    in currentResults)
                {
                    resultList.Items.Add(entry);
                }

                statusLabel.Text =
                    currentResults.Count +
                    " Results";
            }
            finally
            {
                resultList.EndUpdate();
            }
        }

        // =====================================================
        // RESULT DRAWING
        // =====================================================

        private void ResultList_DrawItem(
            object sender,
            DrawItemEventArgs e)
        {
            if (e.Index < 0 ||
                e.Index >= resultList.Items.Count)
            {
                return;
            }

            WordEntry entry =
                (WordEntry)resultList.Items[
                    e.Index];

            bool selected =
                (e.State &
                 DrawItemState.Selected) != 0;

            Color background;

            if (selected)
            {
                background =
                    Color.FromArgb(
                        75,
                        75,
                        75);
            }
            else if (e.Index % 2 == 0)
            {
                background =
                    Color.FromArgb(
                        40,
                        40,
                        40);
            }
            else
            {
                background =
                    Color.FromArgb(
                        56,
                        56,
                        56);
            }

            using (SolidBrush brush =
                new SolidBrush(background))
            {
                e.Graphics.FillRectangle(
                    brush,
                    e.Bounds);
            }

            int totalWidth =
                e.Bounds.Width;

            int persianWidth =
                (int)(
                    totalWidth * 0.60);

            Rectangle persianRect =
                new Rectangle(
                    e.Bounds.X + 6,
                    e.Bounds.Y,
                    persianWidth - 10,
                    e.Bounds.Height);

            Rectangle pronunciationRect =
                new Rectangle(
                    e.Bounds.X +
                    persianWidth + 2,
                    e.Bounds.Y,
                    totalWidth -
                    persianWidth - 8,
                    e.Bounds.Height);

            using (Font persianFont =
                new Font(
                    "Segoe UI",
                    11f,
                    FontStyle.Regular))
            {
                TextRenderer.DrawText(
                    e.Graphics,
                    entry.Word,
                    persianFont,
                    persianRect,
                    Color.White,
                    TextFormatFlags.Right |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.NoPadding);
            }

            using (Font pronunciationFont =
                new Font(
                    "Segoe UI",
                    9.5f,
                    FontStyle.Regular))
            {
                TextRenderer.DrawText(
                    e.Graphics,
                    entry.Pronunciation,
                    pronunciationFont,
                    pronunciationRect,
                    Color.White,
                    TextFormatFlags.Left |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.NoPadding);
            }

            e.DrawFocusRectangle();
        }

        // =====================================================
        // COPY
        // =====================================================

        private void InitializeContextMenu()
        {
            resultContextMenu =
                new ContextMenuStrip();

            ToolStripMenuItem copyItem =
                new ToolStripMenuItem(
                    "Copy");

            copyItem.Click +=
                delegate
                {
                    CopySelectedPersianWord();
                };

            resultContextMenu.Items.Add(
                copyItem);

            resultList.ContextMenuStrip =
                resultContextMenu;
        }

        private void ResultList_MouseDown(
            object sender,
            MouseEventArgs e)
        {
            if (e.Button !=
                MouseButtons.Right)
            {
                return;
            }

            int index =
                resultList.IndexFromPoint(
                    e.Location);

            if (index >= 0 &&
                index < resultList.Items.Count)
            {
                resultList.SelectedIndex =
                    index;
            }
        }

        private void ResultList_KeyDown(
            object sender,
            KeyEventArgs e)
        {
            if (e.Control &&
                e.KeyCode == Keys.C)
            {
                CopySelectedPersianWord();

                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void ResultList_DoubleClick(
            object sender,
            EventArgs e)
        {
            // Intentionally empty.
        }

        private void CopySelectedPersianWord()
        {
            if (resultList.SelectedIndex < 0)
                return;

            WordEntry entry =
                resultList.SelectedItem
                as WordEntry;

            if (entry == null)
                return;

            Clipboard.SetText(
                entry.Word);
        }

        // =====================================================
        // DATABASE
        // =====================================================

        private void LoadDefaultDictionary()
        {
            string path =
                Path.Combine(
                    Application.StartupPath,
                    "PS.txt");

            if (File.Exists(path))
            {
                LoadDictionary(
                    path,
                    "Default");
            }
            else
            {
                entries.Clear();
                currentResults.Clear();

                currentDatabaseName =
                    "Default";

                currentDatabaseLineCount =
                    0;

                UpdateDatabaseLabel();
                DisplayResults();
            }
        }

        private bool LoadDictionary(
            string path,
            string databaseName)
        {
            List<WordEntry> newEntries =
                new List<WordEntry>();

            int lineCount = 0;

            try
            {
                using (StreamReader reader =
                    new StreamReader(
                        path,
                        Encoding.UTF8,
                        true))
                {
                    string line;

                    while ((line =
                        reader.ReadLine()) != null)
                    {
                        lineCount++;

                        if (string.IsNullOrWhiteSpace(
                            line))
                        {
                            continue;
                        }

                        string[] parts =
                            line.Split(
                                new[] { '\t' },
                                2);

                        if (parts.Length != 2)
                            continue;

                        string word =
                            parts[0];

                        string pronunciation =
                            parts[1];

                        if (word.Length == 0 ||
                            pronunciation.Length == 0)
                        {
                            continue;
                        }

                        newEntries.Add(
                            new WordEntry(
                                word,
                                pronunciation));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not load database.\r\n\r\n" +
                    ex.Message,
                    "Moughoof",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return false;
            }

            entries =
                newEntries;

            currentDatabaseName =
                databaseName;

            currentDatabaseLineCount =
                lineCount;

            currentResults.Clear();

            UpdateDatabaseLabel();
            DisplayResults();

            if (!placeholderActive &&
                !string.IsNullOrWhiteSpace(
                    inputBox.Text))
            {
                pronunciationLabel.Text =
                    FindPronunciation(
                        inputBox.Text.Trim());
            }

            return true;
        }

        private void UpdateDatabaseLabel()
        {
            if (databaseLabel == null)
                return;

            databaseLabel.Text =
                "Database: " +
                currentDatabaseName +
                ", " +
                currentDatabaseLineCount +
                " words";
        }

        private void DatabaseLabel_Click(
            object sender,
            EventArgs e)
        {
            using (OpenFileDialog dialog =
                new OpenFileDialog())
            {
                dialog.Title =
                    "Select dictionary database";

                dialog.Filter =
                    "Text files (*.txt)|*.txt|" +
                    "All files (*.*)|*.*";

                dialog.Multiselect = false;

                if (dialog.ShowDialog() !=
                    DialogResult.OK)
                {
                    return;
                }

                string databaseName =
                    Path.GetFileNameWithoutExtension(
                        dialog.FileName);

                LoadDictionary(
                    dialog.FileName,
                    databaseName);
            }
        }

        // =====================================================
        // EXPORT
        // =====================================================

        private void ExportButton_Click(
            object sender,
            EventArgs e)
        {
            if (currentResults.Count == 0)
            {
                MessageBox.Show(
                    "There are no results to export.",
                    "Moughoof",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            using (SaveFileDialog dialog =
                new SaveFileDialog())
            {
                dialog.Title =
                    "Export results";

                dialog.Filter =
                    "Text files (*.txt)|*.txt";

                dialog.FileName =
                    "Moughoof-results.txt";

                if (dialog.ShowDialog() !=
                    DialogResult.OK)
                {
                    return;
                }

                try
                {
                    using (StreamWriter writer =
                        new StreamWriter(
                            dialog.FileName,
                            false,
                            new UTF8Encoding(false)))
                    {
                        writer.WriteLine(
                            "Word\tPronunciation");

                        foreach (
                            WordEntry entry
                            in currentResults)
                        {
                            writer.WriteLine(
                                entry.Word +
                                "\t" +
                                entry.Pronunciation);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Could not export results.\r\n\r\n" +
                        ex.Message,
                        "Moughoof",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }
    }
}
