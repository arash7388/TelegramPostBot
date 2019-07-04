using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;

namespace BTCPostBot
{
    class Program
    {

        private static readonly TelegramBotClient Bot = new TelegramBotClient("597852449:AAERAuRB3lgZfyCgRFuCioXC92KZowaDJSM");
        //private static string[] Groups = ConfigurationManager.AppSettings["Groups"].Split(',');
        //private static int EachRowItems = Convert.ToInt32(ConfigurationManager.AppSettings["RowItems"]);
        //private static string Path = ConfigurationManager.AppSettings["Path"];
        private static int _dollarPrice = Convert.ToInt32(ConfigurationManager.AppSettings["DollarPrice"]);
        private static int _coinsCount = Convert.ToInt32(ConfigurationManager.AppSettings["CoinsCount"]);
        private static string _botName = ConfigurationManager.AppSettings["BotName"];
        private static string _introText = @" به بات " + _botName + " خوش آمدید";

        static void Main(string[] args)
        {
            Bot.OnCallbackQuery += BotOnCallbackQueryReceived;
            Bot.OnMessage += BotOnMessageReceived;
            Bot.OnMessageEdited += BotOnMessageReceived;
            //Bot.OnInlineQuery += BotOnInlineQueryReceived;
            Bot.OnInlineResultChosen += BotOnChosenInlineResultReceived;
            Bot.OnReceiveError += BotOnReceiveError;

            var me = Bot.GetMeAsync().Result;

            System.Console.Title = me.Username;

            Bot.StartReceiving();
            System.Console.ReadLine();
            Bot.StopReceiving();
        }


        private static void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            Debugger.Break();
        }

        private static void BotOnChosenInlineResultReceived(object sender, ChosenInlineResultEventArgs chosenInlineResultEventArgs)
        {
            System.Console.WriteLine($"Received choosen inline result: {chosenInlineResultEventArgs.ChosenInlineResult.ResultId}");
        }

        //private static async void BotOnInlineQueryReceived(object sender, InlineQueryEventArgs inlineQueryEventArgs)
        //{
        //    var results = GetLocation();

        //    await Bot.AnswerInlineQueryAsync(inlineQueryEventArgs.InlineQuery.Id, results, isPersonal: true, cacheTime: 0);
        //}

        //private static InlineQueryResult[] GetLocation()
        //{
        //    InlineQueryResult[] results =
        //    {
        //        new InlineQueryResultLocation
        //        {
        //            Id = "1",
        //            Latitude = 40.7058316f, // displayed result
        //            Longitude = -74.2581888f,
        //            Title = "New York",
        //            InputMessageContent = new InputLocationMessageContent // message if result is selected
        //            {
        //                Latitude = 40.7058316f,
        //                Longitude = -74.2581888f,
        //            }
        //        },
        //        new InlineQueryResultLocation
        //        {
        //            Id = "2",
        //            Longitude = 52.507629f, // displayed result
        //            Latitude = 13.1449577f,
        //            Title = "Berlin",
        //            InputMessageContent = new InputLocationMessageContent // message if result is selected
        //            {
        //                Longitude = 52.507629f,
        //                Latitude = 13.1449577f
        //            }
        //        }
        //    };
        //    return results;
        //}

        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;

            if (message == null || message.Type != MessageType.Text) return;

            //if (message.Text.StartsWith("/inline")) // send inline keyboard
            //{
            //    await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            //    var keyboard = new InlineKeyboardMarkup(new[]
            //    {
            //        new[] // first row
            //        {
            //            new InlineKeyboardButton("1.1"),
            //            new InlineKeyboardButton("1.2"),
            //        },
            //        new[] // second row
            //        {
            //            new InlineKeyboardButton("2.1"),
            //            new InlineKeyboardButton("2.2"),
            //        }
            //    });

            //    await Task.Delay(500); // simulate longer running task

