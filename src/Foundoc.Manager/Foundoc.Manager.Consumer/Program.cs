using System;
using System.Configuration;
using System.Diagnostics;

namespace Foundoc.Manager.Consumer
{
    public class Program
    {
        private static IDisposable _instance;
        private static ConsoleHost _consoleHost;

        static void Main(string[] args)
        {
            try
            {
                var address = ResolveHostAddress("1800");
                _consoleHost = new ConsoleHost(address);

                _consoleHost.Start();

                Process.Start("http://localhost:1800/foundocmanager");

                Console.WriteLine("Press any key to exit...");
                Console.ReadLine();
                _consoleHost.Stop();

            }
            catch (Exception)
            {
                
                throw;
                _consoleHost.Stop();
            }
        }

        private static string ResolveHostAddress(string cmdLinePort)
        {
            string baseAddress = "http://localhost"; //"http://+:" may be needed for a deployed version, but this requires netsh http add urlacl url=http://+:80/MyUri user=DOMAIN\user to run as something other than administrator
            var port = "1800"; //default port if none specified in config or command line

            if (!String.IsNullOrEmpty(ConfigurationManager.AppSettings["Service.BaseAddress"]))
            {
                baseAddress = ConfigurationManager.AppSettings["Service.BaseAddress"];
            }

            if (!String.IsNullOrEmpty(ConfigurationManager.AppSettings["Service.Port"]))
            {
                port = ConfigurationManager.AppSettings["Service.Port"];
            }
            if (!String.IsNullOrEmpty(cmdLinePort))
            {
                port = cmdLinePort;
            }

            return baseAddress + ":" + port;
        }
    }
}
