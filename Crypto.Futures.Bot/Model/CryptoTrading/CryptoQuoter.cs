using Crypto.Futures.Bot.Interface;
using Crypto.Futures.Exchanges;
using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot.Model.CryptoTrading
{
    internal class CryptoQuoter : IQuoter
    {
        public IFuturesExchange[] Exchanges { get; }

        private ConcurrentDictionary<string, IFuturesSymbol> m_aSubscribed = new ConcurrentDictionary<string, IFuturesSymbol>();    

        public CryptoQuoter(IFuturesExchange[] aExchanges)
        {
            Exchanges = aExchanges;
        }


        /// <summary>
        /// Get the real decimals for a symbol based on contract size
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <returns></returns>
        private int GetRealDecimals( IFuturesSymbol oSymbol )
        {
            if(oSymbol.ContractSize == 1)
            {
                return oSymbol.QuantityDecimals;
            }
            else
            {
                decimal nDecimals = -(decimal)Math.Log10((double)oSymbol.ContractSize);
                int nResult = (int)(nDecimals + oSymbol.QuantityDecimals);
                if ( oSymbol.ContractSize >= 10)
                {
                    return nResult;
                }
                return nResult;
            }
        }


        /// <summary>
        /// Get the best quantity for a set of symbols at a given price and money amount
        /// </summary>
        /// <param name="aSymbols"></param>
        /// <param name="nPrice"></param>
        /// <param name="nMoney"></param>
        /// <returns></returns>
        public decimal? GetBestQuantity(IFuturesSymbol[] aSymbols, decimal nPrice, decimal nMoney)
        {
            int nMinDecimals = 1000;
            foreach (var oSymbol in aSymbols)
            {
                int nActDecimals = GetRealDecimals(oSymbol);
                if (nActDecimals < nMinDecimals)
                {
                    nMinDecimals = nActDecimals;
                }
            }
            decimal nQuantity = nMoney / nPrice;
            nQuantity = Math.Floor(nQuantity * (decimal)Math.Pow(10, (double)nMinDecimals)) / (decimal)Math.Pow(10, (double)nMinDecimals);
            return nQuantity;
        }


        /// <summary>
        /// Get subscription key for symbol
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <returns></returns>
        private string GetSubscriptionKey(IFuturesSymbol oSymbol)
        {
            return $"{oSymbol.Exchange.ExchangeType}-{oSymbol.Symbol}";
        }

        /// <summary>
        /// Check if symbol needs subscription
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <returns></returns>
        private bool NeedsSubscription(IFuturesSymbol oSymbol)
        {
            string strKey = GetSubscriptionKey(oSymbol);    
            return !m_aSubscribed.ContainsKey(strKey);
        }

        /// <summary>
        /// Subscribe to symbol price updates
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <returns></returns>
        private async Task<bool> SubscribeSymbol(IFuturesSymbol oSymbol)
        {
            if( !oSymbol.Exchange.Market.Websocket.Started )
            {
                bool bStarted = await oSymbol.Exchange.Market.Websocket.Start();
                if (!bStarted) return false;
                await Task.Delay(500);
            }

            var oSub = await oSymbol.Exchange.Market.Websocket.Subscribe(oSymbol, Futures.Exchanges.WebsocketModel.WsMessageType.OrderbookPrice);
            if( oSub != null )
            {
                string strKey = GetSubscriptionKey(oSymbol);
                m_aSubscribed[strKey] = oSymbol;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get orderbook price 
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <returns></returns>
        private async Task<IOrderbookPrice?> GetOrderbookPrice(IFuturesSymbol oSymbol)
        {
            if (NeedsSubscription(oSymbol))
            {
                bool bSubscribed = await SubscribeSymbol(oSymbol);
                if (!bSubscribed) return null;
                await Task.Delay(2500);
            }

            var oData = oSymbol.Exchange.Market.Websocket.DataManager.GetData(oSymbol);
            if (oData == null) return null;
            return oData.LastOrderbookPrice;
        }

        /// <summary>
        /// Get long price
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <param name="nQuantity"></param>
        /// <returns></returns>
        public async Task<decimal?> GetLongPrice(IFuturesSymbol oSymbol, decimal nQuantity = 0)
        {
            var oOrderPrice = await GetOrderbookPrice(oSymbol);
            if (oOrderPrice == null) return null;
            return oOrderPrice.AskPrice;
        }

        /// <summary>
        /// Get short price
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <param name="nQuantity"></param>
        /// <returns></returns>
        public async Task<decimal?> GetShortPrice(IFuturesSymbol oSymbol, decimal nQuantity = 0)
        {
            var oOrderPrice = await GetOrderbookPrice(oSymbol);
            if (oOrderPrice == null) return null;
            return oOrderPrice.BidPrice;
        }
    }
}
