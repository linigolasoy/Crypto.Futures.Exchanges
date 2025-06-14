using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Factory
{

    internal class ApiKeyParsed
    {
        [JsonProperty("ExchangeType")]
        public string ExchangeType { get; set; } = string.Empty;
        [JsonProperty("ApiKey")]
        public string ApiKey { get; set; } = string.Empty;
        [JsonProperty("ApiSecret")]
        public string ApiSecret { get; set; } = string.Empty;
        [JsonProperty("ApiPassword")]
        public string? ApiPassword { get; set; } = string.Empty;
    }

    internal class SetupMoneyParsed
    {
        [JsonProperty("Leverage")]
        public int Leverage { get; set; } = 1;
        [JsonProperty("Amount")]
        public decimal Amount { get; set; } = 0;
    }

    internal class ArbitrageParsed
    {
        [JsonProperty("MinimumPercent")]
        public decimal Percent { get; set; } = 1;
        [JsonProperty("MaxOperations")]
        public int MaxOperations { get; set; } = 0;
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

        [JsonProperty("MoneyDefinition")]
        public SetupMoneyParsed MoneyParsed { get; set; } = new SetupMoneyParsed();
        [JsonProperty("Arbitrage")]
        public ArbitrageParsed Arbitrage { get; set; } = new ArbitrageParsed();

    }

    /// <summary>
    /// Api key implementation for the exchange setup.
    /// </summary>
    internal class CryptoApiKey : IApiKey
    {
        public CryptoApiKey(ExchangeType eType, string strKey, string strSecret, string? strPassword)
        {
            ExchangeType = eType;
            ApiKey = strKey;
            ApiSecret = strSecret;
            ApiPassword = strPassword;
        }
        public string ApiKey { get; }
        public string ApiSecret { get; }
        public string? ApiPassword { get; } 
        public ExchangeType ExchangeType { get; }
    }


    internal class MoneySetup : IMoneySetup
    {
        public MoneySetup( SetupMoneyParsed oParsed )
        {
            Money = oParsed.Amount;
            Leverage = oParsed.Leverage;
        }

        public decimal Money { get; }

        public decimal Leverage { get; }
    }

    internal class ArbitrageSetup : IArbitrageSetup
    {
        public ArbitrageSetup(ArbitrageParsed oParsed )
        {
            MinimumPercent = oParsed.Percent;
            MaxOperations = oParsed.MaxOperations;
        }
        public decimal MinimumPercent { get; }

        public int MaxOperations { get; }
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
                        aFound.Add(new CryptoApiKey(eType, oKey.ApiKey, oKey.ApiSecret, oKey.ApiPassword));
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
            MoneyDefinition = new MoneySetup(oParsed!.MoneyParsed);
            Arbitrage = new ArbitrageSetup(oParsed!.Arbitrage);
        }

        public IApiKey[] ApiKeys { get; }

        public ExchangeType[] ExchangeTypes { get; }

        public string LogPath { get; }

        public IMoneySetup MoneyDefinition { get; } 

        public IArbitrageSetup Arbitrage { get; }
    }
}
