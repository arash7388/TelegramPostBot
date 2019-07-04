using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using NAudio.Wave;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using File = System.IO.File;

namespace PostBot
{
    public class Engvid
    {
        public static Mutex CrossJobMutex = new Mutex(false);
        private static readonly string ApiToken = "597852449:AAERAuRB3lgZfyCgRFuCioXC92KZowaDJSM";
        private static TelegramBotClient Bot = new TelegramBotClient(ApiToken);
        private static long EngcafevipChatId = -1001377736407; //eng cage vip ->@engcafevip
        private static long EngcafeListenAMinChatId = -1001403031261; //@engcafelistenamin
        //private static long EnglishWithSametChatId = -1001128563406;//eng cafe ->@englishwithsamet
        private static long ChatId = -1001259894749; //eng cafe ->@engcafeengvidemma
        //-1001385538411 @engcafeengvidemma
        private static readonly string EngvidTextFilePath = ConfigurationManager.AppSettings["EngvidTextFilePath"];

        public static async Task UploadEngVid()
        {
            //var me = await Bot.GetMeAsync();

            //string html = GetHtml("https://www.engvid.com/english-teacher/emma/");
            //try
            //{
                
            //string adr = @"https://r3---sn-aigzrn7d.googlevideo.com/videoplayback?initcwndbps=403750&signature=2667101C9C5FCA5B8345D2C427A3A008005C9B87.05950D803779E6717B2C9D42ED1F08760AED921D&c=WEB&expire=1542150186&ei=ygPrW9LKI4i91wKB6LrQAg&key=yt6&mime=video%2Fmp4&lmt=1539087982492643&dur=650.901&ratebypass=yes&clen=34328606&gir=yes&fvip=3&sparams=clen%2Cdur%2Cei%2Cgir%2Cid%2Cinitcwndbps%2Cip%2Cipbits%2Citag%2Clmt%2Cmime%2Cmm%2Cmn%2Cms%2Cmv%2Cpl%2Cratebypass%2Crequiressl%2Csource%2Cexpire&requiressl=yes&txp=5531432&itag=18&source=youtube&mv=m&mt=1542128449&ms=au%2Crdu&pl=24&id=o-AKbDo2tPsV54MLSMKTNA8hzYh1gdylBbP9zXM22TIPUl&ipbits=0&mn=sn-aigzrn7d%2Csn-aigl6ner&mm=31%2C29&ip=82.102.8.108";
            //var msg1 = Utility.SendVideoToChannel("@engcafeengvidemma", adr, "#EngVid #Emma #EngVidNo").GetAwaiter().GetResult();
            //}
            //catch (Exception ex)
            //{
            //    var a = 111;
            //}
            //because right side box of site loads with ajax 
            //var path = @"..\..\emma.html";
            //var htmlDoc = new HtmlDocument();
            //htmlDoc.Load(path);

            //var sUrls = "";

            //string html = GetHtml("emma.html");
            //HtmlDocument htmlDoc = new HtmlDocument();
            //htmlDoc.LoadHtml(html);
            //List<string> urls = new List<string>();

            //var internalLink1 = htmlDoc.GetElementbyId("lessonlinks_all_content");
            //var lessonLinks = internalLink1.SelectNodes(@"//a[contains(@class, 'lessonlinks_all_lesson_link')]");

            //foreach (HtmlNode lessonLink in lessonLinks)
            //{
            //    urls.Add(lessonLink.Attributes["href"].Value);
            //    sUrls += lessonLink.Attributes["href"].Value;
            //}

            var urls = File.ReadAllLines(EngvidTextFilePath);
            //string videoSession = "";
            //using (StreamReader reader = new StreamReader(EngvidTextFilePath))
            //{
            //    vocab = reader.ReadLine();
            //}
            int count = 0;

            foreach (string url in urls)
            {
                Console.WriteLine($"url is {url}");

                string html = Utility.GetHtml(url);
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(html);

                var iframe = htmlDoc.DocumentNode.SelectSingleNode(@"//iframe");
                var src = iframe.Attributes["src"].Value;
                var yt = new YoutubeUrlResolver();

                src = src.Replace("https://www.youtube.com/embed/", "");
                var ind = src.IndexOf("?");
                src = src.Substring(0, ind);

                var finalLink = "https://www.youtube.com/watch?v=" + src;
                var links = yt.Extractor(finalLink);

                var qs = "";
                foreach (var link in links)
                {
                    qs += link.ElementAt(1) + ";";
                }

                foreach (var link in links)
                {
                    if(qs.Contains("small") && link.ElementAt(1).ToLower() != "small")
                    continue;
                    
                    try
                    {
                        Console.WriteLine($"Send video to channel... count:{count}");
                        
                        var client = new WebClient();
                        client.DownloadFileAsync(new Uri(link.ElementAt(0)),"e:\\em");
                        var msg = await Utility.SendVideoToChannel("@engcafeengvidemma", link.ElementAt(0), "#EngVid #Emma #EngVidNo" + count++);
                        Utility.AppendToJsonFile(msg, MessageCategory.EngVid);
                        break;

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error engvid! {ex.Message}.");
                        //await RetrySendAudio(file, k, duration, MessageCategory.ESLPod);
                    }
                       
                    //WriteLine(link.ElementAt(0) + "\n"); // url of the video file at a particular resolution
                    //Console.WriteLine(link.ElementAt(1) + "\n\n"); //quality of the video file
                    
                }

                //Console.ReadLine();
            }
        }
       
    }

    public class YoutubeUrlResolver
    {
        public List<List<string>> Extractor(string url)
        {
            var html_content = "";
            using (var client = new WebClient())
            {
                client.Headers.Add("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_10_1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2227.1 Safari/537.36");
                html_content += client.DownloadString(url);
            }

            var Regex1 = new Regex(@"url=(.*?tags=\\u0026)", RegexOptions.Multiline);
            var matched = Regex1.Match(html_content);
            var download_infos = new List<List<string>>();
            foreach (var matched_group in matched.Groups)
            {
                var urls = Regex.Split(WebUtility.UrlDecode(matched_group.ToString().Replace("\\u0026", " &")), ",?url=");

                foreach (var vid_url in urls.Skip(1))
                {
                    var download_url = vid_url.Split(' ')[0].Split(',')[0].ToString();
                    Console.WriteLine(download_url);

                    // for quality info of the video
                    var Regex2 = new Regex("(quality=|quality_label=)(.*?)(,|&| |\")");
                    var qualityInfo = Regex2.Match(vid_url);
                    var quality = qualityInfo.Groups[2].ToString(); //quality_info
                    download_infos.Add((new List<string> { download_url, quality })); //contains url and resolution
                }
            }

            return download_infos;
        }
    }

}
