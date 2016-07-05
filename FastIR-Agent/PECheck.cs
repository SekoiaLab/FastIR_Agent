using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace FastIR_Agent
{
    class PECheck
    {
        // SEKOIA Public key for binary signature
        private static String SEKOIA_PUBKEY = "3082010A0282010100CF8F6995A286734FF205FF1F5F" +
                                              "36F8AC0BDA4FE6D11A9232CA5C73BB0B13BA53A330F4" +
                                              "BA738682D2121B0C8471CA67757CD5727F6889ABE328" +
                                              "44644040EBE623CB2A0C93C857DA4C635D152CBAA4CC" +
                                              "5D2C37268623EE46BF5C90AAEDC5F7658068E5F24E49" +
                                              "6FBB41741CCE4B57A81006F5936A34878565BE02A438" +
                                              "316BB2047F139E9DFEE9F1273383763E75B1E46980DF" +
                                              "EDDD268FB10868E62329BBB6001A65B73C06D81358F8" +
                                              "E54577CA053BB7EFFEC44562CABAF45E3AA676FD4B00" +
                                              "50522371A33B5F51535ADB2FB4973D30AE02BD43FB3C" +
                                              "38EE6470E103A0C29ED913ED28E3233B934C360C0C7F" +
                                              "3CA05A607DB9E9C97A45B0450C0E4C16416DB1B4963A" +
                                              "CF0203010001";

        public bool checkfile(string filePath)
        {
            if (!File.Exists(filePath))
                return false;

            X509Certificate2 theCertificate;
            try
            {
                X509Certificate theSigner = X509Certificate.CreateFromSignedFile(filePath);
                theCertificate = new X509Certificate2(theSigner);
            }
            catch { return false; }
            if (theCertificate.GetPublicKeyString().Equals(SEKOIA_PUBKEY))
                return true;
            else
                return false;
        }
    }
}
