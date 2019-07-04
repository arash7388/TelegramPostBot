using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BTCPostBot
{
    public static class JobManagement
    {

        private static bool _runningFlag;
        private static readonly Semaphore RunningSemaphore = new Semaphore(1, 1);
        private static int _interval = Convert.ToInt32(ConfigurationManager.AppSettings["PostIntervalHours"]) * 60000;

        private static DateTime _startTime = Convert.ToDateTime(ConfigurationManager.AppSettings["JobStartTime"]);
        private static readonly Timer Timer = new Timer(DoWork, null, Timeout.Infinite, _interval);

        static JobManagement()
        {
            Console.WriteLine("JobManagement Timer created");
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
            Timer.Change(dueTime, _interval);
            Console.WriteLine("JobManagement job timer startTime is:" + startTime);
        }

        public static void Stop()
        {
            Timer.Change(Timeout.Infinite, Timeout.Infinite);
            Console.WriteLine("JobManagement timer stopped");
        }

        private static void Restart()
        {
            Console.WriteLine("Try to restart JobManagement job.");
            Stop();
            Start();
            Console.WriteLine("End of Restarting JobManagement job.");
        }

        private static void DoWork(object state)
        {
            try
            {
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

        private static void DoPost()
        {
            try
            {
                Console.WriteLine($"try to put a post ,time: {DateTime.Now}");
                string text = System.IO.File.ReadAllText(@"../../WriteText.txt");
                CambridgeParser.GetCambridgeTranslation();
                Console.WriteLine($"End of putting post ,time: {DateTime.Now}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"an exception occured! -> {ex.Message}");
            }
        }
    }
}
