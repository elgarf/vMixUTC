using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Licensing
{
    public static class LicenseManager
    {
        public static string Company = "elgarf";
        public static string Software = "vMix UTC";
        public static string Salt = "NaCl";
        static string GetMotherBoardID()
        {
            string MotherBoardID = string.Empty;
            SelectQuery query = new SelectQuery("Win32_BaseBoard");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            ManagementObjectCollection.ManagementObjectEnumerator enumerator = searcher.Get().GetEnumerator();
            while (enumerator.MoveNext())
            {
                ManagementObject info = (ManagementObject)enumerator.Current;
                MotherBoardID = info["SerialNumber"].ToString().Trim();
            }
            return MotherBoardID;
        }

        public static string GenerateActivationKey(string userID = null)
        {
            var u = UnicodeEncoding.Unicode;
            if (userID == null)
                userID = GetMotherBoardID();
            using (var md5 = MD5.Create())
            {
                var t1 = Convert.ToBase64String(md5.ComputeHash(u.GetBytes(userID)));
                var t2 = Convert.ToBase64String(md5.ComputeHash(u.GetBytes(Salt)));
                return Convert.ToBase64String(md5.ComputeHash(u.GetBytes(t1 + t2)));
            }
        }


        public static byte[] SignLicense(License lic, RSAParameters privateKey)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, lic);
                ms.Seek(0, SeekOrigin.Begin);
                var buffer = new byte[ms.Length];
                ms.Read(buffer, 0, buffer.Length);
                var sign = Convert.FromBase64String(SignKey(Convert.ToBase64String(buffer), privateKey));


                var modxor = sign.Aggregate((x, y) => (byte)((x ^ y) % 255));
                buffer = buffer.Select(x => (byte)(x ^ modxor)).Reverse().ToArray();

                var result = new byte[sizeof(ushort) + buffer.Length + sign.Length];

                Array.Copy(buffer, 0, result, sizeof(ushort), buffer.Length);
                Array.Copy(sign, 0, result, sizeof(ushort) + buffer.Length, sign.Length);
                Array.Copy(BitConverter.GetBytes((ushort)buffer.Length), result, sizeof(ushort));

                return result;
            }
        }



        public static License VerifyLicense(byte[] license, string key, RSAParameters publicKey)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream(license))
            {
                var lic_length = new byte[sizeof(ushort)];
                ms.Read(lic_length, 0, lic_length.Length);
                var l = BitConverter.ToUInt16(lic_length, 0);
                var lic = new byte[l];
                ms.Read(lic, 0, l);
                
                var sign = new byte[ms.Length - l - sizeof(ushort)];
                ms.Read(sign, 0, sign.Length);

                var modxor = sign.Aggregate((x, y) => (byte)((x ^ y) % 255));
                lic = lic.Select(x => (byte)(x ^ modxor)).Reverse().ToArray();
                License license_obj;

                using (var ms1 = new MemoryStream(lic))
                {
                    license_obj = (License)bf.Deserialize(ms1);
                }

                if ((VerifyKey(Convert.ToBase64String(lic), Convert.ToBase64String(sign), publicKey) && (license_obj.MachineID == key || (license_obj.MachineID == null && license_obj.Customer == key))))
                    return license_obj;
            }
            return null;
        }

        public static string SignKey(string key, RSAParameters privateKey)
        {
            var u = UnicodeEncoding.Unicode;
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(privateKey);
                return Convert.ToBase64String(rsa.SignData(u.GetBytes(key), CryptoConfig.MapNameToOID("SHA512")));
            }
        }

        public static bool VerifyKey(string data, string signatre, RSAParameters publicKey)
        {
            var u = UnicodeEncoding.Unicode;
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                try
                {
                    rsa.ImportParameters(publicKey);
                    return rsa.VerifyData(u.GetBytes(data), CryptoConfig.MapNameToOID("SHA512"), Convert.FromBase64String(signatre));
                }
                catch
                {
                    return false;
                }
            }
        }

        public static void SaveKey(string data)
        {
            RegistryKey regKey = Registry.CurrentUser;
            var software = regKey.OpenSubKey("SOFTWARE", true);
            var companyRk = software.CreateSubKey(Company);
            var programRk = companyRk.CreateSubKey(Software);
            programRk.SetValue("RegKey", data, RegistryValueKind.String);
        }

        public static bool CheckKey(string activation, RSAParameters publicKey)
        {
            RegistryKey regKey = Registry.CurrentUser;
            var software = regKey.CreateSubKey("SOFTWARE");
            var companyRk = software.CreateSubKey(Company);
            var programRk = companyRk.CreateSubKey(Software);
            string rk = null;
            if ((rk = (string)programRk.GetValue("RegKey")) != null)
            {
                return VerifyKey(activation, rk, publicKey);
            }
            return false;
        }
    }
}
