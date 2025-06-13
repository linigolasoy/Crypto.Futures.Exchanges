using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot.Trading
{
    internal class PaperTrader : ITrader
    {
        public PaperTrader( ITradingBot oBot ) 
        { 
            Bot = oBot;
        }
        public ITradingBot Bot { get; }
        public decimal Money { get => 10; }
        public decimal Leverage { get => 10; }

        public async Task<bool> Close(ITraderPosition oPosition, decimal? nPrice = null)
        {
            IWebsocketSymbolData? oData = oPosition.Symbol.Exchange.Market.Websocket.DataManager.GetData(oPosition.Symbol);
            if (oData == null) return false;

            if (oData.LastTrade == null) return false;
            await Task.Delay(1000);
            oPosition.Update();
            ((TraderPosition)oPosition).DateClose = DateTime.Now;
            return true;
            // ITraderPosition oPosition = new TraderPosition(oSymbol, bLong, nVolume, oData.LastTrade.Price);
            // return oPosition;
        }

        public async Task<ITraderPosition?> Open(IFuturesSymbol oSymbol, bool bLong, decimal nVolume, decimal? nPrice = null)
        {
            IWebsocketSymbolData? oData = oSymbol.Exchange.Market.Websocket.DataManager.GetData(oSymbol);
            if( oData == null ) return null;

            if( oData.LastTrade == null ) return null;  
            await Task.Delay(1000);
            ITraderPosition oPosition = new TraderPosition(oSymbol, bLong, nVolume, oData.LastTrade.Price);
            return oPosition;   
        }
    }
}
