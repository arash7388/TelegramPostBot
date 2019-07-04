using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using HtmlAgilityPack;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace PostBot
{
    public class EslFastJob
    {
        private static readonly string ApiToken = ConfigurationManager.AppSettings["ApiToken"];
        private static readonly string EslFastTextFilePath = ConfigurationManager.AppSettings["EslFastTextFilePath"];
        private static readonly string EslFastPhotoFilePath = ConfigurationManager.AppSettings["EslFastPhotoFilePath"];
        private static readonly long ChatId = Convert.ToInt64(ConfigurationManager.AppSettings["ChatId"]);
        private static TelegramBotClient Bot = new TelegramBotClient(ApiToken);
        private static bool _runningFlag;
        private static readonly Semaphore RunningSemaphore = new Semaphore(1, 1);
        private static int _interval = Convert.ToInt32(ConfigurationManager.AppSettings["EslFastPostIntervalMins"]) * 60000;

        private static DateTime _startTime = Convert.ToDateTime(ConfigurationManager.AppSettings["EslFastJobStartTime"]);
        private static readonly Timer Timer = new Timer(DoWork, null, Timeout.Infinite, _interval);


        static EslFastJob()
        {
            Console.WriteLine("EslFastJob Timer created");
        }
        public static void Start()
        {
            var now = DateTime.Now;
            var startTime = _startTime;
            if (TimeSpan.FromMilliseconds(_interval).TotalDays >= 1)
            {
                var today = now.Date.AddMinutes(startTime.TimeOfDay.TotalMinutes);
                startTime = now <= today ? today : today.AddDays(1);
            }
            else
            {
                var dayIntervalMinutes = TimeSpan.FromMilliseconds(_interval).TotalMinutes;
                if (now > startTime)
                    do startTime = startTime.AddMinutes(dayIntervalMinutes); while (startTime < now);
                else
                {
                    var timeSpan = new TimeSpan(0, int.Parse(dayIntervalMinutes.ToString(CultureInfo.InvariantCulture)), 0);
                    while (startTime > now && startTime.Subtract(now).TotalMinutes >= dayIntervalMinutes)
                        startTime = startTime.Subtract(timeSpan);
                }
            }
            var dueTime = (int)(startTime - DateTime.Now).TotalMilliseconds;
            if (dueTime < 0)
                dueTime = 60000 + dueTime;

            Timer.Change(dueTime, _interval);
            Console.WriteLine("EslFastJob timer startTime is:" + startTime);
        }

        public static void Stop()
        {
            Timer.Change(Timeout.Infinite, Timeout.Infinite);
            Console.WriteLine("EslFastJob timer stopped");
        }

        private static void Restart()
        {
            Console.WriteLine("Try to restart EslFastJob.");
            Stop();
            Start();
            Console.WriteLine("End of Restarting EslFastJob.");
        }

        private static void DoWork(object state)
        {
            try
            {
                try
                {
                    Program.CrossJobMutex.WaitOne();

                    if (!_runningFlag)
                    {
                        RunningSemaphore.WaitOne();
                        _runningFlag = true;
                        RunningSemaphore.Release();
                    }
                    else
                        throw new Exception("Another Thread is Running !");

                    DoPost();

                    RunningSemaphore.WaitOne();
                    _runningFlag = false;
                    RunningSemaphore.Release();
                }
                finally
                {
                    Program.CrossJobMutex.ReleaseMutex();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (ex.Message != "Another Thread is Running !")
                {
                    RunningSemaphore.WaitOne();
                    _runningFlag = false;
                    RunningSemaphore.Release();
                }
            }
        }

        static void Test(string[] args)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                EslFastLevel3();

                //var url = "https://dictionary.cambridge.org/dictionary/english/compromise";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static string GetHtml(string url)
        {
            string urlAddress = url;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            string data = "";
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                }

                data = readStream.ReadToEnd();

                response.Close();
                readStream.Close();
            }

            return data;
        }

        public static void EslFastLevel1()
        {
            string resultText = "";

            try
            {
                string html = GetHtml("https://www.eslfast.com");
                HtmlDocument htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(html);

                var links = htmlDoc.DocumentNode.SelectNodes(@"//table/tr/td/font/blockquote/ul/li/a");
                string s = "";

                foreach (HtmlNode node in links)
                {
                    s += (node.Attributes["href"].Value) + Environment.NewLine;
                }

                resultText += "\n";

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static void EslFastLevel2()
        {
            string resultText = "";

            try
            {
                string html = GetHtml("https://www.rong-chang.com/speak/");
                HtmlDocument htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(html);

                var links = htmlDoc.DocumentNode.SelectNodes(@"//table/tr/td/ul/font/p/a");
                string s = "";

                foreach (HtmlNode node in links)
                {
                    s += (node.Attributes["href"].Value) + Environment.NewLine;
                }

                resultText += "\n";

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static void EslFastLevel3()
        {
            string resultText = "";

            try
            {
                string html = GetHtml("https://www.rong-chang.com/speak/everyday.htm");
                HtmlDocument htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(html);

                var links = htmlDoc.DocumentNode.SelectNodes(@"//p[contains(@class,'MsoNormal')]/a");
                string s = "";

                foreach (HtmlNode node in links)
                {
                    s += (node.Attributes["href"].Value) + Environment.NewLine;
                }

                resultText += "\n";

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //public static void EslFastLevel4()
        //{
        //    string resultText = "";

        //    try
        //    {
        //        string html = GetHtml("https://www.rong-chang.com/speak/s/everyday01.htm");
        //        HtmlDocument htmlDoc = new HtmlDocument();
        //        htmlDoc.LoadHtml(html);

        //        var audio = htmlDoc.DocumentNode.SelectSingleNode(@"//audio");
        //        var mp3Src = audio.Attributes["src"];


        //        foreach (HtmlNode node in links)
        //        {
        //            s += (node.Attributes["href"].Value) + Environment.NewLine;
        //        }

        //        resultText += "\n";

        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e);
        //    }
        //}

        public static void DoPost()
        {
            try
            {
                string link = "";
                using (StreamReader reader = new StreamReader(EslFastTextFilePath))
                {
                    link = reader.ReadLine();
                }

                Console.WriteLine($"try to put a eslfast post ,time: {DateTime.Now}, link:{link}");

                var result = Parse(link);

                if (result)
                {
                    List<string> quotelist = File.ReadAllLines(EslFastTextFilePath).ToList();
                    quotelist.RemoveAt(0);
                    File.WriteAllLines(EslFastTextFilePath, quotelist.ToArray());

                    Console.WriteLine("eslfast job done.");
                }

                Console.WriteLine($"End of putting eslfast post ,time: {DateTime.Now}");

                if (Program.JobsRunOnce)
                    Stop();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"an exception occured! -> {ex.Message}");
            }
        }

        public static bool Parse(string url)
        {
            try
            {
                string html = GetHtml(url);
                HtmlDocument htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(html);

                var audio = htmlDoc.DocumentNode.SelectSingleNode(@"//audio");
                var mp3Src = audio.Attributes["src"].Value;

                string realMp3Adr;
                if (mp3Src.Contains("../"))
                {
                    var dotdotslashCount = Regex.Matches(mp3Src, @"\.\./").Count;
                    var ss = url.Split('/');
                    string r = "";

                    for (int i = 0; i < ss.Length - 1 - dotdotslashCount; i++)
                    {
                        r += ss[i] + "/";
                    }

                    realMp3Adr = r + mp3Src.Replace("../", "");
                }
                else
                {
                    var splitted = url.Split('/');
                    string ss = "";

                    for (int i = 0; i < splitted.Length - 1; i++)
                    {
                        ss += splitted[i] + "/";
                    }

                    realMp3Adr = ss + mp3Src;
                }

                var mainTextElement = htmlDoc.DocumentNode.SelectSingleNode(@"//blockquote");
                var mainText = mainTextElement.InnerText;

                var titleElement = htmlDoc.DocumentNode.SelectSingleNode(@"//h1/font/b");
                var titleText = titleElement.InnerText;

                titleText = Regex.Replace(titleText, @"^([1-9]|[1-9][0-9])\.", "").Trim();
                Utility.SendPhotoToChannel(ChatId, EslFastPhotoFilePath, "").GetAwaiter().GetResult();

                var titleMsg = Bot.SendTextMessageAsync(ChatId,
                    "<b>" + titleText + "</b>" + Environment.NewLine + "#EslFast #EnglishWithSamet", ParseMode.Html);

                if (titleMsg == null)
                    return false;

                var audioMsg = Bot.SendAudioAsync(ChatId, realMp3Adr).GetAwaiter().GetResult();
                if (audioMsg == null)
                    return false;

                var mainTextMsg = Bot.SendTextMessageAsync(ChatId, mainText.AdjustHTMLText(), ParseMode.Html).GetAwaiter().GetResult();
                if (mainTextMsg == null)
                    return false;

                return true;

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return false;
        }

    }
}

