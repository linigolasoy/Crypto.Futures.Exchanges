using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TL;

namespace Crypto.Futures.TelegramSignals.Signals
{
    internal class AfibieSignalEvaluator : ITelegramSignalEvaluator
    {
        public SignalType SignalType { get => SignalType.AfibieCryptoSignals; }

        public ISignal? Evaluate(Dictionary<long, ChatBase> aChats, UpdateNewChannelMessage oMessage)
        {
            throw new NotImplementedException();
        }

        public ISignal? Evaluate(Message oMessage)
        {
            string strText = oMessage.message;
            if (oMessage.reply_to != null) return null;
            string[] aLines = strText.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if( !aLines[0].StartsWith("🚀 New Trade Signal!"))
            {
                return null;
            }

            return null;
        }

        public bool IsCorrectChat(ChatBase oChat)
        {
            return oChat.Title.StartsWith("Afibie Crypto");
        }
    }
}
