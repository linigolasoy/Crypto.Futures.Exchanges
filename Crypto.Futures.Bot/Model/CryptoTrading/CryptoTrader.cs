using Crypto.Futures.Bot.Interface;
using Crypto.Futures.Exchanges;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Crypto.Futures.Bot.Interface.ICryptoTrader;

namespace Crypto.Futures.Bot.Model.CryptoTrading
{

    /*

    /// <summary>
    /// Crypto trader
    /// </summary>
    internal class CryptoTrader : ICryptoTrader
    {
        private class FakePosition : IPosition
        {
            public FakePosition( ICryptoPosition oPosition )
            {
                Id = oPosition.OrderOpen.OrderId;
                Symbol = oPosition.Symbol;
                CreatedAt = oPosition.OrderOpen.CreatedAt;
                IsLong = oPosition.IsLong;
                Quantity = oPosition.OrderOpen.Quantity;
            }
            public string Id { get; }

            public IFuturesSymbol Symbol { get; }

            public DateTime CreatedAt { get; }

            public DateTime UpdatedAt => throw new NotImplementedException();

            public bool IsLong { get; }

            public bool IsOpen { get => true; }

            public decimal AveragePriceOpen { get; }

            public decimal? PriceClose { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public decimal Quantity { get; }

            public decimal Profit => throw new NotImplementedException();

            public WsMessageType MessageType => throw new NotImplementedException();

            public void Update(IWebsocketMessageBase oMessage)
            {
                throw new NotImplementedException();
            }
        }


        private ConcurrentDictionary<ExchangeType, IBalance> m_aBalances = new ConcurrentDictionary<ExchangeType, IBalance>();  
        private ConcurrentDictionary<int, ICryptoPosition> m_aPositionActive = new ConcurrentDictionary<int, ICryptoPosition>();
        private ConcurrentDictionary<int, ICryptoPosition> m_aPositionClosed = new ConcurrentDictionary<int, ICryptoPosition>();
        public CryptoTrader( ICryptoBot oBot) 
        { 
            Logger = oBot.Logger;
            Money = oBot.Setup.MoneyDefinition.Money;
            Leverage = oBot.Setup.MoneyDefinition.Leverage;
            Exchanges = oBot.Exchanges;
        }
        public decimal Money { get; }

        public decimal Leverage { get; }

        public int OrderTimeout { get; set; } = 60;

        // public ICryptoBot Bot { get; }
        public ICommonLogger Logger { get; }

        private IFuturesExchange[] Exchanges { get; }

        public IBalance[] Balances { get => m_aBalances.Values.ToArray(); }

        public ICryptoPosition[] PositionsActive { get => m_aPositionActive.Values.ToArray(); }

        public ICryptoPosition[] PositionsClosed { get => m_aPositionClosed.Values.ToArray(); }


        /// <summary>
        /// Close position on fill or kill mode
        /// </summary>
        /// <param name="oPosition"></param>
        /// <param name="nPrice"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> CloseFillOrKill(ICryptoPosition oPosition, decimal? nPrice = null)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Open order in fill or kill mode
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <param name="bLong"></param>
        /// <param name="nVolume"></param>
        /// <param name="nPrice"></param>
        /// <returns></returns>
        public async Task<ICryptoPosition?> OpenFillOrKill(IFuturesSymbol oSymbol, bool bLong, decimal nVolume, decimal? nPrice = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Close
        /// </summary>
        /// <param name="oPosition"></param>
        /// <param name="nPrice"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> Close(ICryptoPosition oPosition, MustCancelOrderDelegate? OncheckCancel = null, decimal? nPrice = null)
        {
            throw new NotImplementedException();
        }



        /// <summary>
        /// Update balances
        /// </summary>
        public void InitBalances()
        {
            if (m_aBalances.Count == Exchanges.Length) return;
            try
            {
                foreach( var oExchange in Exchanges )
                {
                    IBalance? oBalance = null;  
                    if( !m_aBalances.TryGetValue(oExchange.ExchangeType, out oBalance) )
                    {
                        IBalance[] aFound = oExchange.Account.WebsocketPrivate.Balances;
                        if (aFound == null || aFound.Length <= 0) continue;
                        IBalance? oFound = aFound.FirstOrDefault(p => p.Currency == "USDT");
                        if (oFound == null) continue;
                        m_aBalances.TryAdd(oExchange.ExchangeType, oFound);
                    }
                }
            }
            catch( Exception ex )
            {
                Logger.Error("Error updating balances", ex);
            }
        }


        /// <summary>
        /// Open position
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <param name="bLong"></param>
        /// <param name="nVolume"></param>
        /// <param name="nPrice"></param>
        /// <returns></returns>
        public async Task<ICryptoPosition?> Open(IFuturesSymbol oSymbol, bool bLong, decimal nVolume, MustCancelOrderDelegate? OncheckCancel = null, decimal? nPrice = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Put leverate
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <returns></returns>
        public async Task<bool> PutLeverage(IFuturesSymbol oSymbol)
        {
            try
            {
                bool bResult = await oSymbol.Exchange.Account.SetLeverage(oSymbol, Leverage);
                if( !bResult )
                {
                    Logger.Error($"Could not set leverage on {oSymbol.ToString()} Unknown error");
                }
                return bResult;
            }
            catch (Exception ex)
            {
                Logger.Error($"Could not set leverage on {oSymbol.ToString()}", ex);
            }
            return false;
        }
    }
    */
}
