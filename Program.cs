/*
    ttfs2mix TTFS to MIX utility
    Copyright (C) 2020 Unstoppable

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
*/


using MixLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TTPackageClass;

namespace Ttfs2Mix
{
    static class ProgressStatisticClass
    {
        public static int MIXIndex = 0; //Count to handle
        public static int MIXTotal = 0; //Total to handle

        public static ulong FileCountIndex = 0; //Count to handle
        public static ulong TotalFileCount = 0; //Total to handle

        public static string StatusText = string.Empty;

        public static string CurrentPackage = string.Empty;
        public static bool IsDone = false;
        public static int Mode = -1;
    }

    public static class Program
    {
        //Values for fancy stuff

        static int LoadState = 0; // the spinning line
        static int PBarMax = 20;
        static int PBarVal = 0;

        //Application

        private static Thread Worker;
        internal static List<string> ConsoleOutputList = new List<string>();

        //Version
        public static readonly string Version = "1.2";

        public static void Main(string[] args)
        {
            Console.WriteLine($"Ttfs2Mix utility {Version} - by Unstoppable");

            EmbeddedAssembly.Load("Ttfs2Mix.MixLibrary.dll", "MixLibrary.dll");
            EmbeddedAssembly.Load("Ttfs2Mix.TTPackageClass.dll", "TTPackageClass.dll");

            //Load dependencies from embedded resource, to make it a single executable file.
            AppDomain.CurrentDomain.AssemblyResolve += (sender, e) =>
            {
                return EmbeddedAssembly.Get(e.Name);
            };

            if (args.Length >= 1)
            {
                switch(args[0].ToLower(Data.DefaultCulture))
                {
                    case "help":
                        Console.WriteLine("\"convert <Package ID/Package Name>\": Converts first occurence of TTFS package to MIX file and saves into data folder.");
                        Console.WriteLine("\"multiconvert <Package ID/Package Name>\": Converts all matched TTFS packages to MIX files and saves into data folder.");
                        Console.WriteLine("\"convertall\": Converts all TTFS packages to MIX and saves all into data folder.");
                        Console.WriteLine("\"download <Package ID/Package Name> <URL>\": Finds and downloads first occurence of TTFS package from a remote repository to MIX file and saves into data folder.");
                        Console.WriteLine("\"multidownload <Package ID/Package Name> <URL>\": Finds and downloads all matched TTFS packages from a remote repository to MIX files and saves into data folder.");
                        Console.WriteLine("\"downloadall <URL>\": Finds and downloads all TTFS packages from a remote repository to MIX files and saves all into data folder.");
                        Console.WriteLine("\"help\": Prints the list of available commands.");
                        Console.WriteLine("\"info\": Shows information about utility and used libraries.");
                        return;

                    case "info":
                        Console.WriteLine("Ttfs2Mix is an application to convert TTFS packages into MIX files made by Unstoppable. Supports working with client and FDS.");
                        Console.WriteLine("Ttfs2Mix uses MixLibrary 1.0 by Unstoppable");
                        Console.WriteLine("Ttfs2Mix uses TTPackageClass 1.0 by Unstoppable");
                        Console.WriteLine("Ttfs2Mix uses EmbeddedAssembly.cs class from https://www.codeproject.com/Articles/528178/Load-DLL-From-Embedded-Resource");
                        return;

                    case "convert":
                        ProgressStatisticClass.MIXTotal = 1;
                        ProgressStatisticClass.Mode = 0;
                        Worker = new Thread(() => Convert(string.Join(" ", args.Skip(1))));
                        Worker.IsBackground = true;
                        Worker.Start();
                        break;

                    case "convertall":
                        ProgressStatisticClass.Mode = 1;
                        Worker = new Thread(() => ConvertAll());
                        Worker.IsBackground = true;
                        Worker.Start();
                        break;

                    case "multiconvert":
                        ProgressStatisticClass.Mode = 2;
                        Worker = new Thread(() => MultiConvert(string.Join(" ", args.Skip(1))));
                        Worker.IsBackground = true;
                        Worker.Start();
                        break;

                    case "download":
                        ProgressStatisticClass.MIXTotal = 1;
                        ProgressStatisticClass.Mode = 3;
                        Worker = new Thread(async () => await Download(string.Join(" ", args.Skip(1).Take(args.Length - 2)), args.Last()));
                        Worker.IsBackground = true;
                        Worker.Start();
                        break;

                    case "multidownload":
                        ProgressStatisticClass.Mode = 4;
                        Worker = new Thread(async () => await MultiDownload(string.Join(" ", args.Skip(1).Take(args.Length - 2)), args.Last()));
                        Worker.IsBackground = true;
                        Worker.Start();
                        break;

                    case "downloadall":
                        ProgressStatisticClass.Mode = 5;
                        Worker = new Thread(async () => await DownloadAll(string.Join(" ", args.Skip(1))));
                        Worker.IsBackground = true;
                        Worker.Start();
                        break;

                    default:
                        Console.WriteLine("You have specified an invalid command. Please run this application with \"help\" parameter for commands.");
                        return;
                }

                Console.CursorVisible = false;
                while(!ProgressStatisticClass.IsDone)
                {
                    LoadState++;

                    ulong val = ProgressStatisticClass.FileCountIndex;
                    ulong max = ProgressStatisticClass.TotalFileCount;

                    if (max > 0)
                        PBarVal = (int)Math.Floor(((double)val / (double)max) * PBarMax);
                    else
                        PBarVal = 0;

                    switch(LoadState)
                    {
                        case 0:
                            Console.Write("/ ");
                            break;
                        case 1:
                            Console.Write("| ");
                            break;
                        case 2:
                            Console.Write("\\ ");
                            break;
                        case 3:
                            Console.Write("- ");
                            LoadState = -1;
                            break;
                    }

                    Console.Write($"{ProgressStatisticClass.CurrentPackage} ({(!string.IsNullOrEmpty(ProgressStatisticClass.StatusText) ? ProgressStatisticClass.StatusText : $"{val}/{max}")}) [{new string('#', PBarVal)}{GenerateSpace(PBarMax - PBarVal)}]");
                    Thread.Sleep(100);

                    ResetCurrentLine();
                    while (ConsoleOutputList.Count > 0)
                    {
                        Console.WriteLine(ConsoleOutputList[0]);
                        ConsoleOutputList.RemoveAt(0);
                    }
                }
                Console.CursorVisible = true;

                Console.WriteLine($"Conversion done{(ProgressStatisticClass.MIXTotal - ProgressStatisticClass.MIXIndex > 0 ? $" with {ProgressStatisticClass.MIXTotal - ProgressStatisticClass.MIXIndex} failed packages." : ".")}");
            }
            else
            {
                Console.WriteLine("Please run this application with \"help\" parameter for commands.");
                return;
            }
        }

