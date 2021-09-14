using System;
using System.CommandLine.Invocation;
using System.IO;

namespace BitJuice.Backup
{
    public class CronCommands
    {
        private static readonly string CronFilePath = "/etc/cron.daily/bitjuice-backup";
        private static string BaseDirectory => AppContext.BaseDirectory;
        private static string ExecutableName => AppDomain.CurrentDomain.FriendlyName;

        public static void Install()
        {
            var cron = new FileInfo(CronFilePath);
            using (var writer = new StreamWriter(cron.Create()))
            {
                writer.WriteLine("#!/bin/bash");
                writer.WriteLine();
                writer.WriteLine($"cd {BaseDirectory}");
                writer.WriteLine($"./{ExecutableName} execute");
            }

            Process.StartProcess("chmod", $"+x {cron.FullName}");
        }

        public static void Uninstall()
        {
            File.Delete(CronFilePath);
        }
    }
}
