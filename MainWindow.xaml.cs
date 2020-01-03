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
using System.IO.Compression;
using System.IO;
using Microsoft.Win32;
using System.ComponentModel;
using System.Net;
using System.Threading;
using System.Windows.Threading;
using System.Windows.Media.Converters;


using System.Diagnostics;

namespace Patch_Tasang2
{
    public class webFile
    {
        public string url;
        public bool isPaks;
        public bool iszip;
        
        public webFile(string url,bool iszip = false , bool isPaks = false)
        {
            this.url = url;
            this.iszip = iszip;
            this.isPaks = isPaks;
            
        }
        
    }
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        bool check;
        string strDownPath;
        string strpatchPath;
        string strGameVersion;
        string ServerVersion;
        int index;
        double totalPercent;
        double TotalPercentTempSlot;
        private List<webFile> files;
        public MainWindow()
        {
            InitializeComponent();
            strDownPath = "";
            strpatchPath = "";
            strGameVersion = "";
            ServerVersion = CheckServerVersion();
            check = false;
            index = 0;
            files = new List<webFile>();
            totalPercent = 0.0;
            TotalPercentTempSlot = 0.0;
            RegistrySearch();
            callImage();



        }
        private void RegistrySearch()
        {
            
            try
            {
                
                RegistryKey regKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\App Paths\\Patch_Tasang2.exe");
                //RegistryKey regKey = Registry.CurrentUser.OpenSubKey("Software\\GRETECH\\GomPlayer");
                
                strDownPath = (string)regKey.GetValue("download");
                strpatchPath = (string)regKey.GetValue("patching");
                strGameVersion = (string)regKey.GetValue("GameVersion");
                ServerVersionlabel.Content = ServerVersion;
                GameVersionlabel.Content = strGameVersion;
                if (strGameVersion == ServerVersion)
                {
                    check = true;
                    //downloadbutton.Visibility = Visibility.Visible;
                    downloadbutton.Content = "게임 시작";
                    downloadbutton.Click -= Download_Click;
                    downloadbutton.Click += Start_Click;

                    fileinfolabel.Content = "클라이언트 업데이트가 성공적으로 완료되었습니다.";
                    TotalProgress.Value = 100;
                    PartProgress.Value = 100;
                    percentlabel.Content = "100";


                    
                }
                else if(strGameVersion == "")
                {
                    check = false;
                    GameVersionlabel.Content = "미설치";
                }
                else //다른 버전일 가능성 
                {
                    check = false;
                    
                }
                    
                //percentlabel.Content = strDownPath;

            }
            catch (Exception e1) { }


        }

        static double ConvertBytesToMegabytes(long bytes)
        {
            return (bytes / 1024f) / 1024f;
        }

        private void ButtonFechar_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        
        private void htDownloadFile(string localFile, string clientFile, string name)
        {
            try
            {
                if (!Directory.Exists(localFile))
                {
                    Directory.CreateDirectory(localFile);
                }
                WebClient client = new WebClient();
                
                client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
                
               
                client.DownloadFileAsync(new Uri(clientFile), localFile+"\\"+ name+".zip");
                
            }
            catch(WebException e1) { MessageBox.Show(e1.Message); }
        }

        private void alldownload(string localFile, params webFile[] files)
        {
            downloadbutton.Content = "다운중...";
            downloadbutton.IsEnabled = false;
            index += 1;
                if (!files[0].isPaks)
                    htDownloadFile(strDownPath, files[0].url, "The Beast");

            }
        private void patchdownload(string localFile, params webFile[] files)
        {
            downloadbutton.Content = "다운중...";
            fileinfolabel.Content = "패치 다운로드중...";
            downloadbutton.IsEnabled = false;
            index += 2;
            htDownloadFile(strDownPath, files[index-1].url, "patch"+index);
        }
        
