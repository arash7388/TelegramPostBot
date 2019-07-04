using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Common;
using HtmlAgilityPack;
using NAudio.Wave;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sgml;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using File = System.IO.File;

namespace PostBot
{
    class Program
    {

        public static Mutex CrossJobMutex = new Mutex(false);
        private static readonly string ApiToken = "597852449:AAERAuRB3lgZfyCgRFuCioXC92KZowaDJSM";
        private static TelegramBotClient Bot = new TelegramBotClient(ApiToken);
        private static long EngcafevipChatId = -1001377736407;//eng cage vip ->@engcafevip
        private static long EngcafeListenAMinChatId = -1001403031261;//@engcafelistenamin
        //private static long EnglishWithSametChatId = -1001128563406;//eng cafe ->@englishwithsamet
        private static long ChatId = long.Parse(ConfigurationManager.AppSettings["ChatId"]);
        //private static long EngCafeVOAChatId = -1001259894749; //eng cafe ->@engcafeVOA
        //private static long EngcafeEnglishPodChatId = -1001304367938; //eng cafe ->@engcafeEnglishPod
        //private static long EngcafeESLPodChatId = -1001185105303; //eng cafe ->@engcafeESLPod
        //private static long Engcafeengvidemma = -1001385538411; //eng cafe ->@engcafeengvidemma
        public static bool JobsRunOnce = ConfigurationManager.AppSettings["JobsRunOnce"].ToSafeBool();


        private static string AudioFilesPath
        {
            get { return @"C:\Users\Arash\Documents\Visual Studio 2015\Projects\RD\BTCPostBot\Audio"; }
        }

        //private static string _botName = ConfigurationManager.AppSettings["BotName"];

        static void Main2(string[] args)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            //Engvid.UploadEngVid().GetAwaiter().GetResult();
            //LearnEngTodayJob.GetLastPageLinks();
            LearnEngTodayJob.Start();

            Console.ReadKey();
        }

        static void Main(string[] args)
        {
            //MainAsync(args).GetAwaiter().GetResult();

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;


            bool AllJobsEnabled = ConfigurationManager.AppSettings["AllJobsEnabled"].ToSafeBool();

            if (ConfigurationManager.AppSettings["NewsInLevelsPostEnabled"].ToSafeBool() || AllJobsEnabled)
                NewsInLevelsJob.Start();
            
            if (ConfigurationManager.AppSettings["CambridgePostEnabled"].ToSafeBool() || AllJobsEnabled)
                CambridgeJob.Start();

            if (ConfigurationManager.AppSettings["ListenAMinPostEnabled"].ToSafeBool() || AllJobsEnabled)
                ListenAMinJob.Start();

            SendAdvertisementPhoto();

            if (ConfigurationManager.AppSettings["EslFastPostEnabled"].ToSafeBool() || AllJobsEnabled)
                EslFastJob.Start();

            if (ConfigurationManager.AppSettings["PhotoWithTextPostEnabled"].ToSafeBool() || AllJobsEnabled)
                PhotoWithTextJob.Start();

            if (ConfigurationManager.AppSettings["LearnEngTodayPostEnabled"].ToSafeBool() || AllJobsEnabled)
                LearnEngTodayJob.Start();

            if (ConfigurationManager.AppSettings["CommonMistakesPostEnabled"].ToSafeBool() || AllJobsEnabled)
                CommonMistakesJob.Start();

            if (ConfigurationManager.AppSettings["EslPodEnabled"].ToSafeBool() || AllJobsEnabled)
                EslPodJob.Start();

            SendAdvertisementPhoto();

            Console.ReadKey();

        }

