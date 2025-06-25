using Crypto.Futures.Exchanges.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bingx
{
    internal class BingxRequestData
    {
        public string? Parameters { get; set; } = null;
        public string? Body { get; set; } = null;
        public string Signature { get; set; } = string.Empty;
        public Dictionary<string, string>? Headers { get; set; } = null;
    }

    internal class BingxPrivate
    {

        private const string HEADER_API = "X-BX-APIKEY";
        // private const string HEADER_TIME = "Request-Time";
        // private const string HEADER_SIGNATURE = "Signature";

        /*
            String str = signVo.getAccessKey() + signVo.getReqTime() + signVo.getRequestParam();
            return actualSignature(str, signVo.getSecretKey());
        */


        public BingxPrivate(IFuturesExchange oExchange)
        {
            Exchange = oExchange;
        }
        public IFuturesExchange Exchange { get; }

        private BingxRequestData CreateRequestData(Dictionary<string, string>? aParams, string? strBody)
        {
            BingxRequestData oData = new BingxRequestData();
            long nTimestamp = Util.ToUnixTimestamp(DateTime.Now, true);
            StringBuilder oBuildParams = new StringBuilder();
            JObject? oBodySorted = null;    

            if (strBody != null)
            {
                JObject oBody = JObject.Parse(strBody);
                oBody.Add("timestamp", nTimestamp);
                oBodySorted = new JObject();
                foreach (var oProp in oBody.Properties().OrderBy(p => p.Name))
                {
                    oBodySorted.Add(oProp.Name, oProp.Value);
                    if (oBuildParams.Length > 0) oBuildParams.Append("&");
                    oBuildParams.Append(oProp.Name);
                    oBuildParams.Append('=');
                    oBuildParams.Append(oProp.Value.ToString());
                }

            }
            else
            {
                if( aParams != null )
                {
                    foreach (var oParam in aParams)
                    {
                        if (oBuildParams.Length > 0) oBuildParams.Append("&");
                        oBuildParams.Append(oParam.Key);
                        oBuildParams.Append('=');
                        oBuildParams.Append(oParam.Value);
                    }
                }
                if (oBuildParams.Length > 0) oBuildParams.Append("&");
                // oBuildParams.Append("recvWindow=0");
                oBuildParams.Append("timestamp");
                oBuildParams.Append('=');
                oBuildParams.Append(nTimestamp.ToString());
            }
            string strPayLoad = oBuildParams.ToString();

            string strSignature = Util.EncodePayLoadHmac(strPayLoad, this.Exchange.ApiKey, true).ToLower();
            if(oBodySorted != null)
            {
                oBodySorted.Add("signature", strSignature);
            }
            else
            {
                oBuildParams.Append("&signature=");
                oBuildParams.Append(strSignature);
            }

            // const signature = CryptoJS.HmacSHA256(queryString, api_secret).toString()
            oData.Parameters = (oBodySorted == null ? oBuildParams.ToString() : null);
            oData.Signature = strSignature;
            oData.Headers = new Dictionary<string, string>();
            oData.Headers.Add(HEADER_API, Exchange.ApiKey.ApiKey);
            // oData.Headers.Add(HEADER_TIME, nTimestamp.ToString());
            // oData.Headers.Add(HEADER_SIGNATURE, strSignature);
            oData.Body = null;
            if (oBodySorted != null)
            {
                oData.Body = oBodySorted.ToString();
            }
            return oData;
        }


        public HttpRequestMessage? CreatePrivateRequest(
            HttpMethod oMethod,
            string strUrl,
            Dictionary<string, string>? aParams,
            string? strBody)
        {
            BingxRequestData oData = CreateRequestData(aParams, strBody);

            string strNewUrl = strUrl;
            if( oData.Parameters != null )
            {
                strNewUrl += $"?{oData.Parameters}";
            }
            HttpRequestMessage oMsg = new HttpRequestMessage(oMethod, strNewUrl);
            if (oData.Headers != null)
            {
                foreach (var oHeader in oData.Headers)
                {
                    oMsg.Headers.Add(oHeader.Key, oHeader.Value);
                }
            }
            if (oData.Body != null)
            {
                oMsg.Content = new StringContent(oData.Body, Encoding.UTF8, "application/json");
            }
            return oMsg;
        }
    }
}
