
/* ======================== MOUGHOOf V1.6 ========================
   Layout-only updates:
   - Center both result columns; pronunciation left, Persian right
   - Reference labels fit their text
   - Default form height reduced to 650
   All existing functionality preserved.
   =============================================================== */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Moughoof
{
    public class MainForm : Form
    {
        private Panel inputPanel;
        private Panel outputPanel;
        private Panel matchOptionsPanel;
        private Panel sortOptionsPanel;

        private TextBox inputBox;
        private Label pronunciationLabel;

        private Label matchLabel;
        private RadioButton firstRadio;
        private RadioButton lastRadio;
        private NumericUpDown matchCountBox;
        private Button searchButton;

        private Label sortLabel;
        private RadioButton alphabetRadio;
        private RadioButton lengthRadio;

        private Label statusLabel;
        private ListBox resultsListBox;

        private Label referenceLabel;
        private Label refreshReferenceLabel;
        private Button exportButton;

        private ContextMenuStrip resultsContextMenu;
        private ToolStripMenuItem copyMenuItem;

        private readonly Color backgroundColor =
            Color.FromArgb(32, 32, 32);

        private readonly Color rowColor1 =
            Color.FromArgb(40, 40, 40);

        private readonly Color rowColor2 =
            Color.FromArgb(56, 56, 56);

        private readonly Color selectedColor =
            Color.FromArgb(65, 100, 145);

        private readonly Color controlColor =
            Color.FromArgb(45, 45, 45);

        private readonly Color buttonColor =
            Color.FromArgb(55, 55, 55);

        private readonly List<WordEntry> dictionary =
            new List<WordEntry>();

        private List<WordEntry> currentResults =
            new List<WordEntry>();

        private string lastSearchWord = "";
        private WordEntry lastSearchEntry;

        private string activeReferenceName = "Default";

        private bool referenceLoaded = false;
        private bool isUpdating = false;

        private Font preferredWordFont;
        private Font preferredInputFont;
        private Font pronunciationFont;
        private Font fallbackFont;

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

            public override string ToString()
            {
                return Persian;
            }
        }

        public MainForm()
        {
            InitializeForm();
            InitializeFonts();
            InitializeControls();
            ConnectEvents();

            LoadDefaultDictionary();
        }

        // =========================================================
        // FORM
        // =========================================================

        private void InitializeForm()
        {
            Text = "Moughoof";

            // Changed from 460 x 670 to 460 x 650.
            ClientSize = new Size(460, 650);

            MinimumSize = new Size(460, 550);
            MaximumSize = new Size(460, 2000);

            MinimizeBox = true;
            MaximizeBox = false;

            FormBorderStyle = FormBorderStyle.Sizable;

            BackColor = backgroundColor;
            ForeColor = Color.White;

            Font = new Font("Segoe UI", 9f);

            KeyPreview = true;
        }

        // =========================================================
        // FONT SELECTION
        // Priority: Vazirmatn > B Nazanin > system default
        // =========================================================

        private void InitializeFonts()
        {
            string selectedFont = Font.FontFamily.Name;

            try
            {
                using (InstalledFontCollection installedFonts =
                    new InstalledFontCollection())
                {
                    string[] availableFonts = installedFonts.Families
                        .Select(family => family.Name)
                        .ToArray();

                    string vazirmatn = availableFonts.FirstOrDefault(
                        name => string.Equals(
                            name,
                            "Vazirmatn",
                            StringComparison.OrdinalIgnoreCase));

                    string nazanin = availableFonts.FirstOrDefault(
                        name =>
                            string.Equals(
                                name,
                                "B Nazanin",
                                StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(
                                name,
                                "B_Nazanin",
                                StringComparison.OrdinalIgnoreCase));

                    if (!string.IsNullOrEmpty(vazirmatn))
                    {
                        selectedFont = vazirmatn;
                    }
                    else if (!string.IsNullOrEmpty(nazanin))
                    {
                        selectedFont = nazanin;
                    }
                }
            }
            catch
            {
                // Keep the system's default font if detection fails.
            }

            fallbackFont = new Font(Font.FontFamily, 9f);

            preferredWordFont = CreateFontSafely(
                selectedFont,
                11f,
                FontStyle.Regular,
                fallbackFont);

            preferredInputFont = CreateFontSafely(
                selectedFont,
                11.5f,
                FontStyle.Bold,
                fallbackFont);

            pronunciationFont = CreateFontSafely(
                "Segoe UI",
                9.5f,
                FontStyle.Regular,
                fallbackFont);
        }

        private Font CreateFontSafely(
            string familyName,
            float size,
            FontStyle style,
            Font fallback)
        {
            try
            {
                using (FontFamily family = new FontFamily(familyName))
                {
                    if (family.IsStyleAvailable(style))
                    {
                        return new Font(family, size, style);
                    }

                    return new Font(
                        family,
                        size,
                        FontStyle.Regular);
                }
            }
            catch
            {
                return new Font(
                    fallback.FontFamily,
                    size,
                    style);
            }
        }

        // =========================================================
        // CONTROLS
        // =========================================================

        private void InitializeControls()
        {
            // INPUT AREA

            inputPanel = new Panel
            {
                Location = new Point(10, 8),
                Size = new Size(443, 95),
                BackColor = backgroundColor
            };

            Controls.Add(inputPanel);

            inputBox = new TextBox
            {
                Location = new Point(0, 0),
                Size = new Size(443, 31),

                Font = preferredInputFont,

                BackColor = controlColor,
                ForeColor = Color.Gray,
                BorderStyle = BorderStyle.FixedSingle,

                Text = "Input word",
                RightToLeft = RightToLeft.No
            };

            inputPanel.Controls.Add(inputBox);

            pronunciationLabel = new Label
            {
                Location = new Point(0, 32),
                Size = new Size(443, 20),

                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,

                Font = pronunciationFont,

                ForeColor = Color.LightGray,
                BackColor = Color.Transparent
            };

            inputPanel.Controls.Add(pronunciationLabel);

            // MATCH OPTIONS

            matchOptionsPanel = new Panel
            {
                Location = new Point(0, 59),
                Size = new Size(443, 30),
                BackColor = backgroundColor
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

            searchButton = new Button
            {
                Location = new Point(367, 0),
                Size = new Size(76, 28),

                Text = "SEARCH",

                FlatStyle = FlatStyle.Flat,
                BackColor = buttonColor,
                ForeColor = Color.White
            };

            searchButton.FlatAppearance.BorderColor =
                Color.FromArgb(90, 90, 90);

            matchOptionsPanel.Controls.Add(searchButton);

            // DIVIDER

            Panel divider = new Panel
            {
                Location = new Point(10, 105),
                Size = new Size(443, 1),
                BackColor = Color.FromArgb(75, 75, 75)
            };

            Controls.Add(divider);

            // RESULTS AREA

            outputPanel = new Panel
            {
                Location = new Point(10, 109),
                Size = new Size(443, 488),

                Anchor =
                    AnchorStyles.Top |
                    AnchorStyles.Bottom |
                    AnchorStyles.Left |
                    AnchorStyles.Right,

                BackColor = backgroundColor
            };

            Controls.Add(outputPanel);

            // SORT OPTIONS

            sortOptionsPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(443, 28),
                BackColor = backgroundColor
            };

            outputPanel.Controls.Add(sortOptionsPanel);

            sortLabel = new Label
            {
                Location = new Point(0, 0),
                Size = new Size(50, 26),

                Text = "SORT",
                TextAlign = ContentAlignment.MiddleLeft,

                ForeColor = Color.White,
                BackColor = Color.Transparent
            };

            sortOptionsPanel.Controls.Add(sortLabel);

            lengthRadio = new RadioButton
            {
                Location = new Point(55, 1),
                Size = new Size(65, 24),

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
                Size = new Size(443, 458),

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

                Font = preferredWordFont,

                SelectionMode = SelectionMode.One
            };

            outputPanel.Controls.Add(resultsListBox);

            // DATABASE REFERENCE LABEL
            // AutoSize makes the label width fit its text.

            referenceLabel = new Label
            {
                Location = new Point(10, 601),

                AutoSize = true,

                Anchor =
                    AnchorStyles.Bottom |
                    AnchorStyles.Left,

                TextAlign = ContentAlignment.MiddleLeft,

                Text = "Reference Not Found",

                ForeColor = Color.LightSkyBlue,
                BackColor = Color.Transparent,

                Cursor = Cursors.Hand
            };

            referenceLabel.Font = new Font(
                referenceLabel.Font,
                FontStyle.Underline);

            Controls.Add(referenceLabel);

            // REFRESH REFERENCE LABEL
            // AutoSize makes this label width fit its text too.

            refreshReferenceLabel = new Label
            {
                Location = new Point(10, 625),

                AutoSize = true,

                Anchor =
                    AnchorStyles.Bottom |
                    AnchorStyles.Left,

                TextAlign = ContentAlignment.MiddleLeft,

                Text = "Refresh Reference",

                ForeColor = Color.FromArgb(100, 100, 100),
                BackColor = Color.Transparent,

                Cursor = Cursors.Default,

                Enabled = false
            };

            refreshReferenceLabel.Font = new Font(
                refreshReferenceLabel.Font,
                FontStyle.Underline);

            Controls.Add(refreshReferenceLabel);

            // EXPORT

            exportButton = new Button
            {
                Location = new Point(373, 611),
                Size = new Size(80, 28),

                Anchor =
                    AnchorStyles.Bottom |
                    AnchorStyles.Right,

                Text = "EXPORT",

                FlatStyle = FlatStyle.Flat,
                BackColor = buttonColor,
                ForeColor = Color.White
            };

            exportButton.FlatAppearance.BorderColor =
                Color.FromArgb(90, 90, 90);

            Controls.Add(exportButton);

            // RIGHT-CLICK MENU

            resultsContextMenu = new ContextMenuStrip();

            copyMenuItem = new ToolStripMenuItem("Copy");

            resultsContextMenu.Items.Add(copyMenuItem);

            resultsListBox.ContextMenuStrip =
                resultsContextMenu;
        }

        // =========================================================
        // EVENTS
        // =========================================================

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

            refreshReferenceLabel.Click +=
                RefreshReferenceLabel_Click;

            KeyDown += MainForm_KeyDown;
        }

        // =========================================================
        // DEFAULT REFERENCE: PS.txt
        // =========================================================

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
                UpdateRefreshReferenceLabel();
                ClearSearchResults();

                pronunciationLabel.Text = "";
                statusLabel.Text = "";

                return;
            }

            try
            {
                List<WordEntry> loaded =
                    ReadDictionaryFile(path);

                if (loaded.Count == 0)
                {
                    dictionary.Clear();

                    referenceLoaded = false;
                    activeReferenceName = "Default";

                    UpdateReferenceLabel();
                    UpdateRefreshReferenceLabel();
                    ClearSearchResults();

                    pronunciationLabel.Text = "";
                    statusLabel.Text = "";

                    return;
                }

                dictionary.Clear();
                dictionary.AddRange(loaded);

                referenceLoaded = true;
                activeReferenceName = "PS.txt";

                UpdateReferenceLabel();
                UpdateRefreshReferenceLabel();

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
                UpdateRefreshReferenceLabel();
                ClearSearchResults();

                pronunciationLabel.Text = "";
                statusLabel.Text = "";
            }
        }

        // =========================================================
        // READ DICTIONARY FILE
        // =========================================================

        private List<WordEntry> ReadDictionaryFile(string path)
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

        // =========================================================
        // LOAD CUSTOM REFERENCE
        // =========================================================

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
                            "The selected file contains no valid dictionary entries.",
                            "Moughoof",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

                        return;
                    }

                    dictionary.Clear();
                    dictionary.AddRange(loaded);

                    referenceLoaded = true;

                    activeReferenceName =
                        Path.GetFileName(dialog.FileName);

                    UpdateReferenceLabel();
                    UpdateRefreshReferenceLabel();

                    ClearSearchResults();
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

        // =========================================================
        // REFRESH REFERENCE: RELOAD PS.txt
        // =========================================================

        private void RefreshReferenceLabel_Click(
            object sender,
            EventArgs e)
        {
            if (!refreshReferenceLabel.Enabled)
                return;

            string path = Path.Combine(
                Application.StartupPath,
                "PS.txt");

            if (!File.Exists(path))
            {
                MessageBox.Show(
                    "PS.txt was not found in the application folder.",
                    "Moughoof",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            try
            {
                List<WordEntry> loaded =
                    ReadDictionaryFile(path);

                if (loaded.Count == 0)
                {
                    MessageBox.Show(
                        "PS.txt contains no valid dictionary entries.",
                        "Moughoof",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    return;
                }

                dictionary.Clear();
                dictionary.AddRange(loaded);

                referenceLoaded = true;
                activeReferenceName = "PS.txt";

                UpdateReferenceLabel();
                UpdateRefreshReferenceLabel();

                ClearSearchResults();
                UpdatePronunciationPreview();

                statusLabel.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not refresh PS.txt.\r\n\r\n" + ex.Message,
                    "Moughoof",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
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

        private void UpdateRefreshReferenceLabel()
        {
            bool isDefaultReference =
                referenceLoaded &&
                string.Equals(
                    activeReferenceName,
                    "PS.txt",
                    StringComparison.OrdinalIgnoreCase);

            refreshReferenceLabel.Enabled =
                referenceLoaded && !isDefaultReference;

            refreshReferenceLabel.ForeColor =
                refreshReferenceLabel.Enabled
                    ? Color.LightSkyBlue
                    : Color.FromArgb(100, 100, 100);

            refreshReferenceLabel.Cursor =
                refreshReferenceLabel.Enabled
                    ? Cursors.Hand
                    : Cursors.Default;
        }

        // =========================================================
        // INPUT
        // =========================================================

        private void InputBox_Enter(object sender, EventArgs e)
        {
            if (inputBox.Text == "Input word")
            {
                inputBox.Clear();
                inputBox.ForeColor = Color.White;
            }
        }

        private void InputBox_TextChanged(object sender, EventArgs e)
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

        // =========================================================
        // WORD LOOKUP
        // =========================================================

        private string NormalizePersianLookup(string value)
        {
            if (value == null)
                return "";

            return value
                .Replace('\u200C', ' ')
                .Trim();
        }

        private WordEntry FindExactPersianWord(string word)
        {
            string normalizedWord =
                NormalizePersianLookup(word);

            return dictionary.FirstOrDefault(
                item => string.Equals(
                    NormalizePersianLookup(item.Persian),
                    normalizedWord,
                    StringComparison.Ordinal));
        }

        private WordEntry FindByPronunciation(string word)
        {
            // Case-sensitive pronunciation lookup.
            return dictionary.FirstOrDefault(
                item => string.Equals(
                    item.Pronunciation,
                    word,
                    StringComparison.Ordinal));
        }

        private WordEntry FindSourceWord(string word)
        {
            WordEntry source = FindExactPersianWord(word);

            if (source != null)
                return source;

            return FindByPronunciation(word);
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

        // =========================================================
        // KEYBOARD
        // =========================================================

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
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

        // =========================================================
        // SEARCH
        // =========================================================

        private void SearchButton_Click(object sender, EventArgs e)
        {
            PerformSearch();
        }

        private void PerformSearch()
        {
            if (!referenceLoaded)
            {
                ClearSearchResults();
                statusLabel.Text = "";
                pronunciationLabel.Text = "";
                return;
            }

            string word = GetInputWord();

            if (string.IsNullOrWhiteSpace(word))
            {
                ClearSearchResults();
                pronunciationLabel.Text = "";
                statusLabel.Text = "";
                return;
            }

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

            if (string.IsNullOrEmpty(pronunciation))
            {
                currentResults.Clear();
                RefreshResults();
                return;
            }

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

        // =========================================================
        // MATCH OPTIONS
        // =========================================================

        private void MatchOption_Changed(object sender, EventArgs e)
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

        // =========================================================
        // SORTING
        // =========================================================

        private void SortOption_Changed(object sender, EventArgs e)
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

        // =========================================================
        // RESULTS
        // =========================================================

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
                    currentResults.Count.ToString() + " Results";
            }
            finally
            {
                resultsListBox.EndUpdate();
                isUpdating = false;
            }

            resultsListBox.Invalidate();
        }

        // =========================================================
        // COPY SELECTED PERSIAN WORD
        // =========================================================

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

        private void CopyMenuItem_Click(object sender, EventArgs e)
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

        // =========================================================
        // EXPORT SEARCHED WORD + VISIBLE RESULTS
        // =========================================================

        private void ExportButton_Click(object sender, EventArgs e)
        {
            if (!referenceLoaded)
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

            string safeWord =
                MakeSafeFileName(lastSearchEntry.Persian);

            string suggestedName =
                "پساوند " + safeWord + ".txt";

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Title = "Export Search Results";

                dialog.Filter =
                    "Text files (*.txt)|*.txt|All files (*.*)|*.*";

                dialog.DefaultExt = "txt";
                dialog.AddExtension = true;
                dialog.OverwritePrompt = true;
                dialog.FileName = suggestedName;

                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                try
                {
                    using (StreamWriter writer = new StreamWriter(
                        dialog.FileName,
                        false,
                        new UTF8Encoding(false)))
                    {
                        // Searched source word first.
                        writer.Write(lastSearchEntry.Persian);
                        writer.Write('\t');
                        writer.WriteLine(lastSearchEntry.Pronunciation);

                        // Export currently visible results in order.
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

                    statusLabel.Text =
                        (resultsListBox.Items.Count + 1).ToString() +
                        " Words Exported";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Could not export the search results.\r\n\r\n" +
                        ex.Message,
                        "Moughoof",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private string MakeSafeFileName(string value)
        {
            foreach (char invalid in Path.GetInvalidFileNameChars())
            {
                value = value.Replace(invalid, '_');
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

    // Persian column: 60% of the row, right-aligned.

    int persianWidth =
        (int)(e.Bounds.Width * 0.6);

    Rectangle persianBounds = new Rectangle(
        e.Bounds.Left + 4,
        e.Bounds.Top,
        persianWidth - 8,
        e.Bounds.Height);

    // Latin pronunciation column: remaining width,
    // left-aligned.

    Rectangle latinBounds = new Rectangle(
        e.Bounds.Left + persianWidth,
        e.Bounds.Top,
        e.Bounds.Width - persianWidth - 6,
        e.Bounds.Height);

    // Keep the currently selected Persian font.
    using (Font persianFont = new Font(
        inputBox.Font.FontFamily,
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

    // Keep the current Latin font.
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

        // =========================================================
        // CLEANUP
        // =========================================================

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (preferredWordFont != null)
                preferredWordFont.Dispose();

            if (preferredInputFont != null)
                preferredInputFont.Dispose();

            if (pronunciationFont != null)
                pronunciationFont.Dispose();

            if (fallbackFont != null)
                fallbackFont.Dispose();

            base.OnFormClosed(e);
        }
    }
}
