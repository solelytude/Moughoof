using System;
using System.Drawing;
using System.Windows.Forms;

namespace Moughoof;

public class MainForm : Form
{
    public MainForm()
    {
        Text = "Moughoof";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(500, 600);
        MinimumSize = new Size(400, 500);

        BackColor = Color.FromArgb(30, 30, 30);
        ForeColor = Color.White;
    }
}
