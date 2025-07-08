


using Crypto.Futures.Bot;
using Crypto.Futures.Exchanges;
using Crypto.Futures.Exchanges.Factory;

public class Program
{
    public const string SETUP_FILE = "D:/Data/CryptoFutures/FuturesSetup.json";


    private enum eAction
    {
        None,
        Cancel,
        Close
    }
    /// <summary>
    /// Return if user hit <F> key to end
    /// </summary>
    /// <returns></returns>
    private static eAction NeedsAction()
    {
        eAction eResult = eAction.None;
        if (System.Console.KeyAvailable)
        {
            ConsoleKeyInfo oKeyInfo = System.Console.ReadKey();
            if (oKeyInfo.KeyChar == 'F' || oKeyInfo.KeyChar == 'f') eResult = eAction.Cancel;
            if (oKeyInfo.KeyChar == 'C' || oKeyInfo.KeyChar == 'c') eResult = eAction.Close;
        }
        return eResult;

    }

    /// <summary>
    /// Main task
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static async Task Main(string[] args)
    {
        IExchangeSetup oSetup = ExchangeFactory.CreateSetup(SETUP_FILE);
        ICommonLogger oLogger = ExchangeFactory.CreateLogger(oSetup, "RealArbitrageBot");

        // ITradingBot oBot = BotFactory.CreateNewSymbolBot(oSetup, oLogger);
        ITradingBot oBot = BotFactory.CreateArbitrageBot(oSetup, oLogger, false);

        try
        {
            // ITradingBot oBot = BotFactory.CreateFuturesArbitrageBot(oSetup, oLogger);
            // ITradingBot oBot = BotFactory.CreateSpreadBot(oSetup, oLogger);
            // ITradingBot oBot = new OppositeOrderTester(oSetup, oLogger);

            oLogger.Info("Enter main program");
            await oBot.Start();
            eAction eResult = eAction.None;
            while (eResult != eAction.Cancel)
            {
                eResult = NeedsAction();
                await Task.Delay(500);
            }

            oLogger.Info("Exit main program");
            await Task.Delay(1000);
            await oBot.Stop();
        }
        catch (Exception ex)
        {
            oLogger.Error("Error on main program", ex);
        }

    }
}