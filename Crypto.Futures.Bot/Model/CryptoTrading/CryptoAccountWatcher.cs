using Crypto.Futures.Bot.Interface;
using Crypto.Futures.Exchanges;
using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot.Model.CryptoTrading
{
    internal class CryptoAccountWatcher : IAccountWatcher
    {

        /// <summary>
        /// Events
        /// </summary>
        /// <param name="aExchanges"></param>
        public CryptoAccountWatcher(IFuturesExchange[] aExchanges)
        {
            Exchanges = aExchanges;
        }
        public IFuturesExchange[] Exchanges { get; }

        public event IAccountWatcher.OnBalanceChangeDelegate? OnBalanceChange;
        public event IAccountWatcher.OnPositionChangeDelegate? OnPositionChange;
        public event IAccountWatcher.OnOrderChangeDelegate? OnOrderChange;


        /// <summary>
        /// Account balances retrieval.
        /// </summary>
        /// <returns></returns>
        public IBalance[] GetBalances()
        {
            List<IBalance> aResult = new List<IBalance>();
            foreach (var oExchange in Exchanges)
            {
                aResult.AddRange(oExchange.Account.WebsocketPrivate.Balances);
            }
            return aResult.ToArray();

        }

        public IOrder[] GetOrders()
        {
            throw new NotImplementedException();
        }

        public ICryptoPosition[] GetPositions()
        {
            throw new NotImplementedException();
        }

        public async Task Start()
        {
            foreach (var oExchange in Exchanges)
            {
                // Subscribe to account updates via websocket or polling
                bool bStarted = await oExchange.Account.WebsocketPrivate.Start();
                if( !bStarted)
                {
                    throw new Exception($"Failed to start account watcher for exchange {oExchange.ExchangeType}");
                }
            }
        }

        public async Task Stop()
        {
            foreach (var oExchange in Exchanges)
            {
                await oExchange.Account.WebsocketPrivate.Stop();
            }
        }
    }
}
