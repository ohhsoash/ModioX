﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using ModioX.Extensions;
using ModioX.Forms.Windows;
using ModioX.Io;

namespace ModioX.Models.Database
{
    public partial class ModsData
    {
        /// <summary>
        ///     Get the date/time the database was last updated.
        /// </summary>
        public DateTime LastUpdated { get; set; }

        /// <summary>
        ///     Get the mods from the database.
        /// </summary>
        public List<ModItem> Mods { get; set; }

        /// <summary>
        ///     Get the mod's details such as the id, name, author, etc.
        /// </summary>
        public class ModItem
        {
            public int Id { get; set; }

            public string GameId { get; set; }

            public string Name { get; set; }

            public string Firmware { get; set; }

            public string Region { get; set; }

            public string Author { get; set; }

            public string SubmittedBy { get; set; }

            public string Version { get; set; }

            public string Configuration { get; set; }

            public string Type { get; set; }

            public string Description { get; set; }

            public List<DownloadFiles> DownloadFiles { get; set; }

            /// <summary>
            ///     Get the category type.
            /// </summary>
            /// <param name="categoriesData"></param>
            /// <returns></returns>
            public CategoryType GetCategoryType(CategoriesData categoriesData)
            {
                foreach (CategoriesData.Category category in categoriesData.Categories)
                {
                    if (category.Id.ToLower().Equals(GameId.ToLower()))
                    {
                        return category.CategoryType;
                    }
                }

                return CategoryType.Game;
            }

            /// <summary>
            ///     Check whether install requires a game region to be specified.
            /// </summary>
            public bool RequiresGameRegion => DownloadFiles.Any(x => x.RequiresGameRegion);

            /// <summary>
            ///     Check whether install requires a user id to be specified.
            /// </summary>
            public bool RequiresUserId => DownloadFiles.Any(x => x.RequiresGameRegion);

            /// <summary>
            ///     Check whether install requires a USB device to be connected to console
            /// </summary>
            public bool RequiresUsbDevice => DownloadFiles.Any(x => x.RequiresUsbDevice);

            /// <summary>
            ///     Check whether any files are installed at the 'dev_rebug' (firmware) folder.
            /// </summary>
            /// <returns></returns>
            public bool IsInstallToRebugFolder => DownloadFiles.Any(x => x.IsInstallToRebugFolder);

            /// <summary>
            ///     Check whether mod is for any region.
            /// </summary>
            public bool IsAnyRegion => Region.Equals("ALL") || Region.Equals("-");

            /// <summary>
            ///     Check whether this mod is a game save.
            /// </summary>
            public bool IsGameSave => Type.Equals("GAMESAVE");

            /// <summary>
            ///     Get all the mod types.
            /// </summary>
            /// <returns></returns>
            public List<string> ModTypes
            {
                get
                {
                    List<string> modTypes = new List<string>();

                    foreach (string modType in Type.Split('/'))
                    {
                        modTypes.Add(modType);
                    }

                    return modTypes;
                }
            }

            /// <summary>
            ///     Get all the mod versions if there are multiple.
            /// </summary>
            /// <returns></returns>
            public List<string> Versions
            {
                get
                {
                    List<string> versions = new List<string>();

                    if (Version == "n/a")
                    {
                        versions.Add("n/a");
                    }
                    else if (Version == "-")
                    {
                        versions.Add("-");
                    }
                    else if (Version == "")
                    {
                        versions.Add("-");
                    }
                    else
                    {
                        foreach (string version in Version.Split('/'))
                        {
                            versions.Add("v" + version);
                        }
                    }

                    return versions;
                }
            }

            /// <summary>
            ///     Get all the supported firmwares.
            /// </summary>
            /// <returns></returns>
            public List<string> Firmwares
            {
                get
                {
                    List<string> firmwares = new List<string>();

                    foreach (string firmware in Firmware.Split('/'))
                    {
                        firmwares.Add(firmware);
                    }

                    return firmwares;
                }
            }

            /// <summary>
            ///     Get the supported game regions.
            /// </summary>
            /// <returns></returns>
            public List<string> GameRegions
            {
                get
                {
                    List<string> regions = new List<string>();

                    foreach (string region in Region.Split('/'))
                    {
                        if (region.Equals("ALL"))
                        {
                            regions.Add("All Regions");
                        }
                        else if (region.Equals("-"))
                        {
                            regions.Add("n/a");
                        }
                        else
                        {
                            regions.Add(region);
                        }
                    }

                    return regions;
                }
            }

