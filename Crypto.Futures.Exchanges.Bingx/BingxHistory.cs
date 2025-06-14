using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.Rest;

namespace Crypto.Futures.Exchanges.Bingx
{
    /// <summary>
    /// History data for bing
    /// </summary>
    internal class BingxHistory : IFuturesHistory
    {

        private const int MAX_TASKS = 10;
        private const string ENDP_BARS = "/openApi/swap/v3/quote/klines";

        private BingxFutures m_oExchange;

        public BingxHistory( BingxFutures oExchange)
        {
            m_oExchange = oExchange;
        }
        public IFuturesExchange Exchange { get => m_oExchange; }


        private string? TimeframeToBingx( BarTimeframe eFrame )
        {
            switch( eFrame)
            {
                case BarTimeframe.M1:
                    return "1m";
                case BarTimeframe.M15:
                    return "15m";
                case BarTimeframe.M30:
                    return "30m";
                case BarTimeframe.H1:
                    return "1h";
                case BarTimeframe.H4:
                    return "4h";
                case BarTimeframe.D1:
                    return "1d";
                default:
                    return null;

            }
        }
        public async Task<IBar[]?> GetBars(IFuturesSymbol oSymbol, BarTimeframe eFrame, DateTime dFrom, DateTime dTo)
        {
            string? strInterval = TimeframeToBingx(eFrame);
            if (strInterval == null) return null;

            DateTimeOffset oOffsetFrom = new DateTimeOffset(dFrom.ToUniversalTime());
            DateTimeOffset oOffsetTo = new DateTimeOffset(dTo.ToUniversalTime());
            long nFrom = oOffsetFrom.ToUnixTimeMilliseconds();
            long nTo = oOffsetTo.ToUnixTimeMilliseconds();
            Dictionary<string, string> aParams = new Dictionary<string, string>();
            aParams.Add("symbol", oSymbol.Symbol);
            aParams.Add("interval", strInterval);
            aParams.Add("startTime", nFrom.ToString());
            aParams.Add("endTime", nTo.ToString());
            aParams.Add("limit", "1440");
            // string strEndPoint = $"{ENDP_BARS}?symbol={oSymbol.Symbol}&interval={strInterval}&startTime={nFrom}&endTime={nTo}&limit=1440";
            var oResult = await m_oExchange.RestClient.DoGetArrayParams<IBar?>(ENDP_BARS, null, p => m_oExchange.Parser.ParseBar(oSymbol, eFrame, p), aParams);
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null) return null;
            List<IBar> aResult = new List<IBar>();  
            foreach(var bar in oResult.Data)
            {
                if( bar == null) continue;
                aResult.Add(bar);
            }
            return aResult.OrderBy(p=> p.DateTime).ToArray();   


        }

        public async Task<IBar[]?> GetBars(IFuturesSymbol[] aSymbols, BarTimeframe eFrame, DateTime dFrom, DateTime dTo)
        {
            ITaskManager<IBar[]?> oTaskManager = new BaseTaskManager<IBar[]?>(MAX_TASKS);

            foreach (IFuturesSymbol oSymbol in aSymbols)
            {
                await oTaskManager.Add(GetBars(oSymbol, eFrame, dFrom, dTo));
            }
            var aResults = await oTaskManager.GetResults();
            List<IBar> aBars = new List<IBar>();
            foreach (var oResult in aResults)
            {
                if (oResult == null) continue;
                aBars.AddRange(oResult);
            }

            return aBars.ToArray();
        }
    }
}
