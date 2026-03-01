using System;
using System.IO;
using System.IO.Compression;
using System.Xml.Serialization;

namespace NeathCopyEngine.Helpers
{
    public static class AtomicSerializer
    {
        public static void SerializeCompressedAtomic(object obj, Type type, string targetPath)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (string.IsNullOrWhiteSpace(targetPath))
                throw new ArgumentNullException(nameof(targetPath));

            var normalizedTarget = LongPathHelper.Normalize(targetPath);
            var targetDirectory = Path.GetDirectoryName(normalizedTarget);
            if (!string.IsNullOrWhiteSpace(targetDirectory))
                Directory.CreateDirectory(targetDirectory);

            var tmpPath = normalizedTarget + "." + System.Diagnostics.Process.GetCurrentProcess().Id + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                var serializer = new XmlSerializer(type);

                using (var fileStream = new FileStream(tmpPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    using (var gzipStream = new GZipStream(fileStream, CompressionMode.Compress, true))
                    {
                        serializer.Serialize(gzipStream, obj);
                    }
                }

                if (File.Exists(normalizedTarget))
                {
                    try
                    {
                        File.Replace(tmpPath, normalizedTarget, null);
                    }
                    catch
                    {
                        File.Delete(normalizedTarget);
                        File.Move(tmpPath, normalizedTarget);
                    }
                }
                else
                {
                    File.Move(tmpPath, normalizedTarget);
                }
            }
            finally
            {
                try
                {
                    if (File.Exists(tmpPath))
                        File.Delete(tmpPath);
                }
                catch
                {
                }
            }
        }
    }
}