        #region Helpers
        static string GenerateSpace(int val)
        {
            return new string(' ', val < 0 ? 0 : val);
        }
        
        static void ResetCurrentLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }

        //TTFS => MIX specific code.

        static PathsStruct? Paths;
        static string TTFSFolder;

        private static bool PackageIDCheck(TPIPackageClass Package, string ID)
        {
            return Package.PackageID
                          .ToLower(Data.DefaultCulture)
                          .Equals(ID.ToLower(Data.DefaultCulture));
        }

        private static bool PackageNameCheck(TPIPackageClass Package, string Name)
        {
            return Package.PackageName
                          .Equals(Name);
        }

        private static void CheckFields(ref TTFSDataClass? TTFSData)
        {
            TTFSData = default;

            if (!Paths.HasValue)
                Paths = Data.ReadPaths();

            if (TTFSFolder == null)
                TTFSFolder = Data.GetTTFSDirectory(Paths.Value);

            if (!TTFSData.HasValue)
            {
                if (TTFSFolder != null)
                {
                    try
                    {
                        TTFSData = TTFSClass.FromFile(Path.Combine(TTFSFolder, "packages.dat"));
                    }
                    catch (Exception ex)
                    {
                        ConsoleOutputList.Add($"Error: {ex.Message}");
                        ProgressStatisticClass.IsDone = true;
                        return;
                    }
                }
                else
                {
                    ConsoleOutputList.Add($"Could not auto-detect TTFS folder. Please make sure you are running this utility from root directory game/server folder.");
                    ProgressStatisticClass.IsDone = true;
                }
            }
            else
            {
                TTFSData = TTFSData.Value;
            }
        }

        private static int ExceptionToHTTPCode(WebException ex)
        {
            try
            {
                string Number = ex.Message;
                Number = Number.Remove(0, Number.Length - 4);
                Number = Number.Substring(Number.Length - 1, 1);
                return int.Parse(Number);
            }
            catch
            {
                return -1;
            }
        }

