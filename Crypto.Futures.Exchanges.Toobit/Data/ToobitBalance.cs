using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toobit.Net.Objects.Models;

namespace Crypto.Futures.Exchanges.Toobit.Data
{
    internal class ToobitBalance : IBalance
    {
        public ToobitBalance(IFuturesExchange oExchange, ToobitFuturesBalance oData)
        {
            Exchange = oExchange;
            Balance = oData.TotalBalance;
            Locked = oData.PositionMargin + oData.OrderMargin;
            Avaliable = oData.AvailableBalance;
            Currency = oData.Asset;
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
            IBalance oBalance = (IBalance)oMessage;
            Balance = oBalance.Balance;
            Locked = oBalance.Locked;
            Avaliable = oBalance.Avaliable;
        }
    }
}
