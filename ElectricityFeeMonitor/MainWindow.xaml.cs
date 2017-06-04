using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Win32;
using System.Windows.Forms;

namespace ElectricityFeeMonitor
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        private NotifyIcon notifyIcon;
        public MainWindow()
        {
            InitializeComponent();
            try
            {
                GetRoomNumberFromTxt();
                DataUpdate();
            }
            catch(Exception exception)
            {

            }
            this.notifyIcon = new NotifyIcon();
            this.notifyIcon.BalloonTipText = "Electricity Fee Monitor is running...";
            this.notifyIcon.BalloonTipTitle = "Electricity Fee Monitor";
            this.notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath);
            this.notifyIcon.Visible = true;
            notifyIcon.DoubleClick += new EventHandler(ShowMainWindow);
            System.Windows.Forms.MenuItem openMain = new System.Windows.Forms.MenuItem("Open main window");
            openMain.Click += new EventHandler(ShowMainWindow);
            System.Windows.Forms.MenuItem showBalance = new System.Windows.Forms.MenuItem("Show balance");
            showBalance.Click += new EventHandler(ShowBalance);
            System.Windows.Forms.MenuItem exit = new System.Windows.Forms.MenuItem("Exit");
            exit.Click += new EventHandler(Close);
            System.Windows.Forms.MenuItem[] children = new System.Windows.Forms.MenuItem[] { openMain, showBalance, exit };
            notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(children);

            DispatcherTimer wpfTimer = new DispatcherTimer();
            wpfTimer.Interval = TimeSpan.FromHours(1);
            wpfTimer.Tick += TickTock;
            wpfTimer.Start();
        }
        private void TickTock(object sender, EventArgs e)
        {
            DataUpdate();
        }
        private void ShowMainWindow(object sender,EventArgs e)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            this.ShowInTaskbar = true;
        }
        private void ShowBalance(object sender, EventArgs e)
        {
            DataUpdate();
            System.Windows.MessageBox.Show(lBalance.Content.ToString());
        }
        private void Hide(object sender,EventArgs e)
        {
            this.ShowInTaskbar = false;
            this.Visibility = Visibility.Hidden;
        }
        private void Close(object sender,EventArgs e)
        {
            this.notifyIcon.Dispose();
            System.Windows.Application.Current.Shutdown();
        }
        public void DataUpdate()
        {
            if (Ping())
            {
                Dictionary<string, dynamic> dictionary = GetBalance(GetId(textBox_roomNumber.Text));
                if (dictionary["success"])
                {
                    lOnline.Content = "Online √";
                    lOnline.Foreground = System.Windows.Media.Brushes.Green;
                    lBalance.Content = "￥" + dictionary["balance"];
                    if (Convert.ToDouble(dictionary["balance"]) < 20.0f)
                    {
                        System.Windows.MessageBox.Show("Account balance is less than 20 !", "Low account balance !", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                lOnline.Content = "Offline X";
                lOnline.Foreground = System.Windows.Media.Brushes.Red;
            }
        }
        public void timeCheck(object sender, EventArgs e)
        {
            Convert.ToUInt16(DateTime.Now.ToString("mm"));
        }
        public Boolean Ping()
        {
            Ping ping = new Ping();
            PingReply pR = ping.Send("10.136.2.5",5);
            if (pR.Status == IPStatus.TimedOut) return false;
            else return true;
        }
        public Tuple<CookieContainer, dynamic> GetId(string account)
        {
            string url = "http://10.136.2.5/jnuweb/WebService/JNUService.asmx/Login";
            HttpWebRequest httpRequest = (HttpWebRequest)HttpWebRequest.Create(url);
            string DateTime;
            httpRequest.Method = "POST";
            httpRequest.KeepAlive = true;
            httpRequest.Accept = "*/*";
            httpRequest.ContentType = "application/json";
            httpRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/57.0.2987.133 Safari/537.36";
            httpRequest.Headers.Add("Token", GetToken(out DateTime));
            httpRequest.Headers.Add("DateTime", DateTime);
            CookieContainer cookieJar=new CookieContainer();
            httpRequest.CookieContainer = cookieJar;
            string json = "{\"user\":\"" + account + "\",\"password\":\"2ay/7lGoIrXLc9KeacM7sg==\"}";
            httpRequest.ContentLength = Encoding.UTF8.GetBytes(json).Length;
            using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
            {
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }
            HttpWebResponse httpresponse = (HttpWebResponse)httpRequest.GetResponse();
            Stream rs = httpresponse.GetResponseStream();
            StreamReader sr = new StreamReader(rs, Encoding.UTF8);
            dynamic data = JValue.Parse(sr.ReadToEnd());
            return new Tuple<CookieContainer, dynamic>(cookieJar, data.d.ResultList[0].customerId.Value);
        }
        //Get account information
        public Dictionary<string, dynamic> GetBalance(Tuple<CookieContainer, dynamic> loginData)
        {
            string DateTime;
            string url = "http://10.136.2.5/jnuweb/WebService/JNUService.asmx/GetUserInfo";
            HttpWebRequest httpRequest = (HttpWebRequest)HttpWebRequest.Create(url);
            httpRequest.Method = "POST";
            httpRequest.KeepAlive = true;
            httpRequest.Accept = "*/*";
            httpRequest.ContentType = "application/json";
            httpRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/57.0.2987.133 Safari/537.36";
            httpRequest.Headers.Add("Token", GetToken(loginData.Item2,out DateTime));
            httpRequest.Headers.Add("DateTime", DateTime);
            httpRequest.CookieContainer = loginData.Item1;
            httpRequest.ContentLength = 0;
            HttpWebResponse httpresponse = (HttpWebResponse)httpRequest.GetResponse();
            using (Stream responseStream = httpresponse.GetResponseStream())
            {
                using (StreamReader streamReader = new StreamReader(responseStream, Encoding.UTF8))
                {
                    dynamic data = JValue.Parse(streamReader.ReadToEnd());
                    Dictionary<string, dynamic> dict = new Dictionary<string, dynamic>()
                    {
                        {"success",Convert.ToBoolean(data.d.Success.Value)},
                        {"balance",data.d.ResultList[0].roomInfo[1].keyValue.Value},
                    };
                    return dict;
                }
            }
        }
        public static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
        //Get token for "GetId"
        public static string GetToken(out string dateTime)
        {
            dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string plainText = "{\"userID\":0,\"tokenTime\":\""+ dateTime+"\"}";
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            aes.Key = StringToByteArray("436574536f667445454d537973576562");
            aes.IV = StringToByteArray("1934577290ABCDEF1264147890ACAE45");
            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }
                    return Convert.ToBase64String(ms.ToArray())+ "%0A";
                }
            }
        }
        //Get token for "GetBalance"
        public static string GetToken(dynamic id, out string dateTime)
        {
            dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string plainText = "{\"userID\":"+id+",\"tokenTime\":\"" + dateTime + "\"}";
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            aes.Key = StringToByteArray("436574536f667445454d537973576562");
            aes.IV = StringToByteArray("1934577290ABCDEF1264147890ACAE45");
            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }
                    return Convert.ToBase64String(ms.ToArray()).Insert(64,"%0A");
                }
            }
        }

        private void button_save_Click(object sender, RoutedEventArgs e)
        {
            FileStream fileStream = new FileStream(System.AppDomain.CurrentDomain.BaseDirectory + "\\RoomNumber.txt", FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter streamWriter = new StreamWriter(fileStream);
            streamWriter.Flush();
            streamWriter.BaseStream.Seek(0, SeekOrigin.Begin);
            streamWriter.Write(textBox_roomNumber.Text);
            streamWriter.Flush();
            streamWriter.Close();
            textBox_roomNumber.IsEnabled = false;
            button_edit.IsEnabled = true;
            button_save.IsEnabled = false;
        }

        private void button_edit_Click(object sender, RoutedEventArgs e)
        {
            textBox_roomNumber.IsEnabled = true;
            button_edit.IsEnabled = false;
            button_save.IsEnabled = true;
        }
        private void GetRoomNumberFromTxt()
        {
            try
            {
                FileStream fileStream = new FileStream(System.AppDomain.CurrentDomain.BaseDirectory + "\\RoomNumber.txt", FileMode.Open, FileAccess.Read);
                StreamReader streamReader = new StreamReader(fileStream);
                textBox_roomNumber.Text = "";
                string roomNumber = streamReader.ReadLine();
                textBox_roomNumber.Text = roomNumber;
                streamReader.Close();
            }
            catch(Exception FileNotExistExcption)
            {
                FileStream fileStream = new FileStream(System.AppDomain.CurrentDomain.BaseDirectory + "\\RoomNumber.txt", FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter streamWriter = new StreamWriter(fileStream);
                streamWriter.Flush();
                streamWriter.BaseStream.Seek(0, SeekOrigin.Begin);
                streamWriter.Write("T10201");
                streamWriter.Flush();
                streamWriter.Close();
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            DataUpdate();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                this.ShowInTaskbar = false;
                notifyIcon.Visible = true;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.notifyIcon.Dispose();
        }
    }
}
