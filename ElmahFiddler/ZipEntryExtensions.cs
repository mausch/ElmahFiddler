using System.IO;
using Ionic.Zip;

namespace ElmahFiddler {
    public static class ZipEntryExtensions {
        public static Stream ExtractWithPassword2(this ZipEntry zip, string password) {
            var ms = new MemoryStream();
            if (string.IsNullOrEmpty(password)) {
                zip.Extract(ms);
            } else {
                zip.ExtractWithPassword(ms, password);
            }
            ms.Position = 0;
            return ms;
        }

        public static byte[] ExtractWithPasswordToBytes(this ZipEntry zip, string password) {
            using (var ms = zip.ExtractWithPassword2(password))
                return ms.ReadAll();
        }
    }
}