using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bitmart.Data
{
    internal class BitmartFundingRate : IFundingRate
    {

        public BitmartFundingRate( IFuturesSymbol oSymbol, BitmartSymbolJson oJson) 
        { 
            Symbol = oSymbol;
            Next = Util.NextFundingRate(oJson.FundingIntervalHours);
            Rate = decimal.Parse(oJson.FundingRate, CultureInfo.InvariantCulture);  
        }
        public IFuturesSymbol Symbol { get; }
        public WsMessageType MessageType { get => WsMessageType.FundingRate; }

        public DateTime Next { get; private set; }

        public decimal Rate { get; private set; }
        public void Update(IWebsocketMessage oMessage)
        {
            if (!(oMessage is IFundingRate)) return;
            IFundingRate oFunding = (IFundingRate)oMessage;
            Next = oFunding.Next;
            Rate = oFunding.Rate;
        }

        public static IFundingRate? Parse( IFuturesExchange oExchange, JToken? oToken )
        {
            if (oToken == null) return null;
            if (!(oToken is JObject)) return null;

            BitmartSymbolJson? oJson = oToken.ToObject<BitmartSymbolJson>();
            if (oJson == null) return null;

            IFuturesSymbol? oSymbol = oExchange.SymbolManager.GetSymbol(oJson.Symbol);
            if (oSymbol == null) return null;

            return new BitmartFundingRate(oSymbol, oJson);
        }
    }
}
