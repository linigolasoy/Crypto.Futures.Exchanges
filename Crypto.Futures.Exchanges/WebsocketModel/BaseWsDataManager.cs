using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.WebsocketModel
{
    /// <summary>
    /// Manages data received by public websocket
    /// </summary>
    internal class BaseWsDataManager : IWebsocketDataManager
    {

        private ConcurrentDictionary<string, IWebsocketSymbolData> m_aData = new ConcurrentDictionary<string, IWebsocketSymbolData> ();
        public BaseWsDataManager( IFuturesExchange oExchange ) 
        { 
            Exchange = oExchange;
        }
        public IFuturesExchange Exchange { get; }

        public IWebsocketSymbolData? GetData(string strSymbol)
        {
            throw new NotImplementedException();
        }

        public IWebsocketSymbolData? GetData(IFuturesSymbol oSymbol)
        {
            throw new NotImplementedException();
        }


        private void UpdateSymbolData( BaseSymbolData oData, IWebsocketMessage oMessage )
        {
            switch (oMessage.MessageType)
            {
                case WsMessageType.Trade:
                    oData.LastTrade = (ITrade)oMessage;
                    break;
                case WsMessageType.FundingRate:
                    oData.FundingRate = (IFundingRate)oMessage; 
                    break;
                default:
                    break;
            }

        }
        private IWebsocketSymbolData CreateData( string strSymbol, IWebsocketMessage oMessage )
        {
            BaseSymbolData oData = new BaseSymbolData(oMessage.Symbol);
            UpdateSymbolData(oData, oMessage);
            return oData;   
        }
        private IWebsocketSymbolData UpdateData(string strSymbol, IWebsocketSymbolData oOld, IWebsocketMessage oMessage)
        {
            BaseSymbolData oData = (BaseSymbolData)oOld;
            UpdateSymbolData(oData, oMessage);
            return oOld;
        }

        public void Put(IWebsocketMessage oMessage)
        {
            m_aData.AddOrUpdate(oMessage.Symbol.Symbol, (s) => CreateData(s, oMessage), (s, oOld) => UpdateData(s, oOld, oMessage));
        }
    }
}
