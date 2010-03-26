using System.IO;
using System.Web;

namespace ElmahFiddler {
    public static class HttpRequestExtensions {
        public static byte[] SerializeRequestToBytes(this HttpRequest request) {
            using (var ms = new MemoryStream())
            using (var sw = new StreamWriter(ms)) {
                sw.WriteLine("{0} {1} {2}", request.HttpMethod, request.Url.PathAndQuery, request.ServerVariables["SERVER_PROTOCOL"]);
                sw.WriteLine(request.ServerVariables["ALL_RAW"]);
                sw.WriteLine();
                sw.Flush();
                if (request.ContentLength > 0) {
                    request.InputStream.CopyTo(ms);
                }
                return ms.ToArray();
            }
        }
    }
}