using CoinEx.Net.Objects.Models.V2;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Coinex.Data
{
    internal class CoinexBalance : IBalance
    {
        public CoinexBalance(IFuturesExchange exchange, CoinExFuturesBalance oBalance)
        {
            Exchange = exchange;
            Currency = oBalance.Asset;
            Balance = ( oBalance.Equity == null ? 0: oBalance.Equity.Value);
            Locked = oBalance.Frozen;
            Avaliable = oBalance.Available;
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
