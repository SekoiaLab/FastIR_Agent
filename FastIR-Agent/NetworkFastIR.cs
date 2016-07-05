using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

public struct ret
{
    public bool SSL_status;
    public string Web_return;
};

namespace network_appli
{
    class NetworkFastIR
    {
        private string host;
        private string port;
        private string url;
        private string lpk;

        // CF http://stackoverflow.com/questions/1193529/how-to-store-retreieve-rsa-public-private-key

        public NetworkFastIR(string host, string port, string public_key)
        {
            this.host = host;
            this.port = port;
            this.lpk = public_key;
            this.url = "https://" + this.host + ":" + this.port + "/";
        }

        private bool PinPublicKey(object sender, X509Certificate certificate, X509Chain chain,
                                SslPolicyErrors sslPolicyErrors)
        {
            if (certificate.Equals(null))
                return false;
            String pk = certificate.GetPublicKeyString();
            if (pk.Equals(this.lpk.ToUpper()))
                return true;
            return false;
        }

        public ret query(string uri, string POST = null)
        {
            ret r;
            ServicePointManager.ServerCertificateValidationCallback = PinPublicKey;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            try
            {
                string ret = null;
                WebRequest wr = WebRequest.Create(this.url + uri);
                if (POST != null)
                {
                    wr.Method = "POST";
                    byte[] byteArray = Encoding.UTF8.GetBytes(POST);
                    wr.ContentType = "application/x-www-form-urlencoded";
                    wr.ContentLength = byteArray.Length;
                    using (Stream dataStream = wr.GetRequestStream())
                    {
                        dataStream.Write(byteArray, 0, byteArray.Length);
                    }
                }
                using (WebResponse response = wr.GetResponse())
                {
                    using (Stream ReceiveStream = response.GetResponseStream())
                    {
                        Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
                        using (StreamReader readStream = new StreamReader(ReceiveStream, encode))
                        {
                            Char[] read = new Char[256];
                            int count = readStream.Read(read, 0, 256);
                            while (count > 0)
                            {
                                // Dump the 256 characters on a string and display the string onto the console.
                                String str = new String(read, 0, count);
                                ret = ret + str;
                                count = readStream.Read(read, 0, 256);
                            }
                        }
                    }
                }
                r.SSL_status = true;
                r.Web_return = ret;
                return r;
            }
            catch
            {
                r.SSL_status = false;
                r.Web_return = null;
                return r;
            }
        }
    }
}