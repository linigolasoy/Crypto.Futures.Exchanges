using Bitget.Net.Objects.Models.V2;
using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bitget.Data
{
    internal class BitgetSymbol: BaseSymbol
    {
        public BitgetSymbol( IFuturesExchange oExchange, BitgetContract oContract ):
            base(oExchange, oContract.Symbol, oContract.BaseAsset, oContract.QuoteAsset)
        {
            this.ListDate = ( oContract.LaunchTime == null ? DateTime.MinValue: oContract.LaunchTime.Value.ToLocalTime());
            this.LeverageMax = (int)( oContract.MaxLeverage == null ? 1 : oContract.MaxLeverage.Value);
            this.LeverageMin = (int)(oContract.MinLeverage == null ? 1 : oContract.MinLeverage.Value);
            FeeMaker = oContract.MakerFeeRate;
            FeeTaker = oContract.TakerFeeRate;
            Decimals = oContract.PriceDecimals;
            QuantityDecimals = oContract.QuantityDecimals;
            ContractSize = 1;
            UseContractSize = false;
        }
    }
}
