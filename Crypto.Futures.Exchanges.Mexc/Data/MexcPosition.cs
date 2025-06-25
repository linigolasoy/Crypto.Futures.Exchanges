using Crypto.Futures.Exchanges.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace Crypto.Futures.Exchanges.Mexc.Data
{

    internal class MexcPositionJson
    {
        [JsonProperty("positionId")]
        public long PositionId { get; set; }
        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;
        [JsonProperty("holdVol")]
        public decimal HoldVol { get; set; }
        [JsonProperty("positionType")] // int position type， 1 long 2 short
        public int PositionType { get; set; }
        [JsonProperty("openType")] // int open type， 1 isolated 2 cross
        public int OpenType { get; set; }
        [JsonProperty("state")] // int position state,1 holding. 2 system auto-holding 3 closed
        public int State { get; set; }
        [JsonProperty("frozenVol")]
        public decimal FrozenVol { get; set; }
        [JsonProperty("closeVol")]
        public decimal CloseVol { get; set; }
        [JsonProperty("holdAvgPrice")]
        public decimal HoldAvgPrice { get; set; }
        [JsonProperty("closeAvgPrice")]
        public decimal CloseAvgPrice { get; set; }
        [JsonProperty("openAvgPrice")]
        public decimal OpenAvgPrice { get; set; }
        [JsonProperty("liquidatePrice")]
        public decimal LiquidatePrice { get; set; }
        [JsonProperty("oim")]
        public decimal Oim { get; set; }
        [JsonProperty("adlLevel")]
        public int AdlLevel { get; set; }
        [JsonProperty("im")]
        public decimal Im { get; set; }
        [JsonProperty("holdFee")]
        public decimal HoldFee { get; set; }
        [JsonProperty("realised")]
        public decimal Realised { get; set; }
        [JsonProperty("createTime")]
        public long CreateTime { get; set; }
        [JsonProperty("updateTime")]
        public long UpdateTime { get; set; }
    }
    internal class MexcPosition : IPosition
    {

        internal MexcPosition( IFuturesSymbol oSymbol, MexcPositionJson oJson )
        {
            Id = oJson.PositionId.ToString();
            Symbol = oSymbol;
            IsLong = (oJson.PositionType == 1); // 1 long, 2 short
            CreatedAt = Util.FromUnixTimestamp(oJson.CreateTime, true   );
            UpdatedAt = Util.FromUnixTimestamp(oJson.UpdateTime, true);
            IsOpen = (oJson.State == 1 || oJson.State == 2); // 1 holding, 2 system auto-holding
            AveragePriceOpen = oJson.OpenAvgPrice;
            Quantity = oJson.HoldVol * oSymbol.ContractSize;
        }
        public string Id { get; }

        public DateTime CreatedAt { get; }
        public DateTime UpdatedAt { get; }

        public IFuturesSymbol Symbol { get; }

        public bool IsLong { get; }

        public bool IsOpen { get; }

        public decimal AveragePriceOpen { get; }

        public decimal Quantity { get; }

        public static IPosition? Parse( IFuturesExchange oExchange, JToken? oToken )
        {
            if (oToken == null) return null;
            MexcPositionJson? oJson = oToken.ToObject<MexcPositionJson>();
            if (oJson == null) return null;
            IFuturesSymbol? oSymbol = oExchange.SymbolManager.GetSymbol(oJson.Symbol);
            if (oSymbol == null) return null;
            return new MexcPosition(oSymbol, oJson);    
        }
    }
}