        private static void SendAdvertisementPhoto()
        {
            var dayIndex = (int)DateTime.Now.DayOfWeek;
#if DEBUG
            var advertisingImagePath = string.Join("\\", Assembly.GetExecutingAssembly().Location.Split('\\').TakeWhile(a => a != "bin")) + "\\Images\\" + dayIndex.ToString() + dayIndex.ToString() + ".jpg";
#else
            var advertisingImagePath = string.Join("\\", Assembly.GetExecutingAssembly().Location.Split('\\').TakeWhile(a => a != "PostBot.exe")) + "\\Images\\" + dayIndex.ToString() + dayIndex.ToString() + ".jpg";

#endif

            Console.WriteLine(advertisingImagePath);
            Utility.SendPhotoToChannel(ChatId, advertisingImagePath, "\nJoin my channel and find out more: https://t.me/EnglishWithSamet").GetAwaiter().GetResult();
        }



        //getting url of all entries ...
        public static void ListenAMin()
        {
            string resultText = "";

            try
            {
                var baseUrl = "https://listenaminute.com";
                string html = Utility.GetHtml(baseUrl);
                HtmlDocument htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(html);

                var table = htmlDoc.DocumentNode.SelectNodes(@"//table");


                foreach (HtmlNode node in table)
                {
                    //var googleads = node.SelectNodes(@".//ins");
                    var anchors = node.SelectNodes(@".//a");

                    foreach (HtmlNode anchor in anchors.OrderBy(a=>a.Attributes["href"].Value))
                    {
                        File.AppendAllText(@".../../ListenAMin.txt", baseUrl + "/" + anchor.Attributes["href"].Value + Environment.NewLine);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        static async Task MainAsync(string[] args)
        {
            Bot.OnMessage += BotOnMessageReceived;
            Bot.OnMessageEdited += BotOnMessageReceived;
            Bot.OnInlineResultChosen += BotOnChosenInlineResultReceived;
            Bot.OnReceiveError += BotOnReceiveError;

            Console.WriteLine("Bot started ...");
            var me = Bot.GetMeAsync().Result;
            Console.Title = me.Username;


            //SendMsgToChannel();
            //SendPhotoToChannel();
            //SendAudioToChannel();
            //UploadBBC6Min();
            //Test();
            //await UploadBBC6MinMaterials();
            //await UploadVOAEveryDayGrammarMaterials();
            //await UploadVOANewsWordsMaterials();
            //await UploadVOALearningEnglishTVMaterials();
            //await UploadEnglishPodMaterials();
            //RenameFiles();
            await UploadESLPodMaterials();
            //ForwardMessage();




            Bot.StartReceiving();
            Console.Write("Press any key to exit ...");
            Console.ReadLine();
            Bot.StopReceiving();
        }


        private static void SendMsgToChannel()
        {
            var result = Bot.SendTextMessageAsync(ChatId, "aaa bbb").GetAwaiter().GetResult();
            Utility.AppendToJsonFile(result,MessageCategory.Unknown);

        }

        

        private static void SendAudioToChannel()
        {
            DirectoryInfo d = new DirectoryInfo(AudioFilesPath);
            FileInfo[] Files = d.GetFiles("*.mp3");
            foreach (FileInfo audioFile in Files)
            {

                var filePath = AudioFilesPath + "\\" + audioFile.Name;
                string fileExt = Path.GetExtension(filePath);
                TimeSpan duration = new TimeSpan();
                if (fileExt == ".mp3")
                {
                    //Use NAudio to get the duration of the File as a TimeSpan object
                    duration = new Mp3FileReader(filePath).TotalTime;
                }

                using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    InputOnlineFile file = new InputOnlineFile(stream);
                    file.FileName = filePath.Split('\\').Last();

                    var test =
                        Bot.SendAudioAsync(ChatId, file, file.FileName,duration:duration.Seconds)
                            .GetAwaiter()
                            .GetResult();
                }
            }
        }


        //static void FindMp3s()
        //{
        //    using (var reader = new SgmlReader())
        //    {
        //        reader.DocType = "HTML";
        //        reader.Href = "http://www.bbc.co.uk/learningenglish/english/features/6-minute-english/ep-180913";
        //        var doc = new XmlDocument();
        //        doc.Load(reader);
        //        var anchors = doc.SelectNodes("//a/@href[contains(., 'mp3') or contains(., 'wav')]");
        //        foreach (XmlAttribute href in anchors)
        //        {
        //            using (var client = new WebClient())
        //            {
        //                var data = client.DownloadData(href.Value);
        //                // TODO: do something with the downloaded data
        //            }
        //        }
        //    }
        //}

        static async Task UploadBBC6MinMaterials()
        {
            var firstDateStr = "180322";
            var fileDateTime = new DateTime(Convert.ToInt32(firstDateStr.Substring(0, 2)) + 2000,
                Convert.ToInt32(firstDateStr.Substring(2, 2).PadLeft(2, '0')),
                Convert.ToInt32(firstDateStr.Substring(4, 2).PadLeft(2, '0')));

            int k = 12;

            while (fileDateTime.Year == 2018)
            {

                var yy = fileDateTime.Year - 2000;
                var mm = fileDateTime.Month.ToString().PadLeft(2, '0');
                var dd = fileDateTime.Day.ToString().PadLeft(2, '0');

                string dateStr = yy + mm + dd;

                using (var reader = new SgmlReader())
                {
                    reader.DocType = "HTML";
                    reader.Href = "http://www.bbc.co.uk/learningenglish/english/features/6-minute-english/ep-" + dateStr;
                    var lastSplitted = reader.Href.Split('/').Last();
                    var d = lastSplitted.Substring(lastSplitted.Length - 6);

                    var doc = new XmlDocument();
                    doc.Load(reader);
                    var episodeName = GetBBCEpisodeName(doc);
                    var msg = "BBC 6 Min Eng 20" + d.Substring(0, 2) + "/" +
                              d.Substring(2, 2) +
                              "/" + d.Substring(4, 2) + Environment.NewLine +
                              " شماره " + k++ + Environment.NewLine +
                              "#BBC6Min"
                              + Environment.NewLine +
                              "#BBC6Min20" + d.Substring(0, 2) + Environment.NewLine + episodeName;

                    var src = ExtractImageSource(doc, dateStr);
                    await Utility.SendPhotoToChannel(ChatId,src, msg);
                    Console.WriteLine($"Message and photo sent. date:{dateStr}");

                    var anchors = doc.SelectNodes("//a/@href[contains(., 'mp3') or contains(., 'wav')]");

                    //var script = doc.SelectNodes("//div[@class='widget widget-richtext 6']");
                    //await SendScript(script);

                    if (anchors == null)
                        Console.WriteLine("no mp3 found!");

                    foreach (XmlAttribute href in anchors)
                    {
                        using (var client = new WebClient())
                        {
                            //var data = client.DownloadData(href.Value);
                            var fileName = href.Value.Split('/').Last();
                            var fileTitle = fileName.Substring(20);
                            await
                                Bot.SendAudioAsync(ChatId, href.Value, "BBC",ParseMode.Default,0,null,
                                    fileTitle + Environment.NewLine + "#BBC6MinuteEnglishAudio20" + d);
                            Console.WriteLine($"mp3 sent. date:{dateStr}");
                        }
                    }

                    var pdfAnchors = doc.SelectNodes("//a/@href[contains(., 'pdf')]");

                    if (pdfAnchors == null)
                        Console.Write("no pdf found!");

                    foreach (XmlAttribute href in pdfAnchors)
                    {
                        await
                            Bot.SendDocumentAsync(ChatId, href.Value,
                                episodeName + Environment.NewLine + "#BBC6MinPDF20" + d);
                        Console.WriteLine($"pdf sent. date:{dateStr}");
                    }

                    fileDateTime = fileDateTime.AddDays(7);
                }
            }
        }

        static async Task UploadVOAEveryDayGrammarMaterials()
        {
            var htmls = new List<string>();

            //page is loaded in ajax with p=1 to p=10

            for (int i = 0; i < 10; i++)
            {
                using (var reader = new SgmlReader())
                {
                    reader.DocType = "HTML";

                    string q = i == 0 ? "" : "?p=" + i;
                    reader.Href = " https://learningenglish.voanews.com/z/4716" + q;

                    var doc = new XmlDocument();
                    doc.Load(reader);

                    //var anchors = doc.SelectNodes("//li/div/a[contains(@href,'/a/')]");
                    //var anchors = doc.SelectNodes("//li/div/a[contains(@class,'img-wrap')]");   and contains(@class,'with-date')] and contains(@class,'has-img')] and contains(@class,'size-')]
                    var anchors = doc.SelectNodes("//div[contains(@class,'media-block')]/a ");

                    foreach (XmlNode anchor in anchors)
                    {
                        htmls.Add("https://learningenglish.voanews.com" + anchor.Attributes["href"].Value);
                    }

                }
            }

            foreach (string html in htmls)
            {
                using (var reader = new SgmlReader())
                {
                    reader.DocType = "HTML";
                    reader.Href = html;

                    var doc = new XmlDocument();
                    doc.Load(reader);

                    var anchors = doc.SelectNodes("//div[contains(@class,'c-mmp__player')] ");
                    var captionNode = doc.SelectSingleNode("//h1[@class='']");
                    var caption = captionNode.InnerText;

                    foreach (XmlNode anchor in anchors)
                    {
                        var video = anchor.FirstChild;
                        var videoSrc = video.Attributes["src"].Value;
                        if (videoSrc.Contains(".mp4"))
                            await Bot.SendVideoAsync(ChatId, videoSrc, 0, caption:caption);
                    }
                }
            }
        }

        static async Task UploadVOANewsWordsMaterials()
        {
            var htmls = new List<string>();

            //page is loaded in ajax with p=1 to p=10

            for (int i = 0; i < 5; i++)
            {
                using (var reader = new SgmlReader())
                {
                    reader.DocType = "HTML";

                    string q = i == 0 ? "" : "?p=" + i;
                    reader.Href = " https://learningenglish.voanews.com/z/3620" + q;

                    var doc = new XmlDocument();
                    doc.Load(reader);

                    //var anchors = doc.SelectNodes("//li/div/a[contains(@href,'/a/')]");
                    //var anchors = doc.SelectNodes("//li/div/a[contains(@class,'img-wrap')]");   and contains(@class,'with-date')] and contains(@class,'has-img')] and contains(@class,'size-')]
                    var anchors = doc.SelectNodes("//div[contains(@class,'media-block')]/a ");

                    foreach (XmlNode anchor in anchors)
                    {
                        htmls.Add("https://learningenglish.voanews.com" + anchor.Attributes["href"].Value);
                    }

                }
            }

            foreach (string html in htmls)
            {
                using (var reader = new SgmlReader())
                {
                    reader.DocType = "HTML";
                    reader.Href = html;

                    var doc = new XmlDocument();
                    doc.Load(reader);

                    var anchors = doc.SelectNodes("//div[contains(@class,'c-mmp__player')] ");
                    var captionNode = doc.SelectSingleNode("//h1[@class='']");
                    var caption = captionNode.InnerText + Environment.NewLine + "#VOANewsWords";

                    foreach (XmlNode anchor in anchors)
                    {
                        var video = anchor.FirstChild;
                        var videoSrc = video.Attributes["src"].Value;
                        if (videoSrc.Contains(".mp4"))
                            await Bot.SendVideoAsync(ChatId, videoSrc, 0, caption:caption);
                        Console.WriteLine($"video sent {caption}");
                    }
                }
            }
        }

        static async Task UploadESLPodMaterials()
        {
            try
            {
                //send audio does not have captions and K does not set for mp3s.

                var path = @"C:\Users\Arash\Downloads\Compressed\ESL Podcast - Lessons 051-200 [www.langdownload.com]";
                DirectoryInfo mainDir = new DirectoryInfo(path);
                
                Bot.Timeout = new TimeSpan(1, 0, 0);

                int startFrom = 199;

                foreach (FileInfo file in mainDir.GetFiles().OrderBy(a => a.Name))
                {
                    var fileNameIndex = Convert.ToInt32(file.FullName.Split('\\').Last().Substring(0, 4));
                    if (fileNameIndex < startFrom)
                    {
                        continue;
                    }
                    //if (file.Name != "0058 - Getting Ready to Go .mp3")
                    //    continue;

                    if (file.Extension == ".mp3")
                    {
                        TimeSpan duration = new TimeSpan();

                        //Use NAudio to get the duration of the File as a TimeSpan object
                        duration = new Mp3FileReader(file.FullName).TotalTime;

                        using (
                            var stream = File.Open(file.FullName, FileMode.Open, FileAccess.Read,
                                FileShare.Read))
                        {
                            InputOnlineFile fts = new InputOnlineFile(stream);
                            fts.FileName = file.FullName.Split('\\').Last();
                            var title = fts.FileName + Environment.NewLine;
                            //title += Environment.NewLine + "#ESLPodNo" + k;
                            int k = Convert.ToInt32(fts.FileName.Substring(0, 4));
                            try
                            {
                                var msg = await
                                    Bot.SendAudioAsync(ChatId, fts, title
                                        ,ParseMode.Default, Convert.ToInt32(Math.Truncate(duration.TotalSeconds)), fts.FileName, title);
                                Utility.AppendToJsonFile(msg,MessageCategory.ESLPod);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error! {ex.Message}. Retrying to send audio {fts.FileName}");
                                await RetrySendAudio(file, k, duration, MessageCategory.ESLPod);
                            }

                            Console.WriteLine($"mp3 sent. {fts.FileName}");
                        }
                    }
                    else if (file.Extension == ".pdf")
                    {
                        using (var stream = File.Open(file.FullName, FileMode.Open, FileAccess.Read,
                            FileShare.Read))
                        {
                            InputOnlineFile fts = new InputOnlineFile(stream);
                            fts.FileName = file.FullName.Split('\\').Last();
                            int k = Convert.ToInt32(fts.FileName.Substring(0, 4));

                            var title = fts.FileName + Environment.NewLine + "#ESLPod";
                            title += Environment.NewLine + "#ESLPodNo" + k;
                            try
                            {
                                var result = await Bot.SendDocumentAsync(ChatId, fts, title);
                                Utility.AppendToJsonFile(result, MessageCategory.ESLPod);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error! {ex.Message}. Retrying for send audio {fts.FileName}");
                                await RetrySendDocument(file, k, MessageCategory.ESLPod);
                            }
                            Console.WriteLine($"pdf sent. {fts.FileName}");
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine($"Error! {ex.Message}");
            }
        }

        private static void ForwardMessage()
        {
            //test 
            var filePath = @"C:\Users\Arash\documents\visual studio 2015\Projects\Instagram\BTCPostBot\SentMessages.json";
            var jsonData = File.ReadAllText(filePath);
            var msgList = JsonConvert.DeserializeObject<List<Message>>(jsonData) ?? new List<Message>();

            foreach (Message message in msgList)
            {
                var result = Bot.ForwardMessageAsync(ChatId, message.Chat.Id, message.MessageId).GetAwaiter().GetResult();
            }
            
        }

        public static void TestJson()
        {
            try
            {
                var filePath = @"C:\Users\Arash\documents\visual studio 2015\Projects\Instagram\BTCPostBot\JsonMessages\EslPodMessages.json";

                
                if (!File.Exists(filePath))
                    File.Create(filePath);

                var jsonData = File.ReadAllText(filePath);
                JArray jsonArray = JArray.Parse(jsonData);
               
                var setting = new JsonSerializerSettings();
                setting.NullValueHandling = NullValueHandling.Ignore;
                

                var msgList = JsonConvert.DeserializeObject<List<Message>>(jsonData, setting) ?? new List<Message>();

                jsonData = JsonConvert.SerializeObject(msgList);
                File.WriteAllText(filePath, jsonData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"error in appending message to json file. {ex.Message}");
                throw;
            }
        }
        private static async Task RetrySendDocument(FileInfo file, int fileIndex,MessageCategory messageCategory)
        {
            try
            {
                Bot = new TelegramBotClient(ApiToken);
                var me = Bot.GetMeAsync().Result;
                //Bot.StopReceiving();
                Bot.StartReceiving();
                var retryStream = File.Open(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
                var fts = new InputOnlineFile(retryStream);
              
                fts.FileName = file.FullName.Split('\\').Last();
                var title = fts.FileName + Environment.NewLine + "#" + messageCategory;
                title += Environment.NewLine + "#" + messageCategory + "No" + fileIndex;
                Thread.Sleep(5000);
                var result = await Bot.SendDocumentAsync(ChatId, fts, title);
                Utility.AppendToJsonFile(result, messageCategory);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Retry failed with messag:{ex.Message}. trying again...");
                await RetrySendDocument(file, fileIndex, messageCategory);
            }
        }

        private static async Task RetrySendAudio(FileInfo file, int k, TimeSpan duration,MessageCategory messageCategory)
        {
            try
            {
                Bot = new TelegramBotClient(ApiToken);
                var me = Bot.GetMeAsync().Result;
                //Bot.StopReceiving();
                Bot.StartReceiving();
                Thread.Sleep(5000);
                var retryStream = File.Open(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
                var fts = new InputOnlineFile(retryStream);
               fts.FileName = file.FullName.Split('\\').Last();
                var title = fts.FileName + Environment.NewLine + "#" + messageCategory;
                title += Environment.NewLine + "#"+ messageCategory+ "No" + k;
                
                var result = await Bot.SendAudioAsync(ChatId, fts, duration:Convert.ToInt32(Math.Truncate(duration.TotalSeconds)),caption: fts.FileName, title:title);
                Utility.AppendToJsonFile(result, messageCategory);

            }
            catch (Exception ex)
            {
                Console.Write($"retry failed with message:{ex.Message}. retrying again ...");
                await RetrySendAudio(file, k, duration, messageCategory);
            }
        }

        static async Task UploadEnglishPodMaterials()
        {
            try
            {
                var path =
                    @"C:\Users\Arash\Documents\Visual Studio 2015\Projects\RD\BTCPostBot\Audio\EnglishPod-Advanced";
                DirectoryInfo mainDir = new DirectoryInfo(path);
                int k = 22;
                Bot.Timeout = new TimeSpan(1, 0, 0);
                foreach (DirectoryInfo dir in mainDir.GetDirectories())
                {
                    k++;
                    FileInfo[] files = dir.GetFiles();
                    foreach (FileInfo file in files)
                    {
                        if (file.Extension == ".mp3")
                        {
                            TimeSpan duration = new TimeSpan();

                            //Use NAudio to get the duration of the File as a TimeSpan object
                            duration = new Mp3FileReader(file.FullName).TotalTime;

                            using (var stream = File.Open(file.FullName, FileMode.Open, FileAccess.Read,
                                FileShare.Read))
                            {
                                InputOnlineFile fts = new InputOnlineFile(stream);
                               
                                fts.FileName = file.FullName.Split('\\').Last();
                                var title = fts.FileName + Environment.NewLine + "#EnglishPod";
                                title += Environment.NewLine + "#EnglishPodAdvancedNo" + k;
                                try
                                {
                                    await Bot.SendAudioAsync(ChatId, fts,duration: Convert.ToInt32(Math.Truncate(duration.TotalSeconds)),caption: fts.FileName,title: title);
                                }
                                catch (Exception ex)
                                {
                                    Thread.Sleep(5000);
                                    Console.WriteLine($"Error! {ex.Message}. Retrying for send audio {fts.FileName}");
                                    await Bot.SendAudioAsync(ChatId, fts, duration:Convert.ToInt32(Math.Truncate(duration.TotalSeconds)), caption:fts.FileName,title: title);
                                }

                                Console.WriteLine($"mp3 sent. {fts.FileName}");
                            }
                        }
                        else if (file.Extension == ".pdf")
                        {
                            using (var stream = File.Open(file.FullName, FileMode.Open, FileAccess.Read,
                                FileShare.Read))
                            {
                                InputOnlineFile fts = new InputOnlineFile(stream);
                              
                                fts.FileName = file.FullName.Split('\\').Last();
                                var title = fts.FileName + Environment.NewLine + "#EnglishPod";
                                title += Environment.NewLine + "#EnglishPodAdvancedNo" + k;
                                try
                                {
                                    await Bot.SendDocumentAsync(ChatId, fts, title);
                                }
                                catch (Exception ex)
                                {
                                    Thread.Sleep(5000);
                                    Console.WriteLine($"Error! {ex.Message}. Retrying for send audio {fts.FileName}");
                                    await Bot.SendDocumentAsync(ChatId, fts, title);
                                }
                                Console.WriteLine($"pdf sent. {fts.FileName}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error! {ex.Message}");
            }
        }

        static void RenameFiles()
        {
            var path = @"C:\Users\Arash\Downloads\Compressed\ESL Podcast - Lessons 051-200 [www.langdownload.com]";
            DirectoryInfo mainDir = new DirectoryInfo(path);

            var files = mainDir.GetFiles();
            foreach (FileInfo f in files)
            {
                File.Move(f.FullName, f.DirectoryName + "\\new\\" + f.Name.Replace("[www.langdownload.com]",""));
            }
        }

        static async Task UploadVOALearningEnglishTVMaterials()
        {
            try
            {
                var htmls = new List<string>();
                File.Delete("links.txt");
                //page is loaded in ajax with p=1 to p=10

                for (int i = 0; i < 7; i++)
                {
                    using (var reader = new SgmlReader())
                    {
                        reader.DocType = "HTML";

                        string q = i == 0 ? "" : "?p=" + i;
                        reader.Href = " https://learningenglish.voanews.com/z/3613" + q;

                        var doc = new XmlDocument();
                        doc.Load(reader);

                        //var anchors = doc.SelectNodes("//li/div/a[contains(@href,'/a/')]");
                        //var anchors = doc.SelectNodes("//li/div/a[contains(@class,'img-wrap')]");   and contains(@class,'with-date')] and contains(@class,'has-img')] and contains(@class,'size-')]
                        var anchors = doc.SelectNodes("//div[contains(@class,'media-block')]/a ");

                        foreach (XmlNode anchor in anchors)
                        {
                            var l = "https://learningenglish.voanews.com" + anchor.Attributes["href"].Value;
                            htmls.Add(l);
                            //saving links to a text file
                            File.AppendAllText("links.txt", l + Environment.NewLine);
                        }
                    }
                }

                foreach (string html in htmls)
                {
                    using (var reader = new SgmlReader())
                    {
                        reader.DocType = "HTML";
                        reader.Href = html;

                        var doc = new XmlDocument();
                        doc.Load(reader);

                        var anchors = doc.SelectNodes("//div[contains(@class,'c-mmp__player')] ");
                        var captionNode = doc.SelectSingleNode("//h1[@class='']");
                        var caption = captionNode.InnerText + Environment.NewLine + "#VOALearningEnglishTV";

                        foreach (XmlNode anchor in anchors)
                        {
                            var video = anchor.FirstChild;
                            var videoSrc = video.Attributes["src"].Value;
                            if (videoSrc.Contains(".mp4"))
                            {
                                try
                                {
                                    await Bot.SendVideoAsync(ChatId, videoSrc, 0, caption: caption);
                                    //large files more than 20mb cannot be uploaded.
                                    Console.WriteLine($"video sent {caption} Link:{html}");

                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"video NOT sent {caption} Link:{html} \n {ex.Message}");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occured . {ex.Message}");
            }
        }

        private static string GetBBCEpisodeName(XmlDocument doc)
        {
            var pdfAnchors = doc.SelectNodes("//a/@href[contains(., 'pdf')]");

            if (pdfAnchors == null)
                Console.Write("no pdf found to exract episode name!");

            string episodeName = "";
            foreach (XmlAttribute href in pdfAnchors)
            {
                var pdfFileName = href.Value.Split('/').Last();
                var index = pdfFileName.IndexOf("_6min_english_") + 13;
                if (index == -1) index = pdfFileName.IndexOf("_6min_") + 6;

                episodeName = pdfFileName.Substring(index).Replace("_", " ").Replace(".pdf", "");
            }

            return episodeName;
        }

        private static string ExtractImageSource(XmlDocument doc, string dateStr)
        {
            XmlNodeList anchorsWithImages =
                doc.SelectNodes("//li/a[contains(@href,'/english/features/6-minute-english/ep-" + dateStr + "')]");
            //@href[contains('ep-')
            string src = "";

            foreach (XmlNode node in anchorsWithImages)
            {
                if (node.ChildNodes.Count == 1 && node.ChildNodes[0]?.Name.ToLower() == "img")
                {
                    var img = node.ChildNodes[0];
                    src = img.Attributes["src"].Value;
                }
            }
            return src;
        }

        private static async Task SendScript(XmlNodeList script)
        {
            var xml = script[0].InnerXml;
            var chunks = SplitBySize(xml, 4096);
            foreach (string chunk in chunks)
            {
                //chunk = chunk.Replace("</strong", "");
                var countOfB = CountStringOccurrences(chunk, "<strong>");
                var countOfBClosed = CountStringOccurrences(chunk, "</strong>");

                xml = xml.Replace("</br>", "\n")
                    .Replace("<em>", "").Replace("</em>", "\n")
                    .Replace("<div class=\"text\" dir=\"ltr\">", "")
                    .Replace("<br />", "\n")
                    .Replace("<p>", "").Replace("</p>", "\n")
                    .Replace("<h1>", "").Replace("</h1>", "\n")
                    .Replace("<h2>", "").Replace("</h2>", "\n")
                    .Replace("<h3>", "").Replace("</h3>", "\n")
                    //.Replace("<strong>", "<b>").Replace("</strong>", "</b>\n")
                    .Replace("<strong>", "").Replace("</strong>", "")
                    .Replace("<div>", "").Replace("</div>", "\n");

                await Bot.SendTextMessageAsync(ChatId, chunk, ParseMode.Html);
            }
        }


        /// <summary>
        /// Count occurrences of strings.
        /// </summary>
        public static int CountStringOccurrences(string text, string pattern)
        {
            // Loop through all instances of the string 'text'.
            int count = 0;
            int i = 0;
            while ((i = text.IndexOf(pattern, i)) != -1)
            {
                i += pattern.Length;
                count++;
            }
            return count;
        }

        static List<string> SplitBySize(string str, int chunkSize)
        {
            str = "123123123000";
            chunkSize = 3;

            int count = 0;
            if (str.Length % chunkSize == 0)
            {
                count = str.Length / chunkSize;
                return Enumerable.Range(0, count).Select(i => str.Substring(i * chunkSize, chunkSize)).ToList();

            }

            count = (int)Math.Truncate((decimal)str.Length / chunkSize);
            var x = Enumerable.Range(0, count).Select(i => str.Substring(i * chunkSize, chunkSize)).ToList();
            x.Add(str.Substring(count * chunkSize));
            return x;
        }

        private static void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            Console.Write($"receiveErrorEventArgs.ApiRequestException.Message: {receiveErrorEventArgs.ApiRequestException.Message}");
        }

        private static void BotOnChosenInlineResultReceived(object sender, ChosenInlineResultEventArgs chosenInlineResultEventArgs)
        {
            Console.WriteLine($"Received choosen inline result: {chosenInlineResultEventArgs.ChosenInlineResult.ResultId}");
        }


        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            Console.Write("Message received!");

        }

        
    }

    
}