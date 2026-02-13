namespace Crypto.Futures.FrontEnd
{
    internal static class Program
    {

        public const string SETUP_FILE = "D:/Data/CryptoFutures/FuturesSetup.json";

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new FrmMain());
        }
    }
}