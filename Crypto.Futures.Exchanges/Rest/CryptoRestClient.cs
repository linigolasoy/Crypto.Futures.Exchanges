using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Rest
{
    internal class CryptoRestClient : ICryptoRestClient
    {
        public CryptoRestClient(string strUrl, ICryptoRestParser oParser)
        {
            BaseUrl = strUrl;
            Parser = oParser;
        }
        public string BaseUrl { get; }

        public ICryptoRestParser Parser { get; }

        private HttpClient CreateClient()
        {
            return new HttpClient();
        }

        public async Task<ICryptoRestResult<T>> DoGet<T>(string strEndpoint, Func<JToken?, T> oParserAction, Dictionary<string, object>? aParameters = null)
        {
            string strUrl = $"{BaseUrl}{strEndpoint}";
            if (aParameters != null)
            {
                throw new NotImplementedException();
            }

            var oClient = CreateClient();
            var oResponse = await oClient.GetAsync(strUrl);
            ICryptoRestResult<T> oResult = await CryptoRestResult<T>.CreateFromResponse(oResponse, oParserAction);
            return oResult;
        }

        public async Task<ICryptoRestResult<T[]>> DoGetArray<T>(string strEndpoint, string? strField, Func<JToken, T> oParserAction, Dictionary<string, Object>? aParameters = null)
        {
            string strUrl = $"{BaseUrl}{strEndpoint}";
            if (aParameters != null)
            {
                throw new NotImplementedException();
            }

            var oClient = CreateClient();
            var oResponse = await oClient.GetAsync(strUrl);
            ICryptoRestResult<T[]> oResult = await CryptoRestResult<T>.CreateFromResponseArray(oResponse, strField, oParserAction);
            return oResult;
        }

    }
}
