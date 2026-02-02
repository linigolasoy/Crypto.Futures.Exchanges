using Crypto.Futures.Exchanges.Hyperliquidity.Data;
using Crypto.Futures.Exchanges.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Hyperliquidity.Parsing
{
    internal class MetaTableRow
    {
        [JsonProperty("funding")]
        public string Funding { get; set; } = string.Empty;
        [JsonProperty("openInterest")]
        public string OpenInterest { get; set; } = string.Empty;
        [JsonProperty("prevDayPx")]
        public string PrevDayPx { get; set; } = string.Empty;
        [JsonProperty("dayNtlVlm")]
        public string DayNtlVlm { get; set; } = string.Empty;
        [JsonProperty("premium")]
        public string Premium { get; set; } = string.Empty;
        [JsonProperty("oraclePx")]
        public string OraclePx { get; set; } = string.Empty;
        [JsonProperty("markPx")]
        public string MarkPx { get; set; } = string.Empty;
        [JsonProperty("midPx")]
        public string MidPx { get; set; } = string.Empty;
        [JsonProperty("impactPxs")]
        public string[] ImpactPxs { get; set; } = Array.Empty<string>();
        [JsonProperty("dayBaseVlm")]
        public string DayBaseVlm { get; set; } = string.Empty;
    }


    internal class MetaTokenUniverse
    {
        [JsonProperty("szDecimals")]
        public int Decimals { get; set; } = 0;
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;
        [JsonProperty("maxLeverage")]
        public int MaxLeverage { get; set; } = 0;
        [JsonProperty("marginTableId")]
        public int MarginTableId { get; set; } = 0;
    }


    internal class MetaTokenData
    {
        [JsonProperty("universe")]
        public MetaTokenUniverse[] Universe { get; set; } = Array.Empty<MetaTokenUniverse>();

        [JsonProperty("marginTables")]
        public JArray MarginTables { get; set; } = new JArray();
        [JsonProperty("collateralToken")]
        public int CollateralToken { get; set; } = 0;
    }


    internal class MetaInfoData
    {
        public MetaTokenData? Data { get; set; } = null;

        public MetaTableRow[]? Rows { get; set; } = null;

        public MetaInfoData(string strJson) 
        {
            JArray aArray = JArray.Parse(strJson);
            if (aArray == null) return;
            if (aArray.Count != 2) return;

            JObject oDefinition = (JObject)aArray[0];
            JArray aTables = (JArray)aArray[1];


            MetaTokenData? oData = JsonConvert.DeserializeObject<MetaTokenData>(oDefinition.ToString());
            if (oData == null) return;
            MetaTableRow[]? aMetaRows = JsonConvert.DeserializeObject<MetaTableRow[]>(aTables.ToString());
            if (aMetaRows == null) return;
            Data = oData;
            Rows = aMetaRows;
        }
    }

    /// <summary>
    /// Parser for symbol metadata
    /// </summary>
    internal class SymbolMetadataParser
    {


        public static IFuturesSymbol[] ParseSymbols(string jsonData, IFuturesExchange oExchange)
        {
            MetaInfoData oInfo = new MetaInfoData(jsonData);
            if( oInfo.Data == null || oInfo.Rows == null)
            {
                return Array.Empty<IFuturesSymbol>();
            }

            List<IFuturesSymbol> aResult = new List<IFuturesSymbol>();
            foreach (var oUni in oInfo.Data.Universe)
            {
                // Process each row to create IFuturesSymbol instances
                if (oUni == null) continue;
                if( oUni.MarginTableId < 0 || oUni.MarginTableId >= oInfo.Rows.Length )
                {
                    continue;
                }
                MetaTableRow oRow = oInfo.Rows[oUni.MarginTableId];
                BaseSymbol oSymbol = new BaseSymbol(
                    oExchange,
                    oUni.Name,
                    oUni.Name,
                    "USDT"
                    );
                oSymbol.Decimals = oUni.Decimals;
                oSymbol.QuantityDecimals = oUni.Decimals;
                oSymbol.LeverageMax = oUni.MaxLeverage;
                oSymbol.FeeMaker = 0.00015m;
                oSymbol.FeeTaker = 0.00045m;
                aResult.Add(oSymbol);
            }
            return aResult.ToArray();
        }

        public static IFundingRate[] ParseFunding(string jsonData, IFuturesExchange oExchange)
        {
            // Parsing logic to convert jsonData into ISymbol array
            MetaInfoData oInfo = new MetaInfoData(jsonData);
            if (oInfo.Data == null || oInfo.Rows == null)
            {
                return Array.Empty<IFundingRate>();
            }
            List<IFundingRate> aResult = new List<IFundingRate>();
            DateTime dNow = DateTime.Now;
            DateTime dNextFunding = new DateTime(dNow.Year, dNow.Month, dNow.Day, dNow.Hour, 0, 0, DateTimeKind.Local);
            dNextFunding = dNextFunding.AddHours(1);
            foreach (var oUni in oInfo.Data.Universe)
            {
                // Process each row to create IFuturesSymbol instances
                if (oUni == null) continue;
                if (oUni.MarginTableId < 0 || oUni.MarginTableId >= oInfo.Rows.Length)
                {
                    continue;
                }
                MetaTableRow oRow = oInfo.Rows[oUni.MarginTableId];

                IFuturesSymbol? oSymbol = oExchange.SymbolManager.GetSymbol(oUni.Name);
                if (oSymbol == null) continue;
                IFundingRate oNew = new HyperFundingRate(oSymbol, dNextFunding, decimal.Parse(oRow.Funding, System.Globalization.CultureInfo.InvariantCulture));
                aResult.Add(oNew);
            }

            return aResult.ToArray();
        }

    }
}
