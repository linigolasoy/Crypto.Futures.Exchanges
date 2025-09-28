using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot.Interface.FundingRates
{

    /// <summary>
    /// Find funding rate chances
    /// </summary>
    public interface IFundingChanceFinder
    {
        public IFundingRateBot Bot { get; }

        public Task<IFundingRateChance[]> FindNewChances();
    }
}
