namespace Crypto.Futures.FrontEnd
{
    partial class FrmMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            menuStrip1 = new MenuStrip();
            boToolStripMenuItem = new ToolStripMenuItem();
            MnuArbitrageBot = new ToolStripMenuItem();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(32, 32);
            menuStrip1.Items.AddRange(new ToolStripItem[] { boToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(2321, 42);
            menuStrip1.TabIndex = 1;
            menuStrip1.Text = "menuStrip1";
            // 
            // boToolStripMenuItem
            // 
            boToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { MnuArbitrageBot });
            boToolStripMenuItem.Name = "boToolStripMenuItem";
            boToolStripMenuItem.Size = new Size(70, 38);
            boToolStripMenuItem.Text = "Bot";
            // 
            // MnuArbitrageBot
            // 
            MnuArbitrageBot.Name = "MnuArbitrageBot";
            MnuArbitrageBot.Size = new Size(359, 44);
            MnuArbitrageBot.Text = "Arbitrage";
            MnuArbitrageBot.Click += MnuArbitrageBot_Click;
            // 
            // FrmMain
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(2321, 1075);
            Controls.Add(menuStrip1);
            IsMdiContainer = true;
            MainMenuStrip = menuStrip1;
            Name = "FrmMain";
            Text = "Crypto.Futures.Bot Interface";
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip1;
        private ToolStripMenuItem boToolStripMenuItem;
        private ToolStripMenuItem MnuArbitrageBot;
    }
}
