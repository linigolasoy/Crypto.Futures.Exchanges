using Crypto.Futures.Bot;
using Crypto.Futures.Bot.Interface.FundingRates;
using Crypto.Futures.Exchanges.Factory;
using Crypto.Futures.Exchanges;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.FrontEnd.FundingRates;

namespace Crypto.Futures.FrontEnd
{

    /// <summary>
    /// Funding Rate Form
    /// </summary>
    public partial class FrmFundingRate : Form
    {

        private IFundingRateBot? m_oBot = null;
        private Task? m_oMainTask = null;
        private CancellationTokenSource m_oTokenSource = new CancellationTokenSource();
        private bool m_bProgressing = false;


        public FrmFundingRate()
        {
            InitializeComponent();
        }



        private void PutLogs(IFundingRateBot oBot)
        {
        }

        private void BackWorkerMain_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (m_bProgressing) return;
            m_bProgressing = true;
            if (m_oBot != null)
            {
                DataBalance.UpdateGrids(m_oBot, GridBalances, LblTotalBalance);
                DataPosition.UpdateGrids(m_oBot, GridPositions, LblPnlPositions);
                DataFundingChance.UpdateGrids(m_oBot, GridChances, LblPnlChances);
                PutLogs(m_oBot);
            }
            m_bProgressing = false;
        }

        private void BackWorkerMain_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }
        private void InitGrids()
        {
            GridBalances.DataSource = null;
        }
        private void ToolStart_Click(object sender, EventArgs e)
        {
            ToolStart.Visible = false; ToolStop.Visible = true;
            SplMain.Visible = true;
            InitGrids();
            BackWorkerMain.RunWorkerAsync();
        }

        private void ToolStop_Click(object sender, EventArgs e)
        {
            if (m_oMainTask == null) return;
            m_oTokenSource.Cancel();
            m_oMainTask.Wait();
            // await m_oMainTask;
            ToolStart.Visible = true; ToolStop.Visible = false;
        }


        private async Task MainLoopTask()
        {
            m_oTokenSource = new CancellationTokenSource();
            IExchangeSetup oSetup = ExchangeFactory.CreateSetup(Program.SETUP_FILE);
            ICommonLogger oLogger = ExchangeFactory.CreateLogger(oSetup, "FundingRateBotForm");
            try
            {
                m_oBot = BotFactory.CreateFundingRateBot(oSetup, oLogger, false);

                await m_oBot.Start();
                while (!m_oTokenSource.Token.IsCancellationRequested)
                {
                    BackWorkerMain.ReportProgress(10);
                    await Task.Delay(1000);
                }
                await m_oBot.Stop();
                m_oBot = null;
            }
            catch (Exception ex)
            {
                oLogger.Error("Failed to create bot: " + ex.Message);
                await Task.Delay(2000);

            }



        }
        private void BackWorkerMain_DoWork(object sender, DoWorkEventArgs e)
        {
            m_oMainTask = MainLoopTask();
            m_oMainTask.Wait();

        }

        private void ToolClose_Click(object sender, EventArgs e)
        {
            if (m_oBot == null) return;
            if( m_oBot.MarkToClose)
            {
                MessageBox.Show("Bot is already marked to close, please wait...");
            }
            if (MessageBox.Show("Are you sure you want to mark the bot to close? It will close all positions and chances and stop the bot.", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                m_oBot.MarkToClose = true;
            }
        }
    }
}
