using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot.Interface.FundingRates
{

    /// <summary>
    /// Funding rate bot interface
    /// </summary>
    public interface IFundingRateBot: ICryptoBot
    {

        public IFundingRateChance[] Chances { get; }

        public IFundingChanceFinder ChanceFinder { get; }

    }
}
