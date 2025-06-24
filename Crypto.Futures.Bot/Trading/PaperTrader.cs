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
        public decimal Money { get => Bot.Setup.MoneyDefinition.Money; }
        public decimal Leverage { get => Bot.Setup.MoneyDefinition.Leverage; }

        private const int MAX_DELAY = 600;
        private const int MIN_DELAY = 100;

        public ITraderPosition[] ActivePositions { get=> throw new NotImplementedException(); }
        public ITraderPosition[] ClosedPositions { get => throw new NotImplementedException(); }

        public IBalance[] Balances => throw new NotImplementedException();
        public async Task<bool> Close(ITraderPosition oPosition, decimal? nPrice = null)
        {
            IWebsocketSymbolData? oData = oPosition.Symbol.Exchange.Market.Websocket.DataManager.GetData(oPosition.Symbol);
            if (oData == null) return false;

            if (oData.LastOrderbookPrice == null) return false;
            int nDelay = Random.Shared.Next(MIN_DELAY, MAX_DELAY);
            await Task.Delay(nDelay);
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

            if( oData.LastOrderbookPrice == null ) return null;
            int nDelay = Random.Shared.Next(MIN_DELAY, MAX_DELAY);
            await Task.Delay(nDelay);
            decimal nPriceOpen = (bLong? oData.LastOrderbookPrice.AskPrice : oData.LastOrderbookPrice.BidPrice);  
            ITraderPosition oPosition = new TraderPosition(oSymbol, bLong, nVolume, nPriceOpen);
            return oPosition;   
        }
    }
}
