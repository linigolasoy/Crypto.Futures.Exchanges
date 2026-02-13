using Crypto.Futures.Bot.Interface.FundingRates;
using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.FrontEnd.FundingRates
{
    internal class DataBalance
    {
        private IBalance m_oBalance;
        public DataBalance( IBalance oBalance )
        {

            m_oBalance = oBalance;

        }

        public string Exchange { get => m_oBalance.Exchange.ExchangeType.ToString(); }

        public decimal Balance { get => Math.Round( m_oBalance.Balance, 2); }
        public decimal Available { get => Math.Round( m_oBalance.Avaliable, 2); }

        public decimal Locked { get => Math.Round(m_oBalance.Locked, 2); }

        public static void UpdateGrids(IFundingRateBot oBot, DataGridView oGrid, Label oLblTotal)
        {
            try
            {
                List<DataBalance>? aBalances = new List<DataBalance>();
                decimal nTotalBalance = 0;
                if (oGrid.DataSource != null)
                {
                    aBalances = (List<DataBalance>)oGrid.DataSource;
                }

                IBalance[] aBotBalances = oBot.Trader!.AccountWatcher.GetBalances();
                bool bAdded = false;
                foreach (var oBalance in aBotBalances)
                {
                    nTotalBalance += oBalance.Balance;
                    if (aBalances.Any(p => p.Exchange == oBalance.Exchange.ExchangeType.ToString())) continue;
                    bAdded = true;
                    aBalances.Add(new DataBalance(oBalance));
                }
                if (oGrid.DataSource == null || bAdded)
                {
                    oGrid.DataSource = aBalances;
                    FormatGrid(oGrid);
                }
                oGrid.Refresh();
                oLblTotal.Text = $"{Math.Round(nTotalBalance, 2)}";

            }
            catch (Exception ex)
            {
                oBot.Logger.Error($"Error putting chances: {ex.Message}");
            }
        }

        private static void FormatGrid(DataGridView oGrid)
        {
            if( oGrid.Columns.Count < 4) return;
            oGrid.Columns[0].HeaderText = "Exchange";
            oGrid.Columns[0].DefaultCellStyle.BackColor = System.Drawing.Color.LightGray;
            oGrid.Columns[1].HeaderText = "Balance";
            oGrid.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            oGrid.Columns[2].HeaderText = "Available";
            oGrid.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            oGrid.Columns[3].HeaderText = "Locked";
            oGrid.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }

    }
}
