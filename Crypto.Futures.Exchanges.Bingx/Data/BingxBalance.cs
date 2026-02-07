using BingX.Net.Objects.Models;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bingx.Data
{
    internal class BingxBalance : IBalance
    {
        internal BingxBalance( IFuturesExchange oExchange, BingXFuturesBalance oJson) 
        { 
            Exchange = oExchange;
            Currency = oJson.Asset;

            Avaliable = (oJson.AvailableMargin == null ? 0: oJson.AvailableMargin.Value);
            Locked = (oJson.FrozenMargin == null ? 0 : oJson.FrozenMargin.Value);
            Balance = (oJson.Equity == null ? 0 : oJson.Equity.Value);
        }

        internal BingxBalance(IFuturesExchange oExchange, BingXFuturesBalanceChange oChange)
        {
            Exchange = oExchange;
            Currency = oChange.Asset;
            Avaliable = oChange.BalanceExIsolatedMargin;
            Balance = oChange.Balance;
            Locked = Balance - Avaliable;   
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
            var oBalance = (IBalance)oMessage;
            Balance = oBalance.Balance;
            Avaliable = oBalance.Avaliable;
            Locked = oBalance.Locked;

        }
    }
}

