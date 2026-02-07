using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Hyperliquidity.Data
{
    internal class HyperBalance : IBalance
    {
        public HyperBalance(IFuturesExchange oExchange, string sCurrency, decimal nAvaliable, decimal nLocked)
        {
            Exchange = oExchange;
            Currency = sCurrency;
            Balance = nAvaliable + nLocked;
            Locked = nLocked;
            Avaliable = nAvaliable;
        }
        public IFuturesExchange Exchange { get; }

        public string Currency { get; }

        public decimal Balance { get; }

        public decimal Locked { get; }

        public decimal Avaliable { get; }

        public WsMessageType MessageType { get => WsMessageType.Balance; }

        public void Update(IWebsocketMessageBase oMessage)
        {
            throw new NotImplementedException();
        }
    }
}
