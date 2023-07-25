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
using Silverzone.CommonLib.Ex;
using Silverzone.CommonLib;
using Silverzone.CommonLib.API;
using Silverzone.CommonLib.Log;
using System.Threading;

namespace Silverzone_Launcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal static readonly CommonLib clib = new();
        internal static readonly WebClient client = new();
        internal readonly StateManager sm = new();

        [Flags]
        public enum P_Flags : short
        {
            NoClib = 0,
            Clib = 1,
            NoWClient = 2,
            WClient = 4,
            NoLog = 8,
            Log = 16,
        }

        private readonly short flags;

        #region System Startup
        public MainWindow()
        {
            Logger.LoggerInit();
            if (timer1 == null) throw new Exceptions.ApplicationComponentException();
            InitializeComponent();


            //Start();

            //StateManager sm = new();
            //if (sm == null) throw new Exception(nameof(sm));
            sm.DownloadGamesList();
            sm.DownloadData();
            sm.ProcessData();

            InitTimer();
            AutoUpdater.Start("https://raw.githubusercontent.com/Silverzonegames/Launcher/main/Data/launcher/updates.xml");
        }

        private readonly System.Timers.Timer timer1 = new();
        public void InitTimer() {
            timer1.Elapsed += Tick;
            timer1.Interval = 100; // in milliseconds
            timer1.Start();
        }

        private void Tick(object? sender, EventArgs e) {
            if (sender == null) return;
            try {
                Dispatcher.Invoke(() =>
                {
                    Update();
                });
            }
            catch (Exception ex) {
               Logger.LogException(ex, Logger.Severity.Error);
            }

        }
        #endregion

        #region Update

        internal static void Start() {
            
        }

        public void Update()
        {
            if (Directory.Exists(sm.gamePath + sm.gamesList[sm.currentGame].id)) {
                btn_Play.Content = "Play";
                btn_Play.Background = new SolidColorBrush(Colors.Green);
            }
            else {
                btn_Play.Content = "Download";
                btn_Play.Background = new SolidColorBrush(Colors.Blue);
            }
            if (sm.gamesList[sm.currentGame].version != sm.gamesList[sm.currentGame].latestVersion && !string.IsNullOrEmpty( sm.gamesList[sm.currentGame].version)) {
                btn_Play.Content = "Update to" + sm.gamesList[sm.currentGame].latestVersion;
                btn_Play.Background = new SolidColorBrush(Colors.Blue);
            }

            lbl_playButton.Content = sm.gamesList[sm.currentGame].name;
            lbl_Version.Content = "v" + sm.gamesList[sm.currentGame].version;
            var desc = sm.gamesList[sm.currentGame].desc;
            if (desc == null) return;
            lbl_desc.Content = desc.Replace(@"\", "\n");

            if (string.IsNullOrEmpty(sm.gamesList[sm.currentGame].version) && File.Exists(sm.gamePath + sm.gamesList[sm.currentGame].id + "\\version.txt")) {         
                sm.gamesList[sm.currentGame].version = File.ReadAllText(sm.gamePath + sm.gamesList[sm.currentGame].id + "\\version.txt").TrimEnd().TrimStart();
            }
        }


        #endregion

        protected void btn_Play_Click(object sender, RoutedEventArgs e)
        {
            if (sm.gamesList[sm.currentGame].version != sm.gamesList[sm.currentGame].latestVersion && !string.IsNullOrEmpty(sm.gamesList[sm.currentGame].version))
            {
                GameManager.DownloadGame(sm.currentGame);
            }
            else if (Directory.Exists(sm.gamePath + sm.gamesList[sm.currentGame].id))
            {
                GameManager.PlayGame(sm.currentGame);
            }
            else
            {
                GameManager.DownloadGame(sm.currentGame);
            }
        }

        protected void listBox_gameslist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            sm.currentGame = listBox_gameslist.SelectedIndex;
        }

        private static FlowDocument Description(string _contentpath)
        {
            Markdown renderer = new();
            string htmlSrc = System.IO.File.ReadAllText(_contentpath + "README.md");
            FlowDocument doc = renderer.Transform(htmlSrc);
            return doc;
        }


        public static void GamelistInsert(object? item, MainWindow window)
        {
            if (item == null) throw new Exception("ITEM is null");
            var win = ((MainWindow)window).listBox_gameslist;
            if (win == null) throw new Exception("Window is NULL, help");
            win.Items.Add(item);
        }
    }
}
