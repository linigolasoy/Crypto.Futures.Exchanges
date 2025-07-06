using BitMart.Net.Clients;
using BitMart.Net.Interfaces.Clients;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.Rest;
using CryptoExchange.Net.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bitmart
{
    public class BitmartFutures : IFuturesExchange
    {
        // private BitmartParser m_oParser;
        // internal const string BASE_URL = "https://api-cloud-v2.bitmart.com";
        // internal const string ENDP_SYMBOLS = "/contract/public/details";
        private IBitMartRestClient m_oRestClient;
        public BitmartFutures(IExchangeSetup oSetup, ICommonLogger? oLogger = null)
        {
            Setup = oSetup;
            Logger = oLogger;
            // m_oParser = new BitmartParser(this);
            ApiKey = Setup.ApiKeys.First(p=> p.ExchangeType == this.ExchangeType);
            m_oRestClient = new BitMartRestClient();
            m_oRestClient.SetApiCredentials(new ApiCredentials(ApiKey.ApiKey, ApiKey.ApiSecret, ApiKey.ApiPassword));
            SymbolManager = new FuturesSymbolManager();
            var oTask = RefreshSymbols();
            oTask.Wait(); // Wait for the symbols to be loaded  
            Market = new BitmartMarket(this);  
            // History = new BitmartHistory(this);
            Account = new BitmartAccount(this);
            Trading = new BitmartTrading(this);
        }

        internal IBitMartRestClient RestClient
        {
            get { return m_oRestClient; }
        }

        // internal CryptoRestClient RestClient { get { return new CryptoRestClient(BASE_URL, ApiKey, m_oParser); } }   
        // internal BitmartParser Parser { get => m_oParser; } 
        public IExchangeSetup Setup { get; }
        public IApiKey ApiKey { get; }
        public bool Tradeable { get => true; }

        public ICommonLogger? Logger { get; }

        public ExchangeType ExchangeType { get => ExchangeType.BitmartFutures; }

        public IFuturesMarket Market { get; }

        public IFuturesHistory History { get => throw new NotImplementedException(); }

        public IFuturesTrading Trading { get; }

        public IFuturesAccount Account { get; }

        public IFuturesSymbolManager SymbolManager { get; }

        public async Task<IFuturesSymbol[]?> RefreshSymbols()
        {
            try
            { 
                var oRes = await m_oRestClient.UsdFuturesApi.ExchangeData.GetContractsAsync();
                if (oRes == null || !oRes.Success) return null; 
                List<IFuturesSymbol> aResult = new List<IFuturesSymbol>(); 
                foreach ( var oData in oRes.Data )
                {
                    if (oData == null) continue;
                    if( oData.Status != BitMart.Net.Enums.ContractStatus.Trading) continue; // Only normal symbols
                    IFuturesSymbol oNew = new BitmartSymbol( this, oData); 
                    aResult.Add(oNew);
                }
                /*
                var oResult = await RestClient.DoGetArrayParams<IFuturesSymbol?>(ENDP_SYMBOLS, "symbols", p => m_oParser.ParseSymbols(p));
                if (oResult == null || !oResult.Success) return null;
                if (oResult.Data == null) return null;
                if (oResult.Data.Count() <= 0) return null;
                List<IFuturesSymbol> aResult = new List<IFuturesSymbol>();
                foreach (var oSymbol in oResult.Data)
                {
                    if (oSymbol == null) continue;
                    aResult.Add(oSymbol);
                }
                */
                SymbolManager.SetSymbols(aResult.ToArray());
                return aResult.ToArray();
            }
            catch (Exception ex)
            {
                if (Logger != null) Logger.Error("Error refreshing symbols", ex);
                return null;
            }
        }
    }
}
