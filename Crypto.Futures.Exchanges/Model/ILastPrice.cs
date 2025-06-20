using Crypto.Futures.Exchanges.WebsocketModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Model
{
    public interface ILastPrice : IWebsocketMessage
    {
        public decimal Price { get; }
        public DateTime DateTime { get; }
    }
}
