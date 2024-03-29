﻿using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Windows;
using static System.Environment;
using static AGG_Productions.MainWindow;

namespace AGG_Productions.LauncherData
{
    class PlayButton
    {
        public string Name;
        public bool HTML;
        public GamePaths paths;
        public static VersionManager _VersionManager;
        public string Version;
        public void Start(string Name, bool HTML, string Version)
        {
            this.Version = Version;
            this.Name = Name;
            this.HTML = HTML;
            CheckInternet.CheckInternetState();
            VersionSelector.IsEnabled = false;
            AGGWindow.PlayButtonGUI.IsEnabled = false;
            paths = new GamePaths(Version, Name, GameDir, HTML);
            if (HTML && File.Exists(paths.ExeFile))
            {
                if (File.Exists($@"{CurrentDirectory}\Plugins\HTMLPlayer\HTMLPlayer.exe"))
                {
                    string GameDir = Json.ReadJson(Name);
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        Arguments = $"{Name} {GameDir} {Version}",
                        WorkingDirectory = $@"{CurrentDirectory}\Plugins\HTMLPlayer\",
                        FileName = "HTMLPlayer.exe"
                    };

                    Process.Start(psi);
                    Application.Current.Shutdown();
                }
                else
                {
                    MessageBox.Show("HTMLPlayer Must Be Installed From The Settings");
                    AGGWindow.PlayButtonGUI.IsEnabled = true;
                    VersionSelector.IsEnabled = true;
                }
            }
            else if (File.Exists(paths.ExeFile))
            {
                Process.Start(paths.ExeFile);
                Application.Current.Shutdown();
            }
            else if (CheckInternet.IsOnline == false)
            {
                MessageBox.Show("This version of this game isn't already downloaded. Check your internet and try again");
                AGGWindow.PlayButtonGUI.IsEnabled = true;
                VersionSelector.IsEnabled = true;
            }
            else
            {
                try
                {
                    if (!File.Exists($@"{CurrentDirectory}\Plugins\HTMLPlayer\HTMLPlayer.exe") && HTML)
                    {
                        MessageBox.Show("HTMLPlayer Must Be Installed From The Settings");
                        AGGWindow.PlayButtonGUI.IsEnabled = true;
                        VersionSelector.IsEnabled = true;
                    }
                    else
                    {
                        paths = new GamePaths(Version, Name, GameDir, HTML);
                        FileDownloader downloader = new FileDownloader();

                        if (_VersionManager.VersionLinkPairs.TryGetValue(Version, out string temp))
                        {
                            downloader.DownloadFileCompleted += Downloader_DownloadFileCompleted;
                            downloader.DownloadProgressChanged += Downloader_DownloadProgressChanged;
                            downloader.DownloadFileAsync(temp, $@"{paths.GameVersionFile}\Build({Version}).zip");
                        }
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("You must select a version in the dropdown list");
                    AGGWindow.PlayButtonGUI.IsEnabled = true;
                    VersionSelector.IsEnabled = true;
                }
            }
        }

        private void Downloader_DownloadProgressChanged(object sender, FileDownloader.DownloadProgress progress)
        {
            AGGWindow.GameDownload.Value = progress.ProgressPercentage;
        }

        public void Downloader_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            try
            {
                ZipFile.ExtractToDirectory($@"{paths.GameVersionFile}\Build({Version}).zip", paths.GameVersionFile);
                File.Delete($@"{paths.GameVersionFile}\Build({Version}).zip");
                if (HTML)
                {
                    var Play = new PlayButton();
                    Play.Start(Name, HTML, Version);
                }
                else
                {
                    Process.Start(paths.ExeFile);
                    Application.Current.Shutdown();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                AGGWindow.PlayButtonGUI.IsEnabled = true;
                VersionSelector.IsEnabled = true;
            }
        }
    }
}