using BitMart.Net.Objects.Models;
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

namespace Crypto.Futures.Exchanges.Bitmart.Data
{

    internal class BitmartBalance : IBalance
    {
        internal BitmartBalance( IFuturesExchange oExchange, BitMartFuturesBalance oJson) 
        { 
            Exchange = oExchange;
            Currency = oJson.Asset;
            Balance = oJson.Equity;
            Avaliable = oJson.AvailableBalance;
            Locked = oJson.PositionMargin + oJson.FrozenBalance;
        }

        internal BitmartBalance(IFuturesExchange oExchange, BitMartFuturesBalanceUpdate oUpdate )
        {
            Exchange = oExchange;
            Currency = oUpdate.Asset;
            Balance = oUpdate.Available + oUpdate.Frozen + oUpdate.PositionMargin;
            Avaliable = oUpdate.Available;
            Locked = oUpdate.PositionMargin + oUpdate.Frozen;

        }
        public IFuturesExchange Exchange { get; }

        public string Currency { get; }

        public decimal Balance { get; private set; }

        public decimal Locked { get; private set; }

        public decimal Avaliable { get; private set; }

        public WsMessageType MessageType { get => WsMessageType.Balance; }

        public void Update(IWebsocketMessageBase oMessage)
        {
            if (!(oMessage is IBalance)) return;
            var oBalance = (IBalance)oMessage;
            Balance = oBalance.Balance;
            Avaliable = oBalance.Avaliable;
            Locked = oBalance.Locked;

        }
    }
}
