namespace Crypto.Futures.FrontEnd
{
    partial class FrmFundingRate
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmFundingRate));
            toolStrip1 = new ToolStrip();
            ToolStart = new ToolStripButton();
            ToolStop = new ToolStripButton();
            toolStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // toolStrip1
            // 
            toolStrip1.ImageScalingSize = new Size(32, 32);
            toolStrip1.Items.AddRange(new ToolStripItem[] { ToolStart, ToolStop });
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(2477, 42);
            toolStrip1.TabIndex = 0;
            toolStrip1.Text = "toolStrip1";
            // 
            // ToolStart
            // 
            ToolStart.DisplayStyle = ToolStripItemDisplayStyle.Image;
            ToolStart.Image = (Image)resources.GetObject("ToolStart.Image");
            ToolStart.ImageTransparentColor = Color.Magenta;
            ToolStart.Name = "ToolStart";
            ToolStart.Size = new Size(46, 36);
            ToolStart.Text = "Start bot";
            // 
            // ToolStop
            // 
            ToolStop.DisplayStyle = ToolStripItemDisplayStyle.Image;
            ToolStop.Image = (Image)resources.GetObject("ToolStop.Image");
            ToolStop.ImageTransparentColor = Color.Magenta;
            ToolStop.Name = "ToolStop";
            ToolStop.Size = new Size(46, 36);
            ToolStop.Text = "Stop bot";
            // 
            // FrmFundingRate
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(2477, 932);
            Controls.Add(toolStrip1);
            Name = "FrmFundingRate";
            Text = "Funding Rate Arbitrage";
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ToolStrip toolStrip1;
        private ToolStripButton ToolStart;
        private ToolStripButton ToolStop;
    }
}