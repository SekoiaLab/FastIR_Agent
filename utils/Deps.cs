using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace utils
{
    class Deps
    {
        public string lpk = null;
        public string G_SSL = null;

        public string ReadKey(string KeyName)
        {
            const string HKLMRoot = "HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\FastIR";
            try
            {
                string Value = (string)Registry.GetValue(HKLMRoot, KeyName, null);
                return Value;
            }
            catch
            {
                return null;
            }
        }

        private bool GetPublicKeyCallBack(object sender, X509Certificate certificate, X509Chain chain,
                                SslPolicyErrors sslPolicyErrors)
        {
            if (certificate.Equals(null))
            {
                G_SSL = "Bad URL or no SSL";
                return false;
            }
            String pk = certificate.GetPublicKeyString();
            G_SSL = pk;
            return true;
        }

        public bool GetPublicKey(string URL)
        {
            ServicePointManager.ServerCertificateValidationCallback = GetPublicKeyCallBack;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            try
            {
                WebRequest wr = WebRequest.Create(URL);
                using (WebResponse response = wr.GetResponse()) { }
                return true;
            }
            catch { return false; }
        }

        public bool CheckURL()
        {
            string _URL = ReadKey("URL");
            string _Port = ReadKey("Port");
            lpk = ReadKey("PUBLIC_SSL");
            string FULL_URL = "https://" + _URL + ":" + _Port + "/";
            ServicePointManager.ServerCertificateValidationCallback = PinPublicKey;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            try
            {
                WebRequest wr = WebRequest.Create(FULL_URL);
                wr.Timeout = 1000;
                using (WebResponse response = wr.GetResponse()) { }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool PinPublicKey(object sender, X509Certificate certificate, X509Chain chain,
                                SslPolicyErrors sslPolicyErrors)
        {
            if (certificate.Equals(null))
                return false;
            String pk = certificate.GetPublicKeyString();
            if (pk.Equals(lpk))
                return true;
            return false;
        }
    }
}