            /// <summary>
            ///     Get the game modes actual names from their abbreviated names.
            /// </summary>
            /// <returns></returns>
            public List<string> GameModes
            {
                get
                {
                    List<string> gameModes = new List<string>();

                    foreach (string mode in Configuration.Split('/'))
                    {
                        if (mode.Equals("ALL"))
                        {
                            gameModes.Add("All Modes");
                        }
                        else if (mode.Equals("MP"))
                        {
                            gameModes.Add("Multiplayer");
                        }
                        else if (mode.Equals("ZM"))
                        {
                            gameModes.Add("Zombies");
                        }
                        else if (mode.Equals("SP"))
                        {
                            gameModes.Add("Singleplayer");
                        }
                        else if (mode.Equals("SPEC OPS"))
                        {
                            gameModes.Add("Special Ops");
                        }
                        else if (mode.Equals("-"))
                        {
                            gameModes.Add("n/a");
                        }
                        else
                        {
                            gameModes.Add("n/a");
                        }
                    }

                    return gameModes;
                }
            }

            /// <summary>
            ///     Get the download url specified by the user if there are multiple types
            /// </summary>
            /// <returns>Download Archive URL</returns>
            public DownloadFiles GetDownloadFiles(System.Windows.Forms.Form owner)
            {
                if (DownloadFiles.Count > 1)
                {
                    var downloadNames = DownloadFiles.Select(x => x.Name).ToList();
                    var downloadName = DialogExtensions.ShowListInputDialog(owner, "Install Downloads", downloadNames);

                    if (string.IsNullOrEmpty(downloadName))
                    {
                        return null;
                    }

                    return DownloadFiles.First(x => x.Name.Equals(downloadName));
                }
                else
                {
                    return DownloadFiles.First();
                }
            }

            /// <summary>
            ///     Get the directory for extracting modded files to.
            /// </summary>
            /// <returns></returns>
            public string DownloadDataDirectory(DownloadFiles download) => $@"{UserFolders.AppModsData}{GameId}\{Author}\{StringExtensions.ReplaceInvalidChars(Name)} ({StringExtensions.ReplaceInvalidChars(download.Name)}) (#{Id})\";

            /// <summary>
            ///     Gets the downloaded mods archive file path.
            /// </summary>
            /// <returns>Mods Archive File Path</returns>
            public string ArchiveZipFile(DownloadFiles download) => $@"{UserFolders.AppModsData}{GameId}\{Author}\{StringExtensions.ReplaceInvalidChars(Name)} ({StringExtensions.ReplaceInvalidChars(download.Name)}) (#{Id}).zip";

            /// <summary>
            ///     Downloads the modded files archive and extracts all files to <see cref="DownloadDataDirectory"/>
            /// </summary>
            public void DownloadInstallFiles(DownloadFiles download)
            {
                string archivePath = DownloadDataDirectory(download);
                string archiveFilePath = ArchiveZipFile(download);

                if (!MainWindow.Settings.AlwaysDownloadInstallFiles && File.Exists(archiveFilePath))
                {
                    return;
                }

                if (Directory.Exists(archivePath))
                {
                    UserFolders.DeleteDirectory(archivePath);
                }

                if (!Directory.Exists(archivePath))
                {
                    _ = Directory.CreateDirectory(archivePath);
                }

                if (File.Exists(archiveFilePath))
                {
                    File.Delete(archiveFilePath);
                }

                _ = Directory.CreateDirectory(archivePath);

                /*
                using (WebClient webClient = new WebClient())
                {
                    webClient.Headers.Add("Accept: application/zip");
                    webClient.Headers.Add("User-Agent: Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0)");
                    webClient.DownloadFile(new Uri(Url), archiveFilePath);
                    ZipFile.ExtractToDirectory(archiveFilePath, archivePath);
                }
                */

                using (WebClient webClient = new WebClient())
                {
                    webClient.Headers.Add("Accept: application/zip");
                    webClient.Headers.Add("User-Agent: Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0)");
                    webClient.DownloadFile(new Uri(download.URL), archiveFilePath);
                    ZipFile.ExtractToDirectory(archiveFilePath, archivePath);
                }
            }

