/*
    ttfs2mix TTFS to MIX utility
    Copyright (C) 2020 Unstoppable

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
*/


using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ttfs2Mix
{
    public struct PathsStruct
    {
        public string FileBase { get; internal set; }
        public string FileClient { get; internal set; }
        public string FileFDS { get; internal set; }
        public bool UseRenFolder { get; internal set; }
    }

    internal static class Data
    {
        public static CultureInfo DefaultCulture // To fix upper-lower wrong character issues.
        {
            get => CultureInfo.InvariantCulture;
        }

        public static string ExeLocation
        {
            get => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        public static PathsStruct ReadPaths()
        {
            //For paths
            string fBase = "Renegade";
            string fClient = "Client";
            string fFDS = "FDS";
            bool useRenFolder = false;

            if (File.Exists(Path.Combine(ExeLocation, "data", "paths.ini")))
            {
                using (StreamReader fs = File.OpenText(Path.Combine(ExeLocation, "data", "paths.ini")))
                {
                    bool isValidPaths = false;

                    while (!fs.EndOfStream)
                    {
                        string buf = fs.ReadLine();
                        if (buf.ToLower(DefaultCulture).Equals("[paths]"))
                        {
                            isValidPaths = true;
                        }
                        else if(isValidPaths)
                        {
                            string Key = buf.Substring(0, buf.IndexOf('='));
                            string Value = buf.Remove(0, Key.Length + 1);

                            switch (Key.ToLower(DefaultCulture))
                            {
                                case "filebase":
                                    fBase = Value;
                                    break;
                                case "fileclient":
                                    fClient = Value;
                                    break;
                                case "filefds":
                                    fFDS = Value;
                                    break;
                                case "userenfolder":
                                    useRenFolder = Value == "true" || Value == "1";
                                    break;
                            }
                        }
                    }
                }
            }

            return new PathsStruct
            {
                FileBase = fBase,
                FileClient = fClient,
                FileFDS = fFDS,
                UseRenFolder = useRenFolder
            };
        }

        public static string GetTTFSDirectory(PathsStruct Paths)
        {
            try
            {
                if (Paths.UseRenFolder)
                {
                    if (Directory.Exists(Path.Combine(ExeLocation, Paths.FileBase, Paths.FileClient))) //Client
                    {
                        return Path.Combine(ExeLocation, Paths.FileBase, Paths.FileClient, "ttfs");
                    }
                    else if (Directory.Exists(Path.Combine(ExeLocation, Paths.FileBase, Paths.FileFDS))) //Server
                    {
                        return Path.Combine(ExeLocation, Paths.FileBase, Paths.FileFDS, "ttfs");
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    var AppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

                    if (Directory.Exists(Path.Combine(AppData, Paths.FileBase, Paths.FileClient))) //Client
                    {
                        return Path.Combine(AppData, Paths.FileBase, Paths.FileClient, "ttfs");
                    }
                    else if (Directory.Exists(Path.Combine(ExeLocation, Paths.FileBase, Paths.FileFDS))) //This case will be always server.
                    {
                        return Path.Combine(ExeLocation, Paths.FileBase, Paths.FileFDS, "ttfs");
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch(Exception ex)
            {
                if (ex is ArgumentException || ex is ArgumentNullException)
                {
                    return null;
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
