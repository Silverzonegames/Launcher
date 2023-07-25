using Silverzone.CommonLib.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Silverzone.CommonLib.Ex;

namespace Silverzone_Launcher
{
    public sealed class StateManager
    {
        public static readonly string GamesListURL = "https://raw.githubusercontent.com/Silverzonegames/Launcher/main/Data/launcher/gamelist.txt";
        public List<GameData> gamesList = new List<GameData>();
        public int currentGame = 0;
        public string downloadPath = AppDomain.CurrentDomain.BaseDirectory + @"\Downloads\";
        public string gamePath = AppDomain.CurrentDomain.BaseDirectory + @"\Games\";
        internal static readonly WebClient client = new();
        internal readonly static MainWindow uic = new();

        static StateManager() {
        
        }

        internal StateManager()
        {

        }

        public static StateManager Instance
        {
            get
            {
                return Instance;
            }
        }

        #region Data
        public Task DownloadGamesList()
        {
            string downloadString = client.DownloadString(GamesListURL).TrimStart().TrimEnd();

            string[] lines = downloadString.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            for (int i = 0; i < lines.Length; i++)
            {
                gamesList.Add(new GameData(lines[i]));
            }
            Debug.Write("GamesList " + downloadString);
            return Task.CompletedTask;

        }
        public Task DownloadData()
        {
            Logger.LogWrite("There are " + gamesList.Count + " members in the game list", Logger.Severity.Debug);
            /////////// Read GameData from url ////////////////////
            for (int i = 0; i < gamesList.Count; i++)
            {
                string downloadString = client.DownloadString(gamesList[i].URL);
                gamesList[i].data = downloadString;

            }

            return Task.CompletedTask;

        }
        public Task ProcessData()
        {
            var c_string = gamesList.Count.ToString();
            if (c_string == null || c_string.Length == 0) c_string = "List is empty, please fix";
            Logger.LogWrite(c_string, Logger.Severity.Debug);
            for (int i = 0; i < gamesList.Count; i++)
            {
                string? _data = gamesList[i].data;
                if (_data == null) return Task.CompletedTask;
                //split data by line
                string[] lines = _data.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                //loop through lines
                for (int j = 0; j < lines.Length - 1; j++)
                {
                    //get variables in line for example "name=steampunk"

                    string var = lines[j].Split("=")[0].ToLower().TrimStart().TrimEnd();
                    string value = lines[j].Split("=")[1];

                    switch (var)
                    {
                        case "name":
                            if (gamesList[i].name == null) break;
                            gamesList[i].name = value;
                            break;
                        case "desc":
                            if (gamesList[i].desc == null) break;
                            gamesList[i].desc = value;
                            break;
                        case "download":
                            if (gamesList[i].download == null) break;
                            gamesList[i].download = value;
                            break;
                        case "version":
                            if (gamesList[i].latestVersion == null) break;
                            gamesList[i].latestVersion = value;
                            break;
                        case "zip":
                            if (gamesList[i].zip == null) break;
                            gamesList[i].zip = value;
                            break;
                        case "exe":
                            if (gamesList[i].exe == null) break;
                            gamesList[i].exe = value;
                            break;
                        case "id":
                            if (gamesList[i].id == null) break;
                            gamesList[i].id = value;
                            break;

                    }
                    string alstring = string.Concat("added ", var, ": ", value, " to the game list.");
                    Logger.LogWrite(alstring, Logger.Severity.Debug);
                }
                
            }
            for (int i = 0; i < gamesList.Count; i++)
            {
                try
                {
                    Logger.LogWrite("The current game id is " + gamesList[i].id + " and current game index is:" + i + "game list reference id is: " + gamesList[0].id, Logger.Severity.Debug);
                    Logger.LogWrite("Trying to add " + gamesList[i].id + " to main window", Logger.Severity.Debug);
                    MainWindow.GamelistInsert(gamesList[i].id, uic);
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex, Logger.Severity.Error);
                    return Task.FromException(ex);
                }
            }
            return Task.CompletedTask;
        }
        #endregion
    }
}
