
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
        private class WordEntry
        {
            public string Persian { get; set; }
            public string Pronunciation { get; set; }

            public WordEntry(string persian, string pronunciation)
            {
                Persian = persian;
                Pronunciation = pronunciation;
            }
        }

        private Panel searchPanel;
        private Panel resultsPanel;

        private TextBox inputBox;
        private Label pronunciationLabel;

        private RadioButton firstRadio;
        private RadioButton lastRadio;
        private NumericUpDown matchCount;

        private Button searchButton;
        private Label statusLabel;

        private RadioButton alphabetRadio;
        private RadioButton lengthRadio;

        private ListBox resultsListBox;

        private Label referenceLabel;
        private Button exportButton;

        private ContextMenuStrip resultsContextMenu;
        private ToolStripMenuItem copyMenuItem;

        private readonly Color backgroundColor = Color.FromArgb(32, 32, 32);
        private readonly Color panelColor = Color.FromArgb(40, 40, 40);
        private readonly Color rowColor1 = Color.FromArgb(40, 40, 40);
        private readonly Color rowColor2 = Color.FromArgb(56, 56, 56);
        private readonly Color selectedColor = Color.FromArgb(65, 100, 145);
        private readonly Color foregroundColor = Color.Gainsboro;

        private List<WordEntry> dictionary = new List<WordEntry>();
        private List<WordEntry> currentResults = new List<WordEntry>();

        private string lastSearchWord = "";
        private string activeReferenceName = "Default";
        private bool referenceLoaded = false;
        private bool isUpdating = false;

        public MainForm()
        {
            InitializeForm();
            InitializeControls();
            ConnectEvents();
            LoadDefaultDictionary();
        }

        private void InitializeForm()
        {
            Text = "Moughoof";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(444, 611);
            MinimumSize = new Size(460, 570);
            MaximumSize = new Size(460, 2000);

            BackColor = backgroundColor;
            ForeColor = foregroundColor;
            Font = new Font("Segoe UI", 9F);
            KeyPreview = true;
        }

        private void InitializeControls()
        {
            searchPanel = new Panel
            {
                Location = new Point(10, 8),
                Size = new Size(424, 151),
                BackColor = panelColor,
                Padding = new Padding(4)
            };

            inputBox = new TextBox
            {
                Location = new Point(7, 7),
                Size = new Size(406, 25),
                Font = new Font("Segoe UI", 10F),
                BackColor = Color.FromArgb(48, 48, 48),
                ForeColor = foregroundColor,
                BorderStyle = BorderStyle.FixedSingle,
                RightToLeft = RightToLeft.No
            };

            pronunciationLabel = new Label
            {
                Location = new Point(8, 34),
                Size = new Size(405, 19),
                Text = "",
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.Silver,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 9F)
            };

            firstRadio = new RadioButton
            {
                Location = new Point(8, 60),
                AutoSize = true,
                Text = "FIRST",
                ForeColor = foregroundColor,
                BackColor = Color.Transparent
            };

            lastRadio = new RadioButton
            {
                Location = new Point(83, 60),
                AutoSize = true,
                Text = "LAST",
                ForeColor = foregroundColor,
                BackColor = Color.Transparent,
                Checked = true
            };

            matchCount = new NumericUpDown
            {
                Location = new Point(151, 59),
                Size = new Size(53, 23),
                Minimum = 1,
                Maximum = 50,
                Value = 2,
                BackColor = Color.FromArgb(48, 48, 48),
                ForeColor = foregroundColor,
                BorderStyle = BorderStyle.FixedSingle
            };

            searchButton = new Button
            {
                Location = new Point(282, 57),
                Size = new Size(131, 27),
                Text = "Search",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(58, 58, 58),
                ForeColor = foregroundColor,
                Cursor = Cursors.Hand
            };
            searchButton.FlatAppearance.BorderColor = Color.FromArgb(75, 75, 75);

            alphabetRadio = new RadioButton
            {
                Location = new Point(8, 101),
                AutoSize = true,
                Text = "ALPHABET",
                ForeColor = foregroundColor,
                BackColor = Color.Transparent,
                Checked = true
            };

            lengthRadio = new RadioButton
            {
                Location = new Point(112, 101),
                AutoSize = true,
                Text = "LENGTH",
                ForeColor = foregroundColor,
                BackColor = Color.Transparent
            };

            statusLabel = new Label
            {
                Location = new Point(8, 125),
                Size = new Size(405, 18),
                Text = "",
                ForeColor = Color.Silver,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft
            };

            searchPanel.Controls.Add(inputBox);
            searchPanel.Controls.Add(pronunciationLabel);
            searchPanel.Controls.Add(firstRadio);
            searchPanel.Controls.Add(lastRadio);
            searchPanel.Controls.Add(matchCount);
            searchPanel.Controls.Add(searchButton);
            searchPanel.Controls.Add(alphabetRadio);
            searchPanel.Controls.Add(lengthRadio);
            searchPanel.Controls.Add(statusLabel);

            resultsPanel = new Panel
            {
                Location = new Point(10, 167),
                Size = new Size(424, 404),
                BackColor = panelColor,
                Padding = new Padding(0),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom |
                         AnchorStyles.Left | AnchorStyles.Right
            };

            resultsListBox = new ListBox
            {
                Location = new Point(0, 0),
                Size = new Size(424, 404),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom |
                         AnchorStyles.Left | AnchorStyles.Right,
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 27,
                BorderStyle = BorderStyle.None,
                IntegralHeight = false,
                BackColor = panelColor,
                ForeColor = foregroundColor,
                SelectionMode = SelectionMode.One,
                HorizontalScrollbar = false
            };

            resultsPanel.Controls.Add(resultsListBox);

            referenceLabel = new Label
            {
                Location = new Point(10, 582),
                AutoSize = true,
                Text = "Reference Not Found",
                ForeColor = Color.Silver,
                BackColor = backgroundColor,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Underline),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Left | AnchorStyles.Bottom,
                Margin = Padding.Empty
            };

            exportButton = new Button
            {
                Location = new Point(365, 579),
                Size = new Size(69, 25),
                Text = "Export",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(48, 48, 48),
                ForeColor = foregroundColor,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom,
                Margin = Padding.Empty
            };
            exportButton.FlatAppearance.BorderColor = Color.FromArgb(75, 75, 75);

            resultsContextMenu = new ContextMenuStrip
            {
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = foregroundColor
            };

            copyMenuItem = new ToolStripMenuItem("Copy");
            resultsContextMenu.Items.Add(copyMenuItem);
            resultsListBox.ContextMenuStrip = resultsContextMenu;

            Controls.Add(searchPanel);
            Controls.Add(resultsPanel);
            Controls.Add(referenceLabel);
            Controls.Add(exportButton);
        }

        private void ConnectEvents()
        {
            inputBox.Enter += delegate
            {
                inputBox.SelectAll();
            };

            inputBox.TextChanged += delegate
            {
                UpdatePronunciationPreview();
            };

            searchButton.Click += delegate
            {
                PerformSearch();
            };

            firstRadio.CheckedChanged += MatchOptionChanged;
            lastRadio.CheckedChanged += MatchOptionChanged;

            alphabetRadio.CheckedChanged += SortOptionChanged;
            lengthRadio.CheckedChanged += SortOptionChanged;

            resultsListBox.DrawItem += ResultsListBox_DrawItem;
            resultsListBox.MouseDown += ResultsListBox_MouseDown;

            copyMenuItem.Click += delegate
            {
                CopySelectedPersian();
            };

            exportButton.Click += delegate
            {
                ExportCurrentResults();
            };

            referenceLabel.Click += ReferenceLabel_Click;

            KeyDown += MainForm_KeyDown;
        }

        private void LoadDefaultDictionary()
        {
            string path = Path.Combine(Application.StartupPath, "PS.txt");

            if (!File.Exists(path))
            {
                dictionary.Clear();
                referenceLoaded = false;
                activeReferenceName = "Default";
                UpdateReferenceLabel();
                ClearSearchResults();
                pronunciationLabel.Text = "";
                statusLabel.Text = "";
                return;
            }

            try
            {
                dictionary = ReadDictionaryFile(path);
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

        private List<WordEntry> ReadDictionaryFile(string path)
        {
            List<WordEntry> entries = new List<WordEntry>();

            using (StreamReader reader = new StreamReader(path, Encoding.UTF8, true))
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
                    string pronunciation = line.Substring(tabIndex + 1).Trim();

                    if (persian.Length == 0 || pronunciation.Length == 0)
                        continue;

                    entries.Add(new WordEntry(persian, pronunciation));
                }
            }

            return entries;
        }

        private void ReferenceLabel_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Load Reference";
                dialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                dialog.CheckFileExists = true;
                dialog.Multiselect = false;

                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                try
                {
                    List<WordEntry> loadedDictionary =
                        ReadDictionaryFile(dialog.FileName);

                    dictionary = loadedDictionary;
                    referenceLoaded = true;
                    activeReferenceName = Path.GetFileName(dialog.FileName);

                    UpdateReferenceLabel();
                    ClearSearchResults();
                    UpdatePronunciationPreview();
                    statusLabel.Text = "";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        this,
                        "Could not load the reference.\n\n" + ex.Message,
                        "Load Reference",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
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

        private string NormalizePersianLookup(string value)
        {
            if (value == null)
                return "";

            return value.Replace('\u200C', ' ').Trim();
        }

        private void UpdatePronunciationPreview()
        {
            if (!referenceLoaded)
            {
                pronunciationLabel.Text = "";
                return;
            }

            string input = NormalizePersianLookup(inputBox.Text);

            if (input.Length == 0)
            {
                pronunciationLabel.Text = "";
                return;
            }

            WordEntry entry = dictionary.FirstOrDefault(
                item => NormalizePersianLookup(item.Persian) == input);

            pronunciationLabel.Text =
                entry == null ? "" : entry.Pronunciation;
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                e.Handled = true;
                PerformSearch();
                return;
            }

            if (e.Control && e.KeyCode == Keys.C &&
                resultsListBox.SelectedItem is WordEntry)
            {
                CopySelectedPersian();
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
        }

        private void PerformSearch()
        {
            if (!referenceLoaded)
            {
                ClearSearchResults();
                pronunciationLabel.Text = "";
                statusLabel.Text = "";
                return;
            }

            string input = NormalizePersianLookup(inputBox.Text);
            lastSearchWord = input;

            if (input.Length == 0)
            {
                ClearSearchResults();
                pronunciationLabel.Text = "";
                statusLabel.Text = "";
                return;
            }

            WordEntry sourceEntry = dictionary.FirstOrDefault(
                item => NormalizePersianLookup(item.Persian) == input);

            if (sourceEntry == null)
            {
                ClearSearchResults();
                pronunciationLabel.Text = "";
                statusLabel.Text = "Word not found";
                return;
            }

            pronunciationLabel.Text = sourceEntry.Pronunciation;

            string pronunciation = sourceEntry.Pronunciation;
            int count = (int)matchCount.Value;

            string pattern;

            if (firstRadio.Checked)
            {
                pattern = pronunciation.Substring(
                    0, Math.Min(count, pronunciation.Length));
            }
            else
            {
                pattern = pronunciation.Substring(
                    Math.Max(0, pronunciation.Length - count));
            }

            currentResults = dictionary
                .Where(item =>
                    NormalizePersianLookup(item.Persian) != input &&
                    (firstRadio.Checked
                        ? item.Pronunciation.StartsWith(
                            pattern, StringComparison.Ordinal)
                        : item.Pronunciation.EndsWith(
                            pattern, StringComparison.Ordinal)))
                .ToList();

            SortCurrentResults();
            RefreshResults();
        }

        private void MatchOptionChanged(object sender, EventArgs e)
        {
            if (isUpdating)
                return;

            if (lastSearchWord.Length > 0)
                PerformSearch();
        }

        private void SortOptionChanged(object sender, EventArgs e)
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
                    .OrderBy(item => item.Persian, StringComparer.Ordinal)
                    .ToList();
            }
            else if (lengthRadio.Checked)
            {
                currentResults = currentResults
                    .OrderBy(item => item.Persian.Length)
                    .ThenBy(item => item.Persian, StringComparer.Ordinal)
                    .ToList();
            }
        }

        private void RefreshResults()
        {
            isUpdating = true;

            resultsListBox.BeginUpdate();
            resultsListBox.Items.Clear();

            foreach (WordEntry entry in currentResults)
                resultsListBox.Items.Add(entry);

            resultsListBox.EndUpdate();

            statusLabel.Text = currentResults.Count + " Results";
            resultsListBox.Invalidate();

            isUpdating = false;
        }

        private void ClearSearchResults()
        {
            isUpdating = true;

            currentResults.Clear();
            resultsListBox.Items.Clear();

            isUpdating = false;
        }

        private void ResultsListBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            int index = resultsListBox.IndexFromPoint(e.Location);

            if (index >= 0)
                resultsListBox.SelectedIndex = index;
            else
                resultsListBox.ClearSelected();
        }

        private void CopySelectedPersian()
        {
            WordEntry selected = resultsListBox.SelectedItem as WordEntry;

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
                    "Could not copy the selected word.\n\n" + ex.Message,
                    "Copy",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void ExportCurrentResults()
        {
            if (!referenceLoaded)
            {
                MessageBox.Show(
                    this,
                    "No reference is loaded.",
                    "Export",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            List<WordEntry> itemsToExport = resultsListBox.Items
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
                dialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                dialog.DefaultExt = "txt";
                dialog.AddExtension = true;
                dialog.OverwritePrompt = true;

                string defaultName = activeReferenceName;

                if (string.IsNullOrWhiteSpace(defaultName) ||
                    defaultName == "Default")
                {
                    defaultName = "Results";
                }

                if (!defaultName.EndsWith(
                    ".txt", StringComparison.OrdinalIgnoreCase))
                {
                    defaultName += ".txt";
                }

                dialog.FileName = defaultName;

                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                try
                {
                    using (StreamWriter writer = new StreamWriter(
                        dialog.FileName, false, new UTF8Encoding(false)))
                    {
                        foreach (WordEntry entry in itemsToExport)
                        {
                            writer.Write(entry.Persian);
                            writer.Write('\t');
                            writer.WriteLine(entry.Pronunciation);
                        }
                    }

                    statusLabel.Text =
                        itemsToExport.Count + " results exported";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        this,
                        "Could not export the results.\n\n" + ex.Message,
                        "Export",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private void ResultsListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= resultsListBox.Items.Count)
                return;

            bool selected = (e.State & DrawItemState.Selected) ==
                            DrawItemState.Selected;

            Color background = selected
                ? selectedColor
                : (e.Index % 2 == 0 ? rowColor1 : rowColor2);

            using (SolidBrush brush = new SolidBrush(background))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }

            WordEntry entry = resultsListBox.Items[e.Index] as WordEntry;

            if (entry != null)
            {
                int persianWidth = (int)(e.Bounds.Width * 0.60);

                Rectangle persianRect = new Rectangle(
                    e.Bounds.X + 5,
                    e.Bounds.Y,
                    persianWidth - 10,
                    e.Bounds.Height);

                Rectangle pronunciationRect = new Rectangle(
                    e.Bounds.X + persianWidth,
                    e.Bounds.Y,
                    e.Bounds.Width - persianWidth - 5,
                    e.Bounds.Height);

                TextRenderer.DrawText(
                    e.Graphics,
                    entry.Persian,
                    new Font("Segoe UI", 11F),
                    persianRect,
                    foregroundColor,
                    TextFormatFlags.Right |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.EndEllipsis |
                    TextFormatFlags.NoPadding);

                TextRenderer.DrawText(
                    e.Graphics,
                    entry.Pronunciation,
                    new Font("Segoe UI", 9.5F),
                    pronunciationRect,
                    foregroundColor,
                    TextFormatFlags.Left |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.EndEllipsis |
                    TextFormatFlags.NoPadding);
            }

            e.DrawFocusRectangle();
        }
    }
}
