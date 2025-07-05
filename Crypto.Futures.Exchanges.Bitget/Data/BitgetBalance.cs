using Bitget.Net.Objects.Models.V2;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bitget.Data
{
    internal class BitgetBalance : IBalance
    {
        public BitgetBalance(IFuturesExchange oExchange, BitgetFuturesBalance oBalance)
        {
            Exchange = oExchange;
            Currency = oBalance.MarginAsset;
            Balance = oBalance.Equity;
            Locked = oBalance.Locked;
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
