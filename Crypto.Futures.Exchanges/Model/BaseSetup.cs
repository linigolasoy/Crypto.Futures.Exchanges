using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Model
{

    internal class ApiKeyParsed
    {
        [JsonProperty("ExchangeType")]
        public string ExchangeType { get; set; } = string.Empty;
        [JsonProperty("ApiKey")]
        public string ApiKey { get; set; } = string.Empty;
        [JsonProperty("ApiSecret")]
        public string ApiSecret { get; set; } = string.Empty;
    }

    internal class SetupMoneyParsed
    {
        [JsonProperty("Leverage")]
        public int Leverage { get; set; } = 1;
        [JsonProperty("Amount")]
        public decimal Amount { get; set; } = 0;
        [JsonProperty("Threshold")]
        public decimal Threshold { get; set; } = 1;
        [JsonProperty("CloseOnProfit")]
        public decimal CloseOnProfit { get; set; } = 0.5M;
    }

    internal class SetupParsed
    {
        [JsonProperty("ApiKeys")]
        public List<ApiKeyParsed>? ApiKeys { get; set; } = null;
        [JsonProperty("Money")]
        public SetupMoneyParsed? Money { get; set; } = null;

        [JsonProperty("LogPath")]
        public string LogPath { get; set; } = string.Empty;
        [JsonProperty("HistoryPath")]
        public string HistoryPath { get; set; } = String.Empty;

    }

    /// <summary>
    /// Api key implementation for the exchange setup.
    /// </summary>
    internal class CryptoApiKey : IApiKey
    {
        public CryptoApiKey(ExchangeType eType, string strKey, string strSecret)
        {
            ExchangeType = eType;
            ApiKey = strKey;
            ApiSecret = strSecret;
        }
        public string ApiKey { get; }
        public string ApiSecret { get; }
        public ExchangeType ExchangeType { get; }
    }

    /// <summary>
    /// BaseSetup is a placeholder implementation of IExchangeSetup.
    /// </summary>
    internal class BaseSetup : IExchangeSetup
    {

        public BaseSetup(string strJson)
        {
            SetupParsed? oParsed = JsonConvert.DeserializeObject<SetupParsed>(strJson);
            if (oParsed == null) throw new ArgumentException("Invalid JSON format for setup.");

            List<IApiKey> aFound = new List<IApiKey>();
            if ( oParsed != null && oParsed.ApiKeys != null )
            {
                foreach (var oKey in oParsed.ApiKeys)
                {
                    ExchangeType eType = ExchangeType.BingxFutures;
                    if (Enum.TryParse<ExchangeType>(oKey.ExchangeType, out eType))
                    {
                        aFound.Add(new CryptoApiKey(eType, oKey.ApiKey, oKey.ApiSecret));
                    }
                }

            }
            if (oParsed?.LogPath != null) LogPath = oParsed.LogPath;
            else LogPath = string.Empty;
            ApiKeys = aFound.ToArray();

            ExchangeTypes = ApiKeys.Select(oKey => oKey.ExchangeType)
                .Distinct()
                .ToArray();

            // This constructor is intentionally left empty.
        }

        public IApiKey[] ApiKeys { get; }

        public ExchangeType[] ExchangeTypes { get; }

        public string LogPath { get; }
    }
}
