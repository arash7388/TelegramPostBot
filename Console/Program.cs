using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineKeyboardButtons;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputMessageContents;
using Telegram.Bot.Types.ReplyMarkups;

namespace Console
{
    class Program
    {
        private static readonly TelegramBotClient Bot = new TelegramBotClient("453840120:AAFgmrAVXhpGkJQQUUIU_eGb4RP2M-ni1bE");
        private static string[] Groups = ConfigurationManager.AppSettings["Groups"].Split(',');
        private static int EachRowItems = Convert.ToInt32(ConfigurationManager.AppSettings["RowItems"]);
        private static string Path = ConfigurationManager.AppSettings["Path"];

        private static string  Instructions = @"به شالی هوم خوش آمدید
/Contact - دریافت اطلاعات تماس
/Groups - دریافت عکس ها به صورت طبقه بندی شده
/Location -  دریافت موقعیت مکانی فروشگاه";

        static void Main(string[] args)
        {
            Bot.OnCallbackQuery += BotOnCallbackQueryReceived;
            Bot.OnMessage += BotOnMessageReceived;
            Bot.OnMessageEdited += BotOnMessageReceived;
            Bot.OnInlineQuery += BotOnInlineQueryReceived;
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

        private static async void BotOnInlineQueryReceived(object sender, InlineQueryEventArgs inlineQueryEventArgs)
        {
            var results = GetLocation();

            await Bot.AnswerInlineQueryAsync(inlineQueryEventArgs.InlineQuery.Id, results, isPersonal: true, cacheTime: 0);
        }

        private static InlineQueryResult[] GetLocation()
        {
            InlineQueryResult[] results =
            {
                new InlineQueryResultLocation
                {
                    Id = "1",
                    Latitude = 40.7058316f, // displayed result
                    Longitude = -74.2581888f,
                    Title = "New York",
                    InputMessageContent = new InputLocationMessageContent // message if result is selected
                    {
                        Latitude = 40.7058316f,
                        Longitude = -74.2581888f,
                    }
                },
                new InlineQueryResultLocation
                {
                    Id = "2",
                    Longitude = 52.507629f, // displayed result
                    Latitude = 13.1449577f,
                    Title = "Berlin",
                    InputMessageContent = new InputLocationMessageContent // message if result is selected
                    {
                        Longitude = 52.507629f,
                        Latitude = 13.1449577f
                    }
                }
            };
            return results;
        }

        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;

            if (message == null || message.Type != MessageType.TextMessage) return;

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
            if (message.Text.StartsWith("/Contact")) // send custom keyboard
            {
                var contactTxt = @"فروشگاه شالی هوم
واردات و پخش اجناس دکوری و تزیینی منزل
آدرس: بازار شوش ، پاساژ الغدیر، ط همکف پلاک 21
تلفن تماس:02155183208
همراه:09212836980
@Shalihome";

                await Bot.SendTextMessageAsync(message.Chat.Id, contactTxt);
            }
            else if (message.Text=="/Location")
            {
                var results = GetLocation();
                await Bot.AnswerInlineQueryAsync(message.Chat.Id.ToString(), results, isPersonal: true, cacheTime: 0);
            }
            else if (message.Text.StartsWith("/Groups")) // send custom keyboard
            {
                var rowsCount = Groups.Length / EachRowItems;
                KeyboardButton[][] arr = new KeyboardButton[rowsCount][];

                for (int i = 0; i < rowsCount; i++)
                    arr[i] = new KeyboardButton[EachRowItems];

                for (int j = 0; j < rowsCount; j++)
                    for (int i = 0; i < EachRowItems; i++)
                        arr[j][i] = Groups[i + j*EachRowItems];

                var keyboard = new ReplyKeyboardMarkup(arr);
                keyboard.ResizeKeyboard = true;

                await Bot.SendTextMessageAsync(message.Chat.Id, "لطفا دسته مورد نظر را انتخاب نمایید:",replyMarkup: keyboard);
            }
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
            else if (message.Text== "منوی اصلی" || message.Text =="/start")
            {
                await Bot.SendTextMessageAsync(message.Chat.Id, Instructions);
            }
            else if (Groups.Contains(message.Text)) // send a photo
            {
                await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto);

                string dir = Path + "\\" + message.Text;

                if (!Directory.Exists(dir))
                    await Bot.SendTextMessageAsync(message.Chat.Id, "موردی یافت نشد");
                else
                {
                    string[] fileEntries = Directory.GetFiles(dir);
                    if (fileEntries.Length > 0)
                    {
                        foreach (string fileName in fileEntries)
                        {
                            using (
                                var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read)
                            )
                            {
                                var fts = new FileToSend(fileName, fileStream);

                                await Bot.SendPhotoAsync(message.Chat.Id, fts, fileName.Split('\\').Last());
                            }
                        }
                    }
                }
            }
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
                await Bot.SendTextMessageAsync(message.Chat.Id, Instructions);
            }
        }

        private static async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            await Bot.AnswerCallbackQueryAsync(callbackQueryEventArgs.CallbackQuery.Id,
                $"Received {callbackQueryEventArgs.CallbackQuery.Data}");
        }
    }
}
