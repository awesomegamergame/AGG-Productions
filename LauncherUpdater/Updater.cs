﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows;
using System.Reflection;
using static AGG_Productions.MainWindow;
using AGG_Productions.LauncherData;

namespace AGG_Productions.LauncherUpdater
{
    class Updater
    {
        public static string LauncherLink = "https://www.dropbox.com/s/27bwz9ct96qltlx/AGG%20Productions%20Temp.zip?dl=1";
        public static string LauncherVerLink = "https://www.dropbox.com/s/l0s6jjask4paool/AGG%20Productions%20Version.txt?dl=1";
        public static string HTMLVerLink = "https://raw.githubusercontent.com/awesomegamergame/AGG-Productions/master/HTMLPlayer/Webdata/Binary/HTMLPlayerVersion.txt";
        public static string startPath = @".\AGG Productions Temp";
        public static string exeOld = Path.Combine(CheckFiles.rootPath, "AGG Productions.exe.old");
        public static string pdbOld = Path.Combine(CheckFiles.rootPath, "AGG Productions.pdb.old");
        public static string dllOld = Path.Combine(CheckFiles.rootPath, "Newtonsoft.Json.dll.old");
        public static string shellxml = Path.Combine(CheckFiles.rootPath, "Microsoft.WindowsAPICodePack.Shell.xml");
        public static string corexml = Path.Combine(CheckFiles.rootPath, "Microsoft.WindowsAPICodePack.xml");
        public static string versionFile = Path.Combine(CheckFiles.rootPath, "AGG Productions Version.txt");
        public static string launcherZip = Path.Combine(CheckFiles.rootPath, "AGG Productions Temp.zip");
        public static int VersionDetector = 0;
        public static Version HTMLonlineVersion;
        public static Version onlineVersion;
        public static Version localVersion;
        public static void LauncherUpdate()
        {
            if (File.Exists(exeOld))
            {
                File.Delete(exeOld);
            }
            if (File.Exists(pdbOld))
            {
                File.Delete(pdbOld);
            }
            if (File.Exists(dllOld))
            {
                File.Delete(dllOld);
            }
            if (File.Exists(shellxml))
            {
                File.Delete(shellxml);
            }
            if (File.Exists(corexml))
            {
                File.Delete(corexml);
            }
            if (Directory.Exists(startPath))
            {
                Directory.Delete(startPath);
            }
            Assembly assembly = Assembly.GetExecutingAssembly();
            System.Version version = assembly.GetName().Version;
            string versionS = version.ToString();
            versionS = versionS.Substring(0, versionS.Length - 2);
            localVersion = new Version(versionS);

            try
            {
                onlineVersion = new Version(DownloadString(LauncherVerLink));
                HTMLonlineVersion = new Version(DownloadString(HTMLVerLink));
                if (CheckFiles.FilesCheckPassed == false)
                {
                    InstallGameFiles(false, Version.zero);
                }
                else if (onlineVersion.IsDifferentThan(localVersion))
                {
                    VersionDetector += 1;
                    AGGWindow.UpdateScreen.Visibility = Visibility.Visible;
                    AGGWindow.Yes.Visibility = Visibility.Visible;
                    AGGWindow.No.Visibility = Visibility.Visible;
                    AGGWindow.UpdateText1.Visibility = Visibility.Visible;
                    AGGWindow.UpdateText2.Visibility = Visibility.Visible;
                    AGGWindow.LocalVersion.Visibility = Visibility.Visible;
                    AGGWindow.OnlineVersion.Visibility = Visibility.Visible;
                    AGGWindow.LocalVersionNumber.Content = localVersion;
                    AGGWindow.OnlineVersionNumber.Content = onlineVersion;
                    AGGWindow.LocalVersionNumber.Visibility = Visibility.Visible;
                    AGGWindow.OnlineVersionNumber.Visibility = Visibility.Visible;
                    AGGWindow.AGGB.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error checking for game updates: {ex}");
            }
        }

        public static void InstallGameFiles(bool _isUpdate, Version _onlineVersion)
        {
            try
            {
                FileDownloader LauncherDownload = new FileDownloader();
                if (!_isUpdate)
                {
                    _onlineVersion = new Version(DownloadString(LauncherVerLink));
                }
                LauncherDownload.DownloadFileAsync(LauncherLink, launcherZip, _onlineVersion);
                LauncherDownload.DownloadProgressChanged += LauncherDownload_DownloadProgressChanged;
                LauncherDownload.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadGameCompletedCallback);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error installing game files: {ex}");
            }
        }

