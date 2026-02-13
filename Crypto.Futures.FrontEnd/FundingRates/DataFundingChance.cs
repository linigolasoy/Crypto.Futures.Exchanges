using Crypto.Futures.Bot.Interface.FundingRates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.FrontEnd.FundingRates
{
    internal class DataFundingChance
    {
        private IFundingRateChance m_oChance;
        public DataFundingChance(IFundingRateChance oChance) { m_oChance = oChance; }
        // public IFundingRateChance Chance { get => m_oChance; }

        public int Id { get => m_oChance.Id; }  
        public bool Active { get => m_oChance.IsActive; }
        public string Currency { get => m_oChance.Currency; }

        public string Exchanges { get => $"{m_oChance.SymbolLong.Symbol.Exchange.ExchangeType} / {m_oChance.SymbolShort.Symbol.Exchange.ExchangeType}"; }
        public DateTime OpenTime { get => m_oChance.ChanceOpenDate; }
        public DateTime Next { get => m_oChance.ChanceNextFundingDate; }
        public decimal Percent { get => m_oChance.PercentDifference; }
        public decimal Pnl { get => m_oChance.Pnl; }

        public static void UpdateGrids(IFundingRateBot oBot, DataGridView oGrid, Label oLblTotal)
        {
            try
            {

                List<DataFundingChance>? aChances = new List<DataFundingChance>();
                decimal nTotalChance = 0;
                if (oGrid.DataSource != null)
                {
                    aChances = (List<DataFundingChance>)oGrid.DataSource;
                }
                IFundingRateChance[] aBotChances = oBot.Chances;
                bool bAdded = false;
                foreach (var oChance in aBotChances)
                {
                    DataFundingChance? oFound = aChances.FirstOrDefault(p => p.Id == oChance.Id);
                    if (oFound == null)
                    {
                        oFound = new DataFundingChance(oChance);
                        aChances.Add(oFound);
                        bAdded = true;
                    }
                    nTotalChance += oFound.Pnl;

                }

                // Application.DoEvents();
                if ( ( oGrid.DataSource == null || bAdded ) && aChances.Count > 0 )
                {
                    oGrid.DataSource = null;
                    oGrid.DataSource = aChances;
                    FormatGrid(oGrid);
                }
                oGrid.Refresh();
                oLblTotal.Text = $"{Math.Round(nTotalChance, 2)}";
            }
            catch (Exception ex)
            {
                oBot.Logger.Error($"Error putting chances: {ex.Message}");
            }
        }

        private static void FormatGrid(DataGridView oGrid) 
        {
            if( oGrid.Columns.Count < 7) return;    
            oGrid.Columns[0].Visible = false;

            oGrid.Columns[5].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            oGrid.Columns[6].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }
    }
}
