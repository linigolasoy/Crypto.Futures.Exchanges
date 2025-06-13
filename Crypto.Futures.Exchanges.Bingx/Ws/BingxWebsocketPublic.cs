using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bingx.Ws
{
    internal class BingxWebsocketPublic: BaseWebsocketPublic, IWebsocketPublic
    {
        private const string WS_URL = "wss://open-api-swap.bingx.com/swap-market";
        private Task? m_oMainLoop = null;
        private CancellationTokenSource m_oCancelSource = new CancellationTokenSource();    

        public BingxWebsocketPublic(IFuturesMarket oMarket) : base(oMarket, WS_URL, new BingxWebsocketParser(oMarket))
        {
            this.StartTask = StartPostTask;
            this.StopTask = StopPostTask;
        }

        public async Task<bool> StartPostTask()
        {
            m_oCancelSource = new CancellationTokenSource();
            m_oMainLoop = LoopFundings();
            await Task.Delay(500);
            return true;
        }
        public async Task<bool> StopPostTask()
        {
            m_oCancelSource.Cancel();
            if (m_oMainLoop != null)
            {
                await m_oMainLoop;
                m_oMainLoop = null;
            }
            return true;
        }

        private async Task LoopFundings()
        {
            while (!m_oCancelSource.IsCancellationRequested)
            {
                try
                {
                    IFundingRate[]? aFundings = await this.Market.GetFundingRates();
                    if (aFundings != null)
                    {
                        foreach (var oFunding in aFundings)
                        {
                            this.DataManager.Put(oFunding);
                        }
                    }
                }
                catch( Exception e) 
                { 
                    if( this.Market.Exchange.Logger != null )
                    {
                        this.Market.Exchange.Logger.Error("BingxError on funding rates", e); 
                    }
                }
                await Task.Delay(10000);
            }
        }
    }
}