        private static void LauncherDownload_DownloadProgressChanged(object sender, FileDownloader.DownloadProgress progress)
        {
            AGGWindow.UpdateProgress.Value = progress.ProgressPercentage;
            AGGWindow.RepairBar.Value = progress.ProgressPercentage;
        }

        private static void DownloadGameCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                string onlineVersion = ((Version)e.UserState).ToString();
                if (!Directory.Exists(startPath))
                {
                    if (File.Exists(CheckFiles.LauncherExe))
                    {
                        File.Move(@".\AGG Productions.exe", @".\AGG Productions.exe.old");
                    }
                    if (File.Exists(CheckFiles.Launcherpdb))
                    {
                        File.Move(@".\AGG Productions.pdb", @".\AGG Productions.pdb.old");
                    }
                    if (File.Exists(CheckFiles.JsonDLL))
                    {
                        File.Move(@".\Newtonsoft.Json.dll", @".\Newtonsoft.Json.dll.old");
                    }
                    try
                    {
                        //Declare a temporary path to unzip your files
                        string tempPath = Path.Combine(Directory.GetCurrentDirectory(), "tempUnzip");
                        string extractPath = Directory.GetCurrentDirectory();
                        ZipFile.ExtractToDirectory(launcherZip, tempPath);

                        //build an array of the unzipped files
                        string[] files = Directory.GetFiles(tempPath);

                        foreach (string file in files)
                        {
                            FileInfo f = new FileInfo(file);
                            //Check if the file exists already, if so delete it and then move the new file to the extract folder
                            if (File.Exists(Path.Combine(extractPath, f.Name)))
                            {
                                File.Delete(Path.Combine(extractPath, f.Name));
                                File.Move(f.FullName, Path.Combine(extractPath, f.Name));
                            }
                            else
                            {
                                File.Move(f.FullName, Path.Combine(extractPath, f.Name));
                            }
                        }
                        //Delete the temporary directory.
                        Directory.Delete(tempPath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                    File.Delete(launcherZip);
                    File.Delete(versionFile);
                    Process.Start(CheckFiles.LauncherExe);
                    Application.Current.Shutdown();

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error finishing download: {ex}");
            }
        }
        public static void UpdaterVersion()
        {
            if(VersionDetector == 1)
            {
                InstallGameFiles(true, onlineVersion);
            }
            else if(VersionDetector == 2)
            {
                InstallGameFiles(false, Version.zero);
            }
        }

        private static string DownloadString(string link)
        {
            var request = (HttpWebRequest)WebRequest.Create(link);
            var response = (HttpWebResponse)request.GetResponse();

            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                return reader.ReadToEnd();
            }
        }
    }

    struct Version
    {
        internal static Version zero = new Version(0, 0, 0);

        private readonly short major;
        private readonly short minor;
        private readonly short subMinor;

        internal Version(short _major, short _minor, short _subMinor)
        {
            major = _major;
            minor = _minor;
            subMinor = _subMinor;
        }
        internal Version(string _version)
        {
            string[] _versionStrings = _version.Split('.');
            if (_versionStrings.Length != 3)
            {
                major = 0;
                minor = 0;
                subMinor = 0;
                return;
            }

            major = short.Parse(_versionStrings[0]);
            minor = short.Parse(_versionStrings[1]);
            subMinor = short.Parse(_versionStrings[2]);
        }
        internal bool IsDifferentThan(Version _otherVersion)
        {
            if (major != _otherVersion.major)
            {
                return true;
            }
            else
            {
                if (minor != _otherVersion.minor)
                {
                    return true;
                }
                else
                {
                    if (subMinor != _otherVersion.subMinor)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public override string ToString()
        {
            return $"{major}.{minor}.{subMinor}";
        }
    }
}