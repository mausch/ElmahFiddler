using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using Elmah;
using Fiddler;

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
            var session = new Session(HttpContext.Current.Request.SerializeRequestToBytes(), null);
            var ok = SAZ.WriteSessionArchive(filename, new[] {session}, config.Password);
            return ok ? filename : null;
        }

        public void Dispose() {}

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