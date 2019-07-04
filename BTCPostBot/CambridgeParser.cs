using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using HtmlAgilityPack;

namespace BTCPostBot
{
    public class CambridgeParser
    {
        static void Parse(string url)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                //var url = "https://dictionary.cambridge.org/dictionary/english/order";

                GetCambridgeTranslation(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static string GetCambridgeTranslation(string url)
        {
            string resultText = "";

            try
            {
                string html = GetHtml(url);
                HtmlDocument htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(html);
                var divEntryBody = htmlDoc.DocumentNode.SelectSingleNode(@"//div[@class='entry-body']");

                var header = divEntryBody.SelectSingleNode(@"//div[@class='pos-header']");
                var headerSpan = header.SelectSingleNode(@"//span[@class='hw']");
                var headerText = headerSpan.InnerHtml;

                resultText += "<b>" + headerText + "</b>\n\n";

                var divPosBody = divEntryBody.SelectNodes(@"//div[@class='pos-body']").FirstOrDefault();
                var divseneseBlock = divPosBody.SelectNodes(@".//div[@class='sense-block']");
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
                            resultText += " <i>" + b.InnerText + "</i>";

                        var c = h3.SelectSingleNode(@".//span[@class='guideword']");
                        if (c != null)
                            resultText += c.InnerText;
                        resultText += "</b>\n";
                    }

                    var senseBodies = senseBlock.SelectNodes(@".//div[@class='sense-body']");
                    foreach (HtmlNode senseBody in senseBodies)
                    {
                        var defblocks = senseBody.SelectNodes(@".//div[@class='def-block pad-indent']");
                        foreach (HtmlNode defBlock in defblocks)
                        {
                            var defs = defBlock.SelectNodes(@".//b[@class='def']");
                            foreach (HtmlNode def in defs)
                            {
                                resultText += def.InnerText;
                            }

                            var egs = defBlock.SelectNodes(@".//span[@class='eg']");
                            foreach (HtmlNode eg in egs)
                            {
                                resultText += "<i>" + eg?.InnerText + "</i>\n";
                            }

                        }
                    }

                    resultText += "\n";
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return resultText;
        }

        private static string GetHtml(string url)
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
    }
}
