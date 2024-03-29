﻿using System;
using System.IO;
using System.Net;
using System.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using static System.Environment;
using static AGG_Productions.MainWindow;
using System.Windows.Media.Imaging;
using AGG_Productions.LauncherData;
using AGG_Productions.LauncherUpdater;

namespace AGG_Productions.LauncherFunctions
{
    internal class Dynamicbuttons
    {
        public static void SetupButtons()
        {
            if (CheckInternet.IsOnline)
            {
                var request = (HttpWebRequest)WebRequest.Create(Json.BDataLink);
                var response = (HttpWebResponse)request.GetResponse();

                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    File.WriteAllText($@"{CurrentDirectory}\Cache\ButtonData.json", reader.ReadToEnd());
                }
            }
            if (!File.Exists($@"{CurrentDirectory}\Cache\ButtonData.json"))
                return;
            if (File.Exists($@"{CurrentDirectory}\ButtonData.json"))
            {
                JObject o1 = JObject.Parse(File.ReadAllText($@"{CurrentDirectory}\Cache\ButtonData.json"));
                JObject o2 = JObject.Parse(File.ReadAllText($@"{CurrentDirectory}\ButtonData.json"));
                o1.Merge(o2, new JsonMergeSettings
                {
                    MergeArrayHandling = MergeArrayHandling.Union
                });
                File.WriteAllText($@"{CurrentDirectory}\Cache\ButtonData.json", o1.ToString());
            }
            dynamic obj = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText($@"{CurrentDirectory}\Cache\ButtonData.json"));
            dynamic json = obj.Games;
            foreach (JProperty Names in json)
            {
                WebClient d = new WebClient();
                d.Proxy = WebRequest.DefaultWebProxy;
                string ImageLink = Json.ReadGameVerJson(Names.Name, "image", "ButtonData", "Games");
                string GameName = Json.ReadGameVerJson(Names.Name, "name", "ButtonData", "Games");
                string HTML = Json.ReadGameVerJson(Names.Name, "html", "ButtonData", "Games");
                if (CheckInternet.IsOnline)
                    d.DownloadFile(new Uri(ImageLink), $@"{CurrentDirectory}\Cache\Images\{GameName}.jpg");
                Button newBtn = new Button();
                if (File.Exists($@"{CurrentDirectory}\Cache\Images\{GameName}.jpg"))
                {
                    BitmapImage btm = new BitmapImage();
                    btm.BeginInit();
                    btm.CacheOption = BitmapCacheOption.OnLoad;
                    btm.UriSource = new Uri($@"{CurrentDirectory}\Cache\Images\{GameName}.jpg", UriKind.RelativeOrAbsolute);
                    btm.EndInit();
                    Image img = new Image
                    {
                        Source = btm,
                        Stretch = Stretch.Fill
                    };
                    newBtn.Content = img;
                }
                else
                    newBtn.Content = GameName;
                newBtn.Name = GameName;
                if (HTML.Equals("Yes") || HTML.Equals("yes"))
                    newBtn.Tag = true;
                else
                    newBtn.Tag = false;
                newBtn.Height = 50;
                newBtn.Width = 191;
                newBtn.HorizontalAlignment = HorizontalAlignment.Left;
                AGGWindow.List.Items.Add(newBtn);
                newBtn.Click += new RoutedEventHandler(Game_Click);
            }
        }
    }
}
