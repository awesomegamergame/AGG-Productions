﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows;

namespace AGG_Productions.LauncherUpdater
{
    class Updater
    {
        public static string rootPath = Directory.GetCurrentDirectory();
        public static string versionFile = Path.Combine(rootPath, "AGG Productions Version.txt");
        public static string exeOld = Path.Combine(rootPath, "AGG Productions.exe.old");
        public static string pdbOld = Path.Combine(rootPath, "AGG Productions.pdb.old");
        public static string launcherZip = Path.Combine(rootPath, "AGG Productions Temp.zip");
        public static string startPath = @".\AGG Productions Temp";
        public static string LauncherExe = Path.Combine(rootPath, "AGG Productions.exe");
        public static int VersionDetector = 0;
        public static Version onlineVersion;
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
            if (Directory.Exists(startPath))
            {
                Directory.Delete(startPath);
            }

            if (File.Exists(versionFile))
            {
                Version localVersion = new Version(File.ReadAllText(versionFile));

                try
                {
                    WebClient webClient = new WebClient();
                    onlineVersion = new Version(webClient.DownloadString(UpdateBoardLinks.LauncherVerLink));

                    if (onlineVersion.IsDifferentThan(localVersion))
                    {
                        VersionDetector += 1;
                        MainWindow.UpdateScreen_Image.Visibility = Visibility.Visible;
                        MainWindow.Yes_Button.Visibility = Visibility.Visible;
                        MainWindow.No_Button.Visibility = Visibility.Visible;
                        MainWindow.UpdateText1_Label.Visibility = Visibility.Visible;
                        MainWindow.UpdateText2_Label.Visibility = Visibility.Visible;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error checking for game updates: {ex}");
                }
            }
            else
            {
                VersionDetector += 2;
                MainWindow.UpdateScreen_Image.Visibility = Visibility.Visible;
                MainWindow.Yes_Button.Visibility = Visibility.Visible;
                MainWindow.No_Button.Visibility = Visibility.Visible;
                MainWindow.UpdateText1_Label.Visibility = Visibility.Visible;
                MainWindow.UpdateText2_Label.Visibility = Visibility.Visible;
            }
        }

        public static void InstallGameFiles(bool _isUpdate, Version _onlineVersion)
        {
            try
            {
                WebClient webClient = new WebClient();
                if (!_isUpdate)
                {
                    _onlineVersion = new Version(webClient.DownloadString(UpdateBoardLinks.LauncherVerLink));
                }

                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadGameCompletedCallback);
                webClient.DownloadFileAsync(new Uri(UpdateBoardLinks.LauncherLink), launcherZip, _onlineVersion);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error installing game files: {ex}");
            }
        }

        private static void DownloadGameCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                string onlineVersion = ((Version)e.UserState).ToString();
                if (!Directory.Exists(startPath))
                {
                    File.Move(@".\AGG Productions.exe", @".\AGG Productions.exe.old");
                    File.Move(@".\AGG Productions.pdb", @".\AGG Productions.pdb.old");
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
                    File.WriteAllText(versionFile, onlineVersion);

                    Process.Start(LauncherExe);
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
    }

    public struct Version
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