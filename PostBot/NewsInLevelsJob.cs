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
    public class NewsInLevelsJob
    {
        private static readonly string ApiToken = ConfigurationManager.AppSettings["ApiToken"];
        private static readonly long ChatId = Convert.ToInt64(ConfigurationManager.AppSettings["ChatId"]);
        private static TelegramBotClient Bot = new TelegramBotClient(ApiToken);
        private static readonly string NewsInLevelsPhotoFilePath = ConfigurationManager.AppSettings["NewsInLevelsPhotoFilePath"];
        private static bool _runningFlag;
        private static readonly Semaphore RunningSemaphore = new Semaphore(1, 1);
        private static int _interval = Convert.ToInt32(ConfigurationManager.AppSettings["NewsInLevelsIntervalMins"]) * 60000;

        private static DateTime _startTime = Convert.ToDateTime(ConfigurationManager.AppSettings["NewsInLevelsJobStartTime"]);
        private static readonly Timer Timer = new Timer(DoWork, null, Timeout.Infinite, _interval);


        static NewsInLevelsJob()
        {
            Console.WriteLine("NewsInLevelsJob Timer created");
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
            Console.WriteLine("NewsInLevelsJob timer startTime is:" + startTime);
        }

        public static void Stop()
        {
            Timer.Change(Timeout.Infinite, Timeout.Infinite);
            Console.WriteLine("NewsInLevelsJob timer stopped");
        }

        private static void Restart()
        {
            Console.WriteLine("Try to restart NewsInLevelsJob.");
            Stop();
            Start();
            Console.WriteLine("End of Restarting NewsInLevelsJob.");
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

        public static (string title, string conent) GetArticleContent(string href)
        {
            string html;
            HtmlDocument htmlDoc;
            html = Utility.GetHtml(href);
            htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var mainContent = htmlDoc.DocumentNode.SelectNodes(@"//div[contains(@class,'main-content')]");
            var titleText = mainContent.Descendants("div").FirstOrDefault(a => a.HasClass("article-title")).Descendants("h2").FirstOrDefault().InnerText;
            var nContent = mainContent.Descendants().FirstOrDefault(a => a.Id == "nContent");

            string articleText = "";
            foreach (HtmlNode p in nContent.Descendants("p"))
            {
                articleText += p.InnerText + "\n";
            }

            articleText = articleText.Replace("You can watch the original video in the Level 3 section.", "");
            articleText = articleText.Replace("You can watch the video news lower on this page.", "");
            articleText += "\n#EnglishWithSamet \n#NewsInLevels";
            
            var result = (title: "<b>" + titleText + "</b>", conent: articleText);
            return result;
        }

        public static void DoPost()
        {
            try
            {
                Console.WriteLine($"try to put a newsInLevels post ,time: {DateTime.Now}");

                string html = Utility.GetHtml("https://www.newsinlevels.com");
                HtmlDocument htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(html);

                var firstpageMainContent = htmlDoc.DocumentNode.SelectNodes(@"//div[contains(@class,'main-content')]");
                var topEntry = firstpageMainContent.Descendants().FirstOrDefault(a => a.HasClass("highlighted"));
                var imgwrap = topEntry.Descendants().FirstOrDefault(a => a.HasClass("img-wrap"));
                var anchor = imgwrap.Descendants("a").FirstOrDefault();
                var href = anchor.Attributes.FirstOrDefault(a => a.Name == "href").Value;

                var image = imgwrap.Descendants("img").FirstOrDefault();
                var imageSrc = image.Attributes.FirstOrDefault(a => a.Name == "src").Value;

                (string title, string conent) level1 = GetArticleContent(href);
                (string title, string conent) level2 = GetArticleContent(href.Replace("level-1", "level-2"));
                (string title, string conent) level3 = GetArticleContent(href.Replace("level-1", "level-3"));
                 
                Utility.SendPhotoToChannel(ChatId, NewsInLevelsPhotoFilePath, "News In Levels" + Environment.NewLine + "#EnglishWithSamet").GetAwaiter().GetResult();
                Utility.SendPhotoToChannel(ChatId, imageSrc, "").GetAwaiter().GetResult();

                Bot.SendTextMessageAsync(ChatId, level1.title + Environment.NewLine + level1.conent, ParseMode.Html).GetAwaiter().GetResult();
                Bot.SendTextMessageAsync(ChatId, level2.title + Environment.NewLine + level2.conent, ParseMode.Html).GetAwaiter().GetResult();
                Bot.SendTextMessageAsync(ChatId, level3.title + Environment.NewLine + level3.conent, ParseMode.Html).GetAwaiter().GetResult();

                Console.WriteLine($"End of putting newsInLevels post ,time: {DateTime.Now}");

                if (Program.JobsRunOnce)
                    Stop();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"an exception occured! -> {ex.Message}");
            }
        }


    }
}

