using Crypto.Futures.Exchanges.WebsocketModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Model
{

    /// <summary>
    /// Funding rates are periodic payments made between long and short positions in a futures contract to ensure that the contract price stays in line with the spot price of the underlying asset.
    /// </summary>
    public interface IFundingRate: IWebsocketMessage
    {
        public DateTime Next { get; }
        public decimal Rate { get; }    
    }
}
