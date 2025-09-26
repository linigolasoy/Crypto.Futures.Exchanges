using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.TelegramSignals.Signals
{
    public interface ITelegramSignalEvaluator
    {
        public SignalType SignalType { get; }

        public bool IsCorrectChat(TL.ChatBase oChat);
        public ISignal? Evaluate(Dictionary<long, TL.ChatBase> aChats, TL.UpdateNewChannelMessage oMessage);

        public ISignal? Evaluate(TL.Message oMessage);
    }
}
