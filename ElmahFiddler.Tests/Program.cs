using System;
using System.Linq;
using Fuchu;

namespace ElmahFiddler.Tests {
    internal class Program {
        private static int Main(string[] args) {
            var test = Test.Case("read SAZ", () => {
                var sessions = SAZ.ReadSessionArchive(@"..\..\test.saz", null).ToList();
                Assert.Equal("session count", 1, sessions.Count);
                var session = sessions[0];
                Assert.Equal("host name", "www.w3schools.com", session.hostname);
                Assert.Equal("protocol", Uri.UriSchemeHttp, session.Protocol());
            });

            return test.Run();
        }
    }
}