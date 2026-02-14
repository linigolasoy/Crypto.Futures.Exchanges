using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crypto.Futures.Exchanges.Blofin.Data;
using BloFin.Net.Objects.Models;

namespace Cypto.Futures.Exchanges.Blofin
{
    internal class BlofinSymbolMine : BaseSymbol
    {
        public BlofinSymbolMine(IFuturesExchange oExchange, BloFinSymbol oJson) :
            base(oExchange, oJson.Symbol, oJson.BaseAsset, oJson.QuoteAsset)
        {

            ContractSize = oJson.ContractSize;
            LeverageMin = 1;
            LeverageMax = (int)oJson.MaxLeverage;
            FeeMaker = 0.0002M;
            FeeTaker = 0.0006M;

            decimal nPricePrecision = oJson.TickSize;
            decimal nVolumePrecision = oJson.TickSize;

            Decimals = (int)Math.Log10((double)nPricePrecision) * -1;
            QuantityDecimals = (int)Math.Log10((double)nVolumePrecision) * -1;
            UseContractSize = true;
            ListDate = oJson.ListTime.ToLocalTime();

            // Decimals = oJson.PriceScale;
            // QuantityDecimals = oJson.VolScale;
        }



    }
}
