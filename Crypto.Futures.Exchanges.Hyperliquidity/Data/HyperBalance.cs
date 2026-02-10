using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using HyperLiquid.Net.Objects.Models;
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

        public HyperBalance(IFuturesExchange oExchange, HyperLiquidMarginSummary oSummary)
        {
            Exchange = oExchange;
            Currency = "USDT";
            Balance = oSummary.AccountValue;
            Locked = oSummary.TotalMarginUsed;
            Avaliable = oSummary.AccountValue - Locked;
        }
        public IFuturesExchange Exchange { get; }

        public string Currency { get; }

        public decimal Balance { get; private set; }

        public decimal Locked { get; private set; }

        public decimal Avaliable { get; private set; }

        public WsMessageType MessageType { get => WsMessageType.Balance; }

        public void Update(IWebsocketMessageBase oMessage)
        {
            if( !(oMessage is IBalance)) return;
            Balance = ((IBalance)oMessage).Balance;
            Locked = ((IBalance)oMessage).Locked;
            Avaliable = ((IBalance)oMessage).Avaliable;
        }
    }
}
