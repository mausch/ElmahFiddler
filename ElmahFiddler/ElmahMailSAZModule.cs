using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using Elmah;
using Fiddler;

namespace ElmahFiddler {
    public class ElmahMailSAZModule : IHttpModule {
        private static readonly object sazFilenameKey = new object();
        private static readonly object attachmentKey = new object();
        private ElmahMailSAZConfig config;

        public void Init(HttpApplication context) {
            var modules = from string n in context.Modules select context.Modules[n];
            var mailModule = modules.FirstOrDefault(m => m is ErrorMailModule) as ErrorMailModule;
            if (mailModule == null)
                throw new Exception(string.Format("{0} requires {1} to be installed before it", GetType().Name, typeof(ErrorMailModule).Name));
            config = (ElmahMailSAZConfig) ConfigurationManager.GetSection("elmah/errorMailSAZ") ?? new ElmahMailSAZConfig();
            mailModule.Mailing += MailModuleMailing;
            mailModule.Mailed += MailModuleMailed;
        }

        public void MailModuleMailed(object sender, ErrorMailEventArgs args) {
            var context = HttpContext.Current;
            if (context == null)
                return;
            var saz = context.Items[sazFilenameKey] as string;
            if (saz == null)
                return;
            var attachment = context.Items[attachmentKey] as Attachment;
            if (attachment != null)
                attachment.Dispose();
            File.Delete(saz);
        }

        public void MailModuleMailing(object sender, ErrorMailEventArgs args) {
            var context = HttpContext.Current;
            if (context == null)
                return;
            if (config.ExcludedUrls.Any(rx => rx.IsMatch(context.Request.RawUrl)))
                return;
            var saz = SerializeRequestToSAZ();
            if (saz == null)
                return;
            var saz2 = saz + ".saz";
            File.Move(saz, saz2);
            var attachment = new Attachment(saz2);
            context.Items[sazFilenameKey] = saz2;
            context.Items[attachmentKey] = attachment;
            args.Mail.Attachments.Add(attachment);
        }

        public string SerializeRequestToSAZ() {
            var filename = Path.GetTempFileName();
            var session = new Session(HttpContext.Current.Request.SerializeRequestToBytes(), null);
            var ok = SAZ.WriteSessionArchive(filename, new[] {session}, config.Password);
            if (!ok) {
                File.Delete(filename);
                return null;
            }
            return filename;
        }

        public void Dispose() {}
    }
}