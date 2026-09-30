using System;
using System.Drawing;
using System.Windows.Forms;

namespace Moughoof
{
    public class MainForm : Form
    {
        public MainForm()
        {
            Text = "Moughoof";
            ClientSize = new Size(460, 650);
            BackColor = Color.FromArgb(32, 32, 32);
            ForeColor = Color.White;

            TextBox input = new TextBox();
            input.Location = new Point(20, 20);
            input.Size = new Size(400, 30);
            Controls.Add(input);

            Button button = new Button();
            button.Text = "TEST";
            button.Location = new Point(20, 70);
            button.Size = new Size(100, 35);
            Controls.Add(button);

            Label label = new Label();
            label.Text = "If you can see this, WinForms is working.";
            label.Location = new Point(20, 120);
            label.AutoSize = true;
            label.ForeColor = Color.White;
            Controls.Add(label);
        }
    }
}
