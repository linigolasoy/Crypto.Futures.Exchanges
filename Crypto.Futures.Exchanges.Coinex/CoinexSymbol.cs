using CoinEx.Net.Objects.Models.V2;
using Crypto.Futures.Exchanges.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Crypto.Futures.Exchanges.Coinex
{


    internal class CoinexSymbol : BaseSymbol
    {
        public CoinexSymbol(IFuturesExchange exchange, CoinExFuturesSymbol oJson):
            base(exchange, oJson.Symbol, oJson.BaseAsset, oJson.QuoteAsset)
        {
            LeverageMax = oJson.Leverage.Max();
            LeverageMin = oJson.Leverage.Min();
            FeeMaker = oJson.MakerFeeRate;
            FeeTaker = oJson.TakerFeeRate;
            Decimals = oJson.PricePrecision;
            ContractSize = 1; // Coinex does not specify contract size, assume 1
            UseContractSize = false; // Not used
            QuantityDecimals = oJson.QuantityPrecision;
            Minimum = oJson.MinOrderQuantity;
            ListDate = DateTime.Today.AddYears(-10);
        }

    }
}
