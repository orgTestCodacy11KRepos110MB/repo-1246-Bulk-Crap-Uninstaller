﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Klocman.Extensions;
using Klocman.Native;
using Klocman.Tools;

namespace UninstallTools
{
    public static class UninstallToolsGlobalConfig
    {
        internal static readonly IEnumerable<string> DirectoryBlacklist = new[]
        {
            "Microsoft", "Microsoft Games", "Temp", "Programs", "Common", "Common Files", "Clients",
            "Desktop", "Internet Explorer", "Windows NT", "Windows Photo Viewer", "Windows Mail",
            "Windows Defender", "Windows Media Player", "Uninstall Information", "Reference Assemblies"
        };

        internal static readonly IEnumerable<string> QuestionableDirectoryNames = new[]
        {
            "install", "settings", "config", "configuration",
            "users", "data"
        };

        /// <summary>
        /// Custom "Program Files" directories. Use with dirs that get used to install applications to.
        /// </summary>
        public static string[] CustomProgramFiles { get; set; }

        /// <summary>
        /// Directiories containing programs, both built in "Program Files" and user-defined ones.
        /// </summary>
        internal static IEnumerable<string> AllProgramFiles
            => StockProgramFiles.Concat(CustomProgramFiles ?? Enumerable.Empty<string>());

        internal static IEnumerable<string> JunkSearchDirs => new[]
        {
            WindowsTools.GetEnvironmentPath(CSIDL.CSIDL_PROGRAMS),
            WindowsTools.GetEnvironmentPath(CSIDL.CSIDL_COMMON_PROGRAMS),
            WindowsTools.GetEnvironmentPath(CSIDL.CSIDL_APPDATA),
            WindowsTools.GetEnvironmentPath(CSIDL.CSIDL_COMMON_APPDATA),
            WindowsTools.GetEnvironmentPath(CSIDL.CSIDL_LOCAL_APPDATA)

            //Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) danger?
        }.Distinct();

        internal static IEnumerable<string> StockProgramFiles => new[]
        {
            WindowsTools.GetEnvironmentPath(CSIDL.CSIDL_PROGRAM_FILES),
            WindowsTools.GetProgramFilesX86Path()
        }.Distinct();

        internal static bool IsSystemDirectory(DirectoryInfo dir)
        {
            return //dir.Name.StartsWith("Windows ") //Probably overkill
                   DirectoryBlacklist.Any(y => y.Equals(dir.Name, StringComparison.Ordinal))
                   || dir.Attributes.Contains(FileAttributes.System);
        }

        /// <summary>
        ///     Get a list of directiories containing programs. Optionally user-defined directories are added.
        ///     The boolean value is true if the directory is confirmed to contain 64bit applications.
        /// </summary>
        /// <param name="includeUserDirectories">Add user-defined directories.</param>
        /// <returns></returns>
        public static IEnumerable<KeyValuePair<DirectoryInfo, bool>> GetProgramFilesDirectories(bool includeUserDirectories)
        {
            var pfDirectories = new List<KeyValuePair<DirectoryInfo, bool>>(2);

            var pf64 = new DirectoryInfo(WindowsTools.GetEnvironmentPath(CSIDL.CSIDL_PROGRAM_FILES));
            var pf32 = new DirectoryInfo(WindowsTools.GetProgramFilesX86Path());
            pfDirectories.Add(new KeyValuePair<DirectoryInfo, bool>(pf32, false));
            if (!pf32.FullName.Equals(pf64.FullName))
                pfDirectories.Add(new KeyValuePair<DirectoryInfo, bool>(pf64, true));

            if (includeUserDirectories)
            {
                foreach (var dir in CustomProgramFiles.Distinct())
                {
                    try
                    {
                        var di = new DirectoryInfo(dir);
                        if (di.Exists)
                            pfDirectories.Add(new KeyValuePair<DirectoryInfo, bool>(di, false));
                    }
                    catch
                    {
                        // Ignore missing or inaccessible directories
                    }
                }
            }

            return pfDirectories;
        }
    }
}