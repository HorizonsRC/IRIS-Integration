using System;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.IO;

namespace HRC.Common.Security
{
    public class Encryption
    {
        //not suitable for config, ini, text files etc. More suited to database.
        #region RijndaelEncryption

        private static Encoding LatinEncoding
        {
            get
            {
                return Encoding.GetEncoding(1252); 
            }
        }

        private static readonly byte[] m_Key = { 0xA1, 0xF1, 0xA6, 0xBB, 0xA2, 0x5A, 0x37, 0x6F, 0x81, 0x2E, 0x17, 0x41, 0x72, 0x2C, 0x43, 0x27 };
        private static readonly byte[] m_InitializationVector = { 0xE1, 0xF1, 0xA6, 0xBB, 0xA9, 0x5B, 0x31, 0x2F, 0x81, 0x2E, 0x17, 0x4C, 0xA2, 0x81, 0x53, 0x61 };

        public static string RijndaelEncrypt(string value)
        {
            string encryptedValue = string.Empty;
            
            using (MemoryStream stream = new MemoryStream())            
            {
                RijndaelManaged rijndaelAlgorithm = null;
                try
                {
                    rijndaelAlgorithm = new RijndaelManaged();
                    rijndaelAlgorithm.Key = m_Key;
                    rijndaelAlgorithm.IV = m_InitializationVector;                    
                    ICryptoTransform encryptor = rijndaelAlgorithm.CreateEncryptor(rijndaelAlgorithm.Key, rijndaelAlgorithm.IV);
                    using (CryptoStream cryptoStream = new CryptoStream(stream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter writer = new StreamWriter(cryptoStream))
                        {
                            writer.Write(value);                            
                        }
                    }
                }
                finally
                {                    
                    if (rijndaelAlgorithm != null)
                    {
                        rijndaelAlgorithm.Clear();
                    }
                }
                encryptedValue = LatinEncoding.GetString(stream.ToArray());
            }
            return encryptedValue;
        }

        public static string RijndaelDecrypt(string encryptedValue)
        {
            RijndaelManaged rijndaelAlgorithm = null;
            string value = null;
            try
            {
                rijndaelAlgorithm = new RijndaelManaged();
                rijndaelAlgorithm.Key = m_Key;
                rijndaelAlgorithm.IV = m_InitializationVector;       
                ICryptoTransform decryptor = rijndaelAlgorithm.CreateDecryptor(rijndaelAlgorithm.Key, rijndaelAlgorithm.IV);
                using (MemoryStream stream = new MemoryStream(LatinEncoding.GetBytes(encryptedValue)))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(stream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader reader = new StreamReader(cryptoStream))
                        {
                            value = reader.ReadToEnd();
                        }
                    }
                }
            }
            finally
            {                
                if (rijndaelAlgorithm != null)
                {
                    rijndaelAlgorithm.Clear();
                }
            }
            return value;
        }

        #endregion RijndaelEncryption

        //suitable for web (url-safe)
        #region UrlEncryption

        public static string UrlEncrypt(string value)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(value);
            return HttpServerUtility.UrlTokenEncode(buffer);
        }

        public static string UrlDecrypt(string value)
        {
            byte[] buffer = HttpServerUtility.UrlTokenDecode(value);
            return Encoding.UTF8.GetString(buffer);
        }

        #endregion UrlEncryption

        //suitable for text files
        #region BasicEncryption

        private static MD5 m_MD5;
        private static MD5 MD5
        {
            get
            {
                if (m_MD5 == null)
                {
                    m_MD5 = MD5.Create();
                }
                return m_MD5;
            }
        }

        private static Encoding TextEncoding
        {
            get
            {
                return Encoding.UTF8;
            }
        }

        private static byte[] OptionalEntropy(string salt)
        {
            return MD5.ComputeHash(TextEncoding.GetBytes(salt.ToUpper()));
        }

        public static string BasicDecrypt(string value, string salt)
        {
            string decrypt = BasicDecryptCommand(value, salt);
            return BasicDecryptCommand(decrypt, salt);
        }

        public static string BasicEncrypt(string value, string salt)
        {
            string encrypt = BasicEncryptCommand(value, salt);
            return BasicEncryptCommand(encrypt, salt);
        }

        private static string BasicDecryptCommand(string value, string salt)
        {
            byte[] encryptedData = Convert.FromBase64String(value);            
            byte[] saltData = OptionalEntropy(salt);
            byte[] bytes = new byte[encryptedData.Length - saltData.Length];
            Array.Copy(encryptedData, saltData.Length, bytes, 0, bytes.Length);
            return TextEncoding.GetString(bytes);
        }

        private static string BasicEncryptCommand(string value, string salt)
        {
            byte[] userData = TextEncoding.GetBytes(value);
            byte[] saltData = OptionalEntropy(salt);
            byte[] temp = new byte[userData.Length + saltData.Length];
            saltData.CopyTo(temp, 0);
            userData.CopyTo(temp, saltData.Length);
            return Convert.ToBase64String(temp);            
        }

        #endregion BasicEncryption

        //uses both
        #region DoubleEncryption
        public static string DoubleEncrypt(string value, string salt)
        {
            string basicEncrypted = BasicEncrypt(value, salt);
            return RijndaelEncrypt(basicEncrypted);
        }

        public static string DoubleDecrypt(string value, string salt)
        {
            string decrypted = RijndaelDecrypt(value);
            return BasicDecrypt(decrypted, salt);
        }

        #endregion Encrypt
    }
}
