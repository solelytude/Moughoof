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

            // Process keyboard shortcuts at form level.
            KeyPreview = true;
        }

        // ================================================
        // CONTROLS
        // ================================================

        private void InitializeControls()
        {
            // GROUP 1: INPUT / MATCH / SEARCH

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

                Text = ""
            };

            searchGroup.Controls.Add(statusLabel);

            // GROUP 2: SORT / RESULTS

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

                Font = new Font("Segoe UI", 10f),

                SelectionMode = SelectionMode.One
            };

            resultsGroup.Controls.Add(resultsListBox);

            // DATABASE REFERENCE LABEL

            referenceLabel = new Label
            {
                Location = new Point(15, 585),
                Size = new Size(335, 25),

                Anchor =
                    AnchorStyles.Bottom |
                    AnchorStyles.Left,

                AutoSize = false,
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

            // RIGHT-CLICK MENU

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
        // DEFAULT REFERENCE
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
                ClearSearchResults();

                // Missing reference must not display
                // any status message.
                statusLabel.Text = "";

                return;
            }

            try
            {
                List<WordEntry> loaded =
                    ReadDictionaryFile(path);

                dictionary.Clear();
                dictionary.AddRange(loaded);

                referenceLoaded = true;
                activeReferenceName = "Default";

                UpdateReferenceLabel();
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
                ClearSearchResults();

                pronunciationLabel.Text = "";
                statusLabel.Text = "";
            }
        }

        // ================================================
        // READ DICTIONARY FILE
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

                    // Preserve the stored word and pronunciation.
                    // Remove only line-ending whitespace artifacts.
                    persian = persian.Trim();
                    pronunciation = pronunciation.Trim();

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
        // LOAD CUSTOM REFERENCE
        // ================================================

        private void ReferenceLabel_Click(
            object sender,
            EventArgs e)
        {
            using (OpenFileDialog dialog =
                new OpenFileDialog())
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
                    // Read first. Replace the active reference only
                    // after the file has been read successfully.
                    List<WordEntry> loaded =
                        ReadDictionaryFile(dialog.FileName);

                    dictionary.Clear();
                    dictionary.AddRange(loaded);

                    referenceLoaded = true;

                    activeReferenceName =
                        Path.GetFileName(dialog.FileName);

                    UpdateReferenceLabel();

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

        private string GetInputWord()
        {
            string word = inputBox.Text.Trim();

            if (word == "Input word")
                return "";

            return word;
        }

        // ================================================
        // PERSIAN LOOKUP
        // ================================================

        private string NormalizePersianLookup(string value)
        {
            if (value == null)
                return "";

            // Treat ZWNJ and regular space as equivalent
            // for lookup only. Do not alter stored display text.

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
            if (!referenceLoaded)
            {
                pronunciationLabel.Text = "";
                return;
            }

            string word = GetInputWord();

            WordEntry entry = FindExactWord(word);

            pronunciationLabel.Text =
                entry == null
                    ? ""
                    : entry.Pronunciation;
        }

        // ================================================
        // GLOBAL KEYBOARD HANDLING
        // ================================================

        private void MainForm_KeyDown(
            object sender,
            KeyEventArgs e)
        {
            // Enter triggers Search regardless of which
            // control currently has focus.

            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                e.Handled = true;

                PerformSearch();
                return;
            }

            // Ctrl+C copies the Persian word of the
            // selected result, regardless of focus.

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
        // EXPORT ACTIVE REFERENCE
        // ================================================

        private void ExportButton_Click(
            object sender,
            EventArgs e)
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

            using (SaveFileDialog dialog =
                new SaveFileDialog())
            {
                dialog.Title = "Export Reference";

                dialog.Filter =
                    "Text files (*.txt)|*.txt|All files (*.*)|*.*";

                dialog.DefaultExt = "txt";
                dialog.AddExtension = true;
                dialog.OverwritePrompt = true;

                string suggestedName =
                    activeReferenceName;

                if (string.IsNullOrWhiteSpace(suggestedName) ||
                    suggestedName == "Default")
                {
                    suggestedName = "Reference.txt";
                }
                else if (!suggestedName.EndsWith(
                    ".txt",
                    StringComparison.OrdinalIgnoreCase))
                {
                    suggestedName += ".txt";
                }

                dialog.FileName = suggestedName;

                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                try
                {
                    using (StreamWriter writer =
                        new StreamWriter(
                            dialog.FileName,
                            false,
                            new UTF8Encoding(false)))
                    {
                        foreach (WordEntry item in dictionary)
                        {
                            writer.Write(item.Persian);
                            writer.Write('\t');
                            writer.WriteLine(item.Pronunciation);
                        }
                    }

                    statusLabel.Text = "Export completed";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Could not export the reference.\r\n\r\n" +
                        ex.Message,
                        "Moughoof",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
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
