using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Rest
{
    public class BaseApiCaller : IApiCaller
    {
        private Dictionary<string, string> m_aHeaders = new Dictionary<string, string>();

        public Dictionary<string, string> Headers { get => m_aHeaders; }

        public string UrlBase { get; }

        public BaseApiCaller(string strUrlBase)
        {
            UrlBase = strUrlBase;
        }

        public BaseApiCaller(string strUrlBase, Dictionary<string, string> aHeaders)
        {
            UrlBase = strUrlBase;
            m_aHeaders = aHeaders;
        }


        /// <summary>
        /// Compose Url 
        /// </summary>
        /// <param name="strEntryPoint"></param>
        /// <param name="aParameters"></param>
        /// <returns></returns>
        private string GetCompleteUrl(string strEntryPoint, Dictionary<string, string>? aParameters = null)
        {
            StringBuilder oUrlBuild = new StringBuilder();
            oUrlBuild.Append(UrlBase);
            if (!strEntryPoint.StartsWith("/"))
            {
                oUrlBuild.Append("/");
            }
            oUrlBuild.Append(strEntryPoint);

            if (aParameters != null && aParameters.Count > 0)
            {
                oUrlBuild.Append("?");
                bool bFirst = true;
                foreach (var kvp in aParameters)
                {
                    if (!bFirst)
                    {
                        oUrlBuild.Append("&");
                    }
                    oUrlBuild.Append($"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}");
                    bFirst = false;
                }
            }
            return oUrlBuild.ToString();

        }

        private HttpClient CreateClient()
        {
            HttpClient oClient = new HttpClient();
            if (m_aHeaders.Count > 0)
            {
                foreach (var kvp in m_aHeaders)
                {
                    oClient.DefaultRequestHeaders.Add(kvp.Key, kvp.Value);
                }
            }
            return oClient;
        }



        public async Task<IApiResult<string>> GetAsync(string strEntryPoint, Dictionary<string, string>? aParameters = null)
        {
            string strEndUrl = GetCompleteUrl(strEntryPoint, aParameters);

            HttpClient oClient = CreateClient();

            try
            {
                var oResponse = await oClient.GetAsync(strEndUrl);
                if (oResponse.IsSuccessStatusCode)
                {
                    string strContent = await oResponse.Content.ReadAsStringAsync();
                    return BaseApiResult<string>.CreateSuccessResult(strContent);
                }
                else
                {
                    return BaseApiResult<string>.CreateFromResponse(oResponse);
                }
            }
            catch (Exception ex)
            {
                return BaseApiResult<string>.CreateFromException(ex);
            }


        }

        public void SetHeader(string strKey, string strValue)
        {
            m_aHeaders[strKey] = strValue;
        }


        /// <summary>
        /// POST async
        /// </summary>
        /// <param name="strEntryPoint"></param>
        /// <param name="strBody"></param>
        /// <returns></returns>
        public async Task<IApiResult<string>> PostAsync(string strEntryPoint, string strBody)
        {
            string strEndUrl = GetCompleteUrl(strEntryPoint);

            HttpClient oClient = CreateClient();

            try
            {
                HttpContent oContent = new StringContent(strBody, Encoding.UTF8, "application/json");
                var oResponse = await oClient.PostAsync(strEndUrl, oContent);
                if (oResponse.IsSuccessStatusCode)
                {
                    string strContent = await oResponse.Content.ReadAsStringAsync();
                    return BaseApiResult<string>.CreateSuccessResult(strContent);
                }
                else
                {
                    return BaseApiResult<string>.CreateFromResponse(oResponse);
                }
            }
            catch (Exception ex)
            {
                return BaseApiResult<string>.CreateFromException(ex);
            }

        }

        public async Task<IApiResult<string>> PostAsync<T>(string strEntryPoint, T oBody)
        {
            string strJsonBody = JsonConvert.SerializeObject(oBody);
            return await PostAsync(strEntryPoint, strJsonBody);
        }
    }
}
