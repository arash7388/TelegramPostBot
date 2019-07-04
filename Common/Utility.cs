using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Linq;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;


namespace Common
{
    public static class Utility
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

        public static string ToFaGString(this decimal value)
        {
            string result;
            if (Math.Truncate(value) == value)
                result = string.Format("{0:N0}", value);
            else
                result = string.Format("{0:N}", value);
            return result.ToFaString();
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

        public static string ToFaDate(this DateTime dt)
        {
            string result = "";
            if (!dt.Equals(DateTime.MinValue))
            {
                result += _persianAssistante.GetYear(dt).ToString("000#") + "/";
                result += _persianAssistante.GetMonth(dt).ToString("0#") + "/";
                result += _persianAssistante.GetDayOfMonth(dt).ToString("0#");
            }
            return result;
        }

        public static string ToSafeString(this object obj)
        {
            string converted = "";

            if (obj != null)
                converted = obj.ToString();

            return converted;
        }

        public static bool ToSafeBool(this object obj)
        {
            bool converted = false;
            int r;

            if (obj != null)
                if(int.TryParse(obj.ToString(), out r))
                {
                    if (r == 0)
                        return false;

                    if (r == 1)
                        return true;
                }
            
            converted = Convert.ToBoolean(obj);
            return converted;
        }
       
        public static int ToSafeInt(this object objNumber)
        {
            int converted = 0;

            if (objNumber != null)
                Int32.TryParse(objNumber.ToString(), out converted);

            return converted;
        }

        public static decimal ToSafeDecimal(this object objNumber)
        {
            decimal converted = 0;

            if (objNumber != null)
                Decimal.TryParse(objNumber.ToString(), out converted);

            return converted;
        }

        public static T ToEnum<T>(this string strEnum)
        {
            T t = default(T);
            try
            {
                t = (T)Enum.Parse(typeof(T), strEnum);
            }
            catch (Exception ex)
            {
                Debuging.Warning(ex, "Class: Utility, Method: ToEnum cannot cast the strEnum = " + strEnum + " To Enum ");
            }
            return t;
        }

        public static void DownloadFile(string strURL)
        {
            try
            {
                // string strURL = Server.MapPath(filePath);
                WebClient req = new WebClient();
                HttpResponse response = HttpContext.Current.Response;
                response.Clear();
                response.ClearContent();
                response.ClearHeaders();
                response.Buffer = true;
                response.AddHeader("Content-Disposition", "attachment;filename=\"" + strURL + "\"");
                byte[] data = req.DownloadData(strURL);
                response.BinaryWrite(data);
                response.End();
            }
            catch (Exception ex)
            {
                Debuging.Error(ex, "Utility.DownloadFile");
            }
        }

        public static string GenerateEmailString(Panel resultPanel)
        {
            StringBuilder strBuilder = new StringBuilder();
            StringWriter strWrt = new StringWriter(strBuilder);
            HtmlTextWriter htmTextWriter = new HtmlTextWriter(strWrt);
            resultPanel.RenderControl(htmTextWriter);
            return strBuilder.ToString();
        }

        public static ActionResult SendEmail(string smtpHost, string from, string to, string body, string subject, string fromPassword)
        {
            ActionResult result = new ActionResult();

            try
            {
                if (!IsValidEmail(to) || !IsValidEmail(@from)) throw new LocalException("EmailAddress is wrong", "آدرس ایمیل صحیح نیست", "toEmailAddress", to, "fromEmailAddress", @from);

                MailMessage message = new MailMessage(@from, to, subject, body)
                {
                    BodyEncoding = Encoding.UTF8,
                    IsBodyHtml = true,
                    DeliveryNotificationOptions = DeliveryNotificationOptions.None,
                    Priority = MailPriority.High,
                    SubjectEncoding = Encoding.UTF8
                };

                SmtpClient client = new SmtpClient(smtpHost); //example:"mto-foods.ir"
                client.Credentials = new NetworkCredential(@from, fromPassword);
                client.Send(message);

                result.IsSuccess = true;
                result.ResultMessage = MessageText.SUCCESS;
            }
            catch (LocalException ex)
            {
                Debuging.Warning(ex, "SendEmail Function");
                result.IsSuccess = false;
                result.EnglishMessage = ex.Message;
                result.ResultMessage = ex.ResultMessage;

            }
            catch (Exception ex)
            {
                ex.Data.Add("From", @from);
                ex.Data.Add("To", to);
                ex.Data.Add("Body", body);
                ex.Data.Add("Subject", subject);
                Debuging.Error(ex, "SendEmail Function");
                result.IsSuccess = false;
                result.EnglishMessage = ex.Message;
                result.ResultMessage = MessageText.UNKNOWN_ERROR;
            }
            return result;
        }

        //public static ActionResult SendEmailFromSupportMail(string body, string subject, string receiverEmail)
        //{
        //    ActionResult result = new ActionResult();

        //    try
        //    {
        //        if (!IsValidEmail(receiverEmail)) throw new LocalException("receiverEmail is wrong", "آدرس ایمیل گیرنده صحیح نیست", "receiverEmail", receiverEmail);

        //        MailMessage message = new MailMessage("customer.support.ctr@gmail.com", receiverEmail, subject, body)
        //        {
        //            BodyEncoding = Encoding.UTF8,
        //            IsBodyHtml = true,
        //            DeliveryNotificationOptions = DeliveryNotificationOptions.None,
        //            Priority = MailPriority.High,
        //            SubjectEncoding = Encoding.UTF8
        //        };

        //        var gmailClient = new SmtpClient();
        //        gmailClient.UseDefaultCredentials = false;

        //        gmailClient.Host = "smtp.gmail.com";
        //        gmailClient.Port = 587;
        //        gmailClient.EnableSsl = true;

        //        gmailClient.Credentials = new System.Net.NetworkCredential("customer.support.ctr", "atc@8873");

        //        gmailClient.Send(message);

        //        result.IsSuccess = true;
        //        result.ResultMessage = MessageText.SUCCESS;
        //    }
        //    catch (LocalException ex)
        //    {
        //        //Debuging.Warning(ex, "SendEmail Function");
        //        //Debuging.Info("Transaction Result Email was not sent to " + to);
        //        result.IsSuccess = false;
        //        result.EnglishMessage = ex.Message;
        //        result.ResultMessage = ex.ResultMessage;

        //    }
        //    catch (Exception ex)
        //    {
        //        ex.Data.Add("receiverEmail", receiverEmail);
        //        ex.Data.Add("Body", body);
        //        ex.Data.Add("Subject", subject);
        //        //Debuging.Error(ex, "SendEmail Function");
        //        //Debuging.Info("Transaction Result Email was not sent to " + to);
        //        result.IsSuccess = false;
        //        result.EnglishMessage = ex.Message;
        //        result.ResultMessage = MessageText.UNKNOWN_ERROR;
        //    }
        //    return result;
        //}

        public static bool IsValidEmail(this string email)
        {
            if (String.IsNullOrEmpty(email) && email.Length < 6)
                return false;
            Regex regex = new Regex("^([\\w-\\.]+)@((\\[[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}\\.)|(([\\w-]+\\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\\]?)$");


            return regex.IsMatch(email);
        }

        //private static string AesKey = "uxH//yJ3zKqImVUR//9Vu7uqmZm7/8xm";
        private static string AesKey = "axH//yJ3zKqImVUb//9Vu7uqmZm7/8xn";
        public static string AesEncrypt(string data)
        {
            byte[] toEncryptArry = Encoding.UTF8.GetBytes(data);
            byte[] keyArry = Convert.FromBase64String(AesKey);
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider
            {
                Key = keyArry,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.ISO10126
            };
            ICryptoTransform cTransform = aes.CreateEncryptor();
            byte[] encrypted = cTransform.TransformFinalBlock(toEncryptArry, 0, toEncryptArry.Length);
            string toBase64 = Convert.ToBase64String(encrypted);
            return toBase64;
        }

        public static string AesDecrypt(string data)
        {
            byte[] toDecryptArry = Convert.FromBase64String(data);
            byte[] keyArry = Convert.FromBase64String(AesKey);
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider
            {
                Key = keyArry,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.ISO10126
            };
            ICryptoTransform cTransform = aes.CreateDecryptor();
            byte[] decrypted = cTransform.TransformFinalBlock(toDecryptArry, 0, toDecryptArry.Length);
            string ut8String = Encoding.UTF8.GetString(decrypted);
            return ut8String;
        }

        public static string EncryptTripleDES(string strToEncrypt, string strKey)
        {
            string encryptData = "";
            try
            {
                TripleDESCryptoServiceProvider objDESCrypto = new TripleDESCryptoServiceProvider();
                MD5CryptoServiceProvider objHashMD5 = new MD5CryptoServiceProvider();
                byte[] byteHash, byteBuff;
                string strTempKey = strKey;
                byteHash = objHashMD5.ComputeHash(Encoding.UTF8.GetBytes(strTempKey));
                objDESCrypto.Key = byteHash;
                objDESCrypto.Mode = CipherMode.ECB; //CBC, CFB
                byteBuff = Encoding.UTF8.GetBytes(strToEncrypt);
                encryptData =
                    Convert.ToBase64String(objDESCrypto.CreateEncryptor()
                        .TransformFinalBlock(byteBuff, 0, byteBuff.Length));
            }
            catch (Exception ex)
            {
                Debuging.Error(ex, "EncryptTripleDES");
            }
            return encryptData;
        }

        public static string DecryptTripleDES(string strEncrypted, string strKey)
        {
            string decryptData = "";
            try
            {
                TripleDESCryptoServiceProvider objDESCrypto = new TripleDESCryptoServiceProvider();
                MD5CryptoServiceProvider objHashMD5 = new MD5CryptoServiceProvider();
                byte[] byteHash, byteBuff;
                string strTempKey = strKey;
                byteHash = objHashMD5.ComputeHash(Encoding.UTF8.GetBytes(strTempKey));
                objDESCrypto.Key = byteHash;
                objDESCrypto.Mode = CipherMode.ECB; //CBC, CFB
                byteBuff = Convert.FromBase64String(strEncrypted);
                string strDecrypted =
                    Encoding.UTF8.GetString(objDESCrypto.CreateDecryptor()
                        .TransformFinalBlock(byteBuff, 0, byteBuff.Length));
                decryptData = strDecrypted;
            }
            catch (Exception ex)
            {
                Debuging.Error(ex, "DecryptTripleDES");
            }
            return decryptData;
        }

        public static string GetImage(ImageSource source, int id)
        {
            return "ImageHandler.ashx?source=" + (int)source + "&id=" + id;
        }


        //another way except httpHandler to show image content
        public static string GetImgSource(byte[] imgBytes)
        {
            if (imgBytes == null)
                return "";

            Stream stream = new MemoryStream(imgBytes);
            BinaryReader br = new BinaryReader(stream);
            Byte[] bytes = br.ReadBytes((Int32)stream.Length);
            string base64String = Convert.ToBase64String(bytes, 0, bytes.Length);
            var src = "data:image/png;base64," + base64String;
            return src;
        }

        public static int GetDateDifNumberOfDays(DateTime startDate, DateTime endDate)
        {
            return (int)(startDate - endDate).TotalDays;
        }

        private const int MaxLenghtSlug = 45;

        public static string GenerateSlug(string title)
        {
            var slug = RemoveAccent(title).ToLower();
            slug = Regex.Replace(slug, @"[^a-z0-9-\u0600-\u06FF]", "-");
            slug = Regex.Replace(slug, @"\s+", "-").Trim();
            slug = Regex.Replace(slug, @"-+", "-");
            slug = slug.Substring(0, slug.Length <= MaxLenghtSlug ? slug.Length : MaxLenghtSlug).Trim();

            return slug;
        }

        private static string RemoveAccent(string text)
        {
            var bytes = Encoding.GetEncoding("UTF-8").GetBytes(text);
            return Encoding.UTF8.GetString(bytes);
        }

        public static string GetMonthName(int month)
        {
            switch (month)
            {
                case 1:
                    return "January";
                case 2:
                    return "February";
                case 3:
                    return "March";
                case 4:
                    return "April";
                case 5:
                    return "May";
                case 6:
                    return "June";
                case 7:
                    return "July";
                case 8:
                    return "August";
                case 9:
                    return "September";
                case 10:
                    return "October";
                case 11:
                    return "November";
                case 12:
                    return "December";


                default:
                    return "";
            }
        }

        public static string ShowLTR(this string value)
        {
            return "<span dir=\"ltr\">" + value + "</span>";
        }

        public static string ShowRTL(this string value)
        {
            return "<span dir=\"rtl\">" + value + "</span>";
        }

        public static DateTime ToEnDate(this string value)
        {
            try
            {
                int temp2 = 0;
                int year = int.Parse(value.Substring(0, temp2 = value.IndexOf("/", 0)));
                int tempindex = temp2 + 1;
                int month = int.Parse(value.Substring(tempindex, (temp2 = value.IndexOf("/", tempindex)) - tempindex));
                tempindex = temp2 + 1;
                int day = int.Parse(value.Substring(tempindex, value.Length - tempindex));

                return _persianAssistante.ToDateTime(year, month, day, 0, 0, 0, 0);
            }
            catch (Exception ex)
            {
                Debuging.Error(ex, "To En Date Function");
                return DateTime.MinValue;
            }
        }

        public static DateTime ToEnDateTime(string date, string time)
        {
            try
            {
                int temp2 = 0;
                int year = int.Parse(date.Substring(0, temp2 = date.IndexOf("/", 0)));
                int tempindex = temp2 + 1;
                int month =
                    int.Parse(date.Substring(tempindex, (temp2 = date.IndexOf("/", tempindex)) - tempindex));
                tempindex = temp2 + 1;
                int day = int.Parse(date.Substring(tempindex, date.Length - tempindex));
                temp2 = 0;
                int hour = int.Parse(time.Substring(0, temp2 = time.IndexOf(":", 0)));
                tempindex = temp2 + 1;
                int minute =
                    int.Parse(time.Substring(tempindex, (temp2 = time.IndexOf(":", tempindex)) - tempindex));
                tempindex = temp2 + 1;
                int second = int.Parse(time.Substring(tempindex, time.Length - tempindex));
                return _persianAssistante.ToDateTime(year, month, day, hour, minute, second, 0);
            }
            catch (Exception ex)
            {
                Debuging.Error(ex, "To En DateTime Function");
                return DateTime.MinValue;
            }
        }

        public static string ToPersianChars(decimal number)
        {
            var intNum = Convert.ToInt32(number);
            return intNum.ToSafeString().ToFarsiChars();
        }

        public static Bitmap AddWatermark(string sourcePhotoPath, string watermarkPath)
        {
            Bitmap outputImage = null;
            Graphics g = null;

            try
            {
                var sourcePhoto = new Bitmap(sourcePhotoPath);
                outputImage = new Bitmap(sourcePhoto.Width, sourcePhoto.Height, PixelFormat.Format24bppRgb);

                g = Graphics.FromImage(outputImage);
                g.CompositingMode = CompositingMode.SourceCopy;
                Rectangle destRect = new Rectangle(0, 0, sourcePhoto.Width, sourcePhoto.Height);

                g.DrawImage(sourcePhoto, destRect, 0, 0, sourcePhoto.Width, sourcePhoto.Height, GraphicsUnit.Pixel);
                g.CompositingMode = CompositingMode.SourceOver;

                var waterMark = new Bitmap(watermarkPath);

                Rectangle destWatermarkRect;

                // adding watermark
                if (sourcePhoto.Width > waterMark.Width * 2)
                    destWatermarkRect = new Rectangle(sourcePhoto.Width - waterMark.Width, sourcePhoto.Height - waterMark.Height, waterMark.Width, waterMark.Height);
                else
                {
                    waterMark = new Bitmap(waterMark, new Size(sourcePhoto.Width / 2, Convert.ToInt32(Math.Truncate(((decimal)sourcePhoto.Width / 2) / waterMark.Width * waterMark.Height))));
                    destWatermarkRect = new Rectangle(sourcePhoto.Width - waterMark.Width, sourcePhoto.Height - waterMark.Height, waterMark.Width, waterMark.Height);
                }

                g.DrawImage(waterMark, destWatermarkRect);

                //for testing result
                //outputImage.Save("e:\\result.jpg", ImageFormat.Jpeg);

            }
            catch (Exception x)
            {
                Console.WriteLine("error..." + x.Message);
            }

            return outputImage;
        }
    }

