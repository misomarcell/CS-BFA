using System;

namespace ActiveBackup
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Settings.CheckSettings();

            PeriodicTask.Run(DiskManager.CheckForBackup, new TimeSpan(0, 0, 1));
            Console.ReadKey();
        }
    }
}