            /// <summary>
            ///     Download mods archive to the specified local folder path
            /// </summary>
            /// <param name="categoriesData"></param>
            /// <param name="download"></param>
            /// <param name="localPath">Path to downloads mods archive at folder</param>
            public void DownloadArchiveAtPath(CategoriesData categoriesData, DownloadFiles download, string localPath)
            {
                string zipFileName = $"{StringExtensions.ReplaceInvalidChars(Name)} v{Version} for {GameId.ToUpper()}.zip";
                string zipFilePath = Path.Combine(localPath, zipFileName);

                GenerateReadMeAtPath(categoriesData, DownloadDataDirectory(download));

                using (WebClient webClient = new WebClient())
                {
                    webClient.Headers.Add("Accept: application/zip");
                    webClient.Headers.Add("User-Agent: Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0)");
                    webClient.DownloadFile(new Uri(download.URL), zipFilePath);
                    Archives.AddFilesToZip(zipFilePath, new string[] { Path.Combine(DownloadDataDirectory(download), "README.txt") });
                }
            }

            /// <summary>
            ///     Creates and writes the mod information to a text file at the specified path
            /// </summary>
            /// <param name="categoriesData"></param>
            /// <param name="directoryPath"></param>
            public void GenerateReadMeAtPath(CategoriesData categoriesData, string directoryPath)
            {
                if (!Directory.Exists(directoryPath))
                {
                    _ = Directory.CreateDirectory(directoryPath);
                }

                // Create contents and write them to readme file 
                File.WriteAllLines(Path.Combine(directoryPath, "README.txt"), new string[]
                {
                    "Mod ID: #" + Id.ToString(),
                    "Category: " + categoriesData.GetCategoryById(GameId).Title,
                    "Name: " + Name,
                    "System Type: " + string.Join(", ", Firmwares),
                    "Mod Type: " + Type,
                    "Version: " + Version,
                    "Region: " + string.Join(", ", GameRegions),
                    "Created By: " + Author,
                    "Submitted By: " + SubmittedBy,
                    "Game Type: " + string.Join(", ", GameModes),
                    "Downloads: " + string.Join(", ", DownloadFiles.Select(x => x.URL)),
                    "-------------------------------------------------",
                    "Description:\n" + Description
                });
            }
        }

        public partial class DownloadFiles
        {
            public string Name { get; set; }

            public string URL { get; set; }

            public List<string> InstallPaths { get; set; }

            /// <summary>
            ///     Check whether any files are being installed to a game folder.
            /// </summary>
            public bool RequiresGameRegion => InstallPaths.Any(x => x.Contains("{REGION}"));

            /// <summary>
            ///     Check whether any files are being installed to a profile user's folder.
            /// </summary>
            public bool RequiresUserId => InstallPaths.Any(x => x.Contains("{USERID}"));

            /// <summary>
            ///     Check whether any files are being installed to a USB device.
            /// </summary>
            public bool RequiresUsbDevice => InstallPaths.Any(x => x.Contains("{USBDEV}"));
            
            /// <summary>
            ///     Check whether any files are installed at the 'dev_rebug' (firmware) folder.
            /// </summary>
            /// <returns></returns>
            public bool IsInstallToRebugFolder => InstallPaths.Any(x => x.StartsWith("/dev_rebug/"));
        }

        /// <summary>
        ///     Get all of the mod types from the specified <see cref="CategoriesData.Category.Id"/>
        /// </summary>
        /// <param name="categoryId"><see cref="CategoriesData.Category.Id"/></param>
        /// <returns></returns>
        public List<string> AllModTypesForCategoryId(string categoryId)
        {
            List<string> modTypes = new List<string>();

            foreach (ModItem modItem in Mods)
            {
                if (string.Equals(modItem.GameId, categoryId, StringComparison.CurrentCultureIgnoreCase))
                {
                    modTypes.AddRange(modItem.ModTypes);
                }
            }

            modTypes.Sort();
            return modTypes.Distinct().ToList();
        }

