using Crypto.Futures.Bot.Interface;
using Crypto.Futures.Exchanges;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot.Model.CryptoTrading
{

    /// <summary>
    /// Dummy balance
    /// </summary>
    internal class DummyBalance : IBalance
    {

        public DummyBalance(IFuturesExchange oExchange, IExchangeSetup oSetup)
        {
            Currency = "USDT";
            Balance = oSetup.MoneyDefinition.Money * 4;
            Exchange = oExchange;
        }

        public IFuturesExchange Exchange { get; }

        public decimal Balance { get; set; } = 0;

        public decimal Locked { get; set; } = 0;

        public decimal Avaliable { get => Balance - Locked;}

        public WsMessageType MessageType { get => WsMessageType.Balance; }

        public string Currency { get; }

        public void Update(IWebsocketMessageBase oMessage)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Paper trader
    /// </summary>
    internal class CryptoPaperTrader : ICryptoTrader
    {
        private DummyBalance[] m_aBalances = Array.Empty<DummyBalance>();
        private IFuturesExchange[] m_aExchanges;
        private IExchangeSetup m_oSetup;
        public CryptoPaperTrader(ICryptoBot oBot)
        {
            // Bot = oBot;
            Logger = oBot.Logger;
            Money = oBot.Setup.MoneyDefinition.Money;
            Leverage = oBot.Setup.MoneyDefinition.Leverage;
            m_aExchanges = oBot.Exchanges;
            m_oSetup = oBot.Setup;
            InitBalances();
        }

        public CryptoPaperTrader(ICommonLogger oLogger, IExchangeSetup oSetup, IFuturesExchange[] aExchanges)
        {
            Logger = oLogger;
            Money = oSetup.MoneyDefinition.Money;
            Leverage = oSetup.MoneyDefinition.Leverage;
            m_aExchanges = aExchanges;
            m_oSetup = oSetup;
            InitBalances();

        }

        public decimal Money { get; }

        public decimal Leverage { get; }

        public int OrderTimeout { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        // public ICryptoBot Bot { get; }
        public ICommonLogger Logger { get; }

        public IBalance[] Balances { get => m_aBalances; }

        public ICryptoPosition[] PositionsActive => throw new NotImplementedException();

        public ICryptoPosition[] PositionsClosed => throw new NotImplementedException();

        /// <summary>
        /// Close position
        /// </summary>
        /// <param name="oPosition"></param>
        /// <param name="OncheckCancel"></param>
        /// <param name="nPrice"></param>
        /// <returns></returns>
        public async Task<bool> Close(ICryptoPosition oPosition, ICryptoTrader.MustCancelOrderDelegate? OncheckCancel = null, decimal? nPrice = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Close position fill or kill
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
        /// Initialize balances
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void InitBalances()
        {
            List<DummyBalance> aBalances = new List<DummyBalance>();
            foreach (var oExchange in m_aExchanges)
            {
                aBalances.Add(new DummyBalance(oExchange, m_oSetup));
            }
            m_aBalances = aBalances.ToArray();
        }

        /// <summary>
        /// Open position
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <param name="bLong"></param>
        /// <param name="nVolume"></param>
        /// <param name="OncheckCancel"></param>
        /// <param name="nPrice"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<ICryptoPosition?> Open(IFuturesSymbol oSymbol, bool bLong, decimal nVolume, ICryptoTrader.MustCancelOrderDelegate? OncheckCancel = null, decimal? nPrice = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Open position fill or kill
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <param name="bLong"></param>
        /// <param name="nVolume"></param>
        /// <param name="nPrice"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<ICryptoPosition?> OpenFillOrKill(IFuturesSymbol oSymbol, bool bLong, decimal nVolume, decimal? nPrice = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Set leverage for symbol
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> PutLeverage(IFuturesSymbol oSymbol)
        {
            throw new NotImplementedException();
        }
    }
}
