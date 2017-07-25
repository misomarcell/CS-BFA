using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;

namespace ActiveBackup
{
    public class DiskManager
    {
        /// <summary>
        /// Returns the free space of the given disk in percentage.
        /// </summary>
        public static decimal GetDiskUsage(string diskLetter)
        {
            DriveInfo drive = new DriveInfo(diskLetter);
            decimal freeSpace = drive.AvailableFreeSpace;
            decimal totalSpace = drive.TotalSize;

            return decimal.Round((freeSpace / totalSpace) * 100, 2, System.MidpointRounding.ToEven);
        }

        /// <summary>
        /// Checks if the working drive reached one of the limit values and starts the copy/move action.
        /// </su
        /// mmary>
        public static void CheckForBackup()
        {
            Debug.WriteLine("Working Drive: " + GetDiskUsage(Settings.workingDrive) + "% Free");
            if ((100 - GetDiskUsage(Settings.workingDrive)) >= Settings.HIGH)
            {
                Console.WriteLine("\nBackupping files...");
                BackupDrive();
            }
        }

        /// <summary>
        /// It starts the backup process, decides if a path is file or folder and calls the BackupFile/Folder method depending.
        /// </summary>
        public static void BackupDrive()
        {
            Log log = new Log();
            log.WriteLog("Starting backup.");

            foreach (string item in GetMixedPaths())
            {
                if ((File.GetAttributes(item) & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    BackupFolder(new DirectoryInfo(item), new DirectoryInfo(Path.Combine(Settings.backupFolder, new DirectoryInfo(item).Name)));
                    log.WriteLog("Backed up folder: " + item);
                }
                else
                {
                    if (!Settings.IsItWhitelisted(item))
                    {
                        BackupFile(item);
                        log.WriteLog("Backed up file: " + item);
                    }
                }

                //Check the drive usage after every file/folder copied and stop the process if it under the low limit.
                if ((100 - GetDiskUsage(Settings.workingDrive)) <= Settings.LOW)
                {
                    break;
                }
            }
            Console.WriteLine("Backup completed! Work - " +
                (100 - GetDiskUsage(Settings.workingDrive)) + "% Free | Backup - " +
                (100 - GetDiskUsage(Settings.backupFolder)) + "% Free.");

            log.WriteLog("Backup completed.");
            if (log.entries.Count > 1 ){ Log.Serialize(log); }
        }

        /// <summary>
        /// Returns a list with file and directory paths on the given drive to backup both of them.
        /// UPDATE: Filters out the non watched folders.
        /// </summary>
        private static List<string> GetMixedPaths()
        {
            List<String> mixedPaths = new List<String>();

            foreach (string watchFolder in Settings.WATCH_LIST)
            {
                foreach (string directory in Directory.GetDirectories(watchFolder))
                {
                    mixedPaths.Add(directory);
                }

                foreach (string file in Directory.GetFiles(watchFolder))
                {
                    mixedPaths.Add(file);
                }
            }
                

            mixedPaths.Sort();
            return mixedPaths;
        }

        /// <summary>
        /// Check if the folder not pre-generated the backups it recursively.
        /// </summary>
        private static void BackupFolder(DirectoryInfo source, DirectoryInfo target)
        {
            //TODO: Beautify this.
            if ( source.Name.StartsWith("$")  || source.Name == "System Volume Information" ) 
            {
                return;
            }

            if (!Directory.Exists(target.FullName))
            {
                Directory.CreateDirectory(target.FullName);
            }

            foreach (string file in Directory.GetFiles(source.FullName))
            {
                string dest = Path.Combine(target.FullName, Path.GetFileName(file));
                File.Copy(file, dest, true);
            }

            foreach (string folder in Directory.GetDirectories(source.FullName))
            {
                string dest = Path.Combine(target.FullName, Path.GetFileName(folder));
                BackupFolder(new DirectoryInfo(folder), new DirectoryInfo(dest));
            }

            if (!Settings.SAFETY)
            {
                Directory.Delete(source.FullName, true);
            }
        }

        private static void BackupFile(string source)
        {
            try
            {
                File.Copy(source, Path.Combine(Settings.backupFolder, Path.GetFileName(source)));
            }
            catch (Exception e)
            {
                Console.WriteLine("Couldn't backup file: " + source + " : " + e.Message);
            }

            if (!Settings.SAFETY)
            {
                File.Delete(source);
            }
        }
    }
}