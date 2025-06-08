using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Rest
{
    internal class StdResultResponse
    {
        [JsonProperty("code")]
        public long ErrorCode { get; set; }

        [JsonProperty("data")]
        public JToken? Data { get; set; }
    }

    internal class CryptoRestResult<T> : ICryptoRestResult<T>
    {
        private CryptoRestResult(ICryptoErrorCode oErrorCode)
        {
            Error = oErrorCode;
            Success = false;
        }

        public CryptoRestResult(T oResult)
        {
            Success = true;
            Data = oResult;
        }

        public bool Success { get; private set; } = false;

        public ICryptoErrorCode? Error { get; private set; } = null;

        public T? Data { get; private set; }

        public static async Task<ICryptoRestResult<T>> CreateFromResponse(HttpResponseMessage oResponse, Func<JToken?, T> oParserAction)
        {
            ICryptoErrorCode? oHttpError = CryptoRestError.Create(oResponse);
            if (oHttpError != null) return new CryptoRestResult<T>(oHttpError);

            string strResponse = await oResponse.Content.ReadAsStringAsync();
            StdResultResponse? oStdResponse = JsonConvert.DeserializeObject<StdResultResponse>(strResponse);


            if (oStdResponse == null) oHttpError = CryptoRestError.Create(-99999, "Could not parse error code");

            if (oHttpError != null) return new CryptoRestResult<T>(oHttpError);

            T? oData = oParserAction(oStdResponse!.Data);
            return new CryptoRestResult<T>(oData);
        }

        public static async Task<ICryptoRestResult<T[]>> CreateFromResponseArray(HttpResponseMessage oResponse, string? strField, Func<JToken, T?> oParserAction)
        {
            ICryptoErrorCode? oHttpError = CryptoRestError.Create(oResponse);
            if (oHttpError != null) return new CryptoRestResult<T[]>(oHttpError);

            string strResponse = await oResponse.Content.ReadAsStringAsync();
            StdResultResponse? oStdResponse = JsonConvert.DeserializeObject<StdResultResponse>(strResponse);


            if (oStdResponse == null) oHttpError = CryptoRestError.Create(-99999, "Could not parse error code");

            if (oHttpError != null && oStdResponse!.Data == null) oHttpError = CryptoRestError.Create(-99998, "No Json received");

            if (oHttpError != null && !(oStdResponse!.Data is JArray)) oHttpError = CryptoRestError.Create(-99997, "Not a Json array");

            if (oHttpError != null) return new CryptoRestResult<T[]>(oHttpError);

            JArray? oArray = null;
            if (strField != null)
            {
                JObject? oObject = (JObject)(oStdResponse!.Data!);
                if( !oObject.ContainsKey(strField))
                {
                    oHttpError = CryptoRestError.Create(-99995, $"Field '{strField}' not found in response");
                    return new CryptoRestResult<T[]>(oHttpError!);
                }
                oArray = (JArray)(oObject[strField]!);
            }
            else
            {
                oArray = (JArray)(oStdResponse!.Data!);
            }
            if( oArray == null )
            {
                oHttpError = CryptoRestError.Create(-99996, "Not a Json array");
                return new CryptoRestResult<T[]>(oHttpError!);
            }

            List<T> aList = new List<T>();
            foreach (var oItem in oArray)
            {
                T? oParsed = oParserAction(oItem);
                if (oParsed == null) continue; // skip nulls
                aList.Add(oParsed);
            }
            return new CryptoRestResult<T[]>(aList.ToArray());
        }

    }
}
