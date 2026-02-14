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
            ToolClose = new ToolStripButton();
            SplMain = new SplitContainer();
            splitContainer2 = new SplitContainer();
            GridBalances = new DataGridView();
            PanBalances = new Panel();
            LblBalances = new Label();
            LblTotalBalance = new Label();
            splitContainer1 = new SplitContainer();
            GridPositions = new DataGridView();
            panel1 = new Panel();
            label1 = new Label();
            LblPnlPositions = new Label();
            GridChances = new DataGridView();
            panel2 = new Panel();
            label2 = new Label();
            LblPnlChances = new Label();
            BackWorkerMain = new System.ComponentModel.BackgroundWorker();
            toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)SplMain).BeginInit();
            SplMain.Panel1.SuspendLayout();
            SplMain.Panel2.SuspendLayout();
            SplMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)GridBalances).BeginInit();
            PanBalances.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)GridPositions).BeginInit();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)GridChances).BeginInit();
            panel2.SuspendLayout();
            SuspendLayout();
            // 
            // toolStrip1
            // 
            toolStrip1.ImageScalingSize = new Size(32, 32);
            toolStrip1.Items.AddRange(new ToolStripItem[] { ToolStart, ToolStop, ToolClose });
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
            ToolStart.Click += ToolStart_Click;
            // 
            // ToolStop
            // 
            ToolStop.DisplayStyle = ToolStripItemDisplayStyle.Image;
            ToolStop.Image = (Image)resources.GetObject("ToolStop.Image");
            ToolStop.ImageTransparentColor = Color.Magenta;
            ToolStop.Name = "ToolStop";
            ToolStop.Size = new Size(46, 36);
            ToolStop.Text = "Stop bot";
            ToolStop.Visible = false;
            ToolStop.Click += ToolStop_Click;
            // 
            // ToolClose
            // 
            ToolClose.DisplayStyle = ToolStripItemDisplayStyle.Image;
            ToolClose.Image = (Image)resources.GetObject("ToolClose.Image");
            ToolClose.ImageTransparentColor = Color.Magenta;
            ToolClose.Name = "ToolClose";
            ToolClose.Size = new Size(46, 36);
            ToolClose.Text = "Close";
            ToolClose.ToolTipText = "Close All Positions";
            ToolClose.Click += ToolClose_Click;
            // 
            // SplMain
            // 
            SplMain.Dock = DockStyle.Fill;
            SplMain.Location = new Point(0, 42);
            SplMain.Name = "SplMain";
            // 
            // SplMain.Panel1
            // 
            SplMain.Panel1.Controls.Add(splitContainer2);
            // 
            // SplMain.Panel2
            // 
            SplMain.Panel2.Controls.Add(splitContainer1);
            SplMain.Size = new Size(2477, 890);
            SplMain.SplitterDistance = 664;
            SplMain.TabIndex = 1;
            SplMain.Visible = false;
            // 
            // splitContainer2
            // 
            splitContainer2.Dock = DockStyle.Fill;
            splitContainer2.Location = new Point(0, 0);
            splitContainer2.Name = "splitContainer2";
            splitContainer2.Orientation = Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.Controls.Add(GridBalances);
            splitContainer2.Panel1.Controls.Add(PanBalances);
            splitContainer2.Size = new Size(664, 890);
            splitContainer2.SplitterDistance = 310;
            splitContainer2.TabIndex = 0;
            // 
            // GridBalances
            // 
            GridBalances.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            GridBalances.Dock = DockStyle.Fill;
            GridBalances.Location = new Point(0, 51);
            GridBalances.Name = "GridBalances";
            GridBalances.RowHeadersWidth = 82;
            GridBalances.Size = new Size(664, 259);
            GridBalances.TabIndex = 1;
            // 
            // PanBalances
            // 
            PanBalances.BackColor = Color.Gold;
            PanBalances.BorderStyle = BorderStyle.FixedSingle;
            PanBalances.Controls.Add(LblBalances);
            PanBalances.Controls.Add(LblTotalBalance);
            PanBalances.Dock = DockStyle.Top;
            PanBalances.Location = new Point(0, 0);
            PanBalances.Name = "PanBalances";
            PanBalances.Size = new Size(664, 51);
            PanBalances.TabIndex = 2;
            // 
            // LblBalances
            // 
            LblBalances.BackColor = Color.Gold;
            LblBalances.Dock = DockStyle.Fill;
            LblBalances.Font = new Font("Segoe UI Black", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            LblBalances.Location = new Point(0, 0);
            LblBalances.Name = "LblBalances";
            LblBalances.Size = new Size(499, 49);
            LblBalances.TabIndex = 0;
            LblBalances.Text = "BALANCES";
            LblBalances.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // LblTotalBalance
            // 
            LblTotalBalance.BackColor = Color.Gold;
            LblTotalBalance.BorderStyle = BorderStyle.FixedSingle;
            LblTotalBalance.Dock = DockStyle.Right;
            LblTotalBalance.Font = new Font("Segoe UI Black", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            LblTotalBalance.Location = new Point(499, 0);
            LblTotalBalance.Name = "LblTotalBalance";
            LblTotalBalance.Size = new Size(163, 49);
            LblTotalBalance.TabIndex = 1;
            LblTotalBalance.Text = "0,00";
            LblTotalBalance.TextAlign = ContentAlignment.MiddleRight;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(GridPositions);
            splitContainer1.Panel1.Controls.Add(panel1);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(GridChances);
            splitContainer1.Panel2.Controls.Add(panel2);
            splitContainer1.Size = new Size(1809, 890);
            splitContainer1.SplitterDistance = 311;
            splitContainer1.TabIndex = 0;
            // 
            // GridPositions
            // 
            GridPositions.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            GridPositions.Dock = DockStyle.Fill;
            GridPositions.Location = new Point(0, 51);
            GridPositions.Name = "GridPositions";
            GridPositions.RowHeadersWidth = 82;
            GridPositions.Size = new Size(1809, 260);
            GridPositions.TabIndex = 4;
            GridPositions.CellFormatting += GridPositions_CellFormatting;
            // 
            // panel1
            // 
            panel1.BackColor = Color.Gold;
            panel1.BorderStyle = BorderStyle.FixedSingle;
            panel1.Controls.Add(label1);
            panel1.Controls.Add(LblPnlPositions);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(1809, 51);
            panel1.TabIndex = 3;
            // 
            // label1
            // 
            label1.BackColor = Color.Gold;
            label1.Dock = DockStyle.Fill;
            label1.Font = new Font("Segoe UI Black", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(0, 0);
            label1.Name = "label1";
            label1.Size = new Size(1644, 49);
            label1.TabIndex = 0;
            label1.Text = "POSITIONS";
            label1.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // LblPnlPositions
            // 
            LblPnlPositions.BackColor = Color.Gold;
            LblPnlPositions.BorderStyle = BorderStyle.FixedSingle;
            LblPnlPositions.Dock = DockStyle.Right;
            LblPnlPositions.Font = new Font("Segoe UI Black", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            LblPnlPositions.Location = new Point(1644, 0);
            LblPnlPositions.Name = "LblPnlPositions";
            LblPnlPositions.Size = new Size(163, 49);
            LblPnlPositions.TabIndex = 1;
            LblPnlPositions.Text = "0,00";
            LblPnlPositions.TextAlign = ContentAlignment.MiddleRight;
            // 
            // GridChances
            // 
            GridChances.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            GridChances.Dock = DockStyle.Fill;
            GridChances.Location = new Point(0, 51);
            GridChances.Name = "GridChances";
            GridChances.RowHeadersWidth = 82;
            GridChances.Size = new Size(1809, 524);
            GridChances.TabIndex = 5;
            GridChances.CellFormatting += GridChances_CellFormatting;
            // 
            // panel2
            // 
            panel2.BackColor = Color.Gold;
            panel2.BorderStyle = BorderStyle.FixedSingle;
            panel2.Controls.Add(label2);
            panel2.Controls.Add(LblPnlChances);
            panel2.Dock = DockStyle.Top;
            panel2.Location = new Point(0, 0);
            panel2.Name = "panel2";
            panel2.Size = new Size(1809, 51);
            panel2.TabIndex = 4;
            // 
            // label2
            // 
            label2.BackColor = Color.Gold;
            label2.Dock = DockStyle.Fill;
            label2.Font = new Font("Segoe UI Black", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label2.Location = new Point(0, 0);
            label2.Name = "label2";
            label2.Size = new Size(1644, 49);
            label2.TabIndex = 0;
            label2.Text = "CHANCES";
            label2.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // LblPnlChances
            // 
            LblPnlChances.BackColor = Color.Gold;
            LblPnlChances.BorderStyle = BorderStyle.FixedSingle;
            LblPnlChances.Dock = DockStyle.Right;
            LblPnlChances.Font = new Font("Segoe UI Black", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            LblPnlChances.Location = new Point(1644, 0);
            LblPnlChances.Name = "LblPnlChances";
            LblPnlChances.Size = new Size(163, 49);
            LblPnlChances.TabIndex = 1;
            LblPnlChances.Text = "0,00";
            LblPnlChances.TextAlign = ContentAlignment.MiddleRight;
            // 
            // BackWorkerMain
            // 
            BackWorkerMain.WorkerReportsProgress = true;
            BackWorkerMain.DoWork += BackWorkerMain_DoWork;
            BackWorkerMain.ProgressChanged += BackWorkerMain_ProgressChanged;
            BackWorkerMain.RunWorkerCompleted += BackWorkerMain_RunWorkerCompleted;
            // 
            // FrmFundingRate
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(2477, 932);
            Controls.Add(SplMain);
            Controls.Add(toolStrip1);
            Name = "FrmFundingRate";
            Text = "Funding Rate Arbitrage";
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            SplMain.Panel1.ResumeLayout(false);
            SplMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)SplMain).EndInit();
            SplMain.ResumeLayout(false);
            splitContainer2.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
            splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)GridBalances).EndInit();
            PanBalances.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)GridPositions).EndInit();
            panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)GridChances).EndInit();
            panel2.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ToolStrip toolStrip1;
        private ToolStripButton ToolStart;
        private ToolStripButton ToolStop;
        private SplitContainer SplMain;
        private System.ComponentModel.BackgroundWorker BackWorkerMain;
        private SplitContainer splitContainer2;
        private Label LblBalances;
        private DataGridView GridBalances;
        private Panel PanBalances;
        private Label LblTotalBalance;
        private Panel panel1;
        private Label label1;
        private Label LblPnlPositions;
        private DataGridView GridPositions;
        private SplitContainer splitContainer1;
        private Panel panel2;
        private Label label2;
        private Label LblPnlChances;
        private DataGridView GridChances;
        private ToolStripButton ToolClose;
    }
}