using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CHIP8Emulator
{
    public partial class DebugConsole : Form
    {
        private int _iMaxLineCount = 1000;

        public int MaxLineCount
        {
            get { return _iMaxLineCount; }
            set { _iMaxLineCount = value; }
        }

        public DebugConsole()
        {
            InitializeComponent();
        }

        private void rtbConsoleTextBox_TextChanged(object sender, EventArgs e)
        {
            if (rtbConsoleTextBox.Lines.Length > MaxLineCount)
            {
                rtbConsoleTextBox.Lines = rtbConsoleTextBox.Lines.Skip(Math.Abs(MaxLineCount - rtbConsoleTextBox.Lines.Length)).ToArray();
            }
            // autoscroll
            rtbConsoleTextBox.SelectionStart = rtbConsoleTextBox.Text.Length;
            rtbConsoleTextBox.ScrollToCaret();
        }

        public void AddLine(string line)
        {
            List<string> lsTMP = new List<string>(rtbConsoleTextBox.Lines);
            lsTMP.Add(line);
            rtbConsoleTextBox.Lines = lsTMP.ToArray();
        }
    }
}
