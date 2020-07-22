using System;
using System.IO;

namespace CloudfrontDemonstration
{
    public class FileHelper
    {
        public static bool CreateDirectory(string path)
        {
            try
            {
                var directory = new DirectoryInfo(path);
                if (!directory.Exists)
                {
                    directory.Create();
                }
                return true;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static bool SaveFileIntoDisk(byte[] fileBytes, string diskPath)
        {
            try
            {
                var result = true;

                using (var output = new FileStream(diskPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                {
                    output.Write(fileBytes, 0, fileBytes.Length);
                }
                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