        #region
        /// <summary>
        /// ZIP 파일 추출하기
        /// </summary>
        /// <param name="zipFilePath">ZIP 파일 경로</param>
        /// <param name="backupFolder">백업 폴더</param>
        private void ExtractZIPFile(string zipFilePath, string backupFolder)
        {
            

            int count = 0;
                int readcount = 0;
                if (!Directory.Exists(backupFolder))
                {
                    Directory.CreateDirectory(backupFolder);
                }

                using (ZipArchive zipArchive = ZipFile.OpenRead(zipFilePath))
                {


                    readcount = zipArchive.Entries.Count();
                
                    foreach (ZipArchiveEntry zipArchiveEntry in zipArchive.Entries)
                    {
                        try
                        {
                        
                        count += 1;

                        string folderPath = Path.GetDirectoryName(Path.Combine(backupFolder, zipArchiveEntry.FullName));

                            if (!Directory.Exists(folderPath))
                            {
                                Directory.CreateDirectory(folderPath);
                            }
                            else
                            {
                                
                                zipArchiveEntry.ExtractToFile(Path.Combine(backupFolder, zipArchiveEntry.FullName),true);
                            }

                        }
                        catch (PathTooLongException)
                        {
                        }
                    }
                }

                try
                {
                    System.IO.File.Delete(zipFilePath);
                }
                catch (System.IO.IOException e)
                {
                    Console.WriteLine(e.Message);
                    return;
                }


            

            //downloadbutton.BeginInit();

        }

        #endregion
        
