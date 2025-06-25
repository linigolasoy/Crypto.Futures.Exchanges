using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bitget
{
    internal class BitgetRequestData
    {
        public string? Parameters { get; set; } = null;
        public string? Body { get; set; } = null;
        public string Signature { get; set; } = string.Empty;
        public Dictionary<string, string>? Headers { get; set; } = null;
    }

    internal class BitgetPrivate
    {

        private const string HEADER_API = "ACCESS-KEY";
        private const string HEADER_TIME = "ACCESS-TIMESTAMP";
        private const string HEADER_SIGNATURE = "ACCESS-SIGN";
        private const string HEADER_PASSWORD = "ACCESS-PASSPHRASE";


        public BitgetPrivate(IFuturesExchange oExchange)
        {
            Exchange = oExchange;
        }
        public IFuturesExchange Exchange { get; }

        private BitgetRequestData CreateRequestData(HttpMethod oMethod, string strEndPoint, Dictionary<string, string>? aParams, string? strBody)
        {
            BitgetRequestData oData = new BitgetRequestData();
            long nTimestamp = Util.ToUnixTimestamp(DateTime.Now, true);
            string strEndPointPayLoad = strEndPoint;
            if (strBody != null)
            {
                strEndPointPayLoad += strBody;
            }
            else
            {
                StringBuilder oBuildParams = new StringBuilder();
                if (aParams != null)
                {
                    foreach (var oParam in aParams)
                    {
                        if (oBuildParams.Length > 0) oBuildParams.Append("&");
                        oBuildParams.Append(oParam.Key);
                        oBuildParams.Append('=');
                        oBuildParams.Append(oParam.Value);
                    }
                    oData.Parameters = oBuildParams.ToString();
                }

                strEndPointPayLoad += (oBuildParams.Length > 0 ? "?" + oBuildParams.ToString() : string.Empty);
            }
            
            string strPayLoad = $"{nTimestamp.ToString()}{oMethod.ToString()}{strEndPointPayLoad}";
            string strSignature = Util.EncodePayLoadHmac(strPayLoad, this.Exchange.ApiKey, false);

            oData.Signature = strSignature;
            oData.Headers = new Dictionary<string, string>();
            oData.Headers.Add(HEADER_API, Exchange.ApiKey.ApiKey);
            oData.Headers.Add(HEADER_TIME, nTimestamp.ToString());
            oData.Headers.Add(HEADER_SIGNATURE, strSignature);
            oData.Headers.Add(HEADER_PASSWORD, Exchange.ApiKey.ApiPassword!);
            oData.Body = strBody;
            return oData;
        }


        public HttpRequestMessage? CreatePrivateRequest(
            HttpMethod oMethod,
            string strUrl,
            Dictionary<string, string>? aParams,
            string? strBody)
        {
            Uri oUri = new Uri(strUrl);
            BitgetRequestData oData = CreateRequestData(oMethod, oUri.LocalPath, aParams, strBody);

            string strNewUrl = strUrl;
            if( oData.Parameters != null ) strNewUrl += $"?{oData.Parameters}";
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
