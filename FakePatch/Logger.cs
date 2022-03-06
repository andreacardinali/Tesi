using System;
using System.Diagnostics;
using System.IO;
using static FakePatch.Globals;

namespace FakePatch
{
    public enum LogTarget
    {
        None, Console, File, Database, EventLog
    }

    public enum LogLevel
    {
        Error, Info, Debug
    }

    public static class LogHelper
    {
        private static LogBase logger = null;
        public static void Log(string message, LogLevel loglevel = LogLevel.Info, LogTarget target = gLogTarget)
        {
            switch (target)
            {
                case LogTarget.None:
                    break;
                case LogTarget.Console:
                    Console.WriteLine("[" + loglevel.ToString().ToUpper() + "]" + message);
                    break;
                case LogTarget.File:
                    logger = new FileLogger();
                    logger.LogMessage(message, loglevel);
                    break;
                case LogTarget.Database:
                    logger = new DBLogger();
                    logger.LogMessage(message, loglevel);
                    break;
                case LogTarget.EventLog:
                    logger = new EventLogger();
                    logger.LogMessage(message, loglevel);
                    break;
                default:
                    return;
            }
        }
    }
    public abstract class LogBase
    {
        protected readonly object lockObj = new object();
        public abstract void LogMessage(string message, LogLevel loglevel);
    }

    public class FileLogger : LogBase
    {
        public string filePath = gLogFile.FullName;
        public override void LogMessage(string message, LogLevel loglevel)
        {
            lock (lockObj)
            {
                using (StreamWriter streamWriter = new StreamWriter(new FileStream(filePath, FileMode.Append)))
                {
                    streamWriter.AutoFlush = true;
                    streamWriter.WriteLine(DateTime.Now + " [" + loglevel.ToString().ToUpper() + "] " + message);
                    streamWriter.Close();
                }
            }
        }
    }

    public class EventLogger : LogBase
    {
        public override void LogMessage(string message, LogLevel loglevel)
        {
            lock (lockObj)
            {
                EventLogEntryType EntryType = new EventLogEntryType();
                switch (loglevel)
                {
                    case LogLevel.Error:
                        EntryType = EventLogEntryType.Error;
                        break;
                    case LogLevel.Info:
                        EntryType = EventLogEntryType.Information;
                        break;
                    case LogLevel.Debug:
                        EntryType = EventLogEntryType.Information;
                        break;
                }


                EventLog m_EventLog = new EventLog();
                if (!EventLog.SourceExists(gEventSourceName))
                {
                    EventLog.CreateEventSource(gEventSourceName, gEventLogName);
                }
                m_EventLog.Source = Globals.gEventSourceName;
                m_EventLog.WriteEntry(message, EntryType);
            }
        }
    }

    public class DBLogger : LogBase
    {
        string connectionString = string.Empty;
        public override void LogMessage(string message, LogLevel loglevel)
        {
            lock (lockObj)
            {
                //Code to log data to the database
            }
        }
    }
}
