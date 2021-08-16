using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace auto_update
{
    class IoUtil
    {
        private const int BufferSize = 4 * 1024;

        public static void WriteToFile(string filePath, Stream stream)
        {
            using (var requestStream = stream)
            {
                using (var fs = File.Open(filePath, FileMode.OpenOrCreate))
                {
                    WriteTo(stream, fs);
                }
            }
        }

        public static void WriteTo(Stream src, Stream dest)
        {
            var buffer = new byte[BufferSize];
            int bytesRead;
            while ((bytesRead = src.Read(buffer, 0, buffer.Length)) > 0)
            {
                dest.Write(buffer, 0, bytesRead);
            }
            dest.Flush();
        }

        public static long WriteTo(Stream orignStream, Stream destStream, long totalSize)
        {
            var buffer = new byte[BufferSize];

            long alreadyRead = 0;
            while (alreadyRead < totalSize)
            {
                var readSize = orignStream.Read(buffer, 0, BufferSize);
                if (readSize <= 0)
                    break;

                if (alreadyRead + readSize > totalSize)
                    readSize = (int)(totalSize - alreadyRead);
                alreadyRead += readSize;
                destStream.Write(buffer, 0, readSize);
            }
            destStream.Flush();
            return alreadyRead;
        }

        public static bool ReadFrom(Stream src, out string content)
        {
            content = "";
            if (null == src) return false;

            using (var sr = new StreamReader(src))
            {
                content = sr.ReadToEnd();
            }

            return true;
        }
    }
    
}
