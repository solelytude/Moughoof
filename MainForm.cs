
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
        private Panel outputPanel;
        private Panel sortOptionsPanel;
        private Panel divider;

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
        private Button exportButton;

        private ContextMenuStrip resultsContextMenu;
        private ToolStripMenuItem copyMenuItem;

        private readonly Color backgroundColor =
            Color.FromArgb(32, 32, 32);

        private readonly Color panelColor =
            Color.FromArgb(38, 38, 38);

        private readonly Color rowColor1 =
            Color.FromArgb(40, 40, 40);

        private readonly Color rowColor2 =
            Color.FromArgb(56, 56, 56);

        private readonly Color selectedColor =
            Color.FromArgb(65, 100, 145);

        private readonly Color foregroundColor =
            Color.White;

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

            public WordEntry(string persian, string pronunciation)
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
            InitializeControls();
            ConnectEvents();
            LoadDefaultDictionary();
        }

        // ==================================================
        // FORM
        // ==================================================

        private void InitializeForm()
        {
            Text = "Moughoof";

            ClientSize = new Size(460, 650);
            MinimumSize = new Size(460, 530);
            MaximumSize = new Size(460, 2000);

            MinimizeBox = true;
            MaximizeBox = false;
            FormBorderStyle = FormBorderStyle.Sizable;

            BackColor = backgroundColor;
            ForeColor = foregroundColor;
            Font = new Font("Segoe UI", 9f);

            KeyPreview = true;
        }

        // ==================================================
        // CONTROLS AND LAYOUT
        // ==================================================

        private void InitializeControls()
        {
            // INPUT PANEL

            inputPanel = new Panel
            {
                Location = new Point(10, 8),
                Size = new Size(440, 151),
                BackColor = backgroundColor,
                Anchor = AnchorStyles.Top |
                         AnchorStyles.Left |
                         AnchorStyles.Right
            };

            Controls.Add(inputPanel);

            inputBox = new TextBox
            {
                Location = new Point(5, 4),
                Size = new Size(430, 30),
                Font = new Font("Segoe UI", 11.5f, FontStyle.Bold),
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = Color.Gray,
                BorderStyle = BorderStyle.FixedSingle,
                Text = "Input word",
                RightToLeft = RightToLeft.No,
                Anchor = AnchorStyles.Top |
                         AnchorStyles.Left |
                         AnchorStyles.Right
            };

            inputPanel.Controls.Add(inputBox);

            pronunciationLabel = new Label
            {
                Location = new Point(5, 35),
                Size = new Size(430, 20),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.LightGray,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top |
                         AnchorStyles.Left |
                         AnchorStyles.Right
            };

            inputPanel.Controls.Add(pronunciationLabel);

            // MATCH OPTIONS PANEL
            // Its radio buttons are isolated from the sort options.

            matchOptionsPanel = new Panel
            {
                Location = new Point(3, 59),
                Size = new Size(260, 31),
                BackColor = backgroundColor
            };

            inputPanel.Controls.Add(matchOptionsPanel);

            matchLabel = new Label
            {
                Location = new Point(2, 2),
                Size = new Size(52, 25),
                Text = "MATCH",
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = foregroundColor,
                BackColor = Color.Transparent
            };

            matchOptionsPanel.Controls.Add(matchLabel);

            firstRadio = new RadioButton
            {
                Location = new Point(57, 3),
                Size = new Size(65, 24),
                Text = "FIRST",
                ForeColor = foregroundColor,
                BackColor = Color.Transparent,
                AutoCheck = true
            };

            matchOptionsPanel.Controls.Add(firstRadio);

            lastRadio = new RadioButton
            {
                Location = new Point(123, 3),
                Size = new Size(58, 24),
                Text = "LAST",
                ForeColor = foregroundColor,
                BackColor = Color.Transparent,
                Checked = true,
                AutoCheck = true
            };

            matchOptionsPanel.Controls.Add(lastRadio);

            matchCountBox = new NumericUpDown
            {
                Location = new Point(187, 3),
                Size = new Size(52, 24),
                Minimum = 1,
                Maximum = 50,
                Value = 2,
                TextAlign = HorizontalAlignment.Center
            };

            matchOptionsPanel.Controls.Add(matchCountBox);

            // SEARCH BUTTON

            searchButton = new Button
            {
                Location = new Point(5, 98),
                Size = new Size(100, 30),
                Text = "SEARCH",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(55, 55, 55),
                ForeColor = foregroundColor,
                Cursor = Cursors.Hand
            };

            searchButton.FlatAppearance.BorderColor =
                Color.FromArgb(90, 90, 90);

            inputPanel.Controls.Add(searchButton);

            // THIN DIVIDER BETWEEN INPUT AND OUTPUT

            divider = new Panel
            {
                Location = new Point(10, 165),
                Size = new Size(440, 1),
                BackColor = Color.FromArgb(85, 85, 85),
                Anchor = AnchorStyles.Top |
                         AnchorStyles.Left |
                         AnchorStyles.Right
            };

            Controls.Add(divider);

            // OUTPUT PANEL

            outputPanel = new Panel
            {
                Location = new Point(10, 174),
                Size = new Size(440, 397),
                BackColor = backgroundColor,
                Anchor = AnchorStyles.Top |
                         AnchorStyles.Bottom |
                         AnchorStyles.Left |
                         AnchorStyles.Right
            };

            Controls.Add(outputPanel);

            // SORT OPTIONS PANEL
            // Separate parent ensures independent radio groups.

            sortOptionsPanel = new Panel
            {
                Location = new Point(3, 0),
                Size = new Size(235, 29),
                BackColor = backgroundColor,
                Anchor = AnchorStyles.Top |
                         AnchorStyles.Left
            };

            outputPanel.Controls.Add(sortOptionsPanel);

            sortLabel = new Label
            {
                Location = new Point(2, 2),
                Size = new Size(48, 24),
                Text = "SORT",
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = foregroundColor,
                BackColor = Color.Transparent
            };

            sortOptionsPanel.Controls.Add(sortLabel);

            alphabetRadio = new RadioButton
            {
                Location = new Point(51, 2),
                Size = new Size(105, 24),
                Text = "ALPHABET",
                ForeColor = foregroundColor,
                BackColor = Color.Transparent,
                Checked = true,
                AutoCheck = true
            };

            sortOptionsPanel.Controls.Add(alphabetRadio);

            lengthRadio = new RadioButton
            {
                Location = new Point(157, 2),
                Size = new Size(75, 24),
                Text = "LENGTH",
                ForeColor = foregroundColor,
                BackColor = Color.Transparent,
                AutoCheck = true
            };

            sortOptionsPanel.Controls.Add(lengthRadio);

            // RESULTS COUNT AT THE TOP RIGHT

            statusLabel = new Label
            {
                Location = new Point(245, 2),
                Size = new Size(190, 24),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleRight,
                ForeColor = Color.LightGray,
                BackColor = Color.Transparent,
                Text = "",
                Anchor = AnchorStyles.Top |
                         AnchorStyles.Right
            };

            outputPanel.Controls.Add(statusLabel);

            // RESULTS LIST

            resultsListBox = new ListBox
            {
                Location = new Point(5, 31),
                Size = new Size(430, 361),
                Anchor = AnchorStyles.Top |
                         AnchorStyles.Bottom |
                         AnchorStyles.Left |
                         AnchorStyles.Right,
                BackColor = rowColor1,
                ForeColor = foregroundColor,
                BorderStyle = BorderStyle.FixedSingle,
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 27,
                IntegralHeight = false,
                Font = new Font("Segoe UI", 10f),
                SelectionMode = SelectionMode.One,
                HorizontalScrollbar = false
            };

            outputPanel.Controls.Add(resultsListBox);

            // REFERENCE LABEL
            // Kept independent of both panels.

            referenceLabel = new Label
            {
                Location = new Point(10, 610),
                AutoSize = true,
                Text = "Reference Not Found",
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.LightSkyBlue,
                BackColor = backgroundColor,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Bottom |
                         AnchorStyles.Left,
                Margin = Padding.Empty
            };

            referenceLabel.Font = new Font(
                "Segoe UI",
                8.5f,
                FontStyle.Underline);

            Controls.Add(referenceLabel);

            // EXPORT BUTTON

            exportButton = new Button
            {
                Location = new Point(370, 605),
                Size = new Size(80, 29),
                Anchor = AnchorStyles.Bottom |
                         AnchorStyles.Right,
                Text = "EXPORT",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(55, 55, 55),
                ForeColor = foregroundColor,
                Cursor = Cursors.Hand,
                Enabled = false,
                Margin = Padding.Empty
            };

            exportButton.FlatAppearance.BorderColor =
                Color.FromArgb(90, 90, 90);

            Controls.Add(exportButton);

            // RIGHT-CLICK MENU

            resultsContextMenu = new ContextMenuStrip
            {
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = foregroundColor
            };

            copyMenuItem = new ToolStripMenuItem("Copy");
            resultsContextMenu.Items.Add(copyMenuItem);

            resultsListBox.ContextMenuStrip = resultsContextMenu;
        }

        // ==================================================
        // EVENTS
        // ==================================================

        private void ConnectEvents()
        {
            inputBox.Enter += InputBox_Enter;
            inputBox.TextChanged += InputBox_TextChanged;

            searchButton.Click += SearchButton_Click;

            firstRadio.CheckedChanged += MatchOption_Changed;
            lastRadio.CheckedChanged += MatchOption_Changed;

            matchCountBox.ValueChanged += MatchCountBox_ValueChanged;

            alphabetRadio.CheckedChanged += SortOption_Changed;
            lengthRadio.CheckedChanged += SortOption_Changed;

            resultsListBox.DrawItem += ResultsListBox_DrawItem;
            resultsListBox.MouseDown += ResultsListBox_MouseDown;

            copyMenuItem.Click += CopyMenuItem_Click;

            exportButton.Click += ExportButton_Click;
            referenceLabel.Click += ReferenceLabel_Click;

            KeyDown += MainForm_KeyDown;
        }

        // ==================================================
        // DEFAULT REFERENCE
        // ==================================================

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
                List<WordEntry> loaded = ReadDictionaryFile(path);

                dictionary.Clear();
                dictionary.AddRange(loaded);

                referenceLoaded = true;
                activeReferenceName = "Default";

                UpdateReferenceLabel();
                UpdateExportButton();
                ClearSearchResults();

                pronunciationLabel.Text = "";
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

                    int tabIndex = line.IndexOf('\t');

                    if (tabIndex < 0)
                        continue;

                    string persian = line.Substring(0, tabIndex).Trim();
                    string pronunciation =
                        line.Substring(tabIndex + 1).Trim();

                    if (string.IsNullOrWhiteSpace(persian) ||
                        string.IsNullOrWhiteSpace(pronunciation))
                    {
                        continue;
                    }

                    loaded.Add(new WordEntry(persian, pronunciation));
                }
            }

            return loaded;
        }

        // ==================================================
        // LOAD CUSTOM REFERENCE
        // ==================================================

        private void ReferenceLabel_Click(object sender, EventArgs e)
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

                    dictionary.Clear();
                    dictionary.AddRange(loaded);

                    referenceLoaded = true;
                    activeReferenceName =
                        Path.GetFileName(dialog.FileName);

                    UpdateReferenceLabel();
                    UpdateExportButton();
                    ClearSearchResults();

                    UpdatePronunciationPreview();
                    statusLabel.Text = "";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        this,
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
            }
            else
            {
                referenceLabel.Text =
                    "Reference: " + activeReferenceName +
                    ", " + dictionary.Count + " words";
            }

            referenceLabel.AutoSize = true;
        }

        private void UpdateExportButton()
        {
            exportButton.Enabled = referenceLoaded;

            exportButton.ForeColor = referenceLoaded
                ? foregroundColor
                : Color.Gray;
        }

        // ==================================================
        // INPUT
        // ==================================================

        private void InputBox_Enter(object sender, EventArgs e)
        {
            if (inputBox.Text == "Input word")
            {
                inputBox.Clear();
                inputBox.ForeColor = foregroundColor;
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

        // ==================================================
        // PERSIAN LOOKUP
        // ==================================================

        private string NormalizePersianLookup(string value)
        {
            if (value == null)
                return "";

            return value.Replace('\u200C', ' ').Trim();
        }

        private WordEntry FindExactWord(string word)
        {
            string normalizedWord = NormalizePersianLookup(word);

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
                entry == null ? "" : entry.Pronunciation;
        }

        // ==================================================
        // KEYBOARD
        // ==================================================

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

        // ==================================================
        // SEARCH
        // ==================================================

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
                lastSearchWord = "";

                ClearSearchResults();
                pronunciationLabel.Text = "";
                statusLabel.Text = "";

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

            count = Math.Min(count, pronunciation.Length);

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

        // ==================================================
        // MATCH OPTIONS - REAL TIME
        // ==================================================

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

        // ==================================================
        // SORTING - REAL TIME
        // ==================================================

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

        // ==================================================
        // RESULTS
        // ==================================================

        private void RefreshResults()
        {
            isUpdating = true;
            resultsListBox.BeginUpdate();

            try
            {
                resultsListBox.Items.Clear();

                foreach (WordEntry item in currentResults)
                    resultsListBox.Items.Add(item);

                statusLabel.Text = referenceLoaded
                    ? currentResults.Count + " Results"
                    : "";
            }
            finally
            {
                resultsListBox.EndUpdate();
                isUpdating = false;
            }

            resultsListBox.Invalidate();
        }

        // ==================================================
        // COPY SELECTED WORD
        // ==================================================

        private void ResultsListBox_MouseDown(
            object sender,
            MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            int index = resultsListBox.IndexFromPoint(e.Location);

            if (index >= 0 && index < resultsListBox.Items.Count)
                resultsListBox.SelectedIndex = index;
            else
                resultsListBox.ClearSelected();
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
                    this,
                    "Could not copy the selected word.\r\n\r\n" +
                    ex.Message,
                    "Moughoof",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        // ==================================================
        // EXPORT CURRENT LISTBOX CONTENTS
        // ==================================================

        private void ExportButton_Click(object sender, EventArgs e)
        {
            if (!referenceLoaded)
                return;

            List<WordEntry> itemsToExport =
                resultsListBox.Items
                    .Cast<object>()
                    .OfType<WordEntry>()
                    .ToList();

            if (itemsToExport.Count == 0)
            {
                MessageBox.Show(
                    this,
                    "There are no results to export.",
                    "Export",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Title = "Export Current Results";
                dialog.Filter =
                    "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                dialog.DefaultExt = "txt";
                dialog.AddExtension = true;
                dialog.OverwritePrompt = true;

                string suggestedName = "Results.txt";

                if (!string.IsNullOrWhiteSpace(activeReferenceName) &&
                    activeReferenceName != "Default")
                {
                    suggestedName = Path.GetFileNameWithoutExtension(
                        activeReferenceName) + "_Results.txt";
                }

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
                        foreach (WordEntry item in itemsToExport)
                        {
                            writer.Write(item.Persian);
                            writer.Write('\t');
                            writer.WriteLine(item.Pronunciation);
                        }
                    }

                    statusLabel.Text =
                        itemsToExport.Count + " Results Exported";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        this,
                        "Could not export the results.\r\n\r\n" +
                        ex.Message,
                        "Moughoof",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        // ==================================================
        // OWNER-DRAW RESULTS LIST
        // ==================================================

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

            Color itemBackground = selected
                ? selectedColor
                : (e.Index % 2 == 0 ? rowColor1 : rowColor2);

            using (SolidBrush brush = new SolidBrush(itemBackground))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }

            int persianWidth = (int)(e.Bounds.Width * 0.6);

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
                    foregroundColor,
                    TextFormatFlags.Right |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.NoPrefix |
                    TextFormatFlags.SingleLine |
                    TextFormatFlags.EndEllipsis);
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
                    foregroundColor,
                    TextFormatFlags.Left |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.NoPrefix |
                    TextFormatFlags.SingleLine |
                    TextFormatFlags.EndEllipsis);
            }

            e.DrawFocusRectangle();
        }
    }
}
