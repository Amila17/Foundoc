using System;
using Microsoft.Owin.Hosting;

namespace Foundoc.Manager.Consumer
{
    public class ConsoleHost
    {
        private IDisposable _instance;
        private string _address;

        public ConsoleHost(string address)
        {
            _address = address;
        }


        public bool Start()
        {
            try
            {
                var message = "Starting Foundoc Manager Consumer App.";
                Console.WriteLine(message);

                _instance = WebApp.Start<Startup>(_address);

                message = "Foundoc Manager Consumer App started successfully on " + _address;
                Console.WriteLine(message);
            }
            catch (Exception ex)
            {
                var message = string.Format("There was an exception while starting FounDocManagerConsumer. The exception is: {0}{1} Stack Trace: {2}{3} InnerException: {4}", ex.Message, Environment.NewLine, ex.StackTrace, Environment.NewLine, ex.InnerException);
                Console.WriteLine(message);
            }

            return true;
        }

        public bool Stop()
        {
            if (_instance == null)
            {
                return true;
            }

            try
            {
                this._instance.Dispose();
            }
            catch (ObjectDisposedException)
            {
                _instance = null;
                return true;
            }
            catch (Exception ex)
            {
                var message =
                    string.Format(
                        "There was an exception while starting Bede.Profile Service. The exception is: {0}{1} Stack Trace: {2}{3} InnerException: {4}",
                        ex.Message, Environment.NewLine, ex.StackTrace, Environment.NewLine, ex.InnerException);
                Console.WriteLine(message);
                return false;
            }

            return true;
        }
    }
}
