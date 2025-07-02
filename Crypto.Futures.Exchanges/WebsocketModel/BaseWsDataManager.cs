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
    public class BaseWsDataManager : IWebsocketDataManager
    {

        private ConcurrentDictionary<string, IWebsocketSymbolData> m_aData = new ConcurrentDictionary<string, IWebsocketSymbolData> ();

        public BaseWsDataManager( IFuturesExchange oExchange ) 
        { 
            Exchange = oExchange;
        }
        public IFuturesExchange Exchange { get; }

        public DateTime LastUpdate { get; private set; } = DateTime.MinValue;
        public IWebsocketSymbolData? GetData(string strSymbol)
        {
            IWebsocketSymbolData? oFound = null;
            if( !m_aData.TryGetValue (strSymbol, out oFound) ) return null;
            return oFound;
        }

        public IWebsocketSymbolData? GetData(IFuturesSymbol oSymbol)
        {
            return GetData(oSymbol.Symbol);
        }


        private void UpdateSymbolData( BaseSymbolData oData, IWebsocketMessage oMessage )
        {
            switch (oMessage.MessageType)
            {
                /*
                case WsMessageType.Trade:
                    if (oData.LastTrade == null) oData.LastTrade = (ITrade)oMessage;
                    else
                    {
                        oData.LastTrade.Update(oMessage);
                    }
                    break;
                */
                case WsMessageType.LastPrice:
                    ILastPrice oPrice = ( ILastPrice )oMessage;  
                    if (oData.LastPrice == null) oData.LastPrice = (ILastPrice)oMessage;
                    else
                    {
                        oData.LastPrice.Update(oMessage);
                    }
                    LastUpdate = oPrice.DateTime;
                    break;

                case WsMessageType.OrderbookPrice:
                    IOrderbookPrice oTicker = (IOrderbookPrice)oMessage;
                    if (oData.LastOrderbookPrice == null) oData.LastOrderbookPrice = (IOrderbookPrice)oMessage;
                    else
                    {
                        oData.LastOrderbookPrice.Update(oMessage);
                    }
                    LastUpdate = oTicker.DateTime;
                    break;

                case WsMessageType.FundingRate:
                    if (oData.FundingRate == null) oData.FundingRate = (IFundingRate)oMessage; 
                    else 
                    {
                        oData.FundingRate.Update(oMessage);
                    }
                    break;
                default:
                    break;
            }

        }
        private IWebsocketSymbolData CreateData( string strSymbol, IWebsocketMessage oMessage )
        {
            BaseSymbolData oData = new BaseSymbolData(oMessage.Symbol);
            oData.LastUpdate = DateTime.Now;    
            UpdateSymbolData(oData, oMessage);
            return oData;   
        }
        private IWebsocketSymbolData UpdateData(string strSymbol, IWebsocketSymbolData oOld, IWebsocketMessage oMessage)
        {
            BaseSymbolData oData = (BaseSymbolData)oOld;
            oData.LastUpdate = DateTime.Now;
            UpdateSymbolData(oData, oMessage);
            return oOld;
        }

        public void Put(IWebsocketMessage oMessage)
        {
            m_aData.AddOrUpdate(oMessage.Symbol.Symbol, (s) => CreateData(s, oMessage), (s, oOld) => UpdateData(s, oOld, oMessage));
        }
    }
}
