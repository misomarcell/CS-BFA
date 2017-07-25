using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace ActiveBackup
{
    public static class Settings
    {
        public static string workingDrive;
        public static string backupFolder;
        public static List<String> WHITELIST = new List<String>();
        public static List<String> WATCH_LIST = new List<String>();
        public static Boolean SAFETY = false;
        public static int HIGH;
        public static int LOW;

        /// <summary>
        /// Checks all the settings and notify the user if some of them are missing or incorrect.
        /// </summary>
        public static void CheckSettings()
        {
            // Check settings
            if (CheckBackupDrive() &&
                CheckWorkingFolder() &&
                CheckEquality() &&          
                CheckConfigFile()
                )
            {
                WATCH_LIST = CheckWatchList();
                WHITELIST = GetWhitelistedFiles();

                CheckWhitelistedFiles();
                Console.WriteLine("Settings are OK! Program is ready to operate.");
            }
        }

        private static List<String> CheckWatchList()
        {
            List<String> watchList = new List<String>();
            string watchlist = Environment.GetEnvironmentVariable("WATCH_THIS", EnvironmentVariableTarget.Machine);
            foreach (string folder in watchlist.Split(",".ToCharArray()[0]))
            {
                watchList.Add(folder);
                Console.WriteLine("Watching folder: " + folder);
            }

            return watchList;
        }

        /// <summary>
        /// Check the backup folder. Return true if settings if ok, otherwise stops the program.
        /// </summary>
        private static Boolean CheckBackupDrive()
        {
            backupFolder = Environment.GetEnvironmentVariable("BACKUP", EnvironmentVariableTarget.Machine);
            if (backupFolder == String.Empty || !Directory.Exists(backupFolder))
            {
                Console.WriteLine("Backup folder doesn't exist. Please get in touch with an administrator.");

                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
                Environment.Exit(0);
            }

            Console.WriteLine("BACKUP FOLDER: " + backupFolder + " ( " + DiskManager.GetDiskUsage(backupFolder) + "% Free )");
            return true;
        }

        /// <summary>
        /// Check the working drive. Return true if settings if ok, otherwise stops the program.
        /// </summary>
        private static Boolean CheckWorkingFolder()
        {
            workingDrive = Environment.GetEnvironmentVariable("WORKING", EnvironmentVariableTarget.Machine);
            if (workingDrive == String.Empty || !Directory.Exists(workingDrive))
            {
                Console.WriteLine("Working drive doesn't exist. Please get in touch with an administrator.");

                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
                Environment.Exit(0);
            }

            Console.WriteLine("WORKING DRIVE: " + workingDrive + " ( " + DiskManager.GetDiskUsage(workingDrive) + "% Free )");
            return true;
        }

        /// <summary>
        /// Check if the backup folder is the same as the woring drive. Return true if different, exit the program if same
        /// </summary>
        private static Boolean CheckEquality()
        {
            if (workingDrive == backupFolder)
            {
                Console.WriteLine("Working drive can't be the same as the backup folder.  Please get in touch with an administrator.");

                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
                Environment.Exit(0);
            }

            return true;
        }

        /// <summary>
        /// Returns a String List with the whitelisted files.
        /// </summary>
        public static List<String> GetWhitelistedFiles()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(ReadXMLFile());

            List<String> files = new List<String>();
            XmlNodeList fileNodes = doc.DocumentElement.SelectNodes("/whitelist/file");

            foreach (XmlNode fileNode in fileNodes)
            {
                files.Add(fileNode.InnerText);
            }

            return files;
        }

        /// <summary>
        /// Reads the whitelist XML file and returns a string containing the whole file. Notify the use if can't find the file.
        /// </summary>
        private static String ReadXMLFile()
        {
            string xmlFile = Directory.GetCurrentDirectory() + @"\whitelist.xml";
            if (File.Exists(xmlFile))
            {
                return File.ReadAllText(xmlFile);
            }

            Console.WriteLine("ERROR: Whitelist.XML not found. Whitelisted files may be in danger.");
            return "";
        }

        /// <summary>
        /// Checks all the whitelisted files and tells the user if one of them is missing. Return true if all of them are exist.
        /// </summary>
        private static Boolean CheckWhitelistedFiles()
        {
            Console.WriteLine("\n");
            Boolean flag = true;
            foreach (string file in GetWhitelistedFiles())
            {
                if (!File.Exists(file))
                {
                    flag = false;
                    Console.WriteLine("WARNING: Whitelisted file " + file + " does not exist.");
                }
            }

            return flag;
        }

        /// <summary>
        /// Reads the config file and sets the static variables to the stored values. Returns true if it was success.
        /// </summary>
        public static Boolean CheckConfigFile()
        {
            Boolean flag = false;

            if (ConfigurationManager.AppSettings["safety"] != null)
            {
                SAFETY = Convert.ToBoolean(ConfigurationManager.AppSettings["safety"]);
            }
            else
            {
                Console.WriteLine("ERROR: Setting 'safety' not found. Please contact an administrator.");
                flag = true;
            }

            if (ConfigurationManager.AppSettings["high"] != null)
            {
                HIGH = Convert.ToInt32(ConfigurationManager.AppSettings["high"]);
            }
            else
            {
                Console.WriteLine("ERROR: Setting 'high' not found. Please contact an administrator.");
                flag = true;
            }

            if (ConfigurationManager.AppSettings["low"] != null)
            {
                LOW = Convert.ToInt32(ConfigurationManager.AppSettings["low"]);
            }
            else
            {
                Console.WriteLine("ERROR: Setting 'low' not found. Please contact an administrator.");
                flag = true;
            }

            if (LOW <= 0 || HIGH >= 100)
            {
                Console.WriteLine("H: " + HIGH + " L: " + LOW);
                Console.WriteLine("ERROR: Invalid value(s) in App.config. Please contact an administrator.");
                flag = true;
            }

            if (flag)
            {
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
                Environment.Exit(0);
            }

            return true;
        }

        public static Boolean IsItWhitelisted(String file)
        {
            if ( WHITELIST.Contains(file) )
            {
                Console.WriteLine(file + " is whitelisted.");
                return true;
            }
            return false;
        }
    }
}
;