using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Mexc.Data
{

    internal class MexcBalanceJson
    {
        [JsonProperty("currency")]
        public string Currency { get; set; } = string.Empty;
        [JsonProperty("positionMargin")]
        public decimal PositionMargin { get; set; } = 0;
        [JsonProperty("frozenBalance")]
        public decimal FrozenBalance { get; set; } = 0;
        [JsonProperty("availableBalance")]
        public decimal AvailableBalance { get; set; } = 0;
        [JsonProperty("cashBalance")]
        public decimal CashBalance { get; set; } = 0;
        [JsonProperty("equity")]
        public decimal Equity { get; set; } = 0;
        [JsonProperty("unrealized")]
        public decimal Unrealized { get; set; } = 0;
    }
    internal class MexcBalance : IBalance
    {
        internal MexcBalance( IFuturesExchange oExchange, MexcBalanceJson oJson)
        {
            Exchange = oExchange;
            Currency = oJson.Currency;
            Balance = oJson.Equity;
            Locked = oJson.FrozenBalance + oJson.PositionMargin;
            Avaliable = oJson.CashBalance;
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
