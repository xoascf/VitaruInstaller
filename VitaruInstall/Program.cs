using System;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SQLite.Net;
using SQLite.Net.Platform.Win32;
using SQLite.Net.Attributes;

namespace RulesetInstall
{
    class Program
    {
        /// <summary>
        /// In order to change this to do your gamemode, follow our structure and add a release on github. Then, change RulesetName, RulesetDllName, RulesetVersion and RulesetInstantiationInfo to match your gamemode's information.
        /// </summary>
        public const string RulesetName = "vitaru";
        public const string RulesetDllName = @"osu.Game.Rulesets.Vitaru.dll";
        public const string RulesetVersion = "0.4.0";
        public const string GithubName = "Symcol";
        public const string RulesetInstantiationInfo = "osu.Game.Rulesets.Vitaru.VitaruRuleset, osu.Game.Rulesets.Vitaru, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";

        public const string DatabaseName = "client.db";

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

                if (year > 2015 && monthDate > 100 && discrim >= 0 && Directory.Exists(databasePath) && File.Exists(Path.Combine(databasePath, DatabaseName)))
                {
                    string osuVer = "app-" + year + "." + monthDate + "." + discrim;
                    if (File.Exists(Path.Combine(lazerPath, osuVer, RulesetDllName)))
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
                    Console.WriteLine("It seems you don't have osu!lazer installed, please go do that before installing " + RulesetName);
                    Console.ReadKey();
                }
            }

            else
            {
                Console.WriteLine("It seems you don't have osu!lazer installed, please go do that before installing " + RulesetName);
                Console.ReadKey();
            }
        }

        static void Install(string lazerPath, string osuVer)
        {
            string vitaruPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), RulesetName);
            string rulesetFile = Path.Combine(vitaruPath, RulesetDllName);
            string focusPath = Path.Combine(lazerPath, osuVer, RulesetDllName);

            if (!(Process.GetProcessesByName("osu!").FirstOrDefault(p => p.MainModule.FileName.StartsWith(lazerPath)) != default(Process)))
            {
                Console.WriteLine("Downloading latest " + RulesetName + "...");

                if (!Directory.Exists(vitaruPath))
                    Directory.CreateDirectory(vitaruPath);
                if (File.Exists(rulesetFile))
                    File.Delete(rulesetFile);

                string dllLink = "https://github.com/" + GithubName + "/osu/releases/download/" + RulesetVersion + "/" + RulesetDllName;
                using (WebClient client = new WebClient())
                {
                    try
                    {
                        client.DownloadFile(dllLink, rulesetFile);
                    }
                    catch
                    {
                        Console.WriteLine("Failed to downloaded " + RulesetName + " ver {0}!", RulesetVersion);
                        throw new Exception();
                    }
                }
                    
                Console.WriteLine("Succesfully downloaded " + RulesetName + " ver {0}!", RulesetVersion);
                Console.WriteLine("Installing to latest osu!...");
                Console.WriteLine("Installing to version {0}", osuVer);

                if (File.Exists(focusPath))
                {
                    File.Delete(focusPath);
                    File.Copy(rulesetFile, focusPath);
                }
                else
                    File.Copy(rulesetFile, focusPath);

                Console.WriteLine("Adding to database");
                SQLiteConnection connection = new SQLiteConnection(new SQLitePlatformWin32($@"{Environment.CurrentDirectory}/{(IntPtr.Size == 8 ? "x64" : "x86")}"), Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "osu", DatabaseName));
                connection.BeginTransaction();
                connection.InsertOrReplace(new RulesetInfo());
                connection.Close();
                Console.WriteLine("Succesfully installed " + RulesetName + " to osu!lazer version {0}", osuVer);
            }

            else
                Console.WriteLine("It seems osu!lazer is running! Close it to install/reinstall " + RulesetName + "!");
            Console.ReadKey();
        }

        static void Uninstall(string lazerPath, string osuVer)
        {
            string vitaruDllPath = Path.Combine(lazerPath, osuVer, RulesetDllName);
            Console.WriteLine("Will uninstall " + RulesetName + " from osu!lazer version {0}", osuVer);
            if (File.Exists(vitaruDllPath))
            {
                if (!(Process.GetProcessesByName("osu!").FirstOrDefault(p => p.MainModule.FileName.StartsWith(lazerPath)) != default(Process)))
                {
                    File.Delete(vitaruDllPath);
                    Console.WriteLine("Uninstalled " + RulesetName + ", hope you liked it :)");
                }
                else
                    Console.WriteLine("It seems osu!lazer is running! Close it to uninstall " + RulesetName);
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
        public string Name => Program.RulesetName;

        [Indexed(Unique = true)]
        public string InstantiationInfo => Program.RulesetInstantiationInfo;

        [Indexed]
        public bool Available => true;
    }
}
