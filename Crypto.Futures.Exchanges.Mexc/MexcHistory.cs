using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.Rest;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Mexc
{
    internal class MexcHistory : IFuturesHistory
    {
        private MexcFutures m_oExchange;

        private const int MAX_TASKS = 10;
        private const string ENDP_BARS = "/api/v1/contract/kline/";

        public enum MexcIntervals
        {
            Min1,
            Min5,
            Min15,
            Min30,
            Min60,
            Hour4,
            Hour8,
            Day1,
            Week1
        }

        public MexcHistory( MexcFutures oExchange)
        {
            m_oExchange = oExchange;
        }

        public IFuturesExchange Exchange { get => m_oExchange; }


        public static MexcIntervals? TimeframeToMexc(BarTimeframe eFrame)
        {
            switch (eFrame)
            {
                case BarTimeframe.M1:
                    return MexcIntervals.Min1;
                case BarTimeframe.M5:
                    return MexcIntervals.Min5;
                case BarTimeframe.M15:
                    return MexcIntervals.Min15;
                case BarTimeframe.M30:
                    return MexcIntervals.Min30;
                case BarTimeframe.H1:
                    return MexcIntervals.Min60;
                case BarTimeframe.H4:
                    return MexcIntervals.Hour4;
                case BarTimeframe.D1:
                    return MexcIntervals.Day1;
                default: return null;
            }
        }

        public async Task<IBar[]?> GetBars(IFuturesSymbol oSymbol, BarTimeframe eFrame, DateTime dFrom, DateTime dTo)
        {
            MexcIntervals? oInterval = TimeframeToMexc(eFrame);
            if( oInterval == null ) return null;
            
            DateTimeOffset oOffsetFrom = new DateTimeOffset(dFrom.ToUniversalTime());
            DateTimeOffset oOffsetTo = new DateTimeOffset(dTo.ToUniversalTime());
            long nFrom = oOffsetFrom.ToUnixTimeSeconds();
            long nTo = oOffsetTo.ToUnixTimeSeconds(); 
            Dictionary<string,string> aParams = new Dictionary<string,string>();
            aParams.Add("interval", oInterval.Value.ToString());
            aParams.Add("start", nFrom.ToString());
            aParams.Add("end", nTo.ToString());
            var oResult = await m_oExchange.RestClient.DoGetParams<JToken?>($"{ENDP_BARS}{oSymbol.Symbol}", p => p, aParams);
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null) return null;
            return m_oExchange.Parser.ParseBars(oSymbol, eFrame, oResult.Data);
        }

        public async Task<IBar[]?> GetBars(IFuturesSymbol[] aSymbols, BarTimeframe eFrame, DateTime dFrom, DateTime dTo)
        {
            ITaskManager<IBar[]?> oTaskManager = new BaseTaskManager<IBar[]?>(MAX_TASKS);

            foreach(IFuturesSymbol oSymbol in aSymbols)
            {
                await oTaskManager.Add(GetBars(oSymbol, eFrame, dFrom, dTo)); 
            }
            var aResults = await oTaskManager.GetResults();
            List<IBar> aBars = new List<IBar>();    
            foreach( var oResult in aResults)
            {
                if (oResult == null) continue;
                aBars.AddRange(oResult);
            }

            return aBars.ToArray();
        }


        public async Task<IFundingRate[]?> GetFundingRates(IFuturesSymbol oSymbol, DateTime dFrom, DateTime dTo)
        {
            return await GetFundingRates( new IFuturesSymbol[] { oSymbol }, dFrom, dTo);
        }
        public async Task<IFundingRate[]?> GetFundingRates(IFuturesSymbol[] aSymbols, DateTime dFrom, DateTime dTo)
        {
            throw new NotImplementedException();
        }

    }
}
