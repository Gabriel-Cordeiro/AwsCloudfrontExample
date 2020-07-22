using System;

namespace CloudfrontDemonstration
{
    public class Program
    {
        static readonly string savePath = @"C:\projects\file.extension";

        public static void Main(string[] args)
        {
            var url = GetUrl();
            var downloadFile = new DownloadFile();
            var fileSize = 215891968;
            var fileEtag = "e682ae754e6f614451ee6f51fa35742d-13";

            var result = downloadFile.DownloadFileByUrl(url, fileSize, savePath, fileEtag);
        }

        public static string GetUrl()
        {
            var baseUrl = "https://yourcloudfronturl.com";
            var fileName = "fileName";
            var privateKey = "APKAINDWAAAAE7HXWJA";
            var privatekeyPath = AppDomain.CurrentDomain.BaseDirectory + "/Files/CloudfrontRsa.xml";
            var expirationHours = 2;

            var cloudfront = new CloudFrontProvider(privateKey, privatekeyPath, expirationHours);
            var url = cloudfront.GetDownloadURl(baseUrl, fileName, string.Empty);
            return url;
        }
    }
}
