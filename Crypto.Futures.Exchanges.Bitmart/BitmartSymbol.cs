using BitMart.Net.Objects.Models;
using Crypto.Futures.Exchanges.Model;

namespace Crypto.Futures.Exchanges.Bitmart
{



    internal class BitmartSymbol : BaseSymbol
    {
        public BitmartSymbol(IFuturesExchange oExchange, BitMartContract oContract):
            base(oExchange, oContract.Symbol, oContract.BaseAsset, oContract.QuoteAsset)
        {

            // Leverage
            LeverageMax = (int)oContract.MaxLeverage;
            LeverageMin = (int)oContract.MinLeverage;
            // Fees
            FeeMaker = 0.0002m; // Bitmart does not provide maker fee in the symbol info
            FeeTaker = 0.0006m; // Bitmart does not provide taker fee in the symbol info
            // Precision
             
            decimal nPricePrecision = oContract.PricePrecision;
            decimal nVolumePrecision = oContract.QuantityPrecision;

            Decimals = (int) Math.Log10((double)nPricePrecision) * -1;
            QuantityDecimals = (int)Math.Log10((double)nVolumePrecision) * -1;
            // Contract size
            ContractSize = oContract.ContractQuantity;
            UseContractSize = true;
            // Minimum order size
            Minimum = oContract.MinQuantity;
            ListDate = oContract.OpenTime.ToLocalTime();

        }

    }
}
