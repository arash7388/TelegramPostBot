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
    public static class CommonMistakesJob
    {
        private static readonly long ChatId = Convert.ToInt64(ConfigurationManager.AppSettings["ChatId"]);
        private static readonly string CommonMistakesFolderPath = ConfigurationManager.AppSettings["CommonMistakesFolderPath"];
        private static readonly string CommonMistakesPhotoFilePath = ConfigurationManager.AppSettings["CommonMistakesPhotoFilePath"];
        private static bool _runningFlag;
        private static readonly Semaphore RunningSemaphore = new Semaphore(1, 1);
        private static int _interval = Convert.ToInt32(ConfigurationManager.AppSettings["CommonMistakesIntervalMins"]) * 60000;

        private static DateTime _startTime = Convert.ToDateTime(ConfigurationManager.AppSettings["CommonMistakesJobStartTime"]);
        private static readonly Timer Timer = new Timer(DoWork, null, Timeout.Infinite, _interval);

        static CommonMistakesJob()
        {
            Console.WriteLine("CommonMistakesJob Timer created");
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
            Console.WriteLine("CommonMistakesJob timer startTime is:" + startTime);
        }

        public static void Stop()
        {
            Timer.Change(Timeout.Infinite, Timeout.Infinite);
            Console.WriteLine("CommonMistakesJob timer stopped");
        }

        private static void Restart()
        {
            Console.WriteLine("Try to restart CommonMistakesJob.");
            Stop();
            Start();
            Console.WriteLine("End of Restarting CommonMistakesJob.");
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
                Console.WriteLine($"try to put a CommonMistake post ,time: {DateTime.Now}");

                DirectoryInfo di = new DirectoryInfo(CommonMistakesFolderPath);
                FileInfo fi = di.GetFiles().OrderBy(a=>a.Name).FirstOrDefault();

                if(fi!=null)
                {
                    Utility.SendPhotoToChannel(ChatId, CommonMistakesPhotoFilePath,"").GetAwaiter().GetResult();
                    Utility.SendPhotoToChannel(ChatId, fi.FullName, "").GetAwaiter().GetResult();
                    Instagram.UploadToInsta(fi.FullName);
                    File.Delete(fi.FullName);
                }
                else
                {
                    Console.WriteLine("No files found! (CommonMistakes job)");
                }
                
                Console.WriteLine($"End of putting CommonMistakesJob post ,time: {DateTime.Now}");
                if (Program.JobsRunOnce)
                    Stop();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CommonMistakesJob, an exception occured! -> {ex.Message}");
            }
        }
    }
}
