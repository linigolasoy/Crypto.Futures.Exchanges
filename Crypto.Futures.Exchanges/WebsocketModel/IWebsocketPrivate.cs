using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.WebsocketModel
{


    /// <summary>
    /// Private websocket interface
    /// </summary>
    /// 
    public interface IWebsocketPrivate : IPrivateWebsocketManager
    {


        public Task<bool> Start();
        public Task<bool> Stop();


    }
}
