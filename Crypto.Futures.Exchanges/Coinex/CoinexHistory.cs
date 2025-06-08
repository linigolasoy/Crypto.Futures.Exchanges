using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.Rest;

namespace Crypto.Futures.Exchanges.Coinex
{
    internal class CoinexHistory : IFuturesHistory
    {
        private const int MAX_TASKS = 10;
        private const string ENDP_BARS = "/futures/kline";

        private CoinexFutures m_oExchange;

        public CoinexHistory(CoinexFutures oExchange)
        {
            m_oExchange = oExchange;
        }

        public IFuturesExchange Exchange { get => m_oExchange; }


        private string? TimeframeToCoinex(BarTimeframe eFrame)
        {
            switch (eFrame)
            {
                case BarTimeframe.M1:
                    return "1min";
                case BarTimeframe.M15:
                    return "15min";
                case BarTimeframe.M30:
                    return "30min";
                case BarTimeframe.H1:
                    return "1hour";
                case BarTimeframe.H4:
                    return "4hour";
                case BarTimeframe.D1:
                    return "1day";
                default:
                    return null;

            }
        }


        public async Task<IBar[]?> GetBars(IFuturesSymbol oSymbol, BarTimeframe eFrame, DateTime dFrom, DateTime dTo)
        {
            string? strInterval = TimeframeToCoinex(eFrame);
            if (strInterval == null) return null;

            string strEndPoint = $"{ENDP_BARS}?market={oSymbol.Symbol}&period={strInterval}&limit=1000";
            var oResult = await m_oExchange.RestClient.DoGetArray<IBar?>(strEndPoint, null, p => m_oExchange.Parser.ParseBar(oSymbol, eFrame, p));
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null) return null;
            List<IBar> aResult = new List<IBar>();
            foreach( var bar in oResult.Data) 
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
