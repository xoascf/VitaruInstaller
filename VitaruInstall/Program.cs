using System;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SQLite.Net;
using SQLite.Net.Platform.Win32;
using SQLite.Net.Attributes;

namespace VitaruInstall
{
    class Program
    {
        private const string vitaruDllName = @"osu.Game.Rulesets.Vitaru.dll";
        private const string vitaruVersion = "0.4.1";
        private const string databaseName = "client.db";
        static void Main(string[] args)
        {
            string databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "osu");
            string lazerPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "osulazer");
            if (Directory.Exists(lazerPath) && Directory.GetDirectories(lazerPath).Length > 0)
            {
                List<string> wew = new List<string>();
                wew.AddRange(Directory.GetDirectories(lazerPath));
                List<int> yearOfOsuVersion = new List<int>();
                List<int> monthDateOfOsuVersion = new List<int>();
                List<int> discrimOfOsuVersion = new List<int>();
                for (int i = 0; i < wew.Count; i++)
                {
                    string s = wew[i].Split('\\')[wew[i].Split('\\').Length - 1];
                    if (s.StartsWith("app-") && int.TryParse(s.Split('-')[1].Split('.')[0], out int result))
                        yearOfOsuVersion.Add(result);
                }
                for (int i = 0; i < wew.Count; i++)
                {
                    string s = wew[i].Split('\\')[wew[i].Split('\\').Length - 1];
                    if (s.StartsWith("app-") && int.TryParse(s.Split('-')[1].Split('.')[1], out int result))
                        monthDateOfOsuVersion.Add(result);
                }
                for (int i = 0; i < wew.Count; i++)
                {
                    string s = wew[i].Split('\\')[wew[i].Split('\\').Length - 1];
                    if (s.StartsWith("app-") && int.TryParse(s.Split('-')[1].Split('.')[2], out int result))
                        discrimOfOsuVersion.Add(result);
                }
                int year = 0;
                int monthDate = 0;
                int discrim = 0;
                for (int i = 0; i < yearOfOsuVersion.Count; i++)
                {
                    if (yearOfOsuVersion[i] >= year)
                    {
                        year = yearOfOsuVersion[i];
                        if (monthDateOfOsuVersion[i] >= monthDate)
                        {
                            monthDate = monthDateOfOsuVersion[i];
                            discrim = 0;
                            if (discrimOfOsuVersion[i] > discrim)
                                discrim = discrimOfOsuVersion[i];
                        }
                    }
                }
                if (year > 2015 && monthDate > 100 && discrim >= 0 && Directory.Exists(databasePath) && File.Exists(Path.Combine(databasePath, databaseName)))
                {
                    string osuVer = "app-" + year + "." + monthDate + "." + discrim;
                    if (File.Exists(Path.Combine(lazerPath, osuVer, vitaruDllName)))
                    {
                        string what = "";
                        do
                        {
                            what = "";
                            Console.WriteLine("What do you want me to do!?");
                            Console.WriteLine("U: Uninstall");
                            Console.WriteLine("R: Reinstall");
                            what = Console.ReadLine();
                        } while (!(what == "U" || what == "R"));
                        if (what == "U")
                            Uninstall(lazerPath, osuVer);
                        else
                            Install(lazerPath, osuVer);
                    }
                    else
                    {
                        Install(lazerPath, osuVer);
                    }
                }
                else
                {
                    Console.WriteLine("It seems you don't have osu!lazer installed, please go do that before installing vitaru");
                    Console.ReadKey();
                }
            }
            else
            {
                Console.WriteLine("It seems you don't have osu!lazer installed, please go do that before installing vitaru");
                Console.ReadKey();
            }
        }

        static void Install(string lazerPath, string osuVer)
        {
            string vitaruPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "vitaru");
            string vitaruFile = Path.Combine(vitaruPath, vitaruDllName);
            string focusPath = Path.Combine(lazerPath, osuVer, vitaruDllName);
            if (!(Process.GetProcessesByName("osu!").FirstOrDefault(p => p.MainModule.FileName.StartsWith(lazerPath)) != default(Process)))
            {
                Console.WriteLine("Downloading latest vitaru...");
                if (!Directory.Exists(vitaruPath))
                    Directory.CreateDirectory(vitaruPath);
                if (File.Exists(vitaruFile))
                    File.Delete(vitaruFile);
                string dllLink = "https://github.com/Symcol/osu/releases/download/" + vitaruVersion + "/osu.Game.Rulesets.Vitaru.dll";
                using (WebClient client = new WebClient())
                    client.DownloadFile(dllLink, vitaruFile);
                Console.WriteLine("Succesfully downloaded vitaru ver {0}!", vitaruVersion);
                Console.WriteLine("Installing to latest osu!...");
                Console.WriteLine("Installing to version {0}", osuVer);
                if (File.Exists(focusPath))
                {
                    File.Delete(focusPath);
                    File.Copy(vitaruFile, focusPath);
                }
                else
                    File.Copy(vitaruFile, focusPath);
                Console.WriteLine("Adding to database");
                SQLiteConnection connection = new SQLiteConnection(new SQLitePlatformWin32($@"{Environment.CurrentDirectory}/{(IntPtr.Size == 8 ? "x64" : "x86")}"), Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "osu", databaseName));
                connection.BeginTransaction();
                connection.InsertOrReplace(new RulesetInfo());
                connection.Close();
                Console.WriteLine("Succesfully installed Vitaru to osu!lazer version {0}", osuVer);
            }
            else
                Console.WriteLine("It seems osu!lazer is running! Close it to install/reinstall vitaru!");
            Console.ReadKey();
        }
        static void Uninstall(string lazerPath, string osuVer)
        {
            string vitaruDllPath = Path.Combine(lazerPath, osuVer, vitaruDllName);
            Console.WriteLine("Will uninstall Vitaru from osu!lazer version {0}", osuVer);
            if (File.Exists(vitaruDllPath))
            {
                if (!(Process.GetProcessesByName("osu!").FirstOrDefault(p => p.MainModule.FileName.StartsWith(lazerPath)) != default(Process)))
                {
                    File.Delete(vitaruDllPath);
                    Console.WriteLine("Uninstalled Vitaru, hope you liked it :)");
                }
                else
                    Console.WriteLine("It seems osu!lazer is running! Close it to uninstall vitaru");
            }
            else
                Console.WriteLine("The dll was deleted before I could do something...");
            Console.ReadKey();
        }
    }

    public class RulesetInfo
    {
        [PrimaryKey, AutoIncrement]
        public int? ID { get; set; }

        [Indexed(Unique = true)]
        public string Name => "vitaru!";

        [Indexed(Unique = true)]
        public string InstantiationInfo => "osu.Game.Rulesets.Vitaru.VitaruRuleset, osu.Game.Rulesets.Vitaru, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";

        [Indexed]
        public bool Available => true;
    }
}
