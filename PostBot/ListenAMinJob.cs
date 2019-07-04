using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using HtmlAgilityPack;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace PostBot
{
    public static class ListenAMinJob
    {
        //private static long EngcafevipChatId = -1001259894749;
        //private static long EnglishWithSametChatId = -1001128563406;//eng cafe ->@sarasamet


        private static readonly string ApiToken = ConfigurationManager.AppSettings["ApiToken"];

        private static readonly string ListenAMinTextFilePath =
            ConfigurationManager.AppSettings["ListenAMinTextFilePath"];

        private static readonly string ListenAMinPhotoFilePath =
            ConfigurationManager.AppSettings["ListenAMinPhotoFilePath"];

        private static readonly long ChatId = Convert.ToInt64(ConfigurationManager.AppSettings["ChatId"]);
        private static TelegramBotClient Bot = new TelegramBotClient(ApiToken);
        private static bool _runningFlag;
        private static readonly Semaphore RunningSemaphore = new Semaphore(1, 1);

        private static int _interval = Convert.ToInt32(ConfigurationManager.AppSettings["ListenAMinPostIntervalMins"]) *
                                       60000;

        private static DateTime _startTime =
            Convert.ToDateTime(ConfigurationManager.AppSettings["ListenAMinJobStartTime"]);

        private static readonly Timer Timer = new Timer(DoWork, null, Timeout.Infinite, _interval);

        static ListenAMinJob()
        {
            Console.WriteLine("ListenAMinJob Timer created");
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
                    var timeSpan = new TimeSpan(0, int.Parse(dayIntervalMinutes.ToString(CultureInfo.InvariantCulture)),
                        0);
                    while (startTime > now && startTime.Subtract(now).TotalMinutes >= dayIntervalMinutes)
                        startTime = startTime.Subtract(timeSpan);
                }
            }
            var dueTime = (int)(startTime - DateTime.Now).TotalMilliseconds;
            Timer.Change(dueTime, _interval);
            Console.WriteLine("ListenAMinJob job timer startTime is:" + startTime);
        }

        public static void Stop()
        {
            Timer.Change(Timeout.Infinite, Timeout.Infinite);
            Console.WriteLine("ListenAMinJob timer stopped");
        }

        private static void Restart()
        {
            Console.WriteLine("Try to restart ListenAMinJob job.");
            Stop();
            Start();
            Console.WriteLine("End of Restarting ListenAMinJob job.");
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

        public static void DoPost()
        {
            try
            {
                string vocabUrl = "";
                using (StreamReader reader = new StreamReader(ListenAMinTextFilePath))
                {
                    vocabUrl = reader.ReadLine();
                }

                Console.WriteLine($"try to put listen a min entry ,time: {DateTime.Now}, vocab:{vocabUrl}");

                var result = ListenAMinOneVocab(vocabUrl);

                if (result)
                {
                    List<string> quotelist = File.ReadAllLines(ListenAMinTextFilePath).ToList();
                    quotelist.RemoveAt(0);
                    File.WriteAllLines(ListenAMinTextFilePath, quotelist.ToArray());
                }

                Console.WriteLine($"End of putting listen a min entry ,time: {DateTime.Now}");

                if(Program.JobsRunOnce)
                    Stop();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"an exception occured! -> {ex.Message}");
            }
        }

        public static bool ListenAMinOneVocab(string urlOfVocab)
        {
            try
            {
                string html = Utility.GetHtml(urlOfVocab);
                HtmlDocument htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(html);

                var divTitlep = htmlDoc.DocumentNode.SelectSingleNode(@"//div[@class='section-no-color']");
                var title = divTitlep.FirstChild.NextSibling.InnerText.TrimStart().TrimEnd();

                Utility.SendPhotoToChannel(ChatId, ListenAMinPhotoFilePath, "").GetAwaiter().GetResult();
                title = "<b>" + title + "</b>";
                var msgTitle = Bot.SendTextMessageAsync(ChatId, title + Environment.NewLine + "#ListenAMin #EnglishWithSamet", parseMode: ParseMode.Html).GetAwaiter().GetResult();
                if (msgTitle == null)
                    return false;

                var audioTag = htmlDoc.DocumentNode.SelectSingleNode(@"//audio");
                var mp3Name = audioTag.FirstChild.NextSibling.Attributes["src"].Value;
                var msgMp3 = Bot.SendAudioAsync(ChatId, "https://listenaminute.com/" + mp3Name.Substring(0, 1) + "/" + mp3Name, "ListenAMinute").GetAwaiter().GetResult();

                if (msgMp3 == null)
                {
                    //Bot
                    return false;
                }

                var h3 = htmlDoc.DocumentNode.SelectNodes(@"//h3");
                foreach (HtmlNode node in h3)
                {
                    if (node.InnerText == "READ")
                    {
                        var table = node.NextSibling.NextSibling;
                        var script = table.InnerText;
                        script = script.AdjustHTMLText();
                        script = title + Environment.NewLine + script;
                        var msgScript = Bot.SendTextMessageAsync(ChatId, script, parseMode: ParseMode.Html).GetAwaiter().GetResult();

                        if (msgScript == null)
                            return false;
                    }
                }

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
