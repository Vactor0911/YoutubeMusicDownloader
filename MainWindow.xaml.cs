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
using YoutubeExplode;
using MediaToolkit;
using System.Net;
using YoutubeExplode.Videos;
using YoutubeExplode.Converter;
using static MediaToolkit.Model.Metadata;
using Microsoft.Win32;
using System.IO;
using System.Security.Cryptography;
using System.Net.Http;
using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Common;

namespace YoutubeMusicDownloader
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        YoutubeClient youtube = new YoutubeClient();
        YoutubeExplode.Videos.Video video;
        string title = "";

        public MainWindow()
        {
            InitializeComponent();
            tbTitle.IsEnabled = false;
            tbAuthor.IsEnabled = false;
            btnDownload.IsEnabled = false;
        }

        public void Error(string Message)
        {
            MessageBox.Show(Message, "Youtube Music Downloader", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private async void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Check URL
                string url = tbUrl.Text;
                if ( !url.Contains("youtube.com/") && !url.Contains("youtu.be/"))
                {
                    Error("Invalid Youtube URL!");
                    return;
                }

                //Get Youtube Video
                video = await youtube.Videos.GetAsync(url);

                //Print data of video
                title = video.Title;
                lblVideoTitle.Content = title;
                lblVideoDuration.Content = $"Duration : {video.Duration}";
                var thumbnail = video.Thumbnails.FirstOrDefault()?.Url;
                imgThumbnail.Source = new BitmapImage( new Uri(thumbnail) );

                //Initialize textbox
                tbTitle.IsEnabled = true;
                tbAuthor.IsEnabled = true;
                btnDownload.IsEnabled = true;

                if ( !title.Contains("-") )
                {
                    tbTitle.Text = title;
                    tbAuthor.Text = title;
                    return;
                }

                string[] aryTitle = title.Split('-');
                if (aryTitle.Length <= 0 || aryTitle.Length > 2)
                {
                    tbTitle.Text = title;
                    tbAuthor.Text = title;
                }
                else
                {
                    tbTitle.Text = aryTitle[1].Trim();
                    tbAuthor.Text = aryTitle[0].Trim();
                }
            }
            catch (Exception)
            {
                Error("Failed to search youtube video!");
            }
        }

        private void imgThumbnail_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (video == null)
                return;

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png & *.jpg & *.jpeg)|*.png;*.jpg;*.jpeg|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                var image = openFileDialog.FileName;
                imgThumbnail.Source = new BitmapImage( new Uri(image) );
            }
        }

        private async void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            if (video == null)
                return;

            //Get path
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Audio files (*.mp3)|*.mp3";
            saveFileDialog.FileName = tbTitle.Text + " - " + tbAuthor.Text;
            string savePath = "";
            if (saveFileDialog.ShowDialog() == true && saveFileDialog.FileName != string.Empty)
            {
                savePath = saveFileDialog.FileName;
                var streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);
                var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
                await youtube.Videos.Streams.DownloadAsync(streamInfo, savePath);
                MessageBox.Show("Successfully downloaded music!");
            }

            //Save file
            
        }

        private void btnImageReset_Click(object sender, RoutedEventArgs e)
        {
            if (video == null)
                return;

            var thumbnail = video.Thumbnails.FirstOrDefault()?.Url;
            imgThumbnail.Source = new BitmapImage( new Uri(thumbnail) );
        }
    }
}
