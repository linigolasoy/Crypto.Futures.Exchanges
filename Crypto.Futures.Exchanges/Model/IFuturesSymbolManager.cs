using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Crypto.Futures.Exchanges.Model
{
    /// <summary>
    /// Symbol manager, easy access symbol thread safe mode
    /// </summary>
    public interface IFuturesSymbolManager
    {
        public IFuturesSymbol? GetSymbol(string strSymbol);
        public string[] GetAllKeys();
        public IFuturesSymbol[] GetAllValues();

        public void SetSymbols(IFuturesSymbol[] aSymbols);
    }
}
