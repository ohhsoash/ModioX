﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ModioX.Database;
using ModioX.Extensions;
using ModioX.Forms.Windows;
using ModioX.Io;
using ModioX.Models.Database;

namespace ModioX.Models.Resources
{
    public class BackupFilesData
    {
        public List<BackupFile> BackupFiles { get; set; } = new();

        /// <summary>
        /// Create/store a backup of the specified file, and then downloads it locally to a known path
        /// </summary>
        /// <param name="modItem"> </param>
        /// <param name="fileName"> </param>
        /// <param name="installFilePath"> </param>
        public void CreateBackupFile(ModItemData modItem, string fileName, string installFilePath)
        {
            string gameBackupFolder = GetGameBackupFolder(modItem);

            Directory.CreateDirectory(gameBackupFolder);

            BackupFile backupFile = new()
            {
                ConsoleType = modItem.GetPlatform(),
                CategoryId = modItem.CategoryId,
                FileName = fileName,
                LocalPath = Path.Combine(gameBackupFolder, fileName),
                InstallPath = installFilePath,
                CreatedDate = DateTime.Now
            };

            MainWindow.FtpClient.DownloadFile(backupFile.LocalPath, backupFile.InstallPath);
            BackupFiles.Add(backupFile);
        }

        /// <summary>
        /// Gets the <see cref="BackupFile" /> information for the specified game id, file name and
        /// install path
        /// </summary>
        /// <param name="gameId"> Game Id </param>
        /// <param name="fileName"> File Name </param>
        /// <param name="installPath"> File Install Path </param>
        /// <returns> </returns>
        public BackupFile GetGameFileBackup(PlatformPrefix consoleType, string gameId, string fileName, string installPath)
        {
            return
                BackupFiles.FirstOrDefault(backupFile =>
                backupFile.ConsoleType == consoleType &&
                backupFile.CategoryId.EqualsIgnoreCase(gameId) &&
                backupFile.FileName.ContainsIgnoreCase(fileName) &&
                backupFile.InstallPath.ContainsIgnoreCase(installPath));
        }

        /// <summary>
        /// Create and return the game backup files folder for the specified <see cref="ModsData.ModItem" />
        /// </summary>
        /// <param name="modItem"> </param>
        /// <returns> </returns>
        public static string GetGameBackupFolder(ModItemData modItem)
        {
            return Path.Combine(UserFolders.BackupFiles, modItem.CategoryId);
        }

        /// <summary>
        /// Updates the collection of backup files at index if it's exists, otherwise adds a new one.
        /// </summary>
        /// <param name="index"> </param>
        /// <param name="backupFile"> </param>
        public void UpdateBackupFile(int index, BackupFile backupFile)
        {
            switch (BackupFiles[index])
            {
                case null:
                    BackupFiles.Add(backupFile);
                    return;
                default:
                    BackupFiles[index] = backupFile;
                    break;
            }
        }
    }

    /// <summary>
    /// Create a backup file class with the details.
    /// </summary>
    public class BackupFile
    {
        public PlatformPrefix ConsoleType { get; set; }

        public string CategoryId { get; set; }

        public string FileName { get; set; }

        public string LocalPath { get; set; }

        public string InstallPath { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}