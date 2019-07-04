using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using NAudio.Wave;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sgml;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BTCPostBot
{
    class Program
    {


        private static readonly string ApiToken = "597852449:AAERAuRB3lgZfyCgRFuCioXC92KZowaDJSM";
        private static TelegramBotClient Bot = new TelegramBotClient(ApiToken);
        //private static long EngcafevipChatId = -1001377736407;//eng cage vip ->@engcafevip
        //private static long EnglishWithSametChatId = -1001128563406;//eng cafe ->@sarasamet
        private static long ChatId = -1001185105303; 
        //private static long EngCafeVOAChatId = -1001259894749; //eng cafe ->@engcafeVOA
        //private static long EngcafeEnglishPodChatId = -1001304367938; //eng cafe ->@engcafeEnglishPod
        //private static long EngcafeESLPodChatId = -1001185105303; //eng cafe ->@engcafeESLPod


        private static string AudioFilesPath
        {
            get { return @"C:\Users\Arash\Documents\Visual Studio 2015\Projects\RD\BTCPostBot\Audio"; }
        }

        //private static string _botName = ConfigurationManager.AppSettings["BotName"];

        static void Main(string[] args)
        {
            //TestJson();
            MainAsync(args).GetAwaiter().GetResult();

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
            AppendToJsonFile(result,MessageCategory.Unknown);

        }

        private static async Task SendPhotoToChannel(string path, string caption)
        {
            if (path.Contains("http:"))
                await Bot.SendPhotoAsync(ChatId, path, caption);
            else

                using (var stream = System.IO.File.Open(path, FileMode.Open))
                {
                    FileToSend fts = new FileToSend();
                    fts.Content = stream;
                    fts.Filename = path.Split('\\').Last();
                    await Bot.SendPhotoAsync(ChatId, fts, caption);
                }
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

                using (var stream = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    FileToSend fts = new FileToSend();
                    fts.Content = stream;
                    fts.Filename = filePath.Split('\\').Last();

                    var test =
                        Bot.SendAudioAsync(ChatId, fts, duration.Seconds, fts.Filename, fts.Filename)
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
                    await SendPhotoToChannel(src, msg);
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
                                Bot.SendAudioAsync(ChatId, href.Value, 1, "BBC",
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
                            await Bot.SendVideoAsync(ChatId, videoSrc, 0, caption);
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
                            await Bot.SendVideoAsync(ChatId, videoSrc, 0, caption);
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
                
                Bot.UploadTimeout = new TimeSpan(1, 0, 0);

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
                            var stream = System.IO.File.Open(file.FullName, FileMode.Open, FileAccess.Read,
                                FileShare.Read))
                        {
                            FileToSend fts = new FileToSend();
                            fts.Content = stream;
                            fts.Filename = file.FullName.Split('\\').Last();
                            var title = fts.Filename + Environment.NewLine;
                            //title += Environment.NewLine + "#ESLPodNo" + k;
                            int k = Convert.ToInt32(fts.Filename.Substring(0, 4));
                            try
                            {
                                var msg = await
                                    Bot.SendAudioAsync(ChatId, fts,
                                        Convert.ToInt32(Math.Truncate(duration.TotalSeconds)), fts.Filename, title);
                                AppendToJsonFile(msg,MessageCategory.ESLPod);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error! {ex.Message}. Retrying to send audio {fts.Filename}");
                                await RetrySendAudio(file, k, duration, MessageCategory.ESLPod);
                            }

                            Console.WriteLine($"mp3 sent. {fts.Filename}");
                        }
                    }
                    else if (file.Extension == ".pdf")
                    {
                        using (var stream = System.IO.File.Open(file.FullName, FileMode.Open, FileAccess.Read,
                            FileShare.Read))
                        {
                            FileToSend fts = new FileToSend();
                            fts.Content = stream;
                            fts.Filename = file.FullName.Split('\\').Last();
                            int k = Convert.ToInt32(fts.Filename.Substring(0, 4));

                            var title = fts.Filename + Environment.NewLine + "#ESLPod";
                            title += Environment.NewLine + "#ESLPodNo" + k;
                            try
                            {
                                var result = await Bot.SendDocumentAsync(ChatId, fts, title);
                                AppendToJsonFile(result, MessageCategory.ESLPod);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error! {ex.Message}. Retrying for send audio {fts.Filename}");
                                await RetrySendDocument(file, k, MessageCategory.ESLPod);
                            }
                            Console.WriteLine($"pdf sent. {fts.Filename}");
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
            var jsonData = System.IO.File.ReadAllText(filePath);
            var msgList = JsonConvert.DeserializeObject<List<Message>>(jsonData) ?? new List<Message>();

            foreach (Message message in msgList)
            {
                var result = Bot.ForwardMessageAsync(ChatId, message.Chat.Id, message.MessageId).GetAwaiter().GetResult();
            }
            
        }
        private static void AppendToJsonFile(Message msg, MessageCategory messageCategory)
        {
            try
            {
               var filePath = @"C:\Users\Arash\documents\visual studio 2015\Projects\Instagram\BTCPostBot\JsonMessages";

                switch (messageCategory)
                {
                        case MessageCategory.ESLPod:
                        filePath += @"\EslPodMessages.json";
                        break;

                    case MessageCategory.BBC6Min:
                        filePath += @"\BBC6MinMessages.json";
                        break;

                    default:
                        break;
                }

                if (!System.IO.File.Exists(filePath))
                    System.IO.File.Create(filePath);

                var jsonData = System.IO.File.ReadAllText(filePath);
                
                var setting = new JsonSerializerSettings();
                setting.NullValueHandling = NullValueHandling.Ignore;
                //setting.

                var msgList = JsonConvert.DeserializeObject<List<Message>>(jsonData, setting) ?? new List<Message>();

                msgList.Add(msg);

                jsonData = JsonConvert.SerializeObject(msgList);
                System.IO.File.WriteAllText(filePath, jsonData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"error in appending message to json file. {ex.Message}");
                throw;
            }
        }

        public static void TestJson()
        {
            try
            {
                var filePath = @"C:\Users\Arash\documents\visual studio 2015\Projects\Instagram\BTCPostBot\JsonMessages\EslPodMessages.json";

                
                if (!System.IO.File.Exists(filePath))
                    System.IO.File.Create(filePath);

                var jsonData = System.IO.File.ReadAllText(filePath);
                JArray jsonArray = JArray.Parse(jsonData);
               
                var setting = new JsonSerializerSettings();
                setting.NullValueHandling = NullValueHandling.Ignore;
                

                var msgList = JsonConvert.DeserializeObject<List<Message>>(jsonData, setting) ?? new List<Message>();

                jsonData = JsonConvert.SerializeObject(msgList);
                System.IO.File.WriteAllText(filePath, jsonData);
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
                var retryStream = System.IO.File.Open(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
                var fts = new FileToSend();
                fts.Content = retryStream;
                fts.Filename = file.FullName.Split('\\').Last();
                var title = fts.Filename + Environment.NewLine + "#" + messageCategory;
                title += Environment.NewLine + "#" + messageCategory + "No" + fileIndex;
                Thread.Sleep(5000);
                var result = await Bot.SendDocumentAsync(ChatId, fts, title);
                AppendToJsonFile(result, messageCategory);
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
                var retryStream = System.IO.File.Open(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
                var fts = new FileToSend();
                fts.Content = retryStream;
                fts.Filename = file.FullName.Split('\\').Last();
                var title = fts.Filename + Environment.NewLine + "#" + messageCategory;
                title += Environment.NewLine + "#"+ messageCategory+ "No" + k;
                
                var result = await Bot.SendAudioAsync(ChatId, fts, Convert.ToInt32(Math.Truncate(duration.TotalSeconds)), fts.Filename, title);
                AppendToJsonFile(result, messageCategory);

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
                Bot.UploadTimeout = new TimeSpan(1, 0, 0);
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

                            using (var stream = System.IO.File.Open(file.FullName, FileMode.Open, FileAccess.Read,
                                FileShare.Read))
                            {
                                FileToSend fts = new FileToSend();
                                fts.Content = stream;
                                fts.Filename = file.FullName.Split('\\').Last();
                                var title = fts.Filename + Environment.NewLine + "#EnglishPod";
                                title += Environment.NewLine + "#EnglishPodAdvancedNo" + k;
                                try
                                {
                                    await Bot.SendAudioAsync(ChatId, fts, Convert.ToInt32(Math.Truncate(duration.TotalSeconds)), fts.Filename, title);
                                }
                                catch (Exception ex)
                                {
                                    Thread.Sleep(5000);
                                    Console.WriteLine($"Error! {ex.Message}. Retrying for send audio {fts.Filename}");
                                    await Bot.SendAudioAsync(ChatId, fts, Convert.ToInt32(Math.Truncate(duration.TotalSeconds)), fts.Filename, title);
                                }

                                Console.WriteLine($"mp3 sent. {fts.Filename}");
                            }
                        }
                        else if (file.Extension == ".pdf")
                        {
                            using (var stream = System.IO.File.Open(file.FullName, FileMode.Open, FileAccess.Read,
                                FileShare.Read))
                            {
                                FileToSend fts = new FileToSend();
                                fts.Content = stream;
                                fts.Filename = file.FullName.Split('\\').Last();
                                var title = fts.Filename + Environment.NewLine + "#EnglishPod";
                                title += Environment.NewLine + "#EnglishPodAdvancedNo" + k;
                                try
                                {
                                    await Bot.SendDocumentAsync(ChatId, fts, title);
                                }
                                catch (Exception ex)
                                {
                                    Thread.Sleep(5000);
                                    Console.WriteLine($"Error! {ex.Message}. Retrying for send audio {fts.Filename}");
                                    await Bot.SendDocumentAsync(ChatId, fts, title);
                                }
                                Console.WriteLine($"pdf sent. {fts.Filename}");
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
                System.IO.File.Move(f.FullName, f.DirectoryName + "\\new\\" + f.Name.Replace("[www.langdownload.com]",""));
            }
        }

        static async Task UploadVOALearningEnglishTVMaterials()
        {
            try
            {
                var htmls = new List<string>();
                System.IO.File.Delete("links.txt");
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
                            System.IO.File.AppendAllText("links.txt", l + Environment.NewLine);
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
                                    await Bot.SendVideoAsync(ChatId, videoSrc, 0, caption);
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

                await Bot.SendTextMessageAsync(ChatId, chunk, false, false, 0, null, ParseMode.Html);
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
            System.Console.WriteLine($"Received choosen inline result: {chosenInlineResultEventArgs.ChosenInlineResult.ResultId}");
        }


        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            Console.Write("Message received!");

        }
    }


}

public static class Extensions
{
    private static readonly PersianCalendar _persianAssistante = new PersianCalendar();

    public static string ToFaString(this string value)
    {
        // 1728 , 1584
        string result = "";
        if (value != null && !String.IsNullOrEmpty(value))
        {
            char[] resChar = value.ToCharArray();
            for (int i = 0; i < resChar.Length; i++)
            {
                if (resChar[i] >= '0' && resChar[i] <= '9')
                    result += (char)(resChar[i] + 1728); //digitMapingTable[(resChar[i] - '0')];
                else
                    result += resChar[i];
            }
        }
        return result;
    }

    public static string ToFaGString(this string value)
    {
        try
        {
            value = value.Replace(",", "");
            decimal dec = Decimal.Parse(value);
            return dec.ToFaGString();
        }
        catch
        {
            return value;
        }
    }

    public static string ToFaGString(this decimal value)
    {
        string result;
        if (Math.Truncate(value) == value)
            result = $"{value:N0}";
        else
            result = $"{value:N}";
        return result.ToFaString();
    }

    public static string ToFaString(this int value)
    {
        string result = value.ToString();
        return result.ToFaString();
    }

    public static string ToFaDateTime(this DateTime dt)
    {
        string result = "";
        if (!dt.Equals(DateTime.MinValue))
        {
            result += _persianAssistante.GetYear(dt).ToString("000#") + "/";
            result += _persianAssistante.GetMonth(dt).ToString("0#") + "/";
            result += _persianAssistante.GetDayOfMonth(dt).ToString("0#");
            result += " ";
            result += _persianAssistante.GetHour(dt).ToString("0#") + ":";
            result += _persianAssistante.GetMinute(dt).ToString("0#") + ":";
            result += _persianAssistante.GetSecond(dt).ToString("0#");

        }
        return result;
    }
}


