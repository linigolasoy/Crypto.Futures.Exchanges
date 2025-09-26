using Crypto.Futures.Exchanges;
using Crypto.Futures.TelegramSignals.Signals;
using System;
using TL;

namespace Crypto.Futures.TelegramSignals
{
    internal class TelegramMessageHandler : ISignalScanner
    {

        private WTelegram.Client? m_oClient = null;
        private ITelegramSignalEvaluator m_oSignalEvaluator;

        public IExchangeSetup Setup { get; }

        public ICommonLogger Logger { get; }

        public event ISignalScanner.SignalScannerEventHandler? OnSignalFound;
        public event ISignalScanner.CodeNeededDelegate? OnCodeNeeded; // Event to request code input from the user

        public TelegramMessageHandler(IExchangeSetup oSetup, ICommonLogger oLogger, SignalType eType)
        {

            Setup = oSetup;
            Logger = oLogger;
            switch (eType)
            {
                case SignalType.EveningTraderMidCap:
                    m_oSignalEvaluator = new SignalEveningTraderMidCap();
                    break;
                default:
                    throw new ArgumentException("Unsupported signal type for TelegramMessageHandler", nameof(eType));

            }
            // Initialize the Telegram client with the API ID and hash
            // This is where you would set up the Telegram client
            // using the provided API ID and hash.
            // For example:
            // var telegramClient = new TelegramClient(API_ID, API_HASH);
        }



        private async Task M_oClient_OnUpdates(TL.UpdatesBase arg)
        {
            if (arg.Chats.Count <= 0) return;
            if (arg.UpdateList.Length <= 0) return;
            foreach (var oUpdt in arg.UpdateList)
            {
                if (oUpdt == null) continue;
                if (!(oUpdt is TL.UpdateNewChannelMessage)) continue;
                TL.UpdateNewChannelMessage oMessage = (TL.UpdateNewChannelMessage)oUpdt;

                ISignal? oSignal = m_oSignalEvaluator.Evaluate(arg.Chats, oMessage);
                if (oSignal == null) continue;
                if (OnSignalFound != null) OnSignalFound(oSignal);
            }

            return;
        }

        /// <summary>
        /// Prevent logs
        /// </summary>
        /// <param name="nLevel"></param>
        /// <param name="strMessage"></param>
        private void LogFunc(int nLevel, string strMessage)
        {

        }

        /// <summary>
        /// Login into telegram
        /// </summary>
        /// <param name="loginInfo"></param>
        /// <returns></returns>
        private async Task DoLogin(string? loginInfo) // (add this method to your code)
        {
            if (m_oClient == null) return;
            while (m_oClient.User == null)
                switch (await m_oClient.Login(loginInfo)) // returns which config is needed to continue login
                {
                    case "verification_code":
                        {
                            if (OnCodeNeeded != null)
                            {
                                loginInfo = OnCodeNeeded();
                            }
                            else
                            {
                                Console.Write("Code: ");
                                loginInfo = Console.ReadLine();
                            }
                        }
                        break;
                    case "name": loginInfo = "John Doe"; break;    // if sign-up is required (first/last_name)
                    case "password": loginInfo = "secret!"; break; // if user has enabled 2FA
                    default: loginInfo = null; break;
                }
            // Console.WriteLine($"We are logged-in as {client.User} (id {client.User.id})");
        }

        private async Task CreateClient()
        {
            m_oClient = new WTelegram.Client((int)Setup.TelegramSetup.ApiId, Setup.TelegramSetup.ApiHash);
            WTelegram.Helpers.Log = LogFunc;
            await DoLogin(Setup.TelegramSetup.Phone);

        }
        public async Task<bool> Start()
        {
            if (!(await Stop())) return false;
            await CreateClient();
            if (m_oClient == null) return false;
            m_oClient.OnUpdates += M_oClient_OnUpdates;

            await m_oClient.ConnectAsync();

            return true;
        }


        public async Task<bool> Stop()
        {
            if (m_oClient == null) return true;
            m_oClient.OnUpdates -= M_oClient_OnUpdates;
            await Task.Delay(2000);
            m_oClient.Dispose();
            m_oClient = null;
            return true;
        }

        /// <summary>
        /// Get history of signals from a specific date
        /// </summary>
        /// <param name="dFrom"></param>
        /// <returns></returns>
        public async Task<ISignal[]?> GetHistory(DateTime dFrom)
        {
            if (m_oClient == null)
            {
                await CreateClient();
                if (m_oClient == null) return null;
            }
            var oResult = await m_oClient.Messages_GetAllChats(); // Ensure we have all chats loaded    
            if (oResult == null || oResult.chats.Count <= 0) return null;

            TL.Channel? oFound = null;
            foreach (var oChat in oResult.chats)
            {
                switch (oChat.Value)
                {
                    case Chat smallgroup when smallgroup.IsActive:
                        Console.WriteLine($"{oChat.Key}:  Small group: {smallgroup.title} with {smallgroup.participants_count} members");
                        break;
                    case Channel channel when channel.IsChannel:
                        Console.WriteLine($"{oChat.Key}: Channel {channel.username}: {channel.title}");
                        //Console.WriteLine($"              → access_hash = {channel.access_hash:X}");
                        break;
                    case Channel group: // no broadcast flag => it's a big group, also called supergroup or megagroup
                        Console.WriteLine($"{oChat.Key}: Group {group.username}: {group.title}");
                        //Console.WriteLine($"              → access_hash = {group.access_hash:X}");
                        break;
                }
                if (m_oSignalEvaluator.IsCorrectChat(oChat.Value))
                {
                    if (oChat.Value is TL.Channel)
                    {
                        oFound = (TL.Channel)oChat.Value;
                        break;
                    }
                }
            }
            if (oFound == null) return null;

            InputPeer oPeer = oFound.ToInputPeer();

            List<TL.Message> aHistory = new List<TL.Message>();
            DateTime dMin = DateTime.Now;
            List<TL.MessageService> aJoinedByLink = new List<TL.MessageService>();
            for (int nOffsetId = -1; ;)
            {
                var aMessages = await m_oClient.Messages_GetHistory(oPeer, nOffsetId, limit: 1000);
                if (aMessages.Messages.Length == 0) break;
                foreach (var msgBase in aMessages.Messages)
                {
                    var from = aMessages.UserOrChat(msgBase.From ?? msgBase.Peer); // from can be User/Chat/Channel

                    if (msgBase is TL.Message msg)
                    {
                        //if (msg.reply_to == null)
                        {
                            DateTime dActual = msg.Date.ToLocalTime();
                            if (dActual < dMin)
                            {
                                dMin = dActual;
                            }
                            aHistory.Add(msg);
                        }
                    }
                    else if( msgBase is TL.MessageService)
                    {
                        aJoinedByLink.Add((TL.MessageService)msgBase);  
                    }
                    else
                    {
                        //if (msgBase is MessageService ms) { }
                        Console.WriteLine($"{from} ");
                    }
                }
                if (dMin <= dFrom) break;
                nOffsetId = aMessages.Messages[^1].ID;
                await Task.Delay(500);
            }

            if (aHistory.Count <= 0) return null;
            List<ISignal> aResult = new List<ISignal>();
            foreach (var oHist in aHistory.OrderBy(p => p.Date))
            {
                ISignal? oSignal = m_oSignalEvaluator.Evaluate(oHist);
                if (oSignal == null) continue;
                aResult.Add(oSignal);

            }
            return aResult.ToArray();
        }


    }
}
