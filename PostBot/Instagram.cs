using System;
using System.IO;
using System.Net;
using System.Security;
using InstaSharp;
using InstaSharp.Models;

namespace PostBot
{
    public class Instagram
    {
        public static SecureString ConvertToSecureString(string strPassword)
        {
            var secureStr = new SecureString();
            if (strPassword.Length > 0)
            {
                foreach (var c in strPassword.ToCharArray()) secureStr.AppendChar(c);
            }
            return secureStr;
        }

        public static void UploadToInsta(string imagePath)
        {
            var uploader = new InstagramUploader("sarasamet.esl", ConvertToSecureString("blackxs@440"));
            uploader.InvalidLoginEvent += InvalidLoginEvent;
            uploader.ErrorEvent += ErrorEvent;
            uploader.OnCompleteEvent += OnCompleteEvent;
            uploader.OnLoginEvent += OnLoginEvent;
            uploader.SuccessfulLoginEvent += SuccessfulLoginEvent;
            uploader.OnMediaConfigureStarted += OnMediaConfigureStarted;
            uploader.OnMediaUploadStartedEvent += OnMediaUploadStartedEvent;
            uploader.OnMediaUploadeComplete += OnmediaUploadCompleteEvent;
            string caption = "پادکست ها و مطالب متنوع انگلیسی را در کانال تلگرام من به آدرس زیر دنبال کنید" +
                             Environment.NewLine +
                             "https://t.me/EnglishWithSamet/" + Environment.NewLine +
                             Environment.NewLine +
                             "#SaraSamet #EnglishPod #LearnEnglish #ielts #eslteacher #teachingEnglish" +
                             Environment.NewLine;
                             
            uploader.UploadImage(imagePath, caption, false, true);
            Console.WriteLine("Your DeviceID is " + InstaSharp.Properties.Settings.Default.deviceId);
        }

        private static void ReadAPage()
        {
            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create("http://www.bbc.co.uk/learningenglish/english/features/6-minute-english/ep-180816");
            myRequest.Method = "GET";
            WebResponse myResponse = myRequest.GetResponse();
            StreamReader sr = new StreamReader(myResponse.GetResponseStream(), System.Text.Encoding.UTF8);
            string result = sr.ReadToEnd();
            sr.Close();
            myResponse.Close();
        }

        private static void DownloadFile()
        {
            var wc = new WebClient();
            wc.DownloadFileCompleted += Wc_DownloadFileCompleted;

            string path = "http://ichef.bbci.co.uk/images/ic/512xn/p06j819f.jpg";
            Console.WriteLine($"downloading from {path}");
            wc.DownloadFileAsync( new Uri(path),"123.jpg");
            Console.Read();
        }

        private static void Wc_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            Console.WriteLine("Download Completed");
            
        }

        private static void OnMediaUploadStartedEvent(object sender, EventArgs e)
        {
            Console.WriteLine("Attempting to upload image");
        }

        private static void OnmediaUploadCompleteEvent(object sender, EventArgs e)
        {
            Console.WriteLine("The image was uploaded, but has not been configured yet.");
        }


        private static void OnMediaConfigureStarted(object sender, EventArgs e)
        {
            Console.WriteLine("The image has started to be configured");
        }

        private static void SuccessfulLoginEvent(object sender, EventArgs e)
        {
            Console.WriteLine("Logged in! " + ((LoggedInUserResponse)e).FullName);
        }

        private static void OnLoginEvent(object sender, EventArgs e)
        {
            Console.WriteLine("Event fired for login: " + ((NormalResponse)e).Message);
        }

        private static void OnCompleteEvent(object sender, EventArgs e)
        {
            Console.WriteLine("Image posted to Instagram, here are all the urls");
            foreach (var image in ((UploadResponse)e).Images)
            {
                Console.WriteLine("Url: " + image.Url);
                Console.WriteLine("Width: " + image.Width);
                Console.WriteLine("Height: " + image.Height);
            }
        }

        private static void ErrorEvent(object sender, EventArgs e)
        {
            Console.WriteLine("Error  " + ((ErrorResponse)e).Message);
        }

        private static void InvalidLoginEvent(object sender, EventArgs e)
        {
            Console.WriteLine("Error while logging  " + ((ErrorResponse)e).Message);
        }
    }
}
