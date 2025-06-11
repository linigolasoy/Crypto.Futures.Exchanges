using Crypto.Futures.Exchanges.WebsocketModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Model
{
    public interface ITrade: IWebsocketMessage
    {
        public DateTime DateTime { get; }

        public decimal Price { get; }   
        public decimal Volume { get; }

        public bool IsBuy { get; }  
    }
}
