using Crypto.Futures.Exchanges.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bitmart
{
    internal class BitmartFundingRate : IFundingRate
    {

        static List<DateTime> dDates = new List<DateTime>();    
        public BitmartFundingRate( IFuturesSymbol oSymbol, BitmartSymbolJson oJson) 
        { 
            Symbol = oSymbol;
            Next = Util.NextFundingRate(oJson.FundingIntervalHours);
            if (!dDates.Contains(Next))
            {
                dDates.Add(Next);
            }
            Rate = decimal.Parse(oJson.FundingRate, CultureInfo.InvariantCulture);  
        }
        public IFuturesSymbol Symbol { get; }

        public DateTime Next { get; }

        public decimal Rate { get; }

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
