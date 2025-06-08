using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bitmart
{
    internal class BitmartHistory : IFuturesHistory
    {
        private const int MAX_TASKS = 10;
        private const string ENDP_BARS = "/contract/public/kline";

        private const int MAX_BARS = 500;

        private BitmartFutures m_oExchange;
        public BitmartHistory(BitmartFutures oExchange)
        {
            m_oExchange = oExchange;
        }

        public IFuturesExchange Exchange { get => m_oExchange; }

        public async Task<IBar[]?> GetBars(IFuturesSymbol oSymbol, BarTimeframe eFrame, DateTime dFrom, DateTime dTo)
        {
            int nInterval = (int)eFrame;
            DateTimeOffset oOffsetFrom = new DateTimeOffset(dFrom.ToUniversalTime());
            DateTimeOffset oOffsetTo = new DateTimeOffset(dTo.ToUniversalTime());
            long nFrom = oOffsetFrom.ToUnixTimeSeconds();
            long nTo = oOffsetTo.ToUnixTimeSeconds();

            long nSecondsMax = nInterval * MAX_BARS * 60;
            long nFromMin = nTo - nSecondsMax;
            if( nFromMin > nFrom )
            {
                nFrom = nFromMin;
            }

            string strEndPoint = $"{ENDP_BARS}?symbol={oSymbol.Symbol}&step={nInterval}&start_time={nFrom}&end_time={nTo}";
            var oResult = await m_oExchange.RestClient.DoGetArray<IBar?>(strEndPoint, null, p => m_oExchange.Parser.ParseBar(oSymbol, eFrame, p));
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null) return null;
            List<IBar> aResult = new List<IBar>();
            foreach (var bar in oResult.Data)
            {
                if (bar == null) continue;
                aResult.Add(bar);
            }

            return aResult.OrderBy(p => p.DateTime).ToArray();
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
