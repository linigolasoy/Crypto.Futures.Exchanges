using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TL;

namespace Crypto.Futures.TelegramSignals.Signals
{
    internal class SignalEveningTraderMidCap : ITelegramSignalEvaluator
    {

        private const string TAG_LONG = "Long";
        private const string TAG_SHORT = "Short";
        private const string TAG_ENTRY = "Entry:";
        private const string TAG_TP = "TP:";
        private const string TAG_SL = "SL:";
        private const string TAG_ENTER = "You can enter";

        private ConcurrentDictionary<int, ISignal> m_aSignals = new ConcurrentDictionary<int, ISignal>();   
        private static int m_nLastId = 0;   
        public SignalType SignalType { get => SignalType.EveningTraderMidCap; }

        public ISignal? Evaluate(Dictionary<long, ChatBase> aChats, UpdateNewChannelMessage oMessage)
        {
            throw new NotImplementedException();
        }


        private void RemoveSignal(string[] aLines, DateTime dDate)
        {
            string strLine = aLines[0];
            int nPos = strLine.IndexOf('$');
            if (nPos < 0) return;
            string strCurrency = strLine.Substring(nPos+1).Trim();
            
            ISignal[]? aSignals = m_aSignals.Values.Where(p => p.Currency == strCurrency).ToArray();
            if (aSignals == null || aSignals.Length <= 0) return;
            ISignal oSignal = aSignals.OrderByDescending(p=> p.SignalDate).First();
            ((BaseSignal)oSignal).CloseDate = dDate.ToLocalTime(); // set close date to now
            return;
        }
        public ISignal? Evaluate(Message oMessage)
        {
            // if( oMessage.peer_id == 2726338238) return null; // ignore messages from this chat, it is not Evening Trader Mid Cap
            string strText = oMessage.message;
            if (oMessage.reply_to == null) return null;
            string[] aLines = strText.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            if (aLines.Length < 1) return null; // not enough lines to parse
            if (aLines[0].Contains("Cancel")) { RemoveSignal(aLines, oMessage.Date); return null; } // if the first line contains "Cancel", remove the signal
            if (aLines.Length < 3) return null;
            bool bLong = false;
            if (aLines[0].StartsWith(TAG_LONG)) bLong = true;
            else if (aLines[0].StartsWith(TAG_SHORT)) bLong = false;
            else return null;
            int nPos = aLines[0].IndexOf('$');
            if (nPos < 0) return null;
            string strCurrency = aLines[0].Substring(nPos + 1);
            nPos = strCurrency.IndexOf(' ');
            strCurrency = strCurrency.Substring(0, nPos);
            try
            {

                string? strEntryLine = aLines.FirstOrDefault(p => p.Contains(TAG_ENTRY));
                if (strEntryLine == null) return null;
                nPos = strEntryLine.IndexOf(TAG_ENTRY);
                strEntryLine = strEntryLine.Substring(nPos + TAG_ENTRY.Length).Trim();

                string[] aEntriesStr = strEntryLine.Split('-', StringSplitOptions.RemoveEmptyEntries);
                decimal[] aEntries = aEntriesStr.Select(p => Decimal.Parse(p.Trim(), CultureInfo.InvariantCulture)).ToArray();

                string? strTpLine = aLines.FirstOrDefault(p => p.Contains(TAG_TP));
                if (strTpLine == null) return null;
                nPos = strTpLine.IndexOf(TAG_TP);
                strTpLine = strTpLine.Substring(nPos + TAG_TP.Length).Trim();
                List<decimal> aTpList = new List<decimal>();    
                foreach( string strTp in strTpLine.Split('-', StringSplitOptions.RemoveEmptyEntries))
                {
                    if ( decimal.TryParse(strTp, CultureInfo.InvariantCulture, out decimal nTp ) )
                    {
                        aTpList.Add(nTp);
                    }
                }
                decimal[] aTps = aTpList.ToArray();

                string? strSlLine = aLines.FirstOrDefault(p => p.Contains(TAG_SL));
                if (strSlLine == null) return null;
                nPos = strSlLine.IndexOf(TAG_SL);
                strSlLine = strSlLine.Substring(nPos + TAG_SL.Length).Trim();
                decimal nSl = Decimal.Parse(strSlLine, CultureInfo.InvariantCulture);

                bool bPrev = aLines.Any(p => p.Contains(TAG_ENTER));
                ISignal oNew = new BaseSignal(
                    oMessage.Date.ToLocalTime(),
                    strCurrency,
                    SignalType.EveningTraderMidCap,
                    bLong,
                    aEntries,
                    aTps,
                    nSl,
                    bPrev
                );
                m_aSignals.TryAdd(m_nLastId++, oNew);   
                return oNew;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error parsing signal: {strText}");
                return null;
            }
        }

        public bool IsCorrectChat(ChatBase oChat)
        {
            if( oChat.Title.StartsWith("Evening Trader Group - Private"))
            {
                return true;
            }
            return false;
        }
    }
}
