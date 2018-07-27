namespace CHIP8EmulatorTestApp
{
    partial class Registers
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblRegisterName = new System.Windows.Forms.Label();
            this.lblRegisterValue = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblRegisterName
            // 
            this.lblRegisterName.AutoSize = true;
            this.lblRegisterName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.lblRegisterName.Location = new System.Drawing.Point(12, 37);
            this.lblRegisterName.Name = "lblRegisterName";
            this.lblRegisterName.Size = new System.Drawing.Size(17, 16);
            this.lblRegisterName.TabIndex = 0;
            this.lblRegisterName.Text = "V";
            // 
            // lblRegisterValue
            // 
            this.lblRegisterValue.AutoSize = true;
            this.lblRegisterValue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblRegisterValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.lblRegisterValue.Location = new System.Drawing.Point(45, 35);
            this.lblRegisterValue.Name = "lblRegisterValue";
            this.lblRegisterValue.Size = new System.Drawing.Size(23, 18);
            this.lblRegisterValue.TabIndex = 1;
            this.lblRegisterValue.Text = "0x";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Enabled = false;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(171, 24);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            this.menuStrip1.Visible = false;
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.openToolStripMenuItem.Text = "Open";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            // 
            // Registers
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(171, 102);
            this.Controls.Add(this.lblRegisterValue);
            this.Controls.Add(this.lblRegisterName);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "Registers";
            this.Text = "Registers";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblRegisterName;
        private System.Windows.Forms.Label lblRegisterValue;

        public byte[] RegisterList { get; set; }

        public void InitializeRegisters(int numberOfRegisters)
        {
            lblRegisterName.Visible = false;
            lblRegisterValue.Visible = false;
            for (int i = 0; i < numberOfRegisters; i++)
            {
                System.Windows.Forms.Label lblNameTmp = new System.Windows.Forms.Label();
                lblNameTmp.AutoSize = true;
                lblNameTmp.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
                lblNameTmp.Location = new System.Drawing.Point(12, 10);
                lblNameTmp.Name = "lblRegisterName" + i;
                lblNameTmp.Size = new System.Drawing.Size(17, 16);
                lblNameTmp.Text = string.Format("V{0:X1}", i);
                lblNameTmp.Top = ((lblNameTmp.Height + 5) * i) + 10;
                this.Controls.Add(lblNameTmp);

                System.Windows.Forms.Label lblValueTmp = new System.Windows.Forms.Label();
                lblValueTmp.AutoSize = false;
                lblValueTmp.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
                lblValueTmp.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
                lblValueTmp.Location = new System.Drawing.Point(44, 9);
                lblValueTmp.Name = "lblRegisterValue" + i;
                lblValueTmp.Size = new System.Drawing.Size(38, 18);
                lblValueTmp.TabIndex = 1;
                lblValueTmp.Top = ((lblValueTmp.Height + 3) * i) + 9;
                lblValueTmp.Text = "0x";
                this.Controls.Add(lblValueTmp);

            }

            //this.Height = (lblRegisterName.Height + 4) * numberOfRegisters + 9;
        }

        public void UpdateRegisters(byte[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                System.Windows.Forms.Label lbl = (System.Windows.Forms.Label)this.Controls["lblRegisterValue" + i];
                if (null != lbl)
                {
                    lbl.Text = string.Format("0x{0:X2}", values[i]);
                }
            }
        }

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
    }
}