        private void Start_Thread()
        {
            foreach (webFile wb in files)
            {
                if (!wb.isPaks && strGameVersion == "")
                    ExtractZIPFile(strDownPath + "\\The Beast.zip", strDownPath + "\\The Beast");
                else if (wb.isPaks)
                    ExtractZIPFile(strDownPath + "\\patch" + index + ".zip", strDownPath + "\\The Beast\\ContentOptimization\\" + strpatchPath);

            }
            this.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
            {
                RegistryKey regKey = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\App Paths\\Patch_Tasang2.exe");
                regKey.SetValue("GameVersion", ServerVersion);
                fileinfolabel.Content = "클라이언트 업데이트가 성공적으로 완료되었습니다.";
                fileinfolabel.BeginInit();
                fileinfolabel.EndInit();
            });
            this.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
            {




                downloadbutton.IsEnabled = true;
                downloadbutton.Content = "게임 시작";
                downloadbutton.Click -= Download_Click;
                downloadbutton.Click += Start_Click;

                downloadbutton.BeginInit();


            });
        }


        private void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
           
            totalPercent = (e.ProgressPercentage / files.Count) + TotalPercentTempSlot;
            percentlabel.Content = int.Parse(totalPercent.ToString()).ToString();

            TotalProgress.Value = totalPercent;
            PartProgress.Value = e.ProgressPercentage;

            Totalbytelabel.Content = ConvertBytesToMegabytes(e.BytesReceived).ToString("f") + "MB / " + ConvertBytesToMegabytes(e.TotalBytesToReceive).ToString("f") + "MB";
            //fileinfolabel.Content = e.UserState.ToString();
            if(e.TotalBytesToReceive == e.BytesReceived && !check)
            {
                
                //htDownloadFile(files[0].url, strDownPath + "\\The Beast\\ContentOptimization\\" + strpatchPath);
            }
        }
        

        private void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
           
            fileinfolabel.Content = "패치 다운로드 중 ...";
            TotalPercentTempSlot += totalPercent;
            if (index < files.Count)
            {
                PartProgress.Value = 0;

                    

                 index++;
                htDownloadFile(strDownPath, files[index-1].url, "patch" + index);

                /*WebClient nextDownload = new WebClient();
                nextDownload.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                nextDownload.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
                nextDownload.DownloadFileAsync(new Uri(files[index].url), strDownPath + "\\patch"+ index+".zip");*/
                //TotalProgress.Value = (Convert.ToDouble(index) / Convert.ToDouble(files.Count)) * 100;

            }
            else
            {
                this.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
                {

                    fileinfolabel.Content = "해제중...";
                    fileinfolabel.BeginInit();
                    fileinfolabel.EndInit();
                });
                Thread thread = new Thread(Start_Thread);
                thread.Start();
                
                
            }

        }
        private object ExitZip(object obj)
        {
            
            return null;
        }
        private string CheckServerVersion()
        {

            WebClient client2 = new WebClient();

            string line;

            // Read the file and display it line by line.  
            Stream stream = client2.OpenRead("http://artemistime.ipdisk.co.kr:8000/list/HDD1/ServerVersion.txt");
            StreamReader file = new StreamReader(stream);
            line = file.ReadLine();

            file.Close();
            return line;
        }
        private void download_list()
        {

            //webFile list = new webFile("http://artemistime.ipdisk.co.kr:8000/list/HDD1/patchmo.txt");
            WebClient client3 = new WebClient();
            //client.DownloadFileAsync(new Uri(clientFile), localFile);
            
            int counter = 0;
            string line;

            // Read the file and display it line by line.  
            Stream stream = client3.OpenRead("http://artemistime.ipdisk.co.kr:8000/list/HDD1/patchmo.txt");
            StreamReader file = new StreamReader(stream);
            while ((line = file.ReadLine()) != null)
            {
                if(counter == 0)
                    files.Add(new webFile(line, true));
                else
                    files.Add(new webFile(line, true, true));

                counter++;
            }

            file.Close();
            
        }
        private void Download_Click(object sender, RoutedEventArgs e)
        {
            //downloadbutton.Visibility = Visibility.Hidden;
            //downloadbutton.IsEnabled = false;
            download_list();
            if (strDownPath == "")
                MessageBox.Show("클라이언트를 재설치 해주세요.");
            else if(check == false && strGameVersion == "")
            {
                alldownload(strDownPath, files.ToArray());
            }
            else if(strGameVersion != ServerVersion)
            {
                patchdownload(strDownPath, files.ToArray());
            }
            else if(strGameVersion == ServerVersion)
            {
                downloadbutton.Visibility = Visibility.Visible;
                downloadbutton.Content = "게임 시작";
                downloadbutton.Click -= Download_Click;
                downloadbutton.Click += Start_Click;
            }
            
        }
        private void Start_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("링크스타트!");
            Process.Start(new ProcessStartInfo(strDownPath + "\\The Beast\\ContentOptimization.exe"));
        }
        private void topbar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        void OnMouseEnterHandler(object sender, MouseEventArgs e)
        {
            homepage.Foreground = new SolidColorBrush(Color.FromRgb(228,255,255));
        }

        // raised when mouse cursor leaves the area occupied by the element
        void OnMouseLeaveHandler(object sender, MouseEventArgs e)
        {
            homepage.Foreground = new SolidColorBrush(Color.FromRgb(214, 220, 231));
        }
        void OnMouseEnterHandler2(object sender, MouseEventArgs e)
        {
            Youtube.Foreground = new SolidColorBrush(Color.FromRgb(228, 255, 255));
        }

        // raised when mouse cursor leaves the area occupied by the element
        void OnMouseLeaveHandler2(object sender, MouseEventArgs e)
        {
            Youtube.Foreground = new SolidColorBrush(Color.FromRgb(214, 220, 231));
        }

        private void Homepage_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        public BitmapImage LoadImage(string url)   //Image URL -> Bitmap으로.. 
        {
            try
            {
                if (string.IsNullOrEmpty(url))
                    return null;
                WebClient wc = new WebClient();
                Byte[] MyData = wc.DownloadData(url);
                wc.Dispose();
                BitmapImage bimgTemp = new BitmapImage();
                bimgTemp.BeginInit();
                bimgTemp.StreamSource = new MemoryStream(MyData);
                bimgTemp.EndInit();
                return bimgTemp;
            }
            catch
            {
                return null;
            }
        }


        
        private void callImage()
        {

            WebClient client = new WebClient();

            //int counter = 0;
            string line;


            Stream stream = client.OpenRead("http://artemistime.ipdisk.co.kr:8000/list/HDD1/site/www/ImageSource.txt");
            StreamReader file = new StreamReader(stream);
            line = file.ReadLine();
            /*while ((line = file.ReadLine()) != null)
            {
               

                counter++;
            }*/
            
            file.Close();
            
            boardImage.ImageSource = LoadImage(line);

        }


        private void BoardImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start(new ProcessStartInfo("http://artemistime.tk"));
            e.Handled = true;
        }
    }
}
