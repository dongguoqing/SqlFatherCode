using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
namespace Daemon.Common
{
    public class PasswordHelper
    {
        public static byte[] ChangeStringToSHA256(string password)
        {
            return SHA256.Create().ComputeHash(Encoding.Default.GetBytes(password));
        }

        public static string EncodingToString(byte[] password)
        {
            return Encoding.Default.GetString(password);
        }

        public static bool ChangeVarbinaryToByte(string binaryPassword, out List<byte> byteList)
        {
            byteList = new List<byte>();
            string hexPart = binaryPassword.Substring(2);
            for (int i = 0; i < hexPart.Length / 2; i++)
            {
                string hexNumber = hexPart.Substring(i * 2, 2);
                if (!byte.TryParse(hexNumber, System.Globalization.NumberStyles.HexNumber, null as IFormatProvider, out byte result))
                {
                    return false;
                }

                byteList.Add(result);
            }

            return true;
        }

        public static bool Compare(byte[] password1, byte[] password2)
        {
            return Encoding.Default.GetString(password1) == Encoding.Default.GetString(password2);
        }

        private const string SALT = "S!lt^Tr@nsf1ner";
        private const string PASSWORD = "P!ssw0rd^Tr@nsf1nder";

        public static string Rfc2898Encrypt(string plainText, string clientKey)
        {
            if (string.IsNullOrWhiteSpace(plainText))
            {
                return plainText;
            }

            string password = GetPassword(clientKey);
            byte[] salt = GetSalt(clientKey);
            byte[] buffer = new UTF8Encoding(false).GetBytes(plainText);
            using (Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(password, salt))
            using (Aes aes = Aes.Create())
            {
                aes.Key = key.GetBytes(aes.KeySize / 8);
                aes.IV = key.GetBytes(16);
                using (MemoryStream encryptionStream = new MemoryStream())
                using (CryptoStream encrypt = new CryptoStream(encryptionStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    encrypt.Write(buffer, 0, buffer.Length);
                    encrypt.FlushFinalBlock();
                    return Convert.ToBase64String(encryptionStream.ToArray());
                }
            }
        }

        public static string Rfc2898Decrypt(string cipherText, string clientKey)
        {
            if (string.IsNullOrWhiteSpace(cipherText))
            {
                return cipherText;
            }

            string password = GetPassword(clientKey);
            byte[] salt = GetSalt(clientKey);
            byte[] buffer = Convert.FromBase64String(cipherText);
            using (Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(password, salt))
            using (Aes aes = Aes.Create())
            {
                aes.Key = key.GetBytes(aes.KeySize / 8);
                aes.IV = key.GetBytes(16);
                using (MemoryStream decryptionStream = new MemoryStream())
                {
                    using (CryptoStream decrypt = new CryptoStream(decryptionStream, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        decrypt.Write(buffer, 0, buffer.Length);
                        decrypt.FlushFinalBlock();
                        return new UTF8Encoding(false).GetString(decryptionStream.ToArray());
                    }
                }
            }
        }

        private static byte[] GetSalt(string clientKey)
        {
            return Encoding.ASCII.GetBytes(clientKey + SALT);
        }

        private static string GetPassword(string clientKey)
        {
            return clientKey + PASSWORD;
        }
    }
}
