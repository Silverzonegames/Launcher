using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Timers;
using MdXaml;
using AutoUpdaterDotNET;



namespace Silverzone_Launcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string GamesListURL = "https://raw.githubusercontent.com/Silverzonegames/Launcher/main/Data/launcher/gamelist.txt";
        public List<GameData> gamesList = new List<GameData>();
        public int currentGame;
        public string downloadPath = Environment.CurrentDirectory + @"\Downloads\";
        public string gamePath = Environment.CurrentDirectory + @"\Games\";


        #region System Startup
        public MainWindow()
        {
            

            InitializeComponent();


            Start();


            InitTimer();

            AutoUpdater.Start("https://raw.githubusercontent.com/Silverzonegames/Launcher/main/Data/launcher/updates.xml");
        }

        private Timer timer1;
        public void InitTimer() {
            timer1 = new Timer();
            timer1.Elapsed += Tick;
            timer1.Interval = 100; // in milliseconds
            timer1.Start();
        }

        private void Tick(object sender, EventArgs e) {

            try {
                Dispatcher.Invoke(() =>
                {
                    Update();
                });
            }
            catch (Exception) {
               
            }

        }
        #endregion

        #region Update

        public void Start() {
            DownloadGamesList();
            Debug.WriteLine("Download data");
            DownloadData();
            Debug.WriteLine("Process Data");
            ProcessData();
        }

        public void Update()
        {
            if (Directory.Exists(gamePath + gamesList[currentGame].id)) {
                btn_Play.Content = "Play";
                btn_Play.Background = new SolidColorBrush(Colors.Green);
            }
            else {
                btn_Play.Content = "Download";
                btn_Play.Background = new SolidColorBrush(Colors.Blue);
            }
            if (gamesList[currentGame].version != gamesList[currentGame].latestVersion && !string.IsNullOrEmpty( gamesList[currentGame].version)) {
                btn_Play.Content = "Update to" + gamesList[currentGame].latestVersion;
                btn_Play.Background = new SolidColorBrush(Colors.Blue);
            }

            lbl_playButton.Content = gamesList[currentGame].name;
            lbl_Version.Content = "v" + gamesList[currentGame].version;
            lbl_desc.Content = gamesList[currentGame].desc.Replace(@"\", "\n");

            if (string.IsNullOrEmpty(gamesList[currentGame].version) && File.Exists(gamePath + gamesList[currentGame].id + "\\version.txt")) {         
                gamesList[currentGame].version = File.ReadAllText(gamePath + gamesList[currentGame].id + "\\version.txt").TrimEnd().TrimStart();
            }
        }


        #endregion

        #region Data
        public void DownloadGamesList()
        {
            WebClient client = new WebClient();
            string downloadString = client.DownloadString(GamesListURL).TrimStart().TrimEnd();

            string[] lines = downloadString.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            for (int i = 0; i < lines.Length; i++)
            {
                gamesList.Add(new GameData(lines[i]));
            }
            Debug.Write("GamesList " + downloadString);

        }
        public void DownloadData()
        {
            /////////// Read GameData from url ////////////////////
            for (int i = 0; i < gamesList.Count; i++)
            {
                WebClient client = new WebClient();
                string downloadString = client.DownloadString(gamesList[i].URL);
                Debug.WriteLine("Downloaded data: " + downloadString);
                gamesList[i].data = downloadString;

            }
            Debug.WriteLine("Downloaded " + gamesList.Count + " data");

        }
        public void ProcessData()
        {
            for (int i = 0; i < gamesList.Count; i++)
            {
                string _data = gamesList[i].data;
                //split data by line
                string[] lines = _data.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                //loop through lines
                Debug.WriteLine("There are " + lines.Length + " variables");
                for (int j = 0; j < lines.Length - 1; j++)
                {

                    //get variables in line for example "name=steampunk"

                    string var = lines[j].Split("=")[0].ToLower().TrimStart().TrimEnd();
                    string value = lines[j].Split("=")[1];

                    switch (var)
                    {
                        case "name":
                            gamesList[i].name = value;
                            break;
                        case "desc":
                            gamesList[i].desc = value;
                            break;
                        case "download":
                            gamesList[i].download = value;
                            break;
                        case "version":
                            gamesList[i].latestVersion = value;
                            break;
                        case "zip":
                            gamesList[i].zip = value;
                            break;
                        case "exe":
                            gamesList[i].exe = value;
                            break;
                        case "id":
                            gamesList[i].id = value;
                            break;

                    }
                    Debug.WriteLine("added " + var + ":" + value + " to " + gamesList[i].name);

                }

                listBox_gameslist.Items.Add(gamesList[i].name);

            }

        }
        #endregion


        #region Game Management
        WebClient webClient = new WebClient();
        public void DownloadGame(int index)
        {

            //make download dir if it doesnt exist
            if (!Directory.Exists(downloadPath))
                Directory.CreateDirectory(downloadPath);

            //remove previous download
            if (File.Exists(downloadPath + gamesList[index].zip))
            {
                File.Delete(downloadPath + gamesList[index].zip);
            }


            webClient.DownloadProgressChanged += (s, e) => {
                progbar_download.Value = e.ProgressPercentage;
                if (e.ProgressPercentage >= 100)
                {
                    progbar_download.Visibility = Visibility.Hidden;
                    Debug.WriteLine("Download COmplete");
                    InstallGame(index);
                }
            };

            progbar_download.Visibility = Visibility.Visible;

            webClient.DownloadFileAsync(new Uri(gamesList[index].download), downloadPath + gamesList[index].zip);




        }

        public void InstallGame(int index)
        {
            //create game folder if it doenst exist
            if (!Directory.Exists(gamePath))
            {
                Directory.CreateDirectory(gamePath);
            }

            //extract zip
            ZipFile.ExtractToDirectory(downloadPath + gamesList[index].zip, gamePath + gamesList[index].id);

            //delete download
            File.Delete(gamePath + gamesList[index].zip);

            btn_Play.Content = "Play";

        }

        public void DeleteGame(int index)
        {
            if (Directory.Exists(gamePath + gamesList[index].id))
            {
                Directory.Delete(gamePath + gamesList[index].id);
                btn_Play.Content = "Download";
            }
        }

        public void PlayGame(int index)
        {
            Process.Start(gamePath + gamesList[index].id + "\\" + gamesList[index].exe);
        }

        #endregion

        #region Window Elements
        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {



            if (gamesList[currentGame].version != gamesList[currentGame].latestVersion && !string.IsNullOrEmpty(gamesList[currentGame].version))
            {
                DownloadGame(currentGame);
            }
            else if (Directory.Exists(gamePath + gamesList[currentGame].id))
            {
                PlayGame(currentGame);
            }
            else
            {
                DownloadGame(currentGame);
            }
        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            currentGame = listBox_gameslist.SelectedIndex;
        }

        private FlowDocument Description(string _contentpath)
        {
            Markdown renderer = new Markdown();
            string htmlSrc = System.IO.File.ReadAllText(_contentpath + "README.md");
            FlowDocument doc = renderer.Transform(htmlSrc);
            return doc;
        }
        #endregion
    }
}
