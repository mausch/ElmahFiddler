using System;
using System.IO;
using System.Linq;
using System.Web;

namespace ElmahFiddler {
    public static class HttpRequestExtensions {
        public static byte[] SerializeRequestToBytes(this HttpRequest request) {
            return SerializeRequestToBytes(request, r => r.ServerVariables["ALL_RAW"]);
        }

        private static byte[] SerializeRequestToBytes(HttpRequest request, Func<HttpRequest, string> getHeaders) {
            using (var ms = new MemoryStream())
            using (var sw = new StreamWriter(ms)) {
                sw.WriteLine("{0} {1} {2}", request.HttpMethod, request.Url.PathAndQuery, request.ServerVariables["SERVER_PROTOCOL"]);
                sw.WriteLine(getHeaders(request));
                sw.WriteLine();
                sw.Flush();
                if (request.ContentLength > 0) {
                    request.InputStream.CopyTo(ms);
                }
                return ms.ToArray();
            }
        }

        public static byte[] SerializeRequestToBytes(this HttpRequest request, string renameHost) {
            return SerializeRequestToBytes(request, r => {
                r.Headers["Host"] = renameHost;
                var headers = r.Headers.Cast<string>()
                    .Select(h => string.Format("{0}: {1}", h, r.Headers[h]))
                    .ToArray();
                return string.Join(Environment.NewLine, headers);
            });
        }
    }
}