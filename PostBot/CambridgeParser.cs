using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using HtmlAgilityPack;

namespace PostBot
{
    public class CambridgeParser
    {
        public static string Parse(string url)
        {
            var result = "";
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                result = GetCambridgeTranslation(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return result;
        }

        public static string GetCambridgeTranslation(string url)
        {
            string resultText = "";

            try
            {
                string html = Utility.GetHtml(url);
                HtmlDocument htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(html);
                var divEntryBody = htmlDoc.DocumentNode.SelectSingleNode(@"//div[@class='entry-body']");

                var header = divEntryBody.SelectSingleNode(@"//div[@class='pos-header']");
                var headerSpan = header.SelectSingleNode(@"//span[@class='hw']");
                var headerText = headerSpan.InnerHtml;

                resultText += "\U0001F4A1" + " <b>Vocab of the day:</b>";
                resultText += "<b>" + headerText + "</b>\n\n";
                resultText += "\U00003030\U00003030\U00003030\U00003030\U00003030\U00003030\U00003030\U00003030\n\n";

                var divPosBody = divEntryBody.SelectNodes(@"//div[@class='pos-body']").FirstOrDefault();
                var divseneseBlock = divPosBody.SelectNodes(@".//div[@class='sense-block']");

                int senseBlockCounter = 0;
                foreach (HtmlNode senseBlock in divseneseBlock)
                {
                    var h3 = senseBlock.SelectSingleNode(@".//h3[@class='txt-block txt-block--alt2']");

                    if (h3 != null)
                    {
                        resultText += "<b>";
                        var a = h3.SelectSingleNode(@".//span[@class='hw']");
                        resultText += a.InnerText;

                        var b = h3.SelectSingleNode(@".//span[@class='pos']");
                        if (b != null)
                            resultText += " </b><i>" + b.InnerText + "</i><b>";

                        var c = h3.SelectSingleNode(@".//span[@class='guideword']");
                        if (c != null)
                            resultText += " " + c.InnerText.Replace("\n", "").TrimStart(' ').TrimEnd(' ');
                        resultText += "</b>\n";
                    }

                    var senseBodies = senseBlock.SelectNodes(@".//div[@class='sense-body']");
                    foreach (HtmlNode senseBody in senseBodies)
                    {
                        var defblocks = senseBody.SelectNodes(@".//div[@class='def-block pad-indent']");
                        int defBlockCounter = 0;

                        foreach (HtmlNode defBlock in defblocks)
                        {
                            var defs = defBlock.SelectNodes(@".//b[@class='def']");
                            foreach (HtmlNode def in defs)
                            {
                                resultText += "\U0001F6A9 ";
                                resultText += def.InnerText + "\n";
                            }

                            var egs = defBlock.SelectNodes(@".//span[@class='eg']");
                            if(egs!= null)
                                foreach (HtmlNode eg in egs)
                                {
                                    resultText += "\U00002714" + " <i>" + eg?.InnerText + "</i>\n";

                                    if (divseneseBlock.Count > 1)
                                        break;
                                }

                            defBlockCounter++;
                        }
                    }

                    resultText += "\n";
                    senseBlockCounter++;

                    if (senseBlockCounter >= 2)
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            resultText += "#Vocabulary #EnglishWithSamet";

            return resultText;
        }

        
    }
}
