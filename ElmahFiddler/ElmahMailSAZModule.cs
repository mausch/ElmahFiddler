using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using Elmah;
using Fiddler;
using Ionic.Zip;

namespace ElmahFiddler {
    public class ElmahMailSAZModule : IHttpModule, IConfigurationSectionHandler {
        private static readonly object sazFilenameKey = new object();
        private static readonly object attachmentKey = new object();
        private ElmahMailSAZConfig config;

        public void Init(HttpApplication context) {
            var modules = from string n in context.Modules select context.Modules[n];
            var mailModule = modules.FirstOrDefault(m => m is ErrorMailModule) as ErrorMailModule;
            if (mailModule == null)
                return;
            config = (ElmahMailSAZConfig) ConfigurationManager.GetSection("elmah/errorMailSAZ") ?? new ElmahMailSAZConfig();
            mailModule.Mailing += MailModuleMailing;
            mailModule.Mailed += MailModuleMailed;
        }

        public void MailModuleMailed(object sender, ErrorMailEventArgs args) {
            var saz = HttpContext.Current.Items[sazFilenameKey] as string;
            if (saz == null)
                return;
            var attachment = HttpContext.Current.Items[attachmentKey] as Attachment;
            attachment.Dispose();
            File.Delete(saz);
        }

        public void MailModuleMailing(object sender, ErrorMailEventArgs args) {
            if (config.ExcludedUrls.Any(rx => rx.IsMatch(HttpContext.Current.Request.RawUrl)))
                return;
            var saz = SerializeRequestToSAZ();
            if (saz == null)
                return;
            var saz2 = saz + ".saz";
            File.Move(saz, saz2);
            var attachment = new Attachment(saz2);
            HttpContext.Current.Items[sazFilenameKey] = saz2;
            HttpContext.Current.Items[attachmentKey] = attachment;
            args.Mail.Attachments.Add(attachment);
        }

        public string SerializeRequestToSAZ() {
            var filename = Path.GetTempFileName();
            var session = new Session(SerializeRequestToBytes(), null);
            var ok = WriteSessionArchive(filename, new[] {session}, config.Password);
            return ok ? filename : null;
        }

        public static byte[] SerializeRequestToBytes() {
            var request = HttpContext.Current.Request;
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

        /// <summary>
        /// Writes a Fiddler session to SAZ.
        /// Written by Eric Lawrence (http://www.ericlawrence.com)
        /// THIS CODE SAMPLE & SOFTWARE IS LICENSED "AS-IS." YOU BEAR THE RISK OF USING IT. MICROSOFT GIVES NO EXPRESS WARRANTIES, GUARANTEES OR CONDITIONS. 
        /// </summary>
        /// <param name="sFilename"></param>
        /// <param name="arrSessions"></param>
        /// <param name="sPassword"></param>
        /// <returns></returns>
        public static bool WriteSessionArchive(string sFilename, Session[] arrSessions, string sPassword) {
            const string S_VER_STRING = "2.8.9";
            if ((null == arrSessions || (arrSessions.Length < 1))) {
                return false;
            }

            try {
                if (File.Exists(sFilename)) {
                    File.Delete(sFilename);
                }

                using (var oZip = new ZipFile()) {
                    oZip.AddDirectoryByName("raw");

                    if (!String.IsNullOrEmpty(sPassword)) {
                        if (CONFIG.bUseAESForSAZ) {
                            oZip.Encryption = EncryptionAlgorithm.WinZipAes256;
                        }
                        oZip.Password = sPassword;
                    }

                    oZip.Comment = "FiddlerCore SAZSupport (v" + S_VER_STRING + ") Session Archive. See http://www.fiddler2.com";

                    int iFileNumber = 1;
                    foreach (var oSession in arrSessions) {
                        var delegatesCopyOfSession = oSession;

                        string sBaseFilename = @"raw\" + iFileNumber.ToString("0000");
                        string sRequestFilename = sBaseFilename + "_c.txt";
                        string sResponseFilename = sBaseFilename + "_s.txt";
                        string sMetadataFilename = sBaseFilename + "_m.xml";

                        oZip.AddEntry(sRequestFilename, (sn, strmToWrite) => delegatesCopyOfSession.WriteRequestToStream(false, true, strmToWrite));
                        oZip.AddEntry(sResponseFilename, (sn, strmToWrite) => delegatesCopyOfSession.WriteResponseToStream(strmToWrite, false));
                        oZip.AddEntry(sMetadataFilename, (sn, strmToWrite) => delegatesCopyOfSession.WriteMetadataToStream(strmToWrite));

                        iFileNumber++;
                    }

                    oZip.Save(sFilename);
                }

                return true;
            } catch (Exception) {
                return false;
            }
        }

        public void Dispose() {}

        private class ElmahMailSAZConfig {
            public string Password { get; set; }
            public IList<Regex> ExcludedUrls { get; private set; }
            public ElmahMailSAZConfig() {
                ExcludedUrls = new List<Regex>();
            }
        }

        public object Create(object parent, object configContext, XmlNode section) {
            var moduleConfig = new ElmahMailSAZConfig();
            var excludes = section.ChildNodes.Cast<XmlNode>().FirstOrDefault(n => n.Name == "exclude");
            if (excludes != null) {
                foreach (XmlNode n in excludes.ChildNodes)
                    moduleConfig.ExcludedUrls.Add(new Regex(n.InnerText, RegexOptions.Compiled | RegexOptions.IgnoreCase));
            }
            var password = section.ChildNodes.Cast<XmlNode>().FirstOrDefault(n => n.Name == "password");
            if (password != null)
                moduleConfig.Password = password.InnerText;
            return moduleConfig;
        }
    }
}