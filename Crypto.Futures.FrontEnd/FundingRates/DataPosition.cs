using Crypto.Futures.Bot.Interface.FundingRates;
using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.FrontEnd.FundingRates
{
    internal class DataPosition
    {

        private IPosition m_oPosition;
        public DataPosition(IPosition oPosition)
        {
            m_oPosition = oPosition;
        }

        // public bool UpdatedHistory { get; set; } = false;
        public IPosition Position { get => m_oPosition; }
        public string Id { get => m_oPosition.Id; }
        public bool Closed { get => !m_oPosition.IsOpen; }
        public string Exchange { get => m_oPosition.Symbol.Exchange.ExchangeType.ToString(); }
        public string Symbol { get => m_oPosition.Symbol.Symbol; }
        public string Direction { get => m_oPosition.IsLong ? "Long" : "Short"; }
        public decimal Amount { get => m_oPosition.Quantity; }
        public decimal EntryPrice { get => m_oPosition.AveragePriceOpen; }
        public decimal CurrentPrice { get; internal set; } = 0;
        public decimal PnL { get; internal set; } = 0;

        public static void UpdateGrids(IFundingRateBot oBot, DataGridView oGrid, Label oLblTotal)
        {
            try
            {
                List<DataPosition>? aPositions = new List<DataPosition>();
                decimal nTotalPosition = 0;
                bool bAdded = false;
                if (oGrid.DataSource != null)
                {
                    aPositions = (List<DataPosition>)oGrid.DataSource;
                }
                IPosition[] aBotPositions = oBot.Trader!.AccountWatcher.GetPositions();
                foreach (var oPosition in aBotPositions)
                {
                    DataPosition? oFound = aPositions.FirstOrDefault(p =>
                            p.Exchange == oPosition.Symbol.Exchange.ExchangeType.ToString() &&
                            p.Id == oPosition.Id
                        );
                    if (oFound == null)
                    {
                        oFound = new DataPosition(oPosition);
                        aPositions.Add(oFound);
                        bAdded = true;
                    }
                    nTotalPosition += CalculatePnl(oBot, oFound);

                }

                if (oGrid.DataSource == null || bAdded)
                {
                    oGrid.DataSource = null;
                    oGrid.DataSource = aPositions;
                    FormatGrid(oGrid);
                }
                oGrid.Refresh();
                oLblTotal.Text = $"{Math.Round(nTotalPosition, 2)}";
            }
            catch (Exception ex)
            {
                oBot.Logger.Error("DataPosition: Error updating positions grid", ex);
            }

        }

        private static decimal CalculatePnl(IFundingRateBot oBot, DataPosition oPosition)
        {
            if( oPosition.Closed ) return oPosition.PnL; // already closed, no need to calculate
            var oData = oPosition.Position.Symbol.Exchange.Market.Websocket.DataManager.GetData(oPosition.Position.Symbol);
            if (oData == null) return 0;
            IOrderbookPrice? oPrice = oData.LastOrderbookPrice;
            if (oPrice == null) return 0;
            decimal nPrice = (oPosition.Position.IsLong ? oPrice.AskPrice : oPrice.BidPrice);
            oPosition.CurrentPrice = nPrice;
            decimal nPnl = (nPrice - oPosition.EntryPrice) * oPosition.Amount;
            if (!oPosition.Position.IsLong) nPnl = -nPnl;
            oPosition.PnL = Math.Round(nPnl, 2);

            return nPnl;
        }

        private static void FormatGrid(DataGridView oGrid)
        {
            oGrid.Columns[0].Visible = false;
            oGrid.Columns[1].Visible = false;

            oGrid.Columns[2].HeaderText = "Closed";
            oGrid.Columns[2].DefaultCellStyle.BackColor = System.Drawing.Color.LightGray;

            oGrid.Columns[3].HeaderText = "Exchange";
            oGrid.Columns[3].DefaultCellStyle.BackColor = System.Drawing.Color.LightGray;

            oGrid.Columns[4].HeaderText = "Symbol";
            oGrid.Columns[4].DefaultCellStyle.BackColor = System.Drawing.Color.LightGray;


            for (int i = 5; i < oGrid.Columns.Count; i++)
            {
                oGrid.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
        }

    }
}
