using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace PostBot
{
    public class LearnEngTodayJob
    {
        private static readonly long ChatId = Convert.ToInt64(ConfigurationManager.AppSettings["ChatId"]);
        private static readonly string LearnEngTodayTextFilePath = ConfigurationManager.AppSettings["LearnEngTodayTextFilePath"];
        private static readonly string LearnEngTodayPhotoFilePath = ConfigurationManager.AppSettings["LearnEngTodayPhotoFilePath"];
        private static readonly string ApiToken = ConfigurationManager.AppSettings["ApiToken"];
        private static TelegramBotClient Bot = new TelegramBotClient(ApiToken);
        private static bool _runningFlag;
        private static readonly Semaphore RunningSemaphore = new Semaphore(1, 1);
        private static int _interval = Convert.ToInt32(ConfigurationManager.AppSettings["LearnEngTodayIntervalMins"]) * 60000;

        private static DateTime _startTime = Convert.ToDateTime(ConfigurationManager.AppSettings["LearnEngTodayJobStartTime"]);
        private static readonly Timer Timer = new Timer(DoWork, null, Timeout.Infinite, _interval);

        static LearnEngTodayJob()
        {
            Console.WriteLine("LearnEngTodayJob Timer created");
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
            Console.WriteLine("LearnEngTodayJob timer startTime is:" + startTime);
        }

        public static void Stop()
        {
            Timer.Change(Timeout.Infinite, Timeout.Infinite);
            Console.WriteLine("LearnEngTodayJob timer stopped");
        }

        private static void Restart()
        {
            Console.WriteLine("Try to restart LearnEngTodayJob.");
            Stop();
            Start();
            Console.WriteLine("End of Restarting LearnEngTodayJob.");
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

        public static string GetFirstLevelLinks()
        {
            //getting Lists of common idioms by theme:
            string html = Utility.GetHtml("https://www.learn-english-today.com/idioms/idioms_proverbs.html");
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var ul = htmlDoc.DocumentNode.SelectSingleNode(@"//div[contains(@id,'main-content')]/ul/ul/li/ul");
            var links2 = ul.Descendants("a");

            string res = "";

            foreach (HtmlNode htmlNode in links2)
            {
                res += "https://www.learn-english-today.com/idioms/" + htmlNode.Attributes["href"].Value + Environment.NewLine;
            }

            return res;
        }

        public static string GetSecondLevelLinks(string url)
        {
           
            string html = Utility.GetHtml(url);
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var links = htmlDoc.DocumentNode.SelectNodes(@"//div[contains(@id,'idiom-list')]/ul/li/a");
            
            string res = "";

            var splitted = url.Split('/');
            string ss = "";

            for (int i = 0; i < splitted.Length - 1; i++)
            {
                ss += splitted[i] + "/";
            }

            foreach (HtmlNode htmlNode in links)
            {
                res += ss + htmlNode.Attributes["href"].Value + Environment.NewLine;
            }

            return res;
        }

        public static void GetLastPageLinks()
        {
           string html = Utility.GetHtml("https://www.learn-english-today.com/idioms/idioms_proverbs.html");
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var ul = htmlDoc.DocumentNode.SelectSingleNode(@"//div[contains(@id,'main-content')]/ul/ul/li/ul");
            var links2 = ul.Descendants("a");

            var finalLinks = "";

            foreach (HtmlNode htmlNode in links2)
            {
                var res = "https://www.learn-english-today.com/idioms/" + htmlNode.Attributes["href"].Value;
                //finalLinks += GetSecondLevelLinks(res);
                
                html = Utility.GetHtml(res);
                htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(html);

                List<string> links = new List<string>();

                var div = htmlDoc.DocumentNode.SelectSingleNode("//div[@id='main-content']");
                if (div != null)
                {
                    links = div.Descendants("a").Where(a => a.Attributes["href"].Value.Contains("idiom-categories"))
                        .Select(a => "https://www.learn-english-today.com/idioms/" + a.Attributes["href"].Value)
                        .ToList();

                    foreach (string l in links)
                    {
                        finalLinks += l + Environment.NewLine;
                    }
                }
                else
                {
                    //links.Add(res);
                    finalLinks += res + Environment.NewLine;
                }
            }
        }

        public static void DoPost()
        {
            try
            {
                string pageUrl = "";
                using (StreamReader reader = new StreamReader(LearnEngTodayTextFilePath))
                {
                    pageUrl = reader.ReadLine();
                }

                Console.WriteLine($"try to put a post (idiom) ,time: {DateTime.Now}, page:{pageUrl}");

                int idiomIndexInThePage = 0;
                var splitted = pageUrl.Split(',');
                if (splitted.Length > 1)
                    idiomIndexInThePage = Convert.ToInt32(splitted[1]);

                bool idiomsOfPageNotFinished; 
                var msg = GetContent(splitted[0], ++idiomIndexInThePage,out idiomsOfPageNotFinished);

                if (!idiomsOfPageNotFinished) // for deleteing the line which all of its idioms has been published
                {
                    Console.WriteLine("All the idioms of this url has been bpulished... deleteing the line ...");
                    List<string> quotelist = File.ReadAllLines(LearnEngTodayTextFilePath).ToList();
                    quotelist.RemoveAt(0);
                    File.WriteAllLines(LearnEngTodayTextFilePath, quotelist.ToArray());
                    Console.WriteLine("Line deleted successfuly");
                    return;
                }

                if (msg != "")
                {
                    Utility.SendPhotoToChannel(ChatId, LearnEngTodayPhotoFilePath, "").GetAwaiter().GetResult();

                    var res = Bot.SendTextMessageAsync(ChatId, msg, ParseMode.Html).GetAwaiter().GetResult();
                    if (res != null)
                    {
                        List<string> quotelist = File.ReadAllLines(LearnEngTodayTextFilePath).ToList();
                        quotelist.RemoveAt(0);
                        quotelist.Insert(0, splitted[0] + ","+ idiomIndexInThePage);
                        File.WriteAllLines(LearnEngTodayTextFilePath, quotelist.ToArray());
                    }
                    Console.WriteLine("job done.");
                }

                Console.WriteLine($"End of putting idiom post ,time: {DateTime.Now}");

                if (Program.JobsRunOnce)
                    Stop();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"an exception occured! -> {ex.Message}");
            }
        }

        public static string GetContent(string pageUrl,int idiomIndexToFetch, out bool foreachPassedAtLeastOnce)
        {
            string html = Utility.GetHtml(pageUrl);
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            List<string> links = new List<string>();

            var div = htmlDoc.DocumentNode.SelectSingleNode("//div[@id='main-content']");
            if (div != null)
            {
                links = div.Descendants("a").Where(a => a.Attributes["href"].Value.Contains("idiom-categories"))
                    .Select(a => "https://www.learn-english-today.com/idioms/" + a.Attributes["href"].Value)
                    .ToList();
            }
            else
            {
                links.Add(pageUrl);
            }

            foreachPassedAtLeastOnce = false;
            foreach (string entry in links)
            {
                html = Utility.GetHtml(entry);
                htmlDoc.LoadHtml(html);

                var ul = htmlDoc.DocumentNode.SelectSingleNode("//div[@id='idiom-list']/ul");
                var lis = ul.Descendants().Where(x => x.ParentNode == ul && x.Name == "li");

                var counter = 0;
                
                //idiomIndexToFetch is the idiom on the url that should be published
                foreach (HtmlNode li in lis)
                {
                    counter++;
                    if(counter != idiomIndexToFetch)
                        continue;

                    foreachPassedAtLeastOnce = true;
                    var titleElement = li.Descendants().FirstOrDefault(e => e.GetAttributeValue("class", "") == "bg-taupe");
                    if (titleElement == null)
                        Console.WriteLine($"title element is null! (idioms in learnengtoday)");
                    var titleText = titleElement?.InnerText;
                    var descElement = li.Descendants("li").FirstOrDefault();
                    if (descElement == null)
                        Console.WriteLine($"descElement is null! (idioms in learnengtoday)");
                    var descText = descElement?.InnerText;

                    if (titleElement != null && descElement != null)
                    {
                        //U0001F4A1 lamp
                        var resultText = "\U0001F469" + " <b>Idiom of the day:</b>";
                        
                        resultText += "<b>" + titleText + "</b>\n\n";
                        resultText += "\U00003030\U00003030\U00003030\U00003030\U00003030\U00003030\U00003030\U00003030\n\n";
                        
                        resultText += "\U0001F537" + " " +  descText.Replace("\n","").Replace("\t","");
                        var qindex = resultText.IndexOf("\"");
                        if (qindex != -1)
                            resultText = resultText.Insert(qindex, "\n\n \U0001F536");
                        resultText += "\n\n";
                        resultText += "#EnglishWithSamet #Idiom";
                        return resultText;
                    }
                }
            }

            return "";
        }
    }

}
