using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace har2zip
{
    class Program
    {
        static void Main(string[] args)
        {
            var filename = string.Join(" ", args);
            string content = null;

            try {
                content = File.ReadAllText(filename);
            }
            catch (Exception ex) {
                Console.WriteLine("Unable to open file: " + filename);
                Console.WriteLine(ex.Message);
                Environment.Exit(1);
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            HarFile har = null;            
            try {
                har = JsonSerializer.Deserialize<HarFile>(content, options);
            }
            catch (Exception ex) {
                Console.WriteLine("Unable to deserialize file content: " + filename);
                Console.WriteLine(ex.Message);
                Environment.Exit(2);
            }

            List<InMemoryFile> files = har.Log?.Entries?
                    .Where(x => x?.Response?.BodySize > 0)
                    .Select(x => ToInMemoryFile(x))
                    .GroupBy(x => x.FileName)
                    .Select(x => x.First())
                    .ToList();

            try {
                var zip = GetZipArchive(files);
                File.WriteAllBytes(filename + ".zip", zip);
            }
            catch (Exception ex) {
                Console.WriteLine("Unable to create zip file from files:");
                Console.WriteLine(ex.Message);
                Environment.Exit(4);
            }
        }

        public static InMemoryFile ToInMemoryFile(HarEntry entry) {
            try {
                var fileUri = new Uri(entry.Request.Url);
                var filename = $"{fileUri.Host}{fileUri.LocalPath}";
                if (!string.IsNullOrEmpty(fileUri.Query)) {
                    var path = Path.GetExtension(fileUri.LocalPath);
                    filename += $"{fileUri.Query}{path}";
                }

                var content = Unpack(entry.Response.Content);

                var memfile = new InMemoryFile() {
                    FileName = Friendly(filename),
                    Content = content
                };

                return memfile;
            }
            catch (Exception ex) {
                Console.WriteLine("Unable to create in-memory file from entry: ");
                Console.WriteLine(JsonSerializer.Serialize(entry));
                Console.WriteLine(ex.Message);
                Environment.Exit(3);
            }

            return null;
        }

        private static byte[] Unpack(HarContent content) {
            if (content.Encoding?.ToLowerInvariant() == "base64") {
                return Convert.FromBase64String(content.Text);
            }

            return Encoding.UTF8.GetBytes(content.Text);
        }
        
        private static string Friendly(string text) {
            return new Regex("[^a-zA-Z0-9/._-]").Replace(text, "_");
        }

        public static byte[] GetZipArchive(List<InMemoryFile> files)
        {
            byte[] archiveFile;
            using (var archiveStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create, true))
                {
                    foreach (var file in files)
                    {
                        var zipArchiveEntry = archive.CreateEntry(file.FileName, CompressionLevel.Fastest);
                        using (var zipStream = zipArchiveEntry.Open())
                            zipStream.Write(file.Content, 0, file.Content.Length);
                    }
                }

                archiveFile = archiveStream.ToArray();
            }

            return archiveFile;
        }

        public class InMemoryFile
        {
            public string FileName { get; set; }
            public byte[] Content { get; set; }
        }

        public class HarFile {
            public HarLog Log { get; set; }
        }

        public class HarLog {
            public List<HarEntry> Entries { get; set; }
        }

        public class HarEntry {
            public HarRequest Request { get; set; }

            public HarResponse Response { get; set; }
        }

        public class HarRequest {
            public string Url { get; set; }            
        }
        
        public class HarResponse {
            public HarContent Content { get; set; }   

            public long BodySize { get; set; }         
        }

        public class HarContent {
            public string Text { get; set; }

            public string Encoding { get; set; }

            public string MimeType { get; set; }
        }        
    }
}
