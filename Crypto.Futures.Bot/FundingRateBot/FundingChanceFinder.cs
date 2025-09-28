using Crypto.Futures.Bot.Interface.FundingRates;
using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot.FundingRateBot
{
    internal class FundingChanceFinder : IFundingChanceFinder
    {

        private DateTime m_dLastCheck = DateTime.MinValue;  
        public FundingChanceFinder(IFundingRateBot bot)
        {
            Bot = bot;
        }
        public IFundingRateBot Bot { get; }

        public async Task<IFundingRateChance[]> FindNewChances()
        {
            List<IFundingRateChance> aResult = new List<IFundingRateChance>();
            Dictionary<string, List<IFundingRate>> oDictRates = new Dictionary<string, List<IFundingRate>>();

            DateTime dNow = DateTime.Now;

            if (dNow.Minute != 45) return Array.Empty<IFundingRateChance>();
            if ((dNow - m_dLastCheck).TotalMinutes < 30) return Array.Empty<IFundingRateChance>();

            Bot.Logger.Info("FundingChanceFinder: Checking for new funding rate chances");
            m_dLastCheck = dNow;

            DateTime dMin = DateTime.MaxValue;
            foreach (var oExchange in Bot.Exchanges)
            {
                try
                {
                    var aRates = await oExchange.Market.GetFundingRates();
                    if (aRates == null || aRates.Length == 0) continue;
                    foreach (var oRate in aRates)
                    {
                        if (oRate.Symbol.Quote != "USDT") continue;
                        if (!oDictRates.ContainsKey(oRate.Symbol.Base)) oDictRates[oRate.Symbol.Base] = new List<IFundingRate>();
                        if (oRate.Next <= dNow) continue;
                        if (oRate.Next < dMin) dMin = oRate.Next;
                        oDictRates[oRate.Symbol.Base].Add(oRate);
                    }
                }
                catch( Exception )
                {
                    continue;
                }

            }


            if( (dMin - dNow).TotalMinutes > 20 ) return Array.Empty<IFundingRateChance>(); 



            Dictionary<string, IFundingRate[]> aFilterRates = new Dictionary<string, IFundingRate[]>();
            foreach (var item in oDictRates)
            {
                if (item.Value.Count < 2) continue;
                IFundingRate[] aMin = item.Value.Where(p => p.Next == dMin).ToArray();
                if (aMin.Length <= 0) continue;
                IFundingRate[] aOthers = item.Value.Where(p => p.Next > dMin).ToArray();
                IFundingRateChance? oChance = null;
                if (aMin.Length == item.Value.Count)
                {
                    IFundingRate oBuy = item.Value.OrderBy(p => p.Rate).First();
                    IFundingRate oSell = item.Value.OrderByDescending(p => p.Rate).First();
                    decimal nDiff = (oSell.Rate - oBuy.Rate) * 100M;
                    if (nDiff < 0.2m) continue;

                    oChance = new FundingRateChance(Bot, oBuy, oSell, nDiff);
                }
                else if (aOthers.Length > 0)
                {
                    IFundingRate oMin = aMin.OrderBy(p => p.Rate).First();
                    IFundingRate oMax = aMin.OrderByDescending(p => p.Rate).First();

                    IFundingRate oOther = aOthers.First();
                    IFundingRate oFound = oMin;
                    if (Math.Abs(oFound.Rate) < Math.Abs(oMax.Rate)) oFound = oMax;
                    decimal nDiff = Math.Abs(oFound.Rate) * 100M;
                    if (nDiff < 0.2m) continue;
                    IFundingRate oBuy = (oFound.Rate < 0 ? oFound : oOther);
                    IFundingRate oSell = (oFound.Rate > 0 ? oFound : oOther);
                    oChance = new FundingRateChance(Bot, oBuy, oSell, nDiff );

                }
                else
                {
                    continue;
                }
                if(oChance != null)
                {
                    Bot.Logger.Info($" Found possible funding at {oChance.ChanceDate.ToShortTimeString()} of {oChance.PercentDifference} % on buy {oChance.SymbolLong.Symbol.ToString()} and sell {oChance.SymbolShort.Symbol.ToString()}");
                    aResult.Add(oChance);
                }
            }

            return aResult.ToArray();
        }
    }
}
