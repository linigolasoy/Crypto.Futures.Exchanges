using Bitget.Net.Clients;
using Bitget.Net.Enums;
using Bitget.Net.Enums.V2;
using Bitget.Net.Interfaces.Clients;
using Crypto.Futures.Exchanges.Bitget.Data;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.Rest;
using CryptoExchange.Net.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bitget
{
    public class BitgetFutures : IFuturesExchange
    {

        private IBitgetRestClient m_oRestClient;
        public BitgetFutures(IExchangeSetup oSetup, ICommonLogger? oLogger)
        {
            Setup = oSetup;
            Logger = oLogger;
            ApiKey = Setup.ApiKeys.First(p=> p.ExchangeType == this.ExchangeType);

            m_oRestClient = new BitgetRestClient();
            m_oRestClient.SetApiCredentials(new ApiCredentials(ApiKey.ApiKey, ApiKey.ApiSecret, ApiKey.ApiPassword));   

            SymbolManager = new FuturesSymbolManager();
            var oTask = RefreshSymbols();
            oTask.Wait(); // Wait for the symbols to be loaded  
            Market = new BitgetMarket(this);
            // History = new BitgetHistory(this);
            Account = new BitgetAccount(this);
        }

        internal IBitgetRestClient RestClient { get { return m_oRestClient;  } }
        public IExchangeSetup Setup { get; }
        public IApiKey ApiKey { get; }
        public bool Tradeable { get => true; }

        public ICommonLogger? Logger { get; }

        public ExchangeType ExchangeType { get => ExchangeType.BitgetFutures; }

        public IFuturesMarket Market { get; }

        public IFuturesHistory History => throw new NotImplementedException();

        public IFuturesTrading Trading => throw new NotImplementedException();

        public IFuturesAccount Account { get; }

        public IFuturesSymbolManager SymbolManager { get; }

        public async Task<IFuturesSymbol[]?> RefreshSymbols()
        {
            try
            {
                var oResult = await RestClient.FuturesApiV2.ExchangeData.GetContractsAsync( BitgetProductTypeV2.UsdtFutures);
                if (oResult == null || !oResult.Success) return null;
                if (oResult.Data == null) return null;
                if (oResult.Data.Count() <= 0) return null;
                List<IFuturesSymbol> aResult = new List<IFuturesSymbol>();
                foreach (var oSymbol in oResult.Data)
                {
                    if (oSymbol == null) continue;
                    if( oSymbol.Status != FuturesSymbolStatus.Normal) continue; // Only normal symbols   
                    aResult.Add(new BitgetSymbol(this, oSymbol));
                }

                SymbolManager.SetSymbols(aResult.ToArray());
                return aResult.ToArray();
            }
            catch( Exception ex) 
            {
                if (Logger != null) Logger.Error("Error refreshing symbols", ex);
                return null;
            }
        }
    }
}
