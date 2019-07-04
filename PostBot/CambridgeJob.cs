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
    public static class CambridgeJob
    {
        private static readonly long ChatId = Convert.ToInt64(ConfigurationManager.AppSettings["ChatId"]);
        private static readonly string CambridgeTextFilePath = ConfigurationManager.AppSettings["CambridgeTextFilePath"];
        private static readonly string CambridgePhotoFilePath = ConfigurationManager.AppSettings["CambridgePhotoFilePath"];
        private static readonly string ApiToken = ConfigurationManager.AppSettings["ApiToken"];
        private static TelegramBotClient Bot = new TelegramBotClient(ApiToken);
        private static bool _runningFlag;
        private static readonly Semaphore RunningSemaphore = new Semaphore(1, 1);
        private static int _interval = Convert.ToInt32(ConfigurationManager.AppSettings["CambridgePostIntervalMins"]) * 60000;

        private static DateTime _startTime = Convert.ToDateTime(ConfigurationManager.AppSettings["CambridgeJobStartTime"]);
        private static readonly Timer Timer = new Timer(DoWork, null, Timeout.Infinite, _interval);

        static CambridgeJob()
        {
            Console.WriteLine("CambridgeJob Timer created");
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
            Console.WriteLine("CambridgeJob timer startTime is:" + startTime);
        }

        public static void Stop()
        {
            Timer.Change(Timeout.Infinite, Timeout.Infinite);
            Console.WriteLine("CambridgeJob timer stopped");
        }

        private static void Restart()
        {
            Console.WriteLine("Try to restart CambridgeJob.");
            Stop();
            Start();
            Console.WriteLine("End of Restarting CambridgeJob.");
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
                string vocab = "";
                using (StreamReader reader = new StreamReader(CambridgeTextFilePath))
                {
                    vocab = reader.ReadLine();
                }

                Console.WriteLine($"try to put a post ,time: {DateTime.Now}, vocab:{vocab}");

                var msg = CambridgeParser.Parse("https://dictionary.cambridge.org/dictionary/english/" + vocab);

                if (msg != "")
                {
                    Utility.SendPhotoToChannel(ChatId, CambridgePhotoFilePath, "").GetAwaiter().GetResult();

                    var res = Bot.SendTextMessageAsync(ChatId, msg, ParseMode.Html).GetAwaiter().GetResult();
                    if (res != null)
                    {
                        List<string> quotelist = File.ReadAllLines(CambridgeTextFilePath).ToList();
                        quotelist.RemoveAt(0);
                        File.WriteAllLines(CambridgeTextFilePath, quotelist.ToArray());
                    }
                    Console.WriteLine("job done.");
                }

                Console.WriteLine($"End of putting post ,time: {DateTime.Now}");

                if(Program.JobsRunOnce)
                    Stop();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"an exception occured! -> {ex.Message}");
            }
        }
    }
}
