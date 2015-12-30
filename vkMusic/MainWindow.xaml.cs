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


        private void Initializemy()
        {
            myWebClient = new WebClient();
            myWebClient.DownloadFileCompleted += myWebClient_DownloadFileCompleted;
            myWebClient.DownloadProgressChanged += myWebClient_DownloadProgressChanged;
            scrollViewer.ScrollChanged += OnScrollChanged;
            login_Click(null, null);
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
            string userId, token;

            if (Core.browserParser(browser.Source.ToString(), out token, out userId)) authSuccess(true, token, userId);

            else authSuccess(false, null, null);

        }

        private void loadFriendsToList()
        {
            List<Friend> friends = new List<Friend>();
            friends = Core.getFriendList(data.Default.userId);
            foreach (Friend f in friends)
            {
                friendsCombo.Items.Add(string.Format("{0} - {1} {2}", f.id, f.name, f.surname));
            }
        }

        private void authSuccess(bool auth, string t, string i)
        {
            try { w.Close(); }
            catch { }
            if (auth)
            {
                //действия при удачной аутентификации
                loginStatusTxt.Text = string.Format("Вход выполнен({0})", Core.nameById(i));
                // loginBtn.Content = "Выход";
                browser.Visibility = Visibility.Collapsed;
                data.Default.token = t;
                data.Default.userId = i;
                data.Default.auth = true;
                loginBtn.IsEnabled = false;
                loadFriendsToList();
                showSong.IsEnabled = true;
                friendsCombo.IsEnabled = true;
                showSongs(null, null);
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
        int counter = 0;
        private void showSongs(object sender, RoutedEventArgs e)
        {
            songs = getSongs(data.Default.userId);
            data.Default.count = songs.Count;
            generateSongInView(songs);
        }

        private List<Song> getSongs(string id)
        {
            string all;
            data.Default.currentId = id;
            List<Song> songsList = Core.getMusic(id, data.Default.currentOffset, data.Default.count, out all);
            songsCountLbl.Content = string.Format("Песни: 1 - {0} из {1}", data.Default.currentOffset + data.Default.count, all);
            return songsList;
        }

        private void generateSongInView(List<Song> songsList)
        {

            gridRight.ShowGridLines = true;
            gridRight.Children.Clear();

            if (songsList == null) return;

            addSongsToView(songsList);
        }

        private void addSongsToView(List<Song> songsList)
        {
            if (songsList == null) return;
            foreach (Song s in songsList)
            {

                Button btn = new Button();
                Button btnBuff = new Button();
                Button btnPlay = new Button();
                Label lbl = new Label();

                lbl.Content = string.Format("{0} - {1}", s.Artist, s.Name);
                lbl.Tag = s.LyricsId;
                lbl.MouseDoubleClick += lbl_MouseDoubleClick;
                if(s.LyricsExist)lbl.Foreground = new SolidColorBrush(Color.FromRgb(0,50,150));

                btnPlay.Tag = s.url;
                btnPlay.Click += btnPlay_Click;
                btnPlay.Content = "▷";
                btnPlay.FontSize = 20;



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
                btnPlay.SetValue(Grid.RowProperty, counter);
                lbl.SetValue(Grid.RowProperty, counter);


                btn.SetValue(Grid.ColumnProperty, 0);
                btnBuff.SetValue(Grid.ColumnProperty, 1);
                btnPlay.SetValue(Grid.ColumnProperty, 2);
                lbl.SetValue(Grid.ColumnProperty, 3);

                gridRight.RowDefinitions.Add(r);

                lbl.Background = Brushes.LightGray;

                gridRight.Children.Add(btn);
                gridRight.Children.Add(btnBuff);
                gridRight.Children.Add(btnPlay);
                gridRight.Children.Add(lbl);

                counter++;

            }
        }

        void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            List<Button> playButtons = new List<Button>();
            List<Label> playLabels = new List<Label>();

            for (int i = 0; i < songs.Count; i++)
            {
                playButtons.Add(gridRight.Children[i * 4 + 2] as Button);
                playLabels.Add(gridRight.Children[i * 4 + 3] as Label);
            }
            int indexofButton = 0;

            if ((sender as Button).Content.ToString() == "▷")
            {



                foreach (Button playButton in playButtons)
                {
                    playButton.Content = "▷";
                    if ((sender as Button) == playButton)
                    {
                        indexofButton = playButtons.IndexOf(playButton);
                    }
                }

                foreach (Label playLabel in playLabels)
                {
                    playLabel.Background = Brushes.LightGray;
                }
                media.Stop();
                media.Source = new Uri((sender as Button).Tag as String);
                media.Play();

                (sender as Button).Content = "◻";
                playLabels[indexofButton].Background = Brushes.LightGreen;


            }

            else {
                media.Stop();

                (sender as Button).Content = "▷";
                foreach (Label playLabel in playLabels)
                {
                    playLabel.Background = Brushes.LightGray;
                }


            }
        }

        void lbl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Label lbl = sender as Label;
            if (lbl.Tag.ToString() != String.Empty)
            {
                TextViewer.Visibility = Visibility.Visible;
                TextLabel.Content = Core.getLyric(lbl.Tag.ToString());
            }
            else
            {
                TextViewer.Visibility = Visibility.Hidden;
            }
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
            if (data.Default.path == string.Empty)
            {
                MessageBox.Show("Выберите папку для загрузки!");
                return;
            }
            //добавление в список загрузок при клике на кнопку
            Button current = (Button)sender;
            Song s = current.Tag as Song;

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



        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {

            if (scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight && scrollViewer.VerticalOffset > 0)
            {
                data.Default.currentOffset += data.Default.count;
                var currentSongs = getSongs(data.Default.currentId);
                songs.AddRange(currentSongs);
                addSongsToView(currentSongs);
            }

        }

        #endregion



        #region downloader
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

        #region player
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
            List<Button> playButtons = new List<Button>();
            List<Label> playLabels = new List<Label>();

            for (int i = 0; i < songs.Count; i++)
            {
                playButtons.Add(gridRight.Children[i * 4 + 2] as Button);
                playLabels.Add(gridRight.Children[i * 4 + 3] as Label);
            }

            foreach (Button playButton in playButtons)
            {
                playButton.Content = "▷";
            }

            foreach (Label playLabel in playLabels)
            {
                playLabel.Background = Brushes.LightGray;
            }
        }

        private void pausePlayer(object sender, RoutedEventArgs e)
        {
            media.Pause();

        }

        private void media_MediaEnded(object sender, RoutedEventArgs e)
        {
            nextSong(null, null);
        }

        private void media_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {

        }

        private void playPlayer(object sender, RoutedEventArgs e)
        {
            media.Play();
        }

        private void prevSong(object sender, RoutedEventArgs e)
        {
            int index = songs.IndexOf(songs.Find(x => x.url == media.Source.ToString()));
            if (index == 0) return;
            (gridRight.Children[index * 4 + 2] as Button).Content = "▷";
            (gridRight.Children[index * 4 + 3] as Label).Background = Brushes.LightGray;

            media.Stop();
            media.Source = new Uri(songs[--index].url);
            media.Play();

            (gridRight.Children[index * 4 + 2] as Button).Content = "◻";
            (gridRight.Children[index * 4 + 3] as Label).Background = Brushes.LightGreen;
        }

        private void nextSong(object sender, RoutedEventArgs e)
        {

            int index = songs.IndexOf(songs.Find(x => x.url == media.Source.ToString()));
            if (index >= songs.Count - 1)
            {
                data.Default.currentOffset += data.Default.count;
                var currentSongs = getSongs(data.Default.currentId);
                songs.AddRange(currentSongs);
                addSongsToView(currentSongs);
            }

            (gridRight.Children[index * 4 + 2] as Button).Content = "▷";

            (gridRight.Children[index * 4 + 3] as Label).Background = Brushes.LightGray;

            media.Stop();
            media.Source = new Uri(songs[++index].url);
            media.Play();

            (gridRight.Children[index * 4 + 2] as Button).Content = "◻";

            (gridRight.Children[index * 4 + 3] as Label).Background = Brushes.LightGreen;


        }



        #endregion
    }
}
