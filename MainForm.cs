using System;
using System.Drawing;
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

        public MainForm()
        {
            InitializeForm();
            InitializeControls();
            LoadTestResults();
        }

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
        }

        private void InitializeControls()
        {
            // ================================================
            // GROUP 1: INPUT / MATCH / SEARCH
            // ================================================

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
                Text = "5 Results"
            };

            searchGroup.Controls.Add(statusLabel);

            // ================================================
            // GROUP 2: SORT / RESULTS
            // ================================================

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

            // OWNER-DRAW LISTBOX

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

            resultsListBox.DrawItem +=
                ResultsListBox_DrawItem;

            resultsGroup.Controls.Add(resultsListBox);

            // ================================================
            // DATABASE LABEL
            // ================================================

            databaseLabel = new Label
            {
                Location = new Point(15, 585),
                Size = new Size(300, 25),

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

            // ================================================
            // EXPORT
            // ================================================

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

        private void ResultsListBox_DrawItem(
            object sender,
            DrawItemEventArgs e)
        {
            if (e.Index < 0 ||
                e.Index >= resultsListBox.Items.Count)
            {
                return;
            }

            bool selected =
                (e.State & DrawItemState.Selected) ==
                DrawItemState.Selected;

            Color backgroundColor;

            if (selected)
            {
                backgroundColor =
                    SystemColors.Highlight;
            }
            else
            {
                backgroundColor =
                    (e.Index % 2 == 0)
                        ? rowColor1
                        : rowColor2;
            }

            using (SolidBrush backgroundBrush =
                new SolidBrush(backgroundColor))
            {
                e.Graphics.FillRectangle(
                    backgroundBrush,
                    e.Bounds);
            }

            Color textColor = selected
                ? SystemColors.HighlightText
                : Color.White;

            TextRenderer.DrawText(
                e.Graphics,
                resultsListBox.Items[e.Index].ToString(),
                resultsListBox.Font,
                e.Bounds,
                textColor,
                TextFormatFlags.Left |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.NoPrefix);

            e.DrawFocusRectangle();
        }

        private void LoadTestResults()
        {
            resultsListBox.Items.Add("سلام        salam");
            resultsListBox.Items.Add("کتاب        ketAb");
            resultsListBox.Items.Add("دوست        dust");
            resultsListBox.Items.Add("خانه        xAne");
            resultsListBox.Items.Add("مردم        mardom");
        }
    }
}
