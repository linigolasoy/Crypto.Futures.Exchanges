using Crypto.Futures.Exchanges.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Mexc
{

    internal class MexcRequestData
    {
        public string? Parameters { get; set; } = null;
        public string? Body { get; set; } = null;
        public string Signature { get; set; } = string.Empty;
        public Dictionary<string, string>? Headers { get; set; } = null;
    }

    internal class MexcPrivate
    {

        private const string HEADER_API = "ApiKey";
        private const string HEADER_TIME = "Request-Time";
        private const string HEADER_SIGNATURE = "Signature";

        /*
            String str = signVo.getAccessKey() + signVo.getReqTime() + signVo.getRequestParam();
            return actualSignature(str, signVo.getSecretKey());
        */


        public MexcPrivate( IFuturesExchange oExchange ) 
        { 
            Exchange = oExchange;
        }
        public IFuturesExchange Exchange { get; }

        private MexcRequestData CreateRequestData(Dictionary<string, string>? aParams, string? strBody)
        {
            MexcRequestData oData = new MexcRequestData();
            long nTimestamp = Util.ToUnixTimestamp(DateTime.Now, true);
            StringBuilder oBuildParams = new StringBuilder();
            if (strBody != null)
            {
                oBuildParams.Append(strBody);
                
            }
            else if (aParams != null)
            {
                foreach (var oParam in aParams)
                {
                    if (oBuildParams.Length > 0) oBuildParams.Append("&");
                    oBuildParams.Append(oParam.Key);
                    oBuildParams.Append('=');
                    oBuildParams.Append(oParam.Value);
                }
            }
            // if( oBuildParams.Length > 0 ) oBuildParams.Append("&");
            // oBuildParams.Append("recvWindow=5000");
            // oBuildParams.Append("&timestamp");
            // oBuildParams.Append('=');
            // oBuildParams.Append(nTimestamp.ToString());
            string strPayLoad = $"{Exchange.ApiKey.ApiKey}{nTimestamp.ToString()}{oBuildParams.ToString()}";

            string strSignature = Util.EncodePayLoadHmac(strPayLoad, this.Exchange.ApiKey, true).ToLower();

            // const signature = CryptoJS.HmacSHA256(queryString, api_secret).toString()
            oData.Parameters = ( strBody == null ? oBuildParams.ToString() : null);
            oData.Signature = strSignature;
            oData.Headers = new Dictionary<string, string>();
            oData.Headers.Add(HEADER_API, Exchange.ApiKey.ApiKey);
            oData.Headers.Add(HEADER_TIME, nTimestamp.ToString());
            oData.Headers.Add(HEADER_SIGNATURE, strSignature);
            oData.Body = strBody;
            return oData;
        }


        public HttpRequestMessage? CreatePrivateRequest (
            HttpMethod oMethod,
            string strUrl, 
            Dictionary<string, string>? aParams, 
            string? strBody )
        {
            MexcRequestData oData = CreateRequestData(aParams, strBody);

            string strNewUrl = $"{strUrl}";
            if( oData.Parameters != null ) strNewUrl += $"?{oData.Parameters}";
            HttpRequestMessage oMsg = new HttpRequestMessage(oMethod, strNewUrl);
            if (oData.Headers != null)
            {
                foreach (var oHeader in oData.Headers)
                {
                    oMsg.Headers.Add(oHeader.Key, oHeader.Value);
                }
            }
            if(oData.Body != null)
            {
                oMsg.Content = new StringContent(oData.Body, Encoding.UTF8, "application/json");
            }
            else
            {
                oMsg.Content = null;
            }
            return oMsg;
        }
    }
}