        private static void PrintHTTPErrorInfo(int Status)
        {
            switch (Status)
            {
                case 401: //Forbidden
                    ConsoleOutputList.Add($"Authorization required to view this location. Please make sure server allows ttfs2mix or you have permission to access.");
                    break;
                case 404: //Not found
                    ConsoleOutputList.Add($"Unable to find a TTFS repository at the specified location. Please make sure entered URL is correct.");
                    break;
                case 403: //Forbidden
                    ConsoleOutputList.Add($"Server forbid access to this location. Please make sure server allows ttfs2mix or you have permission to access.");
                    break;
                case 500: //Server error
                    ConsoleOutputList.Add($"An internal server error occured while requesting TTFS packages. Please try again later.");
                    break;
                default:
                    ConsoleOutputList.Add($"Server sent an unrecognized status code of {Status}. Please make sure server allows ttfs2mix, or try again later.");
                    break;
            }
        }
        #endregion

        #region Commands
        internal static void Convert(string Package, TTFSDataClass? TTFSData = null)
        {
            CheckFields(ref TTFSData);

            ProgressStatisticClass.StatusText = "Locating...";

            TTFSDataClass TTFS;

            if (TTFSData.HasValue)
            {
                TTFS = TTFSData.Value;
            }
            else
            {
                if (ProgressStatisticClass.Mode == 0)
                    ProgressStatisticClass.IsDone = true;

                return;
            }

            TPIPackageClass TPI = default;
            var IDMatch = TTFS.Packages.FindAll(x => PackageIDCheck(x, Package));
            var NameMatch = TTFS.Packages.FindAll(x => PackageNameCheck(x, Package));

            if (IDMatch.Count == 1) //ID Match
            {
                TPI = IDMatch.First();
            }
            else if (NameMatch.Count == 1) //Name Match
            {
                TPI = NameMatch.First();
            }
            else if (IDMatch.Count > 1) //Too many ID match
            {
                ConsoleOutputList.Add($"Too many matches found with specified identifier \"{Package}\".");
                if (ProgressStatisticClass.Mode == 0)
                    ProgressStatisticClass.IsDone = true;
                return;
            }
            else if (NameMatch.Count > 1) //Too many Name match
            {
                ConsoleOutputList.Add($"Too many matches found with specified identifier \"{Package}\".");
                if (ProgressStatisticClass.Mode == 0)
                    ProgressStatisticClass.IsDone = true;
                return;
            }
            else
            {
                ConsoleOutputList.Add($"Couldn't find any package with specified identifier \"{Package}\".");
                if (ProgressStatisticClass.Mode == 0)
                    ProgressStatisticClass.IsDone = true;
                return;
            }

            ProgressStatisticClass.CurrentPackage = $"{TPI.PackageName} ({TPI.PackageID})";
            ProgressStatisticClass.TotalFileCount = (ulong)TPI.FileCount;
            ProgressStatisticClass.FileCountIndex = 0;

            ProgressStatisticClass.StatusText = string.Empty;

            MixPackageClass MIXPackage = MixPackageClass.CreateMIX();
            foreach(TTFileClass TTFile in TPI.Files)
            {
                try
                {
                    MIXPackage.Files.Add(new MixFileClass
                    {
                        FileName = TTFile.FileName,
                        Data = File.ReadAllBytes(Path.Combine(TTFSFolder, "files", TTFile.FullName.Replace("\\", "_")))
                    });
                }
                catch(Exception ex)
                {
                    ConsoleOutputList.Add($"Skipping file {TTFile.FileName} in package {TPI.PackageName}: {ex.Message}.");
                }

                ProgressStatisticClass.FileCountIndex++;
            }

            ProgressStatisticClass.StatusText = "Saving...";

            var SaveLoc = Path.Combine(Data.ExeLocation, "Data", $"{TPI.PackageName}.mix");
            MIXPackage.Save(SaveLoc);
            ConsoleOutputList.Add($"+ {TPI.PackageName} ({TPI.PackageID})");

            ProgressStatisticClass.MIXIndex++;

            if (ProgressStatisticClass.Mode == 0)
                ProgressStatisticClass.IsDone = true;
        }

        internal static void ConvertAll(TTFSDataClass? TTFSData = null)
        {
            CheckFields(ref TTFSData);

            TTFSDataClass TTFS;

            if (TTFSData.HasValue)
            {
                TTFS = TTFSData.Value;
            }
            else
            {
                ProgressStatisticClass.IsDone = true;
                return;
            }

            if (!ProgressStatisticClass.IsDone)
            {
                ProgressStatisticClass.MIXTotal = TTFS.PackageCount;

                foreach (TPIPackageClass Package in TTFS.Packages)
                {
                    Convert(Package.PackageID, TTFS);
                }
            }

            ProgressStatisticClass.IsDone = true;
        }

