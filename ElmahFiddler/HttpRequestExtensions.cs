using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Collections.Specialized;

namespace ElmahFiddler {
    public static class HttpRequestExtensions {
        public static byte[] SerializeRequestToBytes(this HttpRequestBase request) {
            return SerializeRequestToBytes(request, r => r.ServerVariables["ALL_RAW"]);
        }

        private static byte[] SerializeRequestToBytes(HttpRequestBase request, Func<HttpRequestBase, string> getHeaders) {
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

        public static byte[] SerializeRequestToBytes(this HttpRequestBase request, string renameHost) {
            if (string.IsNullOrEmpty(renameHost))
                return SerializeRequestToBytes(request);
            return SerializeRequestToBytes(request, r => {
                var requestHeaders = new NameValueCollection(r.Headers);
                requestHeaders["Host"] = renameHost;
                var headers = requestHeaders.Cast<string>()
                    .Select(h => string.Format("{0}: {1}", h, r.Headers[h]))
                    .ToArray();
                return string.Join(Environment.NewLine, headers);
            });
        }
    }
}