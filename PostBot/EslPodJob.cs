using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace PostBot
{
    public static class EslPodJob
    {
        private static readonly long ChatId = Convert.ToInt64(ConfigurationManager.AppSettings["ChatId"]);
        private static readonly string EslPodTextFilePath = ConfigurationManager.AppSettings["EslPodTextFilePath"];
        private static readonly string EslPodPhotoFilePath = ConfigurationManager.AppSettings["EslPodPhotoFilePath"];
        private static bool _runningFlag;
        private static readonly Semaphore RunningSemaphore = new Semaphore(1, 1);
        private static int _interval = Convert.ToInt32(ConfigurationManager.AppSettings["EslPodIntervalMins"]) * 60000;
        private static readonly string ApiToken = ConfigurationManager.AppSettings["ApiToken"];
        private static TelegramBotClient Bot = new TelegramBotClient(ApiToken);
        private static DateTime _startTime = Convert.ToDateTime(ConfigurationManager.AppSettings["EslPodJobStartTime"]);
        private static readonly Timer Timer = new Timer(DoWork, null, Timeout.Infinite, _interval);

        static EslPodJob()
        {
            Console.WriteLine("EslPodJob Timer created");
        }
        public static void Start()
        {
            if(_interval==0)
                Console.WriteLine("interval is 0");

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
            Console.WriteLine("EslPodJob timer startTime is:" + startTime);
        }

        public static void Stop()
        {
            Timer.Change(Timeout.Infinite, Timeout.Infinite);
            Console.WriteLine("EslPodJob timer stopped");
        }

        private static void Restart()
        {
            Console.WriteLine("Try to restart EslPodJob.");
            Stop();
            Start();
            Console.WriteLine("End of Restarting EslPodJob.");
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
                Console.WriteLine($"try to put a EslPod post ,time: {DateTime.Now}");
                List<Message> items = new List<Message>();

                using (StreamReader r = new StreamReader(EslPodTextFilePath))
                {
                    string json = r.ReadToEnd();
                    items = JsonConvert.DeserializeObject<List<Message>>(json);
                }
                                
                if(items.Any())
                {
                    Utility.SendPhotoToChannel(ChatId, EslPodPhotoFilePath,"").GetAwaiter().GetResult();
                    var message = items.OrderBy(a => a.MessageId).FirstOrDefault();
                    Bot.ForwardMessageAsync(ChatId, message.Chat.Id, message.MessageId).GetAwaiter().GetResult();
                    items.Remove(message);
                    message = items.OrderBy(a => a.MessageId).FirstOrDefault();
                    Bot.ForwardMessageAsync(ChatId, message.Chat.Id, message.MessageId).GetAwaiter().GetResult();
                    items.Remove(message);
                    var itemsAsString = JsonConvert.SerializeObject(items);

                    System.IO.File.WriteAllText(EslPodTextFilePath, itemsAsString);
                    
                }
                else
                {
                    Console.WriteLine("No messages found! (EslPod job)");
                }
                
                Console.WriteLine($"End of putting EslPodJob post ,time: {DateTime.Now}");
                if (Program.JobsRunOnce)
                    Stop();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EslPodJob, an exception occured! -> {ex.Message}");
            }
        }
    }
}
