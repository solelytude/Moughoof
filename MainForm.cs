
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
        private Panel inputPanel;
        private Panel matchOptionsPanel;
        private Panel divider;
        private Panel outputPanel;
        private Panel sortOptionsPanel;

        private TextBox inputBox;
        private Label pronunciationLabel;

        private Label matchLabel;
        private RadioButton firstRadio;
        private RadioButton lastRadio;
        private NumericUpDown matchCountBox;
        private Button searchButton;

        private Label sortLabel;
        private RadioButton lengthRadio;
        private RadioButton alphabetRadio;
        private Label statusLabel;

        private ListBox resultsListBox;

        private Button exportButton;
        private Label referenceLabel;

        private ContextMenuStrip resultsContextMenu;
        private ToolStripMenuItem copyMenuItem;

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
        private string activeReferenceName = "Default";

        // The original matched database entry is retained for export.
        // It is not added to the visible results ListBox.
        private WordEntry lastSearchEntry = null;

        private bool referenceLoaded = false;
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
            // INPUT PANEL

            inputPanel = new Panel
            {
                Location = new Point(10, 8),
                Size = new Size(443, 95),
                BackColor = BackColor
            };

            Controls.Add(inputPanel);

            // INPUT BOX

            inputBox = new TextBox
            {
                Location = new Point(0, 0),
                Size = new Size(443, 31),

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

            inputPanel.Controls.Add(inputBox);

            // PRONUNCIATION PREVIEW

            pronunciationLabel = new Label
            {
                Location = new Point(0, 32),
                Size = new Size(443, 20),

                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,

                ForeColor = Color.LightGray,
                BackColor = Color.Transparent
            };

            inputPanel.Controls.Add(pronunciationLabel);

            // MATCH OPTIONS

            matchOptionsPanel = new Panel
            {
                Location = new Point(0, 59),
                Size = new Size(443, 30),
                BackColor = BackColor
            };

            inputPanel.Controls.Add(matchOptionsPanel);

            matchLabel = new Label
            {
                Location = new Point(0, 0),
                Size = new Size(50, 28),

                Text = "MATCH",
                TextAlign = ContentAlignment.MiddleLeft,

                ForeColor = Color.White,
                BackColor = Color.Transparent
            };

            matchOptionsPanel.Controls.Add(matchLabel);

            firstRadio = new RadioButton
            {
                Location = new Point(55, 2),
                Size = new Size(62, 24),

                Text = "FIRST",

                ForeColor = Color.White,
                BackColor = Color.Transparent
            };

            matchOptionsPanel.Controls.Add(firstRadio);

            lastRadio = new RadioButton
            {
                Location = new Point(118, 2),
                Size = new Size(58, 24),

                Text = "LAST",

                ForeColor = Color.White,
                BackColor = Color.Transparent,

                Checked = true
            };

            matchOptionsPanel.Controls.Add(lastRadio);

            matchCountBox = new NumericUpDown
            {
                Location = new Point(181, 2),
                Size = new Size(48, 25),

                Minimum = 1,
                Maximum = 50,
                Value = 2,

                TextAlign = HorizontalAlignment.Center
            };

            matchOptionsPanel.Controls.Add(matchCountBox);

            // SEARCH BUTTON

            searchButton = new Button
            {
                Location = new Point(367, 0),
                Size = new Size(76, 28),

                Text = "SEARCH",

                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(55, 55, 55),
                ForeColor = Color.White
            };

            searchButton.FlatAppearance.BorderColor =
                Color.FromArgb(90, 90, 90);

            matchOptionsPanel.Controls.Add(searchButton);

            // DIVIDER

            divider = new Panel
            {
                Location = new Point(10, 105),
                Size = new Size(443, 1),
                BackColor = Color.FromArgb(85, 85, 85)
            };

            Controls.Add(divider);

            // OUTPUT PANEL

            outputPanel = new Panel
            {
                Location = new Point(10, 109),
                Size = new Size(443, 503),

                Anchor =
                    AnchorStyles.Top |
                    AnchorStyles.Bottom |
                    AnchorStyles.Left |
                    AnchorStyles.Right,

                BackColor = BackColor
            };

            Controls.Add(outputPanel);

            // SORT OPTIONS
            // LENGTH is now the default selection.

            sortOptionsPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(443, 28),
                BackColor = BackColor
            };

            outputPanel.Controls.Add(sortOptionsPanel);

            sortLabel = new Label
            {
                Location = new Point(0, 0),
                Size = new Size(50, 27),

                Text = "SORT",
                TextAlign = ContentAlignment.MiddleLeft,

                ForeColor = Color.White,
                BackColor = Color.Transparent
            };

            sortOptionsPanel.Controls.Add(sortLabel);

            lengthRadio = new RadioButton
            {
                Location = new Point(55, 1),
                Size = new Size(62, 24),

                Text = "LENGTH",

                ForeColor = Color.White,
                BackColor = Color.Transparent,

                Checked = true
            };

            sortOptionsPanel.Controls.Add(lengthRadio);

            alphabetRadio = new RadioButton
            {
                Location = new Point(118, 1),
                Size = new Size(105, 24),

                Text = "ALPHABET",

                ForeColor = Color.White,
                BackColor = Color.Transparent,

                Checked = false
            };

            sortOptionsPanel.Controls.Add(alphabetRadio);

            // RESULTS COUNT

            statusLabel = new Label
            {
                Location = new Point(250, 1),
                Size = new Size(193, 25),

                AutoSize = false,
                TextAlign = ContentAlignment.MiddleRight,

                ForeColor = Color.LightGray,
                BackColor = Color.Transparent,

                Text = ""
            };

            sortOptionsPanel.Controls.Add(statusLabel);

            // RESULTS LIST

            resultsListBox = new ListBox
            {
                Location = new Point(0, 30),
                Size = new Size(443, 470),

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

                Font = new Font("Segoe UI", 10f),

                SelectionMode = SelectionMode.One
            };

            outputPanel.Controls.Add(resultsListBox);

            // DATABASE REFERENCE

            referenceLabel = new Label
            {
                Location = new Point(10, 620),

                AutoSize = true,

                Anchor =
                    AnchorStyles.Bottom |
                    AnchorStyles.Left,

                Text = "Reference Not Found",

                ForeColor = Color.LightSkyBlue,
                BackColor = Color.Transparent,

                Cursor = Cursors.Hand
            };

            referenceLabel.Font = new Font(
                referenceLabel.Font,
                FontStyle.Underline);

            Controls.Add(referenceLabel);

            // EXPORT BUTTON

            exportButton = new Button
            {
                Location = new Point(373, 615),
                Size = new Size(80, 28),

                Anchor =
                    AnchorStyles.Bottom |
                    AnchorStyles.Right,

                Text = "EXPORT",

                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(55, 55, 55),
                ForeColor = Color.White,

                Enabled = false
            };

            exportButton.FlatAppearance.BorderColor =
                Color.FromArgb(90, 90, 90);

            Controls.Add(exportButton);

            // RIGHT-CLICK COPY MENU

            resultsContextMenu = new ContextMenuStrip();

            copyMenuItem = new ToolStripMenuItem("Copy");

            resultsContextMenu.Items.Add(copyMenuItem);

            resultsListBox.ContextMenuStrip =
                resultsContextMenu;
        }

        // ================================================
        // EVENTS
        // ================================================

        private void ConnectEvents()
        {
            inputBox.Enter += InputBox_Enter;
            inputBox.TextChanged += InputBox_TextChanged;

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

            resultsListBox.MouseDown +=
                ResultsListBox_MouseDown;

            copyMenuItem.Click +=
                CopyMenuItem_Click;

            exportButton.Click +=
                ExportButton_Click;

            referenceLabel.Click +=
                ReferenceLabel_Click;

            KeyDown += MainForm_KeyDown;
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

                referenceLoaded = false;
                activeReferenceName = "Default";

                UpdateReferenceLabel();
                UpdateExportButton();
                ClearSearchResults();

                pronunciationLabel.Text = "";
                statusLabel.Text = "";

                return;
            }

            try
            {
                List<WordEntry> loaded =
                    ReadDictionaryFile(path);

                dictionary.Clear();
                dictionary.AddRange(loaded);

                referenceLoaded = dictionary.Count > 0;
                activeReferenceName = "Default";

                UpdateReferenceLabel();
                UpdateExportButton();
                ClearSearchResults();
                UpdatePronunciationPreview();

                statusLabel.Text = "";
            }
            catch
            {
                dictionary.Clear();

                referenceLoaded = false;
                activeReferenceName = "Default";

                UpdateReferenceLabel();
                UpdateExportButton();
                ClearSearchResults();

                pronunciationLabel.Text = "";
                statusLabel.Text = "";
            }
        }

        // ================================================
        // READ DATABASE
        // ================================================

        private List<WordEntry> ReadDictionaryFile(string path)
        {
            List<WordEntry> loaded = new List<WordEntry>();

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

                    line = line.TrimStart('\uFEFF');

                    int tabIndex = line.IndexOf('\t');

                    if (tabIndex < 0)
                        continue;

                    string persian =
                        line.Substring(0, tabIndex).Trim();

                    string pronunciation =
                        line.Substring(tabIndex + 1).Trim();

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
        // IMPORT DATABASE
        // ================================================

        private void ReferenceLabel_Click(
            object sender,
            EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Load Reference";

                dialog.Filter =
                    "Text files (*.txt)|*.txt|All files (*.*)|*.*";

                dialog.CheckFileExists = true;
                dialog.Multiselect = false;

                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                try
                {
                    List<WordEntry> loaded =
                        ReadDictionaryFile(dialog.FileName);

                    if (loaded.Count == 0)
                    {
                        MessageBox.Show(
                            "No valid entries were found in this file.\r\n\r\n" +
                            "Each line must contain a Persian word, a TAB, " +
                            "and its pronunciation.",
                            "Moughoof",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);

                        return;
                    }

                    dictionary.Clear();
                    dictionary.AddRange(loaded);

                    referenceLoaded = true;

                    activeReferenceName =
                        Path.GetFileName(dialog.FileName);

                    lastSearchWord = "";
                    lastSearchEntry = null;
                    currentResults.Clear();

                    UpdateReferenceLabel();
                    UpdateExportButton();

                    RefreshResults();
                    UpdatePronunciationPreview();

                    statusLabel.Text = "";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Could not load the selected reference.\r\n\r\n" +
                        ex.Message,
                        "Moughoof",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }
        }

        private void UpdateReferenceLabel()
        {
            if (!referenceLoaded)
            {
                referenceLabel.Text = "Reference Not Found";
                return;
            }

            referenceLabel.Text =
                "Reference: " +
                activeReferenceName +
                ", " +
                dictionary.Count.ToString() +
                " words";
        }

        private void UpdateExportButton()
        {
            exportButton.Enabled =
                referenceLoaded && dictionary.Count > 0;
        }

        // ================================================
        // INPUT AND PRONUNCIATION PREVIEW
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

        private string GetInputWord()
        {
            string word = inputBox.Text.Trim();

            if (word == "Input word")
                return "";

            return word;
        }

        // ================================================
        // NORMALIZATION AND LOOKUP
        // ================================================

        private string NormalizePersianLookup(string value)
        {
            if (value == null)
                return "";

            // Persian ZWNJ and ordinary spaces are equivalent
            // during lookup. Stored words remain unchanged.

            return value
                .Replace('\u200C', ' ')
                .Trim();
        }

        private WordEntry FindExactPersianWord(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
                return null;

            string normalizedWord =
                NormalizePersianLookup(word);

            return dictionary.FirstOrDefault(
                item => string.Equals(
                    NormalizePersianLookup(item.Persian),
                    normalizedWord,
                    StringComparison.Ordinal));
        }

        private WordEntry FindByPronunciation(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            // Match the user's English input against the stored
            // pronunciation without case sensitivity.
            // The stored pronunciation itself is never modified.

            return dictionary.FirstOrDefault(
                item => string.Equals(
                    item.Pronunciation.Trim(),
                    input.Trim(),
                    StringComparison.OrdinalIgnoreCase));
        }

        private WordEntry FindSourceWord(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            // Prefer an exact Persian word if one exists.
            WordEntry persianEntry =
                FindExactPersianWord(input);

            if (persianEntry != null)
                return persianEntry;

            // Otherwise, interpret the input as pronunciation.
            return FindByPronunciation(input);
        }

        private void UpdatePronunciationPreview()
        {
            if (!referenceLoaded)
            {
                pronunciationLabel.Text = "";
                return;
            }

            string word = GetInputWord();

            WordEntry entry = FindSourceWord(word);

            pronunciationLabel.Text =
                entry == null
                    ? ""
                    : entry.Pronunciation;
        }

        // ================================================
        // KEYBOARD HANDLING
        // ================================================

        private void MainForm_KeyDown(
            object sender,
            KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                e.Handled = true;

                PerformSearch();
                return;
            }

            if (e.Control && e.KeyCode == Keys.C)
            {
                if (resultsListBox.SelectedItem is WordEntry)
                {
                    CopySelectedPersian();

                    e.SuppressKeyPress = true;
                    e.Handled = true;
                }
            }
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
            if (!referenceLoaded || dictionary.Count == 0)
            {
                lastSearchWord = "";
                lastSearchEntry = null;

                ClearSearchResults();

                pronunciationLabel.Text = "";
                statusLabel.Text = "";

                return;
            }

            string word = GetInputWord();

            if (string.IsNullOrWhiteSpace(word))
            {
                lastSearchWord = "";
                lastSearchEntry = null;

                ClearSearchResults();
                pronunciationLabel.Text = "";

                return;
            }

            // Input can be either a Persian word or its English
            // pronunciation, for example "سلام" or "salAm".

            WordEntry source = FindSourceWord(word);

            if (source == null)
            {
                lastSearchWord = word;
                lastSearchEntry = null;

                currentResults.Clear();
                RefreshResults();

                pronunciationLabel.Text = "";
                statusLabel.Text = "Word not found";

                return;
            }

            lastSearchWord = word;
            lastSearchEntry = source;

            pronunciationLabel.Text = source.Pronunciation;

            string pronunciation = source.Pronunciation;

            int count = (int)matchCountBox.Value;

            if (count > pronunciation.Length)
                count = pronunciation.Length;

            string pattern;

            if (firstRadio.Checked)
            {
                pattern = pronunciation.Substring(0, count);
            }
            else
            {
                pattern = pronunciation.Substring(
                    pronunciation.Length - count,
                    count);
            }

            // Keep the searched source word out of the visible list.
            // Search matching against other entries remains
            // case-sensitive, as in the original behavior.

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
            lastSearchEntry = null;
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
            if (lengthRadio.Checked)
            {
                currentResults = currentResults
                    .OrderBy(item => item.Persian.Length)
                    .ThenBy(
                        item => item.Persian,
                        StringComparer.CurrentCulture)
                    .ToList();
            }
            else if (alphabetRadio.Checked)
            {
                currentResults = currentResults
                    .OrderBy(
                        item => item.Persian,
                        StringComparer.CurrentCulture)
                    .ToList();
            }
        }

        // ================================================
        // REFRESH RESULTS
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
        // COPY SELECTED PERSIAN WORD
        // ================================================

        private void ResultsListBox_MouseDown(
            object sender,
            MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            int index =
                resultsListBox.IndexFromPoint(e.Location);

            if (index >= 0 &&
                index < resultsListBox.Items.Count)
            {
                resultsListBox.SelectedIndex = index;
            }
            else
            {
                resultsListBox.ClearSelected();
            }
        }

        private void CopyMenuItem_Click(
            object sender,
            EventArgs e)
        {
            CopySelectedPersian();
        }

        private void CopySelectedPersian()
        {
            WordEntry selected =
                resultsListBox.SelectedItem as WordEntry;

            if (selected == null)
                return;

            try
            {
                Clipboard.SetText(selected.Persian);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not copy the selected word.\r\n\r\n" +
                    ex.Message,
                    "Moughoof",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        // ================================================
        // EXPORT CURRENT SEARCH
        // ================================================

        private void ExportButton_Click(
            object sender,
            EventArgs e)
        {
            if (!referenceLoaded || dictionary.Count == 0)
            {
                MessageBox.Show(
                    "No reference is loaded.",
                    "Moughoof",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            if (lastSearchEntry == null)
            {
                MessageBox.Show(
                    "Search for a word before exporting.",
                    "Moughoof",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            // The exported file contains:
            // 1. The searched source word.
            // 2. Every current ListBox result, in displayed order.
            //
            // The source word is exported but is not displayed
            // as a result in the ListBox.

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Title = "Export Current Search";

                dialog.Filter =
                    "Text files (*.txt)|*.txt|All files (*.*)|*.*";

                dialog.DefaultExt = "txt";
                dialog.AddExtension = true;
                dialog.OverwritePrompt = true;

                string safeWord =
                    MakeSafeFileName(lastSearchEntry.Persian);

                dialog.FileName = "پساوند " + safeWord + ".txt";

                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                try
                {
                    using (StreamWriter writer = new StreamWriter(
                        dialog.FileName,
                        false,
                        new UTF8Encoding(false)))
                    {
                        // Write the searched word first.

                        writer.Write(lastSearchEntry.Persian);
                        writer.Write('\t');
                        writer.WriteLine(
                            lastSearchEntry.Pronunciation);

                        // Then write the current results in their
                        // visible ListBox order.

                        foreach (object row in resultsListBox.Items)
                        {
                            WordEntry item = row as WordEntry;

                            if (item == null)
                                continue;

                            writer.Write(item.Persian);
                            writer.Write('\t');
                            writer.WriteLine(item.Pronunciation);
                        }
                    }

                    int exportedCount =
                        resultsListBox.Items.Count + 1;

                    statusLabel.Text =
                        exportedCount.ToString() +
                        " Words Exported";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Could not export the results.\r\n\r\n" +
                        ex.Message,
                        "Moughoof",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private string MakeSafeFileName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "Word";

            foreach (char invalidChar in
                Path.GetInvalidFileNameChars())
            {
                value = value.Replace(invalidChar, '_');
            }

            return value.Trim();
        }

        // ================================================
        // OWNER-DRAW RESULTS
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
