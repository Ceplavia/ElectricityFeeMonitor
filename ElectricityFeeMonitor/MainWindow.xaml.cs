using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
using Newtonsoft.Json; 

/*
    Request URL:http://10.136.2.5/jnuweb/WebService/JNUService.asmx/Login
*/
namespace ElectricityFeeMonitor
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        public static CookieContainer GetCookie(string postString, string postUrl)
        {
            CookieContainer cookie = new CookieContainer();
            HttpWebRequest httpRequest = (HttpWebRequest)HttpWebRequest.Create(postUrl);
            httpRequest.CookieContainer = cookie;
            httpRequest.Method = "POST";
            httpRequest.KeepAlive = true;
            httpRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/57.0.2987.133 Safari/537.36";
            httpRequest.Accept = "*/*";
            httpRequest.ContentType = "application/json; charset=utf-8";
            Byte[] bytes = Encoding.UTF8.GetBytes(postString);
            httpRequest.ContentLength = bytes.Length;
            Stream stream = httpRequest.GetRequestStream();
            stream.Write(bytes, 0, bytes.Length);
            stream.Close();
            HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            return cookie;
        }
        public static string GetContent(CookieContainer cookie,string url)
        {
            string content;
            HttpWebRequest httpRequest = (HttpWebRequest)HttpWebRequest.Create(url);
            httpRequest.CookieContainer = cookie;
            httpRequest.Referer = url;
            httpRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/57.0.2987.133 Safari/537.36";
            httpRequest.Accept = "*/*";
            httpRequest.ContentType = "application/json; charset=utf-8";
            httpRequest.Method = "GET";
            HttpWebResponse httpresponse = (HttpWebResponse)httpRequest.GetResponse();
            using(Stream responseStream = httpresponse.GetResponseStream())
            {
                using(StreamReader streamReader=new StreamReader(responseStream, Encoding.UTF8))
                {
                    content = streamReader.ReadToEnd();
                }
            }
            return content;
        }

        private void button_temp_Click(object sender, RoutedEventArgs e)
        {
            string loginString = textBox_loginInfo.Text;
            try
            {
                CookieContainer cookie = GetCookie(loginString, "http://10.136.2.5/jnuweb/WebService/JNUService.asmx/Login");
                MessageBox.Show(GetContent(cookie, "http://10.136.2.5/jnuweb/WebService/JNUService.asmx/GetAccountBalance"));
            }
            catch(Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }
    }
}
