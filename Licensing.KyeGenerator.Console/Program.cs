using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Licensing.KyeGenerator.Console
{
    class Program
    {

        static void RequestSreingProperty(string property, License lic)
        {
            System.Console.WriteLine("{0}:", property);
            var prop = typeof(License).GetProperty(property);
            prop.SetValue(lic, System.Console.ReadLine());

            if (string.IsNullOrWhiteSpace((string)prop.GetValue(lic)))
                prop.SetValue(lic, null);
        }

        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                System.Console.WriteLine("RSA keys are generated!");
                //return;
            }
            License lic = new License();
            RequestSreingProperty("MachineID", lic);
            RequestSreingProperty("Customer", lic);
            string feature = null;
            System.Console.WriteLine("Write license features:");
            while (!string.IsNullOrWhiteSpace(feature = System.Console.ReadLine()))
            {
                var nameval = feature.Split(':');
                try
                {
                    switch (nameval[2])
                    {
                        case "s":
                            lic.AddFeature(nameval[0], nameval[1]);
                            break;
                        case "i":
                            lic.AddFeature(nameval[0], int.Parse(nameval[1]));
                            break;
                        case "b":
                            lic.AddFeature(nameval[0], bool.Parse(nameval[1]));
                            break;
                        default:
                            break;
                    }
                    
                }
                catch (Exception ex) { System.Console.WriteLine("Bad feature!"); }
            }

            RSACryptoServiceProvider _csp = new RSACryptoServiceProvider();
            if (args.Length > 1)
                _csp.FromXmlString(File.ReadAllText(args[1]));
            else
                File.WriteAllText("Keys.xml", _csp.ToXmlString(true));

            byte[] licSigned = null;
            File.WriteAllBytes("License.lic", licSigned = LicenseManager.SignLicense(lic, _csp.ExportParameters(true)));
            System.Console.WriteLine("{0}", LicenseManager.VerifyLicense(licSigned, lic.MachineID != null ? lic.MachineID : lic.Customer, _csp.ExportParameters(true)) != null);

            System.Console.ReadKey();
            
        }
    }
}
