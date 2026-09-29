using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Moughoof
{
    public class MainForm : Form
    {
        private class WordEntry
        {
            public string Word;
            public string Pronunciation;
        }

        private readonly List<WordEntry> entries =
            new List<WordEntry>();

        private readonly List<WordEntry> currentResults =
            new List<WordEntry>();

        private TextBox inputBox;
        private Label pronunciationLabel;

        private RadioButton firstRadio;
        private RadioButton lastRadio;
        private NumericUpDown matchBox;
        private Button searchButton;

        private Label statusLabel;
        private ListBox resultList;
        private Button exportButton;

        private Font resultPersianFont;
        private Font resultPronunciationFont;

        private bool showingPlaceholder;

        private const string PlaceholderText = "Input word";

        public MainForm()
        {
            Text = "Moughoof";

            StartPosition = FormStartPosition.CenterScreen;

            // عرض ثابت، ارتفاع قابل تغییر
            ClientSize = new Size(460, 520);
            MinimumSize = new Size(460, 400);
            MaximumSize = new Size(460, 2000);

            BackColor = Color.FromArgb(30, 30, 30);
            ForeColor = Color.White;

            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = false;

            resultPersianFont =
                new Font("Segoe UI", 10.0f, FontStyle.Regular);

            resultPronunciationFont =
                new Font("Segoe UI", 8.5f, FontStyle.Regular);

            BuildInterface();
            LoadDictionary();
        }

        private void BuildInterface()
        {
            // -------------------------------------------------
            // Input
            // -------------------------------------------------

            inputBox = new TextBox();

            inputBox.Location = new Point(20, 18);
            inputBox.Size = new Size(420, 30);

            inputBox.Font =
                new Font("Segoe UI", 11.5f);

            inputBox.Text = PlaceholderText;
            inputBox.ForeColor = Color.Gray;
            inputBox.BackColor =
                Color.FromArgb(45, 45, 45);

            inputBox.BorderStyle = BorderStyle.FixedSingle;

            inputBox.RightToLeft = RightToLeft.No;

            inputBox.Enter += InputBox_Enter;
            inputBox.Leave += InputBox_Leave;

            showingPlaceholder = true;

            // -------------------------------------------------
            // Pronunciation
            // -------------------------------------------------

            pronunciationLabel = new Label();

            pronunciationLabel.Location =
                new Point(22, 53);

            pronunciationLabel.Size =
                new Size(416, 22);

            pronunciationLabel.Font =
                new Font("Segoe UI", 8.5f);

            pronunciationLabel.ForeColor =
                Color.Silver;

            pronunciationLabel.Text = "";

            pronunciationLabel.TextAlign =
                ContentAlignment.MiddleLeft;

            // -------------------------------------------------
            // Match controls
            // -------------------------------------------------

            Label matchLabel = new Label();

            matchLabel.Location =
                new Point(20, 82);

            matchLabel.Size =
                new Size(45, 24);

            matchLabel.Text = "Match:";

            matchLabel.ForeColor =
                Color.White;

            matchLabel.TextAlign =
                ContentAlignment.MiddleLeft;


            firstRadio = new RadioButton();

            firstRadio.Location =
                new Point(66, 81);

            firstRadio.Size =
                new Size(60, 25);

            firstRadio.Text = "first";

            firstRadio.ForeColor =
                Color.White;

            firstRadio.Checked = false;


            lastRadio = new RadioButton();

            lastRadio.Location =
                new Point(127, 81);

            lastRadio.Size =
                new Size(57, 25);

            lastRadio.Text = "last";

            lastRadio.ForeColor =
                Color.White;

            lastRadio.Checked = true;


            matchBox = new NumericUpDown();

            matchBox.Location =
                new Point(190, 82);

            matchBox.Size =
                new Size(52, 25);

            matchBox.Minimum = 1;
            matchBox.Maximum = 50;
            matchBox.Value = 2;


            searchButton = new Button();

            searchButton.Location =
                new Point(250, 80);

            searchButton.Size =
                new Size(80, 28);

            searchButton.Text = "Search";

            searchButton.Click +=
                SearchButton_Click;


            statusLabel = new Label();

            statusLabel.Location =
                new Point(338, 82);

            statusLabel.Size =
                new Size(102, 24);

            statusLabel.ForeColor =
                Color.Silver;

            statusLabel.Text =
                "0 matches found.";

            statusLabel.TextAlign =
                ContentAlignment.MiddleLeft;

            // -------------------------------------------------
            // Results
            // -------------------------------------------------

            resultList = new ListBox();

            resultList.Location =
                new Point(20, 118);

            resultList.Size =
                new Size(420, 350);

            resultList.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Bottom |
                AnchorStyles.Left |
                AnchorStyles.Right;

            resultList.Font =
                resultPersianFont;

            resultList.BackColor =
                Color.FromArgb(40, 40, 40);

            resultList.ForeColor =
                Color.White;

            resultList.BorderStyle =
                BorderStyle.FixedSingle;

            resultList.DrawMode =
                DrawMode.OwnerDrawFixed;

            resultList.ItemHeight = 25;

            resultList.IntegralHeight = false;

            resultList.HorizontalScrollbar = false;

            resultList.DrawItem +=
                ResultList_DrawItem;

            // -------------------------------------------------
            // Export
            // -------------------------------------------------

            exportButton = new Button();

            exportButton.Location =
                new Point(20, 480);

            exportButton.Size =
                new Size(80, 28);

            exportButton.Text = "Export";

            exportButton.Anchor =
                AnchorStyles.Bottom |
                AnchorStyles.Left;

            exportButton.Enabled = false;

            exportButton.Click +=
                ExportButton_Click;

            // -------------------------------------------------
            // Add controls
            // -------------------------------------------------

            Controls.Add(inputBox);
            Controls.Add(pronunciationLabel);

            Controls.Add(matchLabel);
            Controls.Add(firstRadio);
            Controls.Add(lastRadio);
            Controls.Add(matchBox);
            Controls.Add(searchButton);
            Controls.Add(statusLabel);

            Controls.Add(resultList);
            Controls.Add(exportButton);
        }

        // =====================================================
        // Placeholder
        // =====================================================

        private void InputBox_Enter(
            object sender,
            EventArgs e)
        {
            if (!showingPlaceholder)
                return;

            inputBox.Text = "";
            inputBox.ForeColor = Color.White;
            inputBox.RightToLeft = RightToLeft.Yes;

            showingPlaceholder = false;
        }

        private void InputBox_Leave(
            object sender,
            EventArgs e)
        {
            if (inputBox.Text.Trim().Length != 0)
                return;

            inputBox.Text = PlaceholderText;
            inputBox.ForeColor = Color.Gray;
            inputBox.RightToLeft = RightToLeft.No;

            showingPlaceholder = true;
        }

        // =====================================================
        // Load dictionary
        // =====================================================

        private void LoadDictionary()
        {
            string filePath =
                Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "PS.txt");

            if (!File.Exists(filePath))
            {
                statusLabel.Text =
                    "PS.txt not found.";

                MessageBox.Show(
                    "PS.txt was not found next to Moughoof.exe.",
                    "Moughoof",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            try
            {
                using (StreamReader reader =
                    new StreamReader(
                        filePath,
                        Encoding.UTF8,
                        true))
                {
                    string line;

                    while ((line = reader.ReadLine()) != null)
                    {
                        if (string.IsNullOrWhiteSpace(line))
                            continue;

                        string[] parts =
                            line.Split(
                                new char[] { '\t' },
                                2);

                        if (parts.Length < 2)
                            continue;

                        string word =
                            parts[0].Trim();

                        string pronunciation =
                            parts[1].Trim();

                        if (word.Length == 0 ||
                            pronunciation.Length == 0)
                            continue;

                        entries.Add(
                            new WordEntry
                            {
                                Word = word,
                                Pronunciation = pronunciation
                            });
                    }
                }

                statusLabel.Text =
                    "0 matches found.";
            }
            catch (Exception ex)
            {
                statusLabel.Text =
                    "Load error.";

                MessageBox.Show(
                    ex.Message,
                    "Moughoof",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =====================================================
        // Search
        // =====================================================

        private void SearchButton_Click(
            object sender,
            EventArgs e)
        {
            if (showingPlaceholder)
                return;

            string inputWord =
                inputBox.Text.Trim();

            if (inputWord.Length == 0)
                return;

            currentResults.Clear();
            resultList.Items.Clear();

            pronunciationLabel.Text = "";

            string pronunciation =
                FindPronunciation(inputWord);

            if (pronunciation.Length == 0)
            {
                statusLabel.Text =
                    "0 matches found.";

                exportButton.Enabled = false;

                return;
            }

            pronunciationLabel.Text =
                pronunciation;

            int count =
                (int)matchBox.Value;

            // مهم:
            // Match فقط روی تلفظ لاتین انجام می‌شود
            // و تشدیدهای نوشته‌شده با تکرار حرف حذف می‌شوند.

            string normalizedPronunciation =
                NormalizePronunciation(pronunciation);

            if (normalizedPronunciation.Length < count)
            {
                statusLabel.Text =
                    "0 matches found.";

                exportButton.Enabled = false;

                return;
            }

            bool useFirst =
                firstRadio.Checked;

            string target;

            if (useFirst)
            {
                target =
                    normalizedPronunciation.Substring(
                        0,
                        count);
            }
            else
            {
                target =
                    normalizedPronunciation.Substring(
                        normalizedPronunciation.Length - count,
                        count);
            }

            foreach (WordEntry entry in entries)
            {
                string normalizedEntry =
                    NormalizePronunciation(
                        entry.Pronunciation);

                if (normalizedEntry.Length < count)
                    continue;

                string part;

                if (useFirst)
                {
                    part =
                        normalizedEntry.Substring(
                            0,
                            count);
                }
                else
                {
                    part =
                        normalizedEntry.Substring(
                            normalizedEntry.Length - count,
                            count);
                }

                // مقایسه case-sensitive است.
                if (part == target)
                {
                    currentResults.Add(entry);
                    resultList.Items.Add(entry);
                }
            }

            statusLabel.Text =
                currentResults.Count +
                " matches found.";

            exportButton.Enabled =
                currentResults.Count > 0;
        }

        // =====================================================
        // Persian word lookup
        // =====================================================

        private string FindPronunciation(
            string inputWord)
        {
            string normalizedInput =
                NormalizePersianWord(inputWord);

            foreach (WordEntry entry in entries)
            {
                string normalizedEntry =
                    NormalizePersianWord(entry.Word);

                if (normalizedEntry == normalizedInput)
                    return entry.Pronunciation;
            }

            return "";
        }

        // =====================================================
        // Persian normalization
        // =====================================================

        private string NormalizePersianWord(
            string value)
        {
            if (value == null)
                return "";

            // نیم‌فاصله و space هر دو معادل هستند.
            // برای جستجوی کلمات مرکب، هر دو حذف می‌شوند.

            StringBuilder result =
                new StringBuilder(
                    value.Length);

            for (int i = 0;
                 i < value.Length;
                 i++)
            {
                char c = value[i];

                if (c == ' ' ||
                    c == '\u200C')
                {
                    continue;
                }

                result.Append(c);
            }

            return result.ToString();
        }

        // =====================================================
        // Latin pronunciation normalization
        // =====================================================

        private string NormalizePronunciation(
            string pronunciation)
        {
            if (pronunciation == null)
                return "";

            if (pronunciation.Length <= 1)
                return pronunciation;

            StringBuilder result =
                new StringBuilder(
                    pronunciation.Length);

            char previous = pronunciation[0];

            result.Append(previous);

            for (int i = 1;
                 i < pronunciation.Length;
                 i++)
            {
                char current =
                    pronunciation[i];

                // تکرار متوالی یک کاراکتر،
                // که در این فرهنگ برای تشدید استفاده شده،
                // فقط یک بار لحاظ می‌شود.
                if (current == previous)
                    continue;

                result.Append(current);
                previous = current;
            }

            return result.ToString();
        }

        // =====================================================
        // Result drawing
        // =====================================================

        private void ResultList_DrawItem(
            object sender,
            DrawItemEventArgs e)
        {
            if (e.Index < 0 ||
                e.Index >= resultList.Items.Count)
                return;

            WordEntry entry =
                resultList.Items[e.Index]
                as WordEntry;

            if (entry == null)
                return;

            bool selected =
                (e.State &
                 DrawItemState.Selected) != 0;

            Color background =
                selected
                    ? Color.FromArgb(65, 65, 65)
                    : Color.FromArgb(40, 40, 40);

            Color persianColor =
                Color.White;

            Color pronunciationColor =
                Color.Silver;

            using (SolidBrush backgroundBrush =
                new SolidBrush(background))
            {
                e.Graphics.FillRectangle(
                    backgroundBrush,
                    e.Bounds);
            }

            // نسبت ستون‌ها: 3 : 2
            int persianWidth =
                (e.Bounds.Width * 3) / 5;

            Rectangle persianRect =
                new Rectangle(
                    e.Bounds.Left + 4,
                    e.Bounds.Top,
                    persianWidth - 8,
                    e.Bounds.Height);

            Rectangle pronunciationRect =
                new Rectangle(
                    e.Bounds.Left +
                    persianWidth +
                    4,
                    e.Bounds.Top,
                    e.Bounds.Width -
                    persianWidth -
                    8,
                    e.Bounds.Height);

            TextRenderer.DrawText(
                e.Graphics,
                entry.Word,
                resultPersianFont,
                persianRect,
                persianColor,
                TextFormatFlags.Right |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.NoPadding);

            TextRenderer.DrawText(
                e.Graphics,
                entry.Pronunciation,
                resultPronunciationFont,
                pronunciationRect,
                pronunciationColor,
                TextFormatFlags.Left |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.NoPadding);

            // خط جداکنندهٔ بسیار ظریف
            using (Pen separatorPen =
                new Pen(Color.FromArgb(55, 55, 55)))
            {
                e.Graphics.DrawLine(
                    separatorPen,
                    e.Bounds.Left,
                    e.Bounds.Bottom - 1,
                    e.Bounds.Right,
                    e.Bounds.Bottom - 1);
            }

            if (selected)
                e.DrawFocusRectangle();
        }

        // =====================================================
        // Export
        // =====================================================

        private void ExportButton_Click(
            object sender,
            EventArgs e)
        {
            if (currentResults.Count == 0)
                return;

            string inputWord =
                showingPlaceholder
                    ? ""
                    : inputBox.Text.Trim();

            string pronunciation =
                pronunciationLabel.Text;

            int count =
                (int)matchBox.Value;

            bool useFirst =
                firstRadio.Checked;

            string normalizedPronunciation =
                NormalizePronunciation(
                    pronunciation);

            string matchPart;

            if (useFirst)
            {
                matchPart =
                    normalizedPronunciation.Substring(
                        0,
                        count);
            }
            else
            {
                matchPart =
                    normalizedPronunciation.Substring(
                        normalizedPronunciation.Length -
                        count,
                        count);
            }

            SaveFileDialog dialog =
                new SaveFileDialog();

            dialog.Filter =
                "Text files (*.txt)|*.txt|All files (*.*)|*.*";

            dialog.DefaultExt = "txt";

            dialog.AddExtension = true;

            dialog.FileName =
                "Moughoof-" +
                SanitizeFileName(inputWord) +
                ".txt";

            if (dialog.ShowDialog() !=
                DialogResult.OK)
            {
                return;
            }

            StringBuilder output =
                new StringBuilder();

            output.AppendLine(
                "Word: " + inputWord);

            output.AppendLine(
                "Pronunciation: " +
                pronunciation);

            output.AppendLine(
                "Match mode: " +
                (useFirst ? "first" : "last"));

            output.AppendLine(
                "Match range: " +
                count);

            output.AppendLine(
                (useFirst ? "Prefix: " : "Suffix: ") +
                matchPart);

            output.AppendLine();

            output.AppendLine(
                "Matches found: " +
                currentResults.Count);

            output.AppendLine();

            foreach (WordEntry entry
                     in currentResults)
            {
                output.AppendLine(
                    entry.Word +
                    "\t" +
                    entry.Pronunciation);
            }

            try
            {
                File.WriteAllText(
                    dialog.FileName,
                    output.ToString(),
                    new UTF8Encoding(false));

                MessageBox.Show(
                    "Export completed.",
                    "Moughoof",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Moughoof",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private string SanitizeFileName(
            string value)
        {
            if (string.IsNullOrEmpty(value))
                return "results";

            char[] invalid =
                Path.GetInvalidFileNameChars();

            StringBuilder result =
                new StringBuilder(value.Length);

            for (int i = 0;
                 i < value.Length;
                 i++)
            {
                char c = value[i];

                bool invalidCharacter = false;

                for (int j = 0;
                     j < invalid.Length;
                     j++)
                {
                    if (c == invalid[j])
                    {
                        invalidCharacter = true;
                        break;
                    }
                }

                if (!invalidCharacter)
                    result.Append(c);
            }

            if (result.Length == 0)
                return "results";

            return result.ToString();
        }

        protected override void Dispose(
            bool disposing)
        {
            if (disposing)
            {
                if (resultPersianFont != null)
                    resultPersianFont.Dispose();

                if (resultPronunciationFont != null)
                    resultPronunciationFont.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
