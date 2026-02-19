using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.IO.Compression;

namespace NeathCopyEngine.Helpers
{
    public class MySerializer
    {
        public static object DeserializeCompressed(Type type, string fileName)
        {
            object result = null;
            var normalizedFileName = LongPathHelper.Normalize(fileName);
            var tmpPath = LongPathHelper.Normalize("tmp");

            //Create the decompressed file.
            using (FileStream compressedFile = new FileStream(normalizedFileName, FileMode.Open, FileAccess.Read))
            {
                using (GZipStream Decompress = new GZipStream(compressedFile, CompressionMode.Decompress))
                {
                    using (var xml = new FileStream(tmpPath, FileMode.Create, FileAccess.Write))
                    {
                        // Copy the decompression stream 
                        // into the output file.
                        Decompress.CopyTo(xml);
                    }

                    // Create an instance of the XmlSerializer class;
                    // specify the type of object to serialize.
                    XmlSerializer serializer = new XmlSerializer(type);

                    //Create the stream to write.
                    using (var reader = new FileStream(tmpPath, FileMode.Open, FileAccess.Read))
                    {
                        // Serialize the object, and close the TextWriter.
                        result = serializer.Deserialize(reader);
                        reader.Close();
                    }

                    File.Delete(tmpPath);

                }
            }

            return result;
        }
        public static void SerializeCompressed(object obj,Type type, string fileName)
        {
            // Create an instance of the XmlSerializer class;
            // specify the type of object to serialize.
            XmlSerializer serializer = new XmlSerializer(type);
            var normalizedFileName = LongPathHelper.Normalize(fileName);
            var tmpPath = LongPathHelper.Normalize("tmp");

            //Create the stream to write.
            using (var writer = new FileStream(tmpPath, FileMode.Create, FileAccess.Write))
            {
                try
                {
                    // Serialize the object, and close the TextWriter.
                    serializer.Serialize(writer, obj);
                }
                catch (Exception ex)
                {
                    writer.Dispose();
                    File.Delete(tmpPath);
                    throw new Exception(string.Format("An error has ocurred in SaveList method of FilesList class: {0}", ex.Message));
                }
            }

            // Create the compressed file.
            using (FileStream outFile = File.Create(normalizedFileName))
            {
                using (GZipStream Compress = new GZipStream(outFile,CompressionMode.Compress))
                {
                    using (var reader = new FileStream(tmpPath, FileMode.Open, FileAccess.Read))
                    {
                        // Copy the source file into 
                        // the compression stream.
                        reader.CopyTo(Compress);
                    }
                }
            }

            File.Delete(tmpPath);
        }

        public static object Deserialize(Type type, string fileName)
        {
            object result = null;
            var normalizedFileName = LongPathHelper.Normalize(fileName);

            // Create an instance of the XmlSerializer class;
            // specify the type of object to serialize.
            XmlSerializer serializer = new XmlSerializer(type);

            //Create the stream to write.
            using (var reader = new FileStream(normalizedFileName, FileMode.Open, FileAccess.Read))
            {
                // Serialize the object, and close the TextWriter.
                result = serializer.Deserialize(reader);
                reader.Close();
            }

            return result;
        }
        public static void Serialize(object obj, Type type, string fileName)
        {
            // Create an instance of the XmlSerializer class;
            // specify the type of object to serialize.
            XmlSerializer serializer = new XmlSerializer(type);
            var normalizedFileName = LongPathHelper.Normalize(fileName);

            //Create the stream to write.
            using (var writer = new FileStream(normalizedFileName, FileMode.Create, FileAccess.Write))
            {
                try
                {
                    // Serialize the object, and close the TextWriter.
                    serializer.Serialize(writer, obj);
                }
                catch (Exception ex)
                {
                    writer.Dispose();
                    throw new Exception(string.Format("An error has ocurred in SaveList method of FilesList class: {0}", ex.Message));
                }
            }
        }
    }
}
