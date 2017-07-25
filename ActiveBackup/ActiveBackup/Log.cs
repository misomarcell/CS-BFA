using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace ActiveBackup
{
    public class Log
    {
        public List<LogEntry> entries;

        public Log()
        {
            entries = new List<LogEntry>();
        }

        /// <summary>
        /// Adds a new entry to the log list. Log.Serialize required!
        /// </summary>
        public void WriteLog(string message)
        {
            entries.Add(new LogEntry(DateTime.Now, message));
        }

        /// <summary>
        /// Returns with a new log file name with the current date and time combined.
        /// </summary>
        private static string GetNewLogFileName()
        {
            return Path.Combine(Settings.backupFolder, "logging_" + DateTime.Now.ToString("yyyyMMddHHmmss")) + ".xml";
        }

        /// <summary>
        /// Saves the given log object with the name of GetNewLogFileName() returns.
        /// </summary>
        public static void Serialize(Log log)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Log));
                using (StreamWriter stream = new StreamWriter(GetNewLogFileName()))
                {
                    serializer.Serialize(stream, log);
                    stream.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }
        }
    }
    public class LogEntry
    {
        public DateTime date;
        public String logEvent;

        public LogEntry(){}
        public LogEntry(DateTime date, String logEvent)
        {
            this.date = date;
            this.logEvent = logEvent;
        }
    }
}