using System.CommandLine.Invocation;
using System.IO;
using System.Reflection;

namespace BitJuice.Backup
{
    public class CronCommands
    {
        private static string CronFilePath = "/etc/cron.daily/bitjuice-backup";

        public static void Install()
        {
            var path = new FileInfo(Assembly.GetExecutingAssembly().Location);
            var dir = path.Directory?.FullName;

            var cron = new FileInfo(CronFilePath);
            using (var writer = new StreamWriter(cron.Create()))
            {
                writer.WriteLine("#!/bin/bash");
                writer.WriteLine();
                writer.WriteLine($"cd {dir}");
                writer.WriteLine("./BitJuice.Backup execute");
            }

            Process.StartProcess("chmod", $"+x {cron.FullName}");
        }

        public static void Uninstall()
        {
            File.Delete(CronFilePath);
        }
    }
}