            //    await Bot.SendTextMessageAsync(message.Chat.Id, "Choose",
            //        replyMarkup: keyboard);
            //}
            //else 
            if (message.Text.ToLower().StartsWith("/start")) // send custom keyboard
            {
                KeyboardButton[][] arr = new KeyboardButton[1][];

                if(!UserIsRegistered(message.Chat.Username))
                {
                    arr[0] = new KeyboardButton[2];
                    arr[0][0] = new KeyboardButton("ثبت نام");
                    //arr[0][1] = new KeyboardButton("وارد کردن رمز ورود");
                    arr[0][1] = new KeyboardButton("قیمت");
                }
                else
                {
                    arr[0] = new KeyboardButton[2];
                    arr[0][0] = new KeyboardButton("قیمت");
                    arr[0][1] = new KeyboardButton("تغییر نرخ");
                }

                var keyboard = new ReplyKeyboardMarkup(arr);
                keyboard.ResizeKeyboard = true;

                await Bot.SendTextMessageAsync(message.Chat.Id, _introText, ParseMode.Default, false, false, 0, keyboard);

                System.Console.ReadLine();
            }
            else if (message.Text.Equals("ثبت نام"))
            {
                //var msg = "در حال حاضر امکان ثبت نام میسر نیست";
                //await Bot.SendTextMessageAsync(message.Chat.Id, msg, ParseMode.Default, false, false, 0);
                var msg = "رمز عبور را وارد نمایید:";
                await Bot.SendTextMessageAsync(message.Chat.Id, msg, ParseMode.Default, false, false, 0);

                System.Console.ReadLine();
            }
            else if (message.Text.Equals("تغییر نرخ"))
            {
                var msg = "این امکان فعلا وجود ندارد:";
                await Bot.SendTextMessageAsync(message.Chat.Id, msg, ParseMode.Default, false, false, 0);

                System.Console.ReadLine();

                //int n;
                //bool isNumeric = int.TryParse(message.Text, out n);

                //if (isNumeric)
                //    _dollarPrice = Convert.ToInt32(message.Text);
                
            }
            //else if (message.Text.Equals("وارد کردن رمز ورود"))
            //{
            //    var msg = "رمز را وارد نمایید:";
            //    await Bot.SendTextMessageAsync(message.Chat.Id, msg, ParseMode.Default, false, false, 0);

            //    System.Console.ReadLine();

            //}
            //else if (message.Text.StartsWith("/Register")) // send custom keyboard
            //{
            //    var data = @"لطفا شماره پرسنلی خود را وارد نمایید;";

            //    await Bot.SendTextMessageAsync(message.Chat.Id, data, ParseMode.Default, false, false, 0, new ReplyKeyboardMarkup());
            //    //var RequestReplyKeyboard = new ReplyKeyboardMarkup(new[]
            //    //    {
            //    //        KeyboardButton.WithRequestLocation("Location"),
            //    //        KeyboardButton.WithRequestContact("Contact"),
            //    //    });
            //    System.Console.ReadLine();

            //}
            else if (message.Text.StartsWith("pas"))
            {
                var pass = "pas1397";
                if (message.Text == pass)
                {
                    if (!UserIsRegistered(message.Chat.Username))
                        RegisterNewUser(message.Chat.Username, message.Text);
                    await SendPriceResponse(message.Chat.Id);
                }
                else
                {
                    var msg = "رمز عبور صحیح نیست";
                    await Bot.SendTextMessageAsync(message.Chat.Id, msg, ParseMode.Default, false, false, 0, new ReplyKeyboardMarkup());
                }
                //var result = 
                //await Bot.AnswerInlineQueryAsync(message.Chat.Id.ToString(), results, isPersonal: true, cacheTime: 0);
            }
            else if (message.Text == "/Location")
            {
                //var result = 
                //await Bot.AnswerInlineQueryAsync(message.Chat.Id.ToString(), results, isPersonal: true, cacheTime: 0);
            }
            else if (message.Text == "قیمت")
            {
                if (UserIsRegistered(message.Chat.Username))
                    await SendPriceResponse(message.Chat.Id);
                else
                {
                    var msg = "شما مجاز به استفاده از سرویس نیستید";
                    await Bot.SendTextMessageAsync(message.Chat.Id, msg, ParseMode.Default, false, false, 0, new ReplyKeyboardMarkup());

                }
            }
            //else if (message.Text.StartsWith("/Groups")) // send custom keyboard
            //{
            //    var rowsCount = Groups.Length / EachRowItems;
            //    KeyboardButton[][] arr = new KeyboardButton[rowsCount][];

