namespace DebugHelperTester
{
    partial class Form1
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
            this.dbgMain = new DebugHelper.DebugViewer();
            this.SuspendLayout();
            // 
            // dbgMain
            // 
            this.dbgMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dbgMain.Location = new System.Drawing.Point(0, 0);
            this.dbgMain.Name = "dbgMain";
            this.dbgMain.SelectedObject = null;
            this.dbgMain.Size = new System.Drawing.Size(672, 493);
            this.dbgMain.TabIndex = 0;
            this.dbgMain.Text = "debugViewer1";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(672, 493);
            this.Controls.Add(this.dbgMain);
            this.Name = "Form1";
            this.Text = "DebugHelper Tester";
            this.ResumeLayout(false);

        }

        #endregion

        private DebugHelper.DebugViewer dbgMain;
    }
}

