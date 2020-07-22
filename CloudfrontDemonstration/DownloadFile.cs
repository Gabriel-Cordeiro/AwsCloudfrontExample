using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;

namespace CloudfrontDemonstration
{
    public class DownloadFile
    {
        static readonly HttpClient httpClient = new HttpClient();

        public DownloadFile()
        {

        }

        public bool DownloadFileByUrl(string url, long fileSize, string diskPath, string fileEtag)
        {
            int multiPartUploadNumber = GetMultiPartUploadNumber(fileEtag);
            var chunkSize = CalcChunkSizeInBytes(fileSize, multiPartUploadNumber);
            long indexFrom = 0;
            long indexTo = chunkSize - 1;
            List<byte> ChunkHashes = new List<byte>();

            while (indexFrom < fileSize)
            {
                httpClient.DefaultRequestHeaders.Range = new RangeHeaderValue(indexFrom, indexTo);
                var stream = httpClient.GetStreamAsync(url).Result;
                var fileBytes = ConvertStreamToByteArray(stream);

                SaveFile(fileBytes, diskPath);

                indexFrom = indexTo + 1;
                indexTo = indexTo + chunkSize;

                var chunkMd5Hash = GenerateMd5HashForChunk(fileBytes);
                ChunkHashes.AddRange(chunkMd5Hash);
            }

            var etagLocal = GenerateEtag(multiPartUploadNumber, ChunkHashes);
            return etagLocal == fileEtag;
        }

        private  string GenerateEtag(int multiPartUploadNumber, List<byte> md5Hashes)
        {
            var hash = new MD5CryptoServiceProvider();

            if (multiPartUploadNumber == 1)
            {
                return BitConverter.ToString(md5Hashes.ToArray()).Replace("-", string.Empty).ToLower();
            }

            var etagLocal = BitConverter.ToString(hash.ComputeHash(md5Hashes.ToArray())).Replace("-", string.Empty).ToLower() + "-" + multiPartUploadNumber;
            return etagLocal;
        }

        private byte[] GenerateMd5HashForChunk(byte[] bytes)
        {
            using (var md5Hash = new MD5CryptoServiceProvider())
            {
                return md5Hash.ComputeHash(bytes);
            }
        }

        private long CalcChunkSizeInBytes(long fileSize, int multiPartUploadNumber)
        {
            bool chunkNotFound = true;
            long awsMinChunkSizeInbytes = 8 * 1024 * 1024;

            while (chunkNotFound)
            {
                long amount = 0;
                amount = awsMinChunkSizeInbytes * multiPartUploadNumber;

                if (amount >= fileSize)
                    chunkNotFound = false;
                else
                    awsMinChunkSizeInbytes *= 2;
            }

            return awsMinChunkSizeInbytes;
        }
        private int GetMultiPartUploadNumber(string etag)
        {
            return etag.Contains("-") ? Convert.ToInt32(etag.Split('-').Last()) : 1;
        }

        private byte[] ConvertStreamToByteArray(Stream stream)
        {
            byte[] byteArray = new byte[16 * 1024];
            using (MemoryStream mStream = new MemoryStream())
            {
                int bit;
                while ((bit = stream.Read(byteArray, 0, byteArray.Length)) > 0)
                {
                    mStream.Write(byteArray, 0, bit);
                }
                return mStream.ToArray();
            }
        }

        private string RemoveFileNameFromPath(string fullPath)
        {
            var lastIndex = fullPath.LastIndexOf(@"\");
            return fullPath.Substring(0, lastIndex);
        }

        private bool SaveFile(byte[] fileBytes, string diskPath)
        {
            try
            {
                var path = RemoveFileNameFromPath(diskPath);
                FileHelper.CreateDirectory(path);
                return FileHelper.SaveFileIntoDisk(fileBytes, diskPath);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
