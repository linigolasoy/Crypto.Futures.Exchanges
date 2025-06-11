using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Rest
{
    public class CryptoRestClient : ICryptoRestClient
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
            HttpClient oResult = new HttpClient();
            oResult.Timeout = TimeSpan.FromSeconds(15);
            return new HttpClient();
        }

        public async Task<ICryptoRestResult<T>> DoGet<T>(string strEndpoint, Func<JToken?, T> oParserAction, Dictionary<string, object>? aParameters = null)
        {
            try
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
            catch (Exception ex)
            {
                ICryptoRestResult<T> oResult = CryptoRestResult<T>.CreateFromException(ex);
                return oResult;
            }
        }

        public async Task<ICryptoRestResult<T[]>> DoGetArray<T>(string strEndpoint, string? strField, Func<JToken, T> oParserAction, Dictionary<string, Object>? aParameters = null)
        {
            try
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
            catch (Exception ex)
            {
                ICryptoRestResult<T[]> oResult = CryptoRestResult<T[]>.CreateFromException(ex);
                return oResult;
            }
        }

    }
}