            //    for (int i = 0; i < rowsCount; i++)
            //        arr[i] = new KeyboardButton[EachRowItems];

            //    for (int j = 0; j < rowsCount; j++)
            //        for (int i = 0; i < EachRowItems; i++)
            //            arr[j][i] = Groups[i + j * EachRowItems];

            //    var keyboard = new ReplyKeyboardMarkup(arr);
            //    keyboard.ResizeKeyboard = true;

            //    await Bot.SendTextMessageAsync(message.Chat.Id, "لطفا دسته مورد نظر را انتخاب نمایید:", replyMarkup: keyboard);
            //}
            //else if (message.Text.StartsWith("/keyboard")) // send custom keyboard
            //{
            //    var keyboard = new ReplyKeyboardMarkup(new[]
            //    {
            //        new [] // first row
            //        {
            //            new KeyboardButton("1.1"),
            //            new KeyboardButton("1.2"),
            //        },
            //        new [] // last row
            //        {
            //            new KeyboardButton("2.1"),
            //            new KeyboardButton("2.2"),
            //        }
            //    });

            //    await Bot.SendTextMessageAsync(message.Chat.Id, "Choose",
            //        replyMarkup: keyboard);
            //}
            else if (message.Text == "منوی اصلی" || message.Text == "/start")
            {
                await Bot.SendTextMessageAsync(message.Chat.Id, _introText);
            }
            //else if (Groups.Contains(message.Text)) // send a photo
            //{
            //    await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto);

            //    string dir = Path + "\\" + message.Text;

            //    if (!Directory.Exists(dir))
            //        await Bot.SendTextMessageAsync(message.Chat.Id, "موردی یافت نشد");
            //    else
            //    {
            //        string[] fileEntries = Directory.GetFiles(dir);
            //        if (fileEntries.Length > 0)
            //        {
            //            foreach (string fileName in fileEntries)
            //            {
            //                using (
            //                    var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read)
            //                )
            //                {
            //                    var fts = new FileToSend(fileName, fileStream);

            //                    await Bot.SendPhotoAsync(message.Chat.Id, fts, fileName.Split('\\').Last());
            //                }
            //            }
            //        }
            //    }
            //}
            //else if (message.Text.StartsWith("/request")) // request location or contact
            //{
            //    var keyboard = new ReplyKeyboardMarkup(new[]
            //    {
            //        new KeyboardButton("Location")
            //        {
            //            RequestLocation = true
            //        },
            //        new KeyboardButton("Contact")
            //        {
            //            RequestContact = true
            //        },
            //    });

