using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CloudfrontDemonstration
{
    public class CloudFrontProvider : CdnProvider
    {
        private readonly SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider();

        #region Constructor

        public CloudFrontProvider(string privateKeyId, string privateKey, int expirationHours) : base(privateKeyId, privateKey, expirationHours)
        {
        }

        #endregion

        public override string GeneratePolicies(string defaultValue, string url, string expiration, string ip)
        {
            return defaultValue.Replace("RESOURCE", url)
                        .Replace("IPADDRESS", ip)
                        .Replace("EXPIRES", expiration);
        }

        public override string GetDownloadURl(string url, string fileName, string ip)
        {
            try
            {
                var cloudfrontUrl = CreateUrl(url, fileName);
                var expiration = GetExpirationTime();
                var policyDefault = GetPolicyStatement(ip);

                var bufferPolicy = Encoding.ASCII.GetBytes(GeneratePolicies(policyDefault, cloudfrontUrl, expiration, ip));
                var urlSafePolicy = GetUrlSafeString(bufferPolicy);
                var bufferPolicyHash = sha1.ComputeHash(bufferPolicy);
                var signedPolicy = GetUrlSafeString(CreateSignedHash(bufferPolicyHash));

                return cloudfrontUrl + "?Policy=" + urlSafePolicy + "&Signature=" + signedPolicy + "&Key-Pair-Id=" + PrivateKeyId;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public override byte[] Sign(string data)
        {
            byte[] plainbytes = Encoding.UTF8.GetBytes(data);
            byte[] hash = sha1.ComputeHash(plainbytes);
            return hash;
        }


        private string CreateUrl(string url, string fileName)
        {
            return ReplaceWhiteSpace($"{url}/{fileName}");
        }

        private string ReplaceWhiteSpace(string text)
        {
            return text.Replace(" ", "+");
        }

        private byte[] CreateSignedHash(byte[] bufferPolicyHash)
        {
            RSAPKCS1SignatureFormatter rsaFormatter = new RSAPKCS1SignatureFormatter(PrivateKey);
            rsaFormatter.SetHashAlgorithm("SHA1");
            byte[] signedHash = rsaFormatter.CreateSignature(bufferPolicyHash);
            return signedHash;
        }

        private static string GetPolicyStatement(string ip)
        {
            string policyString;
            try
            {
                var policyPath = string.IsNullOrEmpty(ip) ? "/Files/PolicyStatement.json" : "/Files/PolicyStatementForIpLimitation.json";
                using (StreamReader reader = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + policyPath))
                {
                    policyString = reader.ReadToEnd();
                }

                return policyString;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private string GetExpirationTime()
        {
            TimeSpan endTimeSpanFromNow = (DateTime.Now.AddHours(ExpirationHours) - DateTime.Now);
            TimeSpan intervalEnd = (DateTime.UtcNow.Add(endTimeSpanFromNow)) - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            int endTimestamp = (int)intervalEnd.TotalSeconds;
            return endTimestamp.ToString();
        }

        private string GetUrlSafeString(byte[] data)
        {
            return Convert.ToBase64String(data).Replace('+', '-').Replace('=', '_').Replace('/', '~');
        }

    }
}
