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
    public partial class DebugWindow : Form
    {
        public int PC { get; set; }
        public int I { get; set; }
        public int OpCode { get; set; }
        public ulong Cycle { get; set; }
        public int SP { get; set; }
        public int SoundTimer { get; set; }
        public int DelayTimer { get; set; }
        public short[] Stack { get; set; }
        public byte[] Registers { get; set; }

        public event EventHandler onStep;
        public event EventHandler onStart;

        public DebugWindow()
        {
            InitializeComponent();
        }

        public void UpdateValues()
        {
            lblPC.Text = string.Format("0x{0:X2}", PC);
            lblI.Text = string.Format("0x{0:X2}", I);
            lblOpCode.Text = string.Format("0x{0:X2}", OpCode);
            lblCycle.Text = Cycle.ToString();
            lblSP.Text = SP.ToString();
            lblSoundTimer.Text = SoundTimer.ToString();
            lblDelayTimer.Text = DelayTimer.ToString();
            lbStackList.Items.Clear();
            foreach (short sItem in Stack)
            {
                lbStackList.Items.Add(string.Format("0x{0:X4}", sItem));
            }
            for (int i = 0; i < Registers.Length; i++)
            {
                ((Label)gbRegistersGroupBox.Controls["lblRegister_" + i]).Text = string.Format("0x{0:X2}", Registers[i]);
            }
        }

        private void btnStepCycle_Click(object sender, EventArgs e)
        {
            OnStep(e);
        }

        protected virtual void OnStep(EventArgs e)
        {
            if (null != onStep)
            {
                onStep(this, e);
            }
        }

        private void btnStartCycle_Click(object sender, EventArgs e)
        {
            OnStart(e);
        }

        protected virtual void OnStart(EventArgs e)
        {
            if (null != onStart)
            {
                onStart(this, e);
            }
        }
    }
}
