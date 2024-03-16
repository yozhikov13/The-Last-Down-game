using Barebones.Logging;
using System.Collections.Generic;

namespace Barebones.MasterServer
{
    /// <summary>
    /// Logging settings wrapper
    /// </summary>
    public class MsfLogController
    {
        public MsfLogController(LogLevel globalLogLevel)
        {
            // Add default appender
            var appenders = new List<LogHandler>()
                {
                    LogAppenders.UnityConsoleAppenderWithNames
                };

            // Initialize the log manager
            LogManager.Initialize(appenders, globalLogLevel);
        }

        /// <summary>
        /// Overrides log levels of all the loggers
        /// </summary>
        /// <param name="logLevel"></param>
        public void ForceLogging(LogLevel logLevel)
        {
            LogManager.ForceLogLevel = logLevel;
        }

        public LogLevel GlobalLogLevel
        {
            get { return LogManager.GlobalLogLevel; }
            set { LogManager.GlobalLogLevel = value; }
        }
    }
}