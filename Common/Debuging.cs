using System;
using System.Collections;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;

namespace Common
{
    public static class Debuging
    {
        private static Semaphore _initMutex = new Semaphore(1, 1);
        
        private static string ShowDateTime()
        {
            string result = " ";
            result += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fffff", DateTimeFormatInfo.InvariantInfo) + " - ";
            result += Thread.CurrentThread.ManagedThreadId + " --> ";
            return result;
        }
        private static int _currentFileNumber = 1;
        private static readonly int _loggingFilesCountInADay = int.Parse(ConfigurationManager.AppSettings["LoggingFilesCountInADay"] ?? "1");
        private static readonly double _logFileDuration = 24.0 / _loggingFilesCountInADay;
        private static bool _isInitialize = false;
        private static DateTime _currentFileDate = DateTime.Now;
        private static readonly string _baseLoggingFolder = ConfigurationSettings.AppSettings["BaseLoggingFolder"];
        private static readonly PersianCalendar _dateAssistance = new PersianCalendar();
        private static readonly TraceSource _secTSource = new TraceSource("ST", SourceLevels.Information);
        private static bool FileParametersChanged()
        {
            return _currentFileDate.Date.CompareTo(DateTime.Today) != 0 || _currentFileNumber != GetFileNumber(DateTime.Now);
        }

        private static int GetFileNumber(DateTime dateTime)
        {
            int result = 1;
            if (_loggingFilesCountInADay > 1)
                result = (int)(dateTime.TimeOfDay.TotalHours / _logFileDuration) + 1;
            return result;
        }

        private static void Initialize()
        {
            if (!_isInitialize || FileParametersChanged())
            {
                _initMutex.WaitOne();
                try
                {
                    if (!_isInitialize || FileParametersChanged())
                    {
                        _currentFileDate = DateTime.Now;
                        Debug.Listeners.Clear();
                        _secTSource.Listeners.Clear();
                        string fileName = _dateAssistance.GetYear(_currentFileDate).ToString("000#") + "-" +
                                          _dateAssistance.GetMonth(_currentFileDate).ToString("0#") + "-" +
                                          _dateAssistance.GetDayOfMonth(_currentFileDate).ToString("0#");

                        _currentFileNumber = GetFileNumber(_currentFileDate);
                        if (_loggingFilesCountInADay > 1)
                            fileName += "-" + _currentFileNumber.ToString("0#");

                        if (!Directory.Exists(_baseLoggingFolder))
                            Directory.CreateDirectory(_baseLoggingFolder);

                        Debug.Listeners.Add(new TextWriterTraceListener(_baseLoggingFolder + fileName + ".txt"));
                        _isInitialize = true;
                    }
                }
                catch { }
                _initMutex.Release();
                Info("Initialize Debugging File.");
            }
        }

        private static void Flush()
        {
            Debug.Flush();
        }


        public static void Info(string infoText)
        {
            Initialize();
            Debug.WriteLine("---- INFO ----" + ShowDateTime() + infoText);
            Flush();
        }
     
        public static void Warning(string infoText)
        {
            Initialize();
            Debug.WriteLine("--- Warning --" + ShowDateTime() + infoText);
            Flush();
        }

       
        public static long Warning(Exception exeption, string place)
        {
            long logId = 0;

            Initialize();
            Debug.WriteLine("--- Warning --" + ShowDateTime() + "with flow exeption" + exeption.GetType());
            Debug.WriteLine("\t-- An exception occured in " + place);
            Debug.WriteLine("\t\tSource: " + exeption.Source);
            Debug.WriteLine("\t\tMessgae: " + exeption.Message);
            Debug.WriteLine("\t\t\tStackTrace: " + exeption.StackTrace);
            Debug.WriteLine("\t\t\tExtra Data: ");

            string exData = "";

            foreach (DictionaryEntry de in exeption.Data)
            {
                Debug.WriteLine(string.Format("\t\t\tThe key  '{0}' and the value is: {1}", de.Key, de.Value));
                exData += string.Format("{0}:{1}#", de.Key, de.Value);
            }

           if (exeption.InnerException != null)
                Error(exeption.InnerException, "Previous Exception");

            Flush();
            return logId;
        }
      
        public static void Error(string infoText)
        {
            Initialize();
            Debug.WriteLine("---- Error ---" + ShowDateTime() + infoText);
            Flush();
        }

    
        public static void Error(Exception exeption, string place)
        {
            Initialize();
            Debug.WriteLine("---- Error ---" + ShowDateTime() + "with flow exeption " + exeption.GetType());
            Debug.WriteLine("\t-- An exception occured in " + place);
            Debug.WriteLine("\t\tSource: " + exeption.Source);
            Debug.WriteLine("\t\tMessgae: " + exeption.Message);
            Debug.WriteLine("\t\t\tStackTrace: " + exeption.StackTrace);
            Debug.WriteLine("\t\t\tExtra Data: ");

            string exData = "";

            foreach (DictionaryEntry de in exeption.Data)
            {
                Debug.WriteLine(string.Format("\t\t\tThe key  '{0}' and the value is: {1}", de.Key, de.Value));
                exData += string.Format("{0}#{1}#", de.Key, de.Value);
            }

            if (exeption.InnerException != null)
                Error(exeption.InnerException, "Previous Exception");

            Flush();
        }

        private static ExceptionSource GetExceptionSource(Assembly callerAsm)
        {
            ExceptionSource exceptionSource;

            if (callerAsm.FullName.ToLower().Contains("common"))
                exceptionSource = ExceptionSource.COMMON;
            else if (callerAsm.FullName.ToLower().Contains("dal"))
                exceptionSource = ExceptionSource.DAL;
            else if (callerAsm.FullName.ToLower().Contains("data"))
                exceptionSource = ExceptionSource.DATA;
            else if (callerAsm.FullName.ToLower().Contains("entity"))
                exceptionSource = ExceptionSource.entity;
            else
                exceptionSource = ExceptionSource.UI;

            return exceptionSource;
        }
    }

    public enum ExceptionSource
    {
        COMMON,
        DAL,
        DATA,
        entity,
        UI
    }
}
