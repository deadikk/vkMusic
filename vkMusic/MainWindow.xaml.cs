using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace vkMusic
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Initializemy();
        }

        
        #region loginRegion
        waiting w;
        private void login_Click(object sender, RoutedEventArgs e)
        {
            w = new waiting();
            w.Show();
            browser.Visibility = Visibility.Visible;
            browser.Navigate(Core.authUrl());
        }
        private void browser_LoadCompleted(object sender, NavigationEventArgs e)
        {
            string userId,token;

            if (Core.browserParser(browser.Source.ToString(), out token , out userId)) authSuccess(true,token,userId);
            
            else authSuccess(false,null,null);

        }

        private void loadFriendsToList() {
            List<Friend> friends = new List<Friend>();
            friends = Core.getFriendList(data.Default.userId);
            foreach (Friend f in friends) {
                friendsCombo.Items.Add(string.Format("{0} - {1} {2}",f.id,f.name,f.surname));
            }
        }

        private void authSuccess(bool auth, string t,string i) {
            try { w.Close(); }
            catch { }
            if (auth) { 
                //действия при удачной аутентификации
                loginStatusTxt.Text = string.Format("Вход выполнен({0})",Core.nameById(i));
               // loginBtn.Content = "Выход";
                browser.Visibility = Visibility.Collapsed;
                data.Default.token = t;
                data.Default.userId = i;
                data.Default.auth = true;
                loginBtn.IsEnabled = false;
                loadFriendsToList();
                showSong.IsEnabled = true;
                friendsCombo.IsEnabled = true;
            
            }

            else
            {
                loginStatusTxt.Text = "Выполните вход в свой аккаунт";
                
                data.Default.auth = false;


            }
        }
        #endregion


        #region songsRegion
        List<Song> songs = new List<Song>();
        private void showSongs(object sender, RoutedEventArgs e)
        {
            getSongs(data.Default.userId);
        }

        private void getSongs(string id) {
            string all;
            data.Default.currentId=id;
            songs = Core.getMusic(id, data.Default.currentOffset, data.Default.count, out all);
            songsCountLbl.Content = string.Format("Песни: {0} - {1} из {2}",data.Default.currentOffset,data.Default.count,all);
            generateSongInView(songs);

        }

        private void generateSongInView(List<Song> songs)
        {
            int counter = 0;
            gridRight.ShowGridLines = true;
            gridRight.Children.Clear();
            gridRight.RowDefinitions.Clear();

            if (songs==null) return;
            foreach (Song s in songs)
            {

                Button btn = new Button();
                Button btnBuff = new Button();
                Button btnPlay = new Button();
                Label lbl = new Label();

                lbl.Content = string.Format("{0} - {1}", s.Artist, s.Name);

                btnPlay.Tag = s.url;
                btnPlay.Click += btnPlay_Click;
                btnPlay.Content = "Play";
               
                
                btn.Content = "Скачать";
                btn.ToolTip = string.Format("{2}\\{0} - {1}.mp3", s.Artist, s.Name, data.Default.path);

                btnBuff.Content = "Copy Link";
                btnBuff.ToolTip = s.url;

                btn.Click += btn_Click;
                btn.Tag = s;

                btnBuff.Click += btnBuff_Click;
                btnBuff.Tag = s;

                RowDefinition r = new RowDefinition();
                r.Height = new GridLength(30);

                btn.SetValue(Grid.RowProperty, counter);
                btnBuff.SetValue(Grid.RowProperty, counter);
                lbl.SetValue(Grid.RowProperty, counter);
                btnPlay.SetValue(Grid.RowProperty, counter);


                btn.SetValue(Grid.ColumnProperty, 0);
                btnBuff.SetValue(Grid.ColumnProperty, 1);
                lbl.SetValue(Grid.ColumnProperty, 2);
                btnPlay.SetValue(Grid.ColumnProperty, 3);

                gridRight.RowDefinitions.Add(r);



                gridRight.Children.Add(btn);
                gridRight.Children.Add(btnBuff);
                gridRight.Children.Add(lbl);
                gridRight.Children.Add(btnPlay);



                counter++;

            }

        }

        Button lastPushed;

        void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button).Content.ToString() == "Play")
            {
                if (lastPushed != null) lastPushed.Content = "Play";
                lastPushed = sender as Button;
                media.Stop();
                media.Source = new Uri((sender as Button).Tag as String);
                media.Play();
                (sender as Button).Content = "Stop";

                
            }

            else {
                media.Stop();
                
                (sender as Button).Content = "Play";
                
                
            }
        }

        void lbl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            

        }

        private void btnBuff_Click(object sender, RoutedEventArgs e)
        {
            Button current = (Button)sender;
            Song s = current.Tag as Song;

            Clipboard.SetData(DataFormats.Text, s.url);
           // MessageBox.Show("Ссылка скопирована");
           
        }

       
       
        void btn_Click(object sender, RoutedEventArgs e)
        {
            if (data.Default.path == string.Empty) {
                MessageBox.Show("Выберите папку для загрузки!");
                return;
            }
            //добавление в список загрузок при клике на кнопку
            Button current=(Button) sender;
            Song s=current.Tag as Song;

            string file = string.Format("{2}\\{0} - {1}.mp3", s.Artist, s.Name, data.Default.path);
            file = file.Replace('"', '_');

            downloadList.Add(new Download { url = s.url, name = file });
            if (downloadList.Count == 1) downloadProcess();
            listDownloads.Items.Clear();
            foreach (Download i in downloadList)
            {
                listDownloads.Items.Add(i.name.Split('-')[1]);
            }
        }
        
        private void friendsCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            getSongs((sender as ComboBox).SelectedItem.ToString().Split(' ')[0]);
        }

        private void Prev_Click(object sender, RoutedEventArgs e)
        {
            data.Default.currentOffset = (data.Default.currentOffset - data.Default.count) < 0 ? 0 : data.Default.currentOffset - data.Default.count;
            getSongs(data.Default.currentId);
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            data.Default.currentOffset += data.Default.count;
            getSongs(data.Default.currentId);

        }
        #endregion

        #region downloader

        private void Initializemy()
        {
            myWebClient = new WebClient();
            myWebClient.DownloadFileCompleted += myWebClient_DownloadFileCompleted;
            myWebClient.DownloadProgressChanged += myWebClient_DownloadProgressChanged;
        }

        private void myWebClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null) MessageBox.Show(e.Error.ToString());
            downloadList.RemoveAt(0);
            downloadProcess();
           // statusTxt.Text = "";
            statusProgress.Value = 0;

            listDownloads.Items.Clear();
            foreach (Download i in downloadList)
            {
                listDownloads.Items.Add(i.name.Split('-')[1]);
            }
        }

        void myWebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            statusProgress.Value = e.ProgressPercentage;
            //statusTxt.Text = downloadList[0].name;
            //downloadList.First(x => x.url == (sender as WebClient).BaseAddress).name; ;

        }

        List<Download> downloadList = new List<Download>();
        WebClient myWebClient;
        private void downloadProcess()
        {


            if (downloadList.Count > 0)
            {

                myWebClient.DownloadFileAsync(new Uri(downloadList[0].url), downloadList[0].name);

            }

            else
            {
                listDownloads.Items.Clear();
            }

        }



        #endregion

        #region payer
        private void folderBtn_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog sv = new System.Windows.Forms.FolderBrowserDialog();
            if (sv.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                folderTxt.Text = sv.SelectedPath;
                data.Default.path = sv.SelectedPath;
            }
        }

        private void media_BufferingStarted(object sender, RoutedEventArgs e)
        {
           

        }

        private void media_BufferingEnded(object sender, RoutedEventArgs e)
        {
            
        }

        private void stopPlayer(object sender, RoutedEventArgs e)
        {
            media.Stop();
        }

        private void pausePlayer(object sender, RoutedEventArgs e)
        {
            media.Pause();
            
        }

        private void media_MediaEnded(object sender, RoutedEventArgs e)
        {

        }

        private void media_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {

        }

        private void playPlayer(object sender, RoutedEventArgs e)
        {
            media.Play();
        }
        #endregion
    }
}
