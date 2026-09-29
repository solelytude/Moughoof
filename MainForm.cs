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

        private readonly List<WordEntry> entries = new List<WordEntry>();

        private TextBox inputBox;
        private NumericUpDown matchBox;
        private Button searchButton;
        private ListBox resultList;
        private Label pronunciationLabel;
        private Label statusLabel;

        public MainForm()
        {
            Text = "Moughoof";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(500, 600);
            MinimumSize = new Size(400, 500);

            BackColor = Color.FromArgb(30, 30, 30);
            ForeColor = Color.White;

            BuildInterface();
            LoadDictionary();
        }

        private void BuildInterface()
        {
            inputBox = new TextBox();
            inputBox.Location = new Point(20, 20);
            inputBox.Size = new Size(460, 32);
            inputBox.Font = new Font("Segoe UI", 12);
            inputBox.RightToLeft = RightToLeft.Yes;
            inputBox.BackColor = Color.FromArgb(45, 45, 45);
            inputBox.ForeColor = Color.White;

            pronunciationLabel = new Label();
            pronunciationLabel.Location = new Point(22, 58);
            pronunciationLabel.Size = new Size(456, 25);
            pronunciationLabel.Font = new Font("Segoe UI", 9);
            pronunciationLabel.ForeColor = Color.Silver;
            pronunciationLabel.Text = "";

            Label matchLabel = new Label();
            matchLabel.Location = new Point(20, 95);
            matchLabel.Size = new Size(55, 25);
            matchLabel.Text = "Match:";
            matchLabel.ForeColor = Color.White;

            matchBox = new NumericUpDown();
            matchBox.Location = new Point(80, 92);
            matchBox.Size = new Size(55, 25);
            matchBox.Minimum = 1;
            matchBox.Maximum = 20;
            matchBox.Value = 2;

            searchButton = new Button();
            searchButton.Location = new Point(150, 90);
            searchButton.Size = new Size(90, 30);
            searchButton.Text = "Search";
            searchButton.Click += SearchButton_Click;

            statusLabel = new Label();
            statusLabel.Location = new Point(250, 95);
            statusLabel.Size = new Size(230, 25);
            statusLabel.ForeColor = Color.Silver;

            resultList = new ListBox();
            resultList.Location = new Point(20, 135);
            resultList.Size = new Size(460, 400);
            resultList.Font = new Font("Segoe UI", 10);
            resultList.BackColor = Color.FromArgb(40, 40, 40);
            resultList.ForeColor = Color.White;

            Controls.Add(inputBox);
            Controls.Add(pronunciationLabel);
            Controls.Add(matchLabel);
            Controls.Add(matchBox);
            Controls.Add(searchButton);
            Controls.Add(statusLabel);
            Controls.Add(resultList);
        }

        private void LoadDictionary()
        {
            string filePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "PS.txt"
            );

            if (!File.Exists(filePath))
            {
                statusLabel.Text = "PS.txt not found.";
                return;
            }

            try
            {
                using (StreamReader reader = new StreamReader(
                    filePath,
                    Encoding.UTF8,
                    true))
                {
                    string line;

                    while ((line = reader.ReadLine()) != null)
                    {
                        if (string.IsNullOrWhiteSpace(line))
                            continue;

                        string[] parts = line.Split(
                            new char[] { '\t' },
                            2
                        );

                        if (parts.Length < 2)
                            continue;

                        string word = parts[0].Trim();
                        string pronunciation = parts[1].Trim();

                        if (word.Length == 0 || pronunciation.Length == 0)
                            continue;

                        entries.Add(new WordEntry
                        {
                            Word = word,
                            Pronunciation = pronunciation
                        });
                    }
                }

                statusLabel.Text = entries.Count + " entries loaded.";
            }
            catch (Exception ex)
            {
                statusLabel.Text = "Error loading PS.txt";
                MessageBox.Show(
                    ex.Message,
                    "Moughoof",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void SearchButton_Click(object sender, EventArgs e)
        {
            string word = inputBox.Text.Trim();

            if (word.Length == 0)
            {
                pronunciationLabel.Text = "";
                resultList.Items.Clear();
                statusLabel.Text = "";
                return;
            }

            string pronunciation = FindPronunciation(word);

            pronunciationLabel.Text = pronunciation;

            if (pronunciation.Length == 0)
            {
                resultList.Items.Clear();
                statusLabel.Text = "Word not found.";
                return;
            }

            int count = (int)matchBox.Value;

            if (pronunciation.Length < count)
            {
                resultList.Items.Clear();
                statusLabel.Text = "Pronunciation is too short.";
                return;
            }

            string suffix = pronunciation.Substring(
                pronunciation.Length - count,
                count
            );

            resultList.Items.Clear();

            int matches = 0;

            foreach (WordEntry entry in entries)
            {
                if (entry.Pronunciation.Length < count)
                    continue;

                string entrySuffix = entry.Pronunciation.Substring(
                    entry.Pronunciation.Length - count,
                    count
                );

                if (entrySuffix == suffix)
                {
                    resultList.Items.Add(
                        entry.Word + "    " + entry.Pronunciation
                    );

                    matches++;
                }
            }

            statusLabel.Text = matches + " matches found.";
        }

        private string FindPronunciation(string word)
        {
            foreach (WordEntry entry in entries)
            {
                if (entry.Word == word)
                    return entry.Pronunciation;
            }

            return "";
        }
    }
}
