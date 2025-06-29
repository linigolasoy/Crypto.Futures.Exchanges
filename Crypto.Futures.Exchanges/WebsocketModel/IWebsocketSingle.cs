using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.WebsocketModel
{
    /// <summary>
    /// Single websocket interface  
    /// </summary>
    internal interface IWebsocketSingle
    {
        public IWebsocketPublic Manager { get; }

        public IWebsocketSubscription[] Subscribed { get; }

        public int Index { get; }   
        public Task<bool> Start();
        public Task<bool> Stop();
    }
}
