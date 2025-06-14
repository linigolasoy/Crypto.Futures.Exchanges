using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Model
{
    /// <summary>
    /// Balance on a exchange
    /// </summary>
    public interface IBalance
    {
        public IFuturesExchange Exchange { get; }   
        public string Currency {  get; }    

        public decimal Balance { get; } 

        public decimal Locked { get; }  
        public decimal Avaliable { get; }   
    }
}