    public static class MessageText
    {
        public static string SUCCESS = "عملیات با موفقیت انجام شد";
        public static string UNKNOWN_ERROR = "به علت بروز مشکل در سیستم عملیات انجام نشد";
    }

    
    public static class DigitToString
    {
        private static readonly string[] yakan = new string[10] { "صفر", "یک", "دو", "سه", "چهار", "پنج", "شش", "هفت", "هشت", "نه" };
        private static readonly string[] dahgan = new string[10] { "", "", "بیست", "سی", "چهل", "پنجاه", "شصت", "هفتاد", "هشتاد", "نود" };
        private static readonly string[] dahyek = new string[10] { "ده", "یازده", "دوازده", "سیزده", "چهارده", "پانزده", "شانزده", "هفده", "هجده", "نوزده" };
        private static readonly string[] sadgan = new string[10] { "", "یکصد", "دویست", "سیصد", "چهارصد", "پانصد", "ششصد", "هفتصد", "هشتصد", "نهصد" };
        private static readonly string[] basex = new string[5] { "", "هزار", "میلیون", "میلیارد", "تریلیون" };
        private static string Getnum3(long num3)
        {
            string s = "";
            long d3, d12;
            d12 = num3 % 100;
            d3 = num3 / 100;
            if (d3 != 0)
                s = sadgan[d3] + " و ";
            if ((d12 >= 10) && (d12 <= 19))
                s = s + dahyek[d12 - 10];
            else
            {
                long d2 = d12 / 10;
                if (d2 != 0)
                    s = s + dahgan[d2] + " و ";
                long d1 = d12 % 10;
                if (d1 != 0)
                    s = s + yakan[d1] + " و ";
                s = s.Substring(0, s.Length - 3);
            }
            ;
            return s;
        }
        public static string ToFarsiChars(this string snum)
        {
            string stotal = "";
            try
            {

                if (snum == "") return "صفر";
                if (snum == "0")
                {
                    return yakan[0];
                }
                else
                {
                    snum = snum.PadLeft(((snum.Length - 1) / 3 + 1) * 3, '0');
                    long L = snum.Length / 3 - 1;
                    for (int i = 0; i <= L; i++)
                    {
                        long b = long.Parse(snum.Substring(i * 3, 3));
                        if (b != 0)
                            stotal = stotal + Getnum3(b) + " " + basex[L - i] + " و ";
                    }
                    stotal = stotal.Substring(0, stotal.Length - 3);
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
            return stotal;
        }
    }
}