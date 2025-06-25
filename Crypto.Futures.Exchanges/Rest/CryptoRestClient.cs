using Newtonsoft.Json;
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
        public CryptoRestClient(string strUrl, IApiKey oApiKey, ICryptoRestParser oParser)
        {
            BaseUrl = strUrl;
            Parser = oParser;
            ApiKey = oApiKey;
        }
        public string BaseUrl { get; }
        public IApiKey ApiKey { get; }  

        public ICryptoRestParser Parser { get; }
        // public event RequestHeaderDelegate? OnRequestHeader = null;

        public Func<HttpMethod, string, Dictionary<string, string>?, string?, HttpRequestMessage?>? RequestEvaluator { get; set; } = null;
        private HttpClient CreateClient()
        {
            HttpClient oResult = new HttpClient();
            oResult.Timeout = TimeSpan.FromSeconds(15);
            return new HttpClient();
        }


        private HttpRequestMessage CreateRequestMessage(HttpMethod oMethod, string strEndPoint, Dictionary<string, string>? aParameters, string? strBody )
        {
            HttpRequestMessage? oMsg = null;
            string strUrl = $"{BaseUrl}{strEndPoint}";
            if (RequestEvaluator != null)
            {
                oMsg = RequestEvaluator(oMethod, strUrl, aParameters, strBody);
            }
            else
            {
                // TODO: Put params
                if (aParameters == null || aParameters.Count <= 0 )
                {
                    oMsg = new HttpRequestMessage(oMethod, strUrl);
                }
                else
                {
                    StringBuilder oBuildParams = new StringBuilder();   
                    foreach (var aParam in aParameters)
                    {
                        if( oBuildParams.Length > 0 ) oBuildParams.Append("&");
                        oBuildParams.Append(aParam.Key);
                        oBuildParams.Append("=");
                        oBuildParams.Append(aParam.Value);
                    }
                    string strUrlNew = $"{strUrl}?{oBuildParams.ToString()}";
                    oMsg = new HttpRequestMessage(oMethod, strUrlNew);
                }
            }
            if (oMsg == null) throw new Exception("Invalid request message");
            return oMsg;    
        }
        public async Task<ICryptoRestResult<T>> DoGetParams<T>(
            string strEndpoint, 
            Func<JToken?, T> oParserAction, 
            Dictionary<string, string>? aParameters = null)
        {
            try
            {
                var oClient = CreateClient();
                HttpRequestMessage oMsg = CreateRequestMessage(HttpMethod.Get, strEndpoint, aParameters, null);

                var oResponse = await oClient.SendAsync(oMsg); // await oClient.GetAsync(strUrl);
                ICryptoRestResult<T> oResult = await CryptoRestResult<T>.CreateFromResponse(oResponse, oParserAction);
                return oResult;
            }
            catch (Exception ex)
            {
                ICryptoRestResult<T> oResult = CryptoRestResult<T>.CreateFromException(ex);
                return oResult;
            }
        }

        public async Task<ICryptoRestResult<T[]>> DoGetArrayParams<T>(
            string strEndpoint, string? strField, 
            Func<JToken, T> oParserAction, 
            Dictionary<string, string>? aParameters = null)
        {
            try
            {
                var oClient = CreateClient();

                HttpRequestMessage oMsg = CreateRequestMessage(HttpMethod.Get, strEndpoint, aParameters, null);
                var oResponse = await oClient.SendAsync(oMsg); // await oClient.GetAsync(strUrl);
                ICryptoRestResult<T[]> oResult = await CryptoRestResult<T>.CreateFromResponseArray(oResponse, strField, oParserAction);
                return oResult;
            }
            catch (Exception ex)
            {
                ICryptoRestResult<T[]> oResult = CryptoRestResult<T[]>.CreateFromException(ex);
                return oResult;
            }
        }


        public async Task<ICryptoRestResult<bool>> DoPostParams<T>( string strEndpoint, T oData)
        {
            try
            {
                var oClient = CreateClient();
                string strBody = JsonConvert.SerializeObject(oData);   
                HttpRequestMessage oMsg = CreateRequestMessage(HttpMethod.Post, strEndpoint, null, strBody);
                var oResponse = await oClient.SendAsync(oMsg); // await oClient.GetAsync(strUrl);
                var oResult = await CryptoRestResult<bool>.CreateFromBoolean(oResponse);
                return oResult;
            }
            catch (Exception ex)
            {
                ICryptoRestResult<bool> oResult = CryptoRestResult<bool>.CreateFromException(ex);
                return oResult;
            }
            throw new NotImplementedException("Post not implemented yet");
        }

    }
}
