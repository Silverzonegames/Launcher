using Silverzone.CommonLib.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Net.Http;

namespace Silverzone_Launcher
{
    internal static class GameManager
    {
        readonly static MainWindow uic = new();
        readonly static WebClient webClient = new();
        readonly static StateManager sm = new();
        public static Task DownloadGame(int index)
        {

            //make download dir if it doesnt exist
            if (!Directory.Exists(sm.downloadPath))
                Directory.CreateDirectory(sm.downloadPath);

            //remove previous download
            if (File.Exists(sm.downloadPath + sm.gamesList[index].zip))
            {
                File.Delete(sm.downloadPath + sm.gamesList[index].zip);
            }


            webClient.DownloadProgressChanged += (s, e) => {
                uic.progbar_download.Value = e.ProgressPercentage;
                if (e.ProgressPercentage >= 100)
                {
                    uic.progbar_download.Visibility = Visibility.Hidden;
                    Debug.WriteLine("Download Complete");
                    InstallGame(index);
                }
            };
            uic.progbar_download.Visibility = Visibility.Visible;

            string? dwnlUri = sm.gamesList[index].download;

            if (dwnlUri == null) return Task.CompletedTask;

            webClient.DownloadFileAsync(new Uri(dwnlUri), sm.downloadPath + sm.gamesList[index].zip);

            return Task.CompletedTask;
        }

        public static void InstallGame(int index)
        {
            //create game folder if it doesn't exist
            if (!Directory.Exists(sm.gamePath))
            {
                Directory.CreateDirectory(sm.gamePath);
            }

            //extract zip, watch for exceptions and send to CS-CLIB's logger
            try
            {
                ZipFile.ExtractToDirectory(sm.downloadPath + sm.gamesList[index].zip, sm.gamePath + sm.gamesList[index].id);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, Logger.Severity.Error);
            }

            //delete download
            File.Delete(sm.gamePath + sm.gamesList[index].zip);

            uic.btn_Play.Content = "Play";

        }

        public static void DeleteGame(int index)
        {
            if (Directory.Exists(sm.gamePath + sm.gamesList[index].id))
            {
                Directory.Delete(sm.gamePath + sm.gamesList[index].id);
                uic.btn_Play.Content = "Download";
            }
        }

        public static void PlayGame(int index)
        {
            Process.Start(sm.gamePath + sm.gamesList[index].id + "\\" + sm.gamesList[index].exe);
        }
    }
}
