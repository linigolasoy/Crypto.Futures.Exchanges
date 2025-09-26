using Crypto.Futures.Exchanges;
using Crypto.Futures.TelegramSignals.Signals;

namespace Crypto.Futures.TelegramSignals
{
    /// <summary>
    /// Signal scanner
    /// </summary>
    public interface ISignalScanner
    {
        public delegate void SignalScannerEventHandler(ISignal signal);

        public event SignalScannerEventHandler? OnSignalFound;

        public delegate string CodeNeededDelegate();

        public event CodeNeededDelegate? OnCodeNeeded; // Event to request code input from the user


        public IExchangeSetup Setup { get; }
        public ICommonLogger Logger { get; }

        public Task<bool> Start();


        public Task<bool> Stop();

        public Task<ISignal[]?> GetHistory(DateTime dFrom);
    }
}
