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

        public static int FileCountIndex = 0; //Count to handle
        public static int TotalFileCount = 0; //Total to handle

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

        public static void Main(string[] args)
        {
            Console.WriteLine("Ttfs2Mix utility 1.0 - by Unstoppable");

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
                        Console.WriteLine("\"convert <Package ID/Package Name>\": Converts TTFS package to MIX file and saves into data folder.");
                        Console.WriteLine("\"convertall\": Converts all TTFS packages to MIX and saves all into data folder.");
                        Console.WriteLine("\"help\": Prints the list of available commands.");
                        Console.WriteLine("\"info\": Shows information about utility and used libraries.");
                        return;

                    case "info":
                        Console.WriteLine("Ttfs2Mix is a application to convert TTFS packages into MIX files made by Unstoppable. Supports working with client and server.");
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
                        Worker = new Thread(ConvertAll);
                        Worker.IsBackground = true;
                        Worker.Start();
                        break;

                    default:
                        Console.WriteLine("You have specified an invalid command. Please run this application with \"help\" parameter for commands.");
                        return;
                }

                while(!ProgressStatisticClass.IsDone)
                {
                    LoadState++;

                    int val = ProgressStatisticClass.FileCountIndex;
                    int max = ProgressStatisticClass.TotalFileCount;

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

                    Console.Write($"{ProgressStatisticClass.CurrentPackage} ({val}/{max}) [{new string('#', PBarVal)}{GenerateSpace(PBarMax - PBarVal)}]");
                    Thread.Sleep(100);

                    ResetCurrentLine();
                    while (ConsoleOutputList.Count > 0)
                    {
                        Console.WriteLine(ConsoleOutputList[0]);
                        ConsoleOutputList.RemoveAt(0);
                    }
                }
            }
            else
            {
                Console.WriteLine("Please run this application with \"help\" parameter for commands.");
                return;
            }
        }

        static string GenerateSpace(int val)
        {
            return new string(' ', val);
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

        internal static void Convert(string Package, TTFSDataClass? TTFSData = null)
        {
            TTFSDataClass TTFS = default;

            if(!Paths.HasValue)
                Paths = Data.ReadPaths();

            if(TTFSFolder == null)
                TTFSFolder = Data.GetTTFSDirectory(Paths.Value);

            if(!TTFSData.HasValue)
            {
                try
                {
                    TTFS = TTFSClass.FromFile(Path.Combine(TTFSFolder, "packages.dat"));
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
                TTFS = TTFSData.Value;
            }

            TPIPackageClass TPI = default;
            if(TTFS.Packages.FindAll(x => PackageIDCheck(x, Package)).Count == 1) //ID Match
            {
                TPI = TTFS.Packages.Find(x => PackageIDCheck(x, Package));
            }
            else if (TTFS.Packages.FindAll(x => PackageNameCheck(x, Package)).Count == 1) //Name Match
            {
                TPI = TTFS.Packages.Find(x => PackageNameCheck(x, Package));
            }
            else if (TTFS.Packages.FindAll(x => PackageIDCheck(x, Package)).Count > 1) //Too many ID match
            {
                ConsoleOutputList.Add($"Too many matches found with specified identifier \"{Package}\".");
                if (ProgressStatisticClass.Mode == 0)
                    ProgressStatisticClass.IsDone = true;
                return;
            }
            else if (TTFS.Packages.FindAll(x => PackageNameCheck(x, Package)).Count > 1) //Too many Name match
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

            ProgressStatisticClass.CurrentPackage = TPI.PackageName;
            ProgressStatisticClass.TotalFileCount = TPI.FileCount;
            ProgressStatisticClass.FileCountIndex = 0;

            MixPackageClass MIXPackage = MixClass.CreateMIX();
            foreach(TTFileClass TTFile in TPI.Files)
            {
                try
                {
                    MIXPackage.Files.Add(new MixFileClass
                    {
                        FileName = TTFile.FileName,
                        Data = File.ReadAllBytes(Path.Combine(TTFSFolder, "files", TTFile.FullName))
                    });
                }
                catch(Exception ex)
                {
                    ConsoleOutputList.Add($"Skipping file {TTFile.FileName} in package {TPI.PackageName}: {ex.Message}.");
                }

                ProgressStatisticClass.FileCountIndex++;
            }

            var SaveLoc = Path.Combine(Data.ExeLocation, "Data", $"{TPI.PackageName}.mix");
            MixClass.Save(MIXPackage, SaveLoc);
            ConsoleOutputList.Add(TPI.PackageName);

            ProgressStatisticClass.MIXIndex++;

            if (ProgressStatisticClass.Mode == 0)
                ProgressStatisticClass.IsDone = true;
        }

        internal static void ConvertAll()
        {
            Paths = Data.ReadPaths();
            TTFSFolder = Data.GetTTFSDirectory(Paths.Value);

            if (TTFSFolder != null)
            {
                TTFSDataClass TTFS;

                try
                {
                    TTFS = TTFSClass.FromFile(Path.Combine(TTFSFolder, "packages.dat"));
                }
                catch(Exception ex)
                {
                    ConsoleOutputList.Add($"Error: {ex.Message}");
                    ProgressStatisticClass.IsDone = true;
                    return;
                }

                ProgressStatisticClass.MIXTotal = TTFS.PackageCount;

                foreach (TPIPackageClass Package in TTFS.Packages)
                {
                    Convert(Package.PackageID, TTFS);
                }
            }
            else
            {
                ConsoleOutputList.Add($"Could not auto-detect TTFS folder. Please make sure you are running this utility from root directory game/server folder.");
            }

            ProgressStatisticClass.IsDone = true;
        }
    }
}
