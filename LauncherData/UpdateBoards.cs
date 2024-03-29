﻿using System;
using System.IO;
using System.Net;
using static System.Environment;
using AGG_Productions.LauncherData;
using static AGG_Productions.MainWindow;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AGG_Productions
{
    public class UpdateBoards
    {
        public static string BoardNameM;
        public static void DownloadBoards(string BoardName, string BoardHTMLLink)
        {
            BoardNameM += BoardName;
            string UpdateBoardDir = $@"{CurrentDirectory}\Cache\UpdateBoards";

            WebClient c = new WebClient();
        A:
            if (Directory.Exists(UpdateBoardDir))
            {
                if (CheckInternet.IsOnline)
                {
                    c.DownloadFileAsync(new Uri(BoardHTMLLink), $@"{CurrentDirectory}\Cache\UpdateBoards\{BoardName}Updates.html");
                }
            }
            else
            {
                Directory.CreateDirectory(UpdateBoardDir);
                goto A;
            }
        }
        public static void SetupBoards()
        {
            if (CheckInternet.IsOnline)
            {
                dynamic obj = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText($@"{CurrentDirectory}\Cache\ButtonData.json"));
                dynamic json = obj.Games;
                foreach (JProperty Version in json)
                {
                    string LinkJson = Json.ReadGameVerJson(Version.Name, "updates", "ButtonData", "Games");
                    DownloadBoards(Version.Name, LinkJson);
                }
            }
        }
    }
    public class ActivateBoard
    {
        public ActivateBoard(string BoardName)
        {
            if (File.Exists($@"{CurrentDirectory}\Cache\UpdateBoards\{BoardName}Updates.html"))
            {
                AGGWindow.UpdateBoard.Source = new Uri($@"{CurrentDirectory}\Cache\UpdateBoards\{BoardName}Updates.html");
            }
            else
            {
                AGGWindow.UpdateBoard.Navigate("about:blank");
            }
        }
    }
}