        internal static void MultiConvert(string Package, TTFSDataClass? TTFSData = null)
        {
            CheckFields(ref TTFSData);

            TTFSDataClass TTFS;

            if (TTFSData.HasValue)
            {
                TTFS = TTFSData.Value;
            }
            else
            {
                ProgressStatisticClass.IsDone = true;
                return;
            }

            if (!ProgressStatisticClass.IsDone)
            {
                var Matches = TTFS.Packages.Where(x => x.PackageName.ToLower(Data.DefaultCulture).Contains(Package.ToLower(Data.DefaultCulture)) ||
                                                                               x.PackageID.ToLower(Data.DefaultCulture).Contains(Package.ToLower(Data.DefaultCulture)));

                ProgressStatisticClass.MIXTotal = Matches.Count();

                if(Matches.Count() < 1)
                {
                    ConsoleOutputList.Add($"Couldn't find any package with specified identifier \"{Package}\".");
                    ProgressStatisticClass.IsDone = true;

                    return;
                }

                foreach (TPIPackageClass TTPackage in Matches)
                {
                    Convert(TTPackage.PackageID, TTFS);
                }
            }

            ProgressStatisticClass.IsDone = true;
        }

        internal static async Task Download(string Package, string Location, TTFSDataClass? TTFSData = null)
        {
            ProgressStatisticClass.StatusText = "Fetching packages...";

            if (TTFSData == null)
            {
                try
                {
                    byte[] Data = await WebDownloader.GetBytesAsync($"{Location}/packages.dat");
                    TTFSData = TTFSClass.FromBytes(Data);
                }
                catch (WebException webex)
                {
                    int Status = ExceptionToHTTPCode(webex);

                    if (Status > 0)
                    {
                        PrintHTTPErrorInfo(Status);
                    }

                    return;
                }
                catch (Exception ex)
                {
                    ConsoleOutputList.Add($"Error: {ex.Message}");
                    ProgressStatisticClass.IsDone = true;
                    return;
                }
            }

            TTFSDataClass TTFS = TTFSData.Value;

            TPIPackageClass TPI = default;
            var IDMatch = TTFS.Packages.FindAll(x => PackageIDCheck(x, Package));
            var NameMatch = TTFS.Packages.FindAll(x => PackageNameCheck(x, Package));

            if (IDMatch.Count == 1) //ID Match
            {
                TPI = IDMatch.First();
            }
            else if (NameMatch.Count == 1) //Name Match
            {
                TPI = NameMatch.First();
            }
            else if (IDMatch.Count > 1) //Too many ID match
            {
                ConsoleOutputList.Add($"Too many matches found with specified identifier \"{Package}\".");
                if (ProgressStatisticClass.Mode == 3)
                    ProgressStatisticClass.IsDone = true;
                return;
            }
            else if (NameMatch.Count > 1) //Too many Name match
            {
                ConsoleOutputList.Add($"Too many matches found with specified identifier \"{Package}\".");
                if (ProgressStatisticClass.Mode == 3)
                    ProgressStatisticClass.IsDone = true;
                return;
            }
            else
            {
                ConsoleOutputList.Add($"Couldn't find any package with specified identifier \"{Package}\".");
                if (ProgressStatisticClass.Mode == 3)
                    ProgressStatisticClass.IsDone = true;
                return;
            }

            ProgressStatisticClass.CurrentPackage = $"{TPI.PackageName} ({TPI.PackageID})";
            ProgressStatisticClass.TotalFileCount = (ulong)TPI.Files.Sum(x => x.FileSize);
            ProgressStatisticClass.FileCountIndex = 0;

            ProgressStatisticClass.StatusText = $"Downloading...";

            int fileindex = 0;
            int totalfiles = TPI.FileCount;

            MixPackageClass MIXPackage = MixPackageClass.CreateMIX();
            foreach(TTFileClass File in TPI.Files)
            {
                byte[] Data = new byte[File.FileSize];

                try
                {
                    Data = await WebDownloader.GetBytesAsync($"{Location}/files/{Uri.EscapeUriString($"{File.CRC}.{File.FileName}")}");

                    MIXPackage.Files.Add(new MixFileClass
                    {
                        FileName = File.FileName,
                        Data = Data
                    });

                    ProgressStatisticClass.FileCountIndex += (ulong)Data.Length;
                }
                catch(Exception ex)
                {
                    if(ex is WebException)
                    {
                        int Status = ExceptionToHTTPCode((WebException)ex);
                        ConsoleOutputList.Add($"Skipping file {File.FileName} in package {TPI.PackageName}: Server returns {Status}.");
                    }
                    else
                    {
                        ConsoleOutputList.Add($"Skipping file {File.FileName} in package {TPI.PackageName}: {ex.Message}.");
                    }

                    ProgressStatisticClass.FileCountIndex += File.FileSize;
                }

                fileindex++;
                ProgressStatisticClass.StatusText = $"Downloading... ({fileindex}/{totalfiles} | {WebDownloader.ParseSize((long)ProgressStatisticClass.FileCountIndex)}/{WebDownloader.ParseSize((long)ProgressStatisticClass.TotalFileCount)})";
            }

            ProgressStatisticClass.StatusText = "Saving...";

            var SaveLoc = Path.Combine(Data.ExeLocation, "Data", $"{TPI.PackageName}.mix");
            MIXPackage.Save(SaveLoc);
            ConsoleOutputList.Add($"+ {TPI.PackageName} ({TPI.PackageID})");

            ProgressStatisticClass.MIXIndex++;

            GC.Collect();

            if (ProgressStatisticClass.Mode == 3)
                ProgressStatisticClass.IsDone = true;
        }

