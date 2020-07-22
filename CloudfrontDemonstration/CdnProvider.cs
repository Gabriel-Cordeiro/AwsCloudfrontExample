using System;
using System.IO;
using System.Security.Cryptography;

namespace CloudfrontDemonstration
{
    public abstract class CdnProvider
    {
        protected CdnProvider()
        {

        }
        protected CdnProvider(string privateKeyId, string privateKey, int expirationHours)
        {
            ExpirationHours = expirationHours;
            PrivateKeyId = privateKeyId;
            CreatePrivateKey(privateKey);
        }


        protected string PrivateKeyId { get; set; }

        protected int ExpirationHours { get; set; }

        protected RSACryptoServiceProvider PrivateKey { get; set; }

        public abstract string GetDownloadURl(string url, string fileName, string ip);

        public abstract string GeneratePolicies(string defaultValue, string url, string expiration, string ip);

        public abstract byte[] Sign(string data);

        private void CreatePrivateKey(string privateKey)
        {
            try
            {
                string xmlString;
                using (StreamReader reader = new StreamReader(privateKey))
                {
                    xmlString = reader.ReadToEnd();
                }

                PrivateKey = new RSACryptoServiceProvider();
                PrivateKey.FromXmlString(xmlString);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
