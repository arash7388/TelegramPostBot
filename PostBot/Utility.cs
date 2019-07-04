using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using File = System.IO.File;

namespace PostBot
{
    public class Utility
    {
        private static readonly string ApiToken = "597852449:AAERAuRB3lgZfyCgRFuCioXC92KZowaDJSM";
        private static TelegramBotClient Bot = new TelegramBotClient(ApiToken);
        //private static long EnglishWithSametChatId = -1001128563406;//eng cafe ->@sarasamet
        private static long EngcafevipChatId = -1001259894749;

        public static string GetHtml(string url)
        {
            string urlAddress = url;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            string data = "";
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                }

                data = readStream.ReadToEnd();

                response.Close();
                readStream.Close();
            }

            return data;
        }

        public static async Task SendPhotoToChannel(long chatId, string path, string caption)
        {
            if (path.Contains("http:") || path.Contains("https:"))
                await Bot.SendPhotoAsync(chatId, path, caption);
            else

                using (var stream = File.Open(path, FileMode.Open))
                {
                    InputOnlineFile fts = new InputOnlineFile(stream);
                    fts.FileName = path.Split('\\').Last();
                    await Bot.SendPhotoAsync(chatId, fts, caption);
                }
        }

        public static async Task<Message> SendVideoToChannel(string chatId, string path, string caption)
        {
            Bot.Timeout = new TimeSpan(1,0,0);
            if (path.Contains("http:") || path.Contains("https:"))
                return await Bot.SendVideoAsync(chatId, path, caption:caption);
            else

                using (var stream = File.Open(path, FileMode.Open))
                {
                    InputOnlineFile fts = new InputOnlineFile(stream);
                    fts.FileName = path.Split('\\').Last();
                    return await Bot.SendVideoAsync(chatId, fts, caption:caption);
                }
        }

        public static void AppendToJsonFile(Message msg, MessageCategory messageCategory)
        {
            try
            {
                var filePath = @"C:\Users\Arash\documents\visual studio 2015\Projects\Instagram\PostBot\JsonMessages";

                switch (messageCategory)
                {
                    case MessageCategory.ESLPod:
                        filePath += @"\EslPodMessages.json";
                        break;

                    case MessageCategory.BBC6Min:
                        filePath += @"\BBC6MinMessages.json";
                        break;

                    case MessageCategory.EngVid:
                        filePath += @"\EngvidEmma.json";
                        break;

                    default:
                        break;
                }

                if (!File.Exists(filePath))
                    File.Create(filePath);

                var jsonData = File.ReadAllText(filePath);
                
                var setting = new JsonSerializerSettings();
                setting.NullValueHandling = NullValueHandling.Ignore;
                //setting.

                var msgList = JsonConvert.DeserializeObject<List<Message>>(jsonData, setting) ?? new List<Message>();

                msgList.Add(msg);

                jsonData = JsonConvert.SerializeObject(msgList);
                File.WriteAllText(filePath, jsonData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"error in appending message to json file. {ex.Message}");
                throw;
            }
        }
    }

    public static class Util
    {
        public static string AdjustHTMLText(this string input)
        {
            input = input.Replace("(adsbygoogle = window.adsbygoogle || []).push({});", "").TrimEnd().TrimStart();
            input = input.Replace("&rdquo;", "”").Replace("&ldquo", "“")
                                .Replace("&lsquo;", "‘").Replace("&rsquo;", "’").Replace("&sbquo;", "‚");

            return input;
        }
    }

}