        internal static async Task MultiDownload(string Package, string Location, TTFSDataClass? TTFSData = null)
        {
            ProgressStatisticClass.StatusText = "Fetching packages...";

            if (TTFSData == null)
            {
                try
                {
                    byte[] Data = await WebDownloader.GetBytesAsync($"{Location}/packages.dat");
                    TTFSData = TTFSClass.FromBytes(Data);
                }
                catch (WebException webex)
                {
                    int Status = ExceptionToHTTPCode(webex);

                    if (Status > 0)
                    {
                        PrintHTTPErrorInfo(Status);
                    }

                    return;
                }
                catch (Exception ex)
                {
                    ConsoleOutputList.Add($"Error: {ex.Message}");
                    ProgressStatisticClass.IsDone = true;
                    return;
                }
            }

            TTFSDataClass TTFS = TTFSData.Value;

            if (!ProgressStatisticClass.IsDone)
            {
                var Matches = TTFS.Packages.Where(x => x.PackageName.ToLower(Data.DefaultCulture).Contains(Package.ToLower(Data.DefaultCulture)) ||
                                                                               x.PackageID.ToLower(Data.DefaultCulture).Contains(Package.ToLower(Data.DefaultCulture)));

                ProgressStatisticClass.MIXTotal = Matches.Count();

                if (Matches.Count() < 1)
                {
                    ConsoleOutputList.Add($"Couldn't find any package with specified identifier \"{Package}\".");
                    ProgressStatisticClass.IsDone = true;

                    return;
                }

                foreach (TPIPackageClass TTPackage in Matches)
                {
                    await Download(TTPackage.PackageID, Location, TTFS);
                }
            }

            ProgressStatisticClass.IsDone = true;
        }

        internal static async Task DownloadAll(string Location, TTFSDataClass? TTFSData = null)
        {
            ProgressStatisticClass.StatusText = "Fetching packages...";

            if (TTFSData == null)
            {
                try
                {
                    byte[] Data = await WebDownloader.GetBytesAsync($"{Location}/packages.dat");
                    TTFSData = TTFSClass.FromBytes(Data);
                }
                catch (WebException webex)
                {
                    int Status = ExceptionToHTTPCode(webex);

                    if (Status > 0)
                    {
                        PrintHTTPErrorInfo(Status);
                    }

                    return;
                }
                catch (Exception ex)
                {
                    ConsoleOutputList.Add($"Error: {ex.Message}");
                    ProgressStatisticClass.IsDone = true;
                    return;
                }
            }

            TTFSDataClass TTFS = TTFSData.Value;

            if (!ProgressStatisticClass.IsDone)
            {
                ProgressStatisticClass.MIXTotal = TTFS.PackageCount;

                foreach (TPIPackageClass Pkg in TTFS.Packages)
                {
                    await Download(Pkg.PackageID, Location, TTFS);
                }
            }

            ProgressStatisticClass.IsDone = true;
        }
        #endregion
    }
}
