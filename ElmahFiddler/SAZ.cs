using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Fiddler;
using Ionic.Zip;

namespace ElmahFiddler {
    public class SAZ {
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

        public static IEnumerable<Session> ReadSessionArchive(string sazFile, string password) {
            using (var zip = ZipFile.Read(sazFile)) {
                return zip.Entries
                    .Where(z => z.FileName.EndsWith("_c.txt"))
                    .Select(z => z.ExtractWithPasswordToBytes(password))
                    .Select(request => new Session(request, new byte[0]))
                    .ToList();
            }
        }
    }
}