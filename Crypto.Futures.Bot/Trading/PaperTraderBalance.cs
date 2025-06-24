using Crypto.Futures.Exchanges;
using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot.Trading
{
    internal class PaperTraderBalance : IBalance
    {

        public PaperTraderBalance(IFuturesExchange oExchange, decimal nBalance)
        {
            Exchange = oExchange;
            StartBalance = nBalance;
            Balance = StartBalance;
            Avaliable = Balance; // Initially all balance is available
        }
        public IFuturesExchange Exchange { get; }

        public string Currency { get => "USDT"; }

        public decimal StartBalance { get; }= 0;
        public decimal Balance { get; set; } = 0;

        public decimal Locked { get; set; } = 0;

        public decimal Avaliable { get; set; } = 0;
    }
}