            //    await Bot.SendTextMessageAsync(message.Chat.Id, "Who or Where are you?", replyMarkup: keyboard);
            //}
            else
            {
                await Bot.SendTextMessageAsync(message.Chat.Id, _introText);
            }
        }

        private static bool UserIsRegistered(string chatUsername)
        {
            var jsonRegedUsers = LoadJsonRegisteredUsers();
            return jsonRegedUsers.Any(a => a.Username.Equals(chatUsername));
        }

        private static async Task SendPriceResponse(long chatId)
        {
            var results = GetApiPrice(null);
            string textResult = "قیمت لحظه ای " + _coinsCount.ToFaString() + " ارز اول با نرخ دلار " + _dollarPrice.ToFaString() + " تومانی در تاریخ " +
                                DateTime.Now.ToFaDateTime().ToFaString() + ":\n";

            int counter = 1;
            foreach (var coin in results)
            {
                var priceToman = (decimal.Parse(coin.price_Usd, CultureInfo.InvariantCulture) * _dollarPrice);
                priceToman = decimal.Round(priceToman);
                
                var priceUsd = decimal.Parse(coin.price_Usd, CultureInfo.InvariantCulture);
                priceUsd  =  priceUsd > 100 ? decimal.Round(priceUsd) : priceUsd;

                var pchange24 = decimal.Parse(coin.percent_change_24h, CultureInfo.InvariantCulture);

                textResult = textResult + (counter++).ToString() + "." + coin.name + "(" + coin.symbol + "):\n" +
                             priceToman.ToFaGString() + " تومان " 
                             + "\n" + "" + priceUsd.ToString().ToFaString().ToFaGString() + " دلار" + "\n" +
                             pchange24.ToString().ToFaString() + " %" + " در " + (24).ToFaString() + " ساعت گذشته" +
                             "\n\n";
            }
            //textResult += "<a href=" + "~/Images/btc.png" + ">btc</a>";
            textResult += "کانال @BTCPost";
            await
                Bot.SendTextMessageAsync(chatId, textResult, ParseMode.Default, false, false, 0);
            System.Console.ReadLine();
        }

        private static async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            await Bot.AnswerCallbackQueryAsync(callbackQueryEventArgs.CallbackQuery.Id,
                $"Received {callbackQueryEventArgs.CallbackQuery.Data}");
        }

        public static List<ApiCoin> GetApiPrice(object state)
        {
            List<ApiCoin> result = new List<ApiCoin>();
            try
            {
                const string uri = @"https://api.coinmarketcap.com/v1/"; //ticker /ethereum/";
                var client = new WebClient();
                var response = client.DownloadString(uri + "/ticker/?limit="+ _coinsCount);
                result = JsonConvert.DeserializeObject<List<ApiCoin>>(response);
            }
            catch (Exception ex)
            {
                //Debuging.Error(ex);
            }
            return result;
        }

        public static string GetPersianSeparatedPrice(string price)
        {
            if (decimal.Parse(price, CultureInfo.InvariantCulture) > 100)
                return price.Substring(0, price.IndexOf('.') > 0 ? price.IndexOf('.') : price.Length).ToFaGString();

            return price.ToFaString();
        }

        public static void RegisterNewUser(string user, string password)
        {
            var jsonRegedUsers = LoadJsonRegisteredUsers();
            if (!jsonRegedUsers.Any(a => a.Username == user))
            {
                var newUser = new User()
                {
                    Username = user,
                    Password = password
                };

                jsonRegedUsers.Add(newUser);
                var output = JsonConvert.SerializeObject(jsonRegedUsers, Formatting.Indented);
                System.IO.File.WriteAllText("RegisteredUsers.json", output);
            }
        }

        public static List<User> LoadJsonRegisteredUsers()
        {
            List<User> users = new List<User>();
            using (StreamReader r = new StreamReader("RegisteredUsers.json"))
            {
                string json = r.ReadToEnd();
                users = JsonConvert.DeserializeObject<List<User>>(json);
            }

            return users;
        }




    }

    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }

    }

    public class ApiCoin
    {
        public string id { get; set; }

        [DisplayName("ارز")]
        public string name { get; set; }

        [DisplayName("نماد")]
        public string symbol { get; set; }

        [DisplayName("قیمت(دلار)")]
        public string price_Usd { get; set; }

        [DisplayName("قیمت(بیت کوین)")]
        public string price_btc { get; set; }

        public string market_cap_usd { get; set; }

        [DisplayName("تغییر 1 ساعت")]
        public string percent_change_1h { get; set; }

        [DisplayName("تغییر 7 روز")]
        public string percent_change_7d { get; set; }

        [DisplayName("تغییر 24 ساعت")]

        public string percent_change_24h { get; set; }

        [DisplayName("رتبه")]
        public string rank { get; set; }
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

}
