using BingX.Net.Objects.Models;
using Crypto.Futures.Exchanges.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Transactions;

namespace Crypto.Futures.Exchanges.Bingx
{

    internal class BingxSymbol: BaseSymbol
    {
        public BingxSymbol( IFuturesExchange oExchange, BingXContract oJson ):
            base(oExchange, oJson.Symbol, oJson.Asset, oJson.Currency)
        {
            LeverageMin = 1;
            LeverageMax = 100;
            FeeMaker = (decimal)oJson.MakerFeeRate;
            FeeTaker = (decimal)oJson.TakerFeeRate;
            Decimals = oJson.PricePrecision;
            ContractSize = 1; // Bingx does not use contract size
            UseContractSize = false; // Bingx does not use contract size
            QuantityDecimals = oJson.QuantityPrecision;
            Minimum = (decimal)oJson.MinOrderQuantity; // Minimum trading unit in USDT
            ListDate = DateTime.MinValue;
        }


    }
}
