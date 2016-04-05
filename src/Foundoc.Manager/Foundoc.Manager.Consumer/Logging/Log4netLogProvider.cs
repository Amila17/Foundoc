using System;
using System.Linq.Expressions;
using System.Reflection;
using Foundoc.Core.Logging;

namespace Foundoc.Manager.Consumer.Logging
{
    public class Log4NetLogProvider : ILogProvider
    {
        private static bool _providerIsAvailableOverride = true;
        private readonly Func<string, object> _getLoggerByNameDelegate;

        public Log4NetLogProvider()
        {
            if (!IsLoggerAvailable())
            {
                throw new InvalidOperationException("log4net.LogManager not found");
            }
            _getLoggerByNameDelegate = GetGetLoggerMethodCall();
        }

        public static bool ProviderIsAvailableOverride
        {
            get { return _providerIsAvailableOverride; }
            set { _providerIsAvailableOverride = value; }
        }

        public ILog GetLogger(string name)
        {
            return new Log4NetLogger(_getLoggerByNameDelegate(name));
        }

        public static bool IsLoggerAvailable()
        {
            return ProviderIsAvailableOverride && GetLogManagerType() != null;
        }

        private static Type GetLogManagerType()
        {
            return Type.GetType("log4net.LogManager, log4net");
        }

        private static Func<string, object> GetGetLoggerMethodCall()
        {
            Type logManagerType = GetLogManagerType();
            MethodInfo method = logManagerType.GetMethod("GetLogger", new[] {typeof (string)});
            ParameterExpression nameParam = Expression.Parameter(typeof (string), "name");
            MethodCallExpression methodCall = Expression.Call(null, method, new Expression[] {nameParam});
            return Expression.Lambda<Func<string, object>>(methodCall, new[] {nameParam}).Compile();
        }

        public class Log4NetLogger : ILog
        {
            private readonly dynamic _logger;

            internal Log4NetLogger(dynamic logger)
            {
                _logger = logger;
            }

            public void Log(LogLevel logLevel, Func<string> messageFunc)
            {
                switch (logLevel)
                {
                    case LogLevel.Info:
                        _logger.Info(messageFunc());
                        break;
                    case LogLevel.Warn:
                        _logger.Warn(messageFunc());
                        break;
                    case LogLevel.Error:
                        _logger.Error(messageFunc());
                        break;
                    case LogLevel.Fatal:
                        _logger.Fatal(messageFunc());
                        break;
                    default:
                        _logger.Debug(messageFunc());
                            // Log4Net doesn't have a 'Trace' level, so all Trace messages are written as 'Debug'
                        break;
                }
            }

            public void Log<TException>(LogLevel logLevel, Func<string> messageFunc, TException exception)
                where TException : Exception
            {
                switch (logLevel)
                {
                    case LogLevel.Info:
                        _logger.Info(messageFunc(), exception);
                        break;
                    case LogLevel.Warn:
                        _logger.Warn(messageFunc(), exception);
                        break;
                    case LogLevel.Error:
                        _logger.Error(messageFunc(), exception);
                        break;
                    case LogLevel.Fatal:
                        _logger.Fatal(messageFunc(), exception);
                        break;
                    default:
                        _logger.Debug(messageFunc(), exception);
                        break;
                }
            }
        }
    }
}