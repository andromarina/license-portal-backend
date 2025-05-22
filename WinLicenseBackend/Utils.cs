using System.IO.Compression;

namespace WinLicenseBackend
{
    public class Utils
    {
        public static MemoryStream CreateZipFromStreams(Dictionary<string, MemoryStream> files)
        {
            var zipStream = new MemoryStream();

            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach (var entry in files)
                {
                    entry.Value.Position = 0; // rewind
                    var zipEntry = archive.CreateEntry(entry.Key);

                    using var entryStream = zipEntry.Open();
                    entry.Value.CopyTo(entryStream);
                }
            }

            zipStream.Position = 0; // rewind for HTTP response
            return zipStream;
        }
    }
}
