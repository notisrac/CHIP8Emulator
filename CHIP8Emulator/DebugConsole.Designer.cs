namespace CHIP8Emulator
{
    partial class DebugConsole
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
            this.rtbConsoleTextBox = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // rtbConsoleTextBox
            // 
            this.rtbConsoleTextBox.BackColor = System.Drawing.Color.Black;
            this.rtbConsoleTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbConsoleTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbConsoleTextBox.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.rtbConsoleTextBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.rtbConsoleTextBox.Location = new System.Drawing.Point(0, 0);
            this.rtbConsoleTextBox.Name = "rtbConsoleTextBox";
            this.rtbConsoleTextBox.ReadOnly = true;
            this.rtbConsoleTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
            this.rtbConsoleTextBox.Size = new System.Drawing.Size(734, 381);
            this.rtbConsoleTextBox.TabIndex = 0;
            this.rtbConsoleTextBox.Text = "";
            this.rtbConsoleTextBox.TextChanged += new System.EventHandler(this.rtbConsoleTextBox_TextChanged);
            // 
            // DebugConsole
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(734, 381);
            this.Controls.Add(this.rtbConsoleTextBox);
            this.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.Name = "DebugConsole";
            this.Text = "DebugConsole";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox rtbConsoleTextBox;
    }
}