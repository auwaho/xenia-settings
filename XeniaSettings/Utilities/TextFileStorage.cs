using System;
using System.IO;
using System.Linq;
using System.Text;

namespace XeniaSettings.Utilities
{
    internal sealed class TextFileData
    {
        public byte[] Bytes { get; }
        public string Content { get; }
        public Encoding Encoding { get; }

        public TextFileData(byte[] bytes, string content, Encoding encoding)
        {
            Bytes = bytes;
            Content = content;
            Encoding = encoding;
        }
    }

    internal static class TextFileStorage
    {
        public static TextFileData Read(string path)
        {
            byte[] bytes = File.ReadAllBytes(path);
            using (var reader = new StreamReader(new MemoryStream(bytes), new UTF8Encoding(false, true), true))
            {
                string content = reader.ReadToEnd();
                return new TextFileData(bytes, content, reader.CurrentEncoding);
            }
        }

        public static bool TryWrite(string path, string content, Encoding encoding, byte[] originalBytes,
            out byte[] writtenBytes, out string error)
        {
            writtenBytes = null;
            error = null;
            string temporaryPath = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                // Only replace the version that was loaded or last saved by this application.
                if (!File.ReadAllBytes(path).SequenceEqual(originalBytes))
                    throw new IOException("The file changed on disk. Reload it before saving.");

                byte[] bytes = encoding.GetPreamble().Concat(encoding.GetBytes(content)).ToArray();
                File.WriteAllBytes(temporaryPath, bytes);
                File.Replace(temporaryPath, path, null);
                writtenBytes = bytes;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                error = ex.Message;
            }
            finally
            {
                try
                {
                    if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                {
                    error += "\nCould not remove temporary file: " + ex.Message;
                }
            }
            return writtenBytes != null;
        }
    }
}