        /// <summary>
        ///     Get the supported firmwares from all of the mods.
        /// </summary>
        /// <returns>Firmwares Supported</returns>
        public List<string> AllFirmwares
        {
            get
            {
                List<string> firmwares = new List<string>();

                foreach (ModItem modItem in Mods)
                {
                    foreach (string firmware in modItem.Firmware.Split('/'))
                    {
                        if (!firmwares.Contains(modItem.Firmware))
                        {
                            firmwares.Add(firmware);
                        }
                    }
                }

                firmwares.Sort();
                return firmwares;
            }
        }

        /// <summary>
        ///     Gets all of the mods for the specified gameId, with results filtered by name, firmware and type
        /// </summary>
        /// <param name="categoryId"></param>
        /// <param name="name"></param>
        /// <param name="firmware"></param>
        /// <param name="type"></param>
        /// <param name="region"></param>
        /// <returns></returns>
        public List<ModItem> GetModItems(string categoryId, string name, string firmware, string type, string region)
        {
            if (categoryId.Equals("fvrt"))
            {
                return (from ModItem modItem in Mods
                        where MainWindow.Settings.FavoritedIds.Contains(modItem.Id.ToString())
                        && modItem.Name.ToLower().Contains(name.ToLower())
                        && modItem.Firmwares.Exists(x => x.ToLower().Contains(firmware.ToLower()))
                        && modItem.Type.ToLower().Contains(type.ToLower())
                        && modItem.Region.ToLower().Contains(region.ToLower())
                        select modItem).Distinct().ToList();
            }
            else
            {
                return (from ModItem modItem in Mods
                        where string.Equals(modItem.GameId.ToLower(), categoryId.ToLower())
                        && modItem.Name.ToLower().Contains(name.ToLower())
                        && modItem.Firmwares.Exists(x => x.ToLower().Contains(firmware.ToLower()))
                        && modItem.Type.ToLower().Contains(type.ToLower())
                        && modItem.Region.ToLower().Contains(region.ToLower())
                        select modItem).Distinct().ToList();
            }
        }

        /// <summary>
        ///     Get the <see cref="ModItem"/> matching the specified <see cref="ModItem.Id"/>
        /// </summary>
        /// <param name="modId"><see cref="ModItem.Id"/></param>
        /// <returns>Mod details for the <see cref="ModItem.Id"/></returns>
        public ModItem GetModById(int modId)
        {
            return Mods.First(modItem => modItem.Id.Equals(modId));
        }

        /// <summary>
        ///     Get all the <see cref="ModItem"/> matching the specified <see cref="CategoriesData.Category.Id"/>.
        /// </summary>
        /// <returns></returns>
        public ModItem[] GetModsByCategoryId(string gameId)
        {
            return (from ModItem modItem in Mods
                    where modItem.GameId.Equals(gameId)
                    select modItem).ToArray();
        }

        /// <summary>
        ///     Get the total number of game mods
        /// </summary>
        /// <returns></returns>
        public int TotalGameMods(CategoriesData categoriesData)
        {
            return (from ModItem modItem in Mods
                    where modItem.GetCategoryType(categoriesData) == CategoryType.Game
                    && modItem.GetCategoryType(categoriesData) != CategoryType.Homebrew
                    && modItem.GetCategoryType(categoriesData) != CategoryType.Resource
                    && modItem.GetCategoryType(categoriesData) != CategoryType.Favorite
                    select modItem).Count();
        }

        /// <summary>
        ///     Get the total number of resources.
        /// </summary>
        /// <returns></returns>
        public int TotalHomebrew(CategoriesData categoriesData)
        {
            return (from ModItem modItem in Mods
                    where modItem.GetCategoryType(categoriesData) == CategoryType.Homebrew
                    && modItem.GetCategoryType(categoriesData) != CategoryType.Game
                    && modItem.GetCategoryType(categoriesData) != CategoryType.Resource
                    && modItem.GetCategoryType(categoriesData) != CategoryType.Favorite
                    select modItem).Count();
        }


        /// <summary>
        ///     Get the total number of resources.
        /// </summary>
        /// <returns></returns>
        public int TotalResources(CategoriesData categoriesData)
        {
            return (from ModItem modItem in Mods
                    where modItem.GetCategoryType(categoriesData) == CategoryType.Resource
                    && modItem.GetCategoryType(categoriesData) != CategoryType.Homebrew
                    && modItem.GetCategoryType(categoriesData) != CategoryType.Game
                    && modItem.GetCategoryType(categoriesData) != CategoryType.Favorite
                    select modItem).Count();
        }
    }
}