using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MyTool.Services
{
    /// <summary>
    /// 加解密
    /// </summary>
    public class _Crypto
    {
        static Encoding TheEncoding = Encoding.UTF8;
        const string _salt = "salt2018zwrkhoxje";

        #region MD5

        public static string HashMD5(string phrase)
        {
            if (phrase == null)
                return null;
            var encoder = new UTF8Encoding();
            var md5Hasher = new MD5CryptoServiceProvider();
            var hashedDataBytes = md5Hasher.ComputeHash(encoder.GetBytes(phrase));
            return ByteArrayToHexString(hashedDataBytes);
        }

        #endregion

        #region SHA

        public static string HashSHA1(string phrase)
        {
            if (phrase == null)
                return null;
            var encoder = new UTF8Encoding();
            var sha1Hasher = new SHA1CryptoServiceProvider();
            var hashedDataBytes = sha1Hasher.ComputeHash(encoder.GetBytes(phrase));
            return ByteArrayToHexString(hashedDataBytes);
        }

        public static string HashSHA256(string phrase)
        {
            // Password+Salt        
            if (phrase == null)
                return null;
            var encoder = new UTF8Encoding();
            var sha256Hasher = new SHA256CryptoServiceProvider();
            var hashedDataBytes = sha256Hasher.ComputeHash(encoder.GetBytes(phrase + _salt));
            return ByteArrayToHexString(hashedDataBytes);
        }

        public static string HashSHA384(string phrase)
        {
            if (phrase == null)
                return null;
            var encoder = new UTF8Encoding();
            var sha384Hasher = new SHA384CryptoServiceProvider();
            var hashedDataBytes = sha384Hasher.ComputeHash(encoder.GetBytes(phrase));
            return ByteArrayToHexString(hashedDataBytes);
        }

        public static string HashSHA512(string phrase)
        {
            if (phrase == null)
                return null;
            var encoder = new UTF8Encoding();
            var sha512Hasher = new SHA512CryptoServiceProvider();
            var hashedDataBytes = sha512Hasher.ComputeHash(encoder.GetBytes(phrase));
            return ByteArrayToHexString(hashedDataBytes);
        }

        #endregion

        #region AES

        public static string EncryptAES(string phrase, string key, bool hashKey = true)
        {
            if (phrase == null || key == null)
                return null;

            var keyArray = HexStringToByteArray(hashKey ? HashMD5(key) : key);
            var toEncryptArray = Encoding.UTF8.GetBytes(phrase);
            byte[] result;

            using (var aes = new AesCryptoServiceProvider
            {
                Key = keyArray,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            })
            {
                var cTransform = aes.CreateEncryptor();
                result = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                aes.Clear();
            }
            return ByteArrayToHexString(result);
        }

        public static string DecryptAES(string hash, string key, bool hashKey = true)
        {
            if (hash == null || key == null)
                return null;

            var result = "";

            try
            {
                var keyArray = HexStringToByteArray(hashKey ? HashMD5(key) : key);
                var toEncryptArray = HexStringToByteArray(hash);

                var aes = new AesCryptoServiceProvider
                {
                    Key = keyArray,
                    Mode = CipherMode.ECB,
                    Padding = PaddingMode.PKCS7
                };

                var cTransform = aes.CreateDecryptor();
                var resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

                aes.Clear();

                result = Encoding.UTF8.GetString(resultArray);
            }
            catch (Exception ex)
            {
                _Log.CreateText("DecryptAES error:" + hash);
                result = "(error)";
            }
        
            return result;
        }

        #endregion

        #region 3DES

        public static string EncryptTripleDES(string phrase, string key, bool hashKey = true)
        {
            var keyArray = HexStringToByteArray(hashKey ? HashMD5(key) : key);
            var toEncryptArray = Encoding.UTF8.GetBytes(phrase);

            var tdes = new TripleDESCryptoServiceProvider
            {
                Key = keyArray,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };

            var cTransform = tdes.CreateEncryptor();
            var resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            tdes.Clear();
            return ByteArrayToHexString(resultArray);
        }

        public static string DecryptTripleDES(string hash, string key, bool hashKey = true)
        {
            var keyArray = HexStringToByteArray(hashKey ? HashMD5(key) : key);
            var toEncryptArray = HexStringToByteArray(hash);

            var tdes = new TripleDESCryptoServiceProvider
            {
                Key = keyArray,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };

            var cTransform = tdes.CreateDecryptor();
            var resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            tdes.Clear();
            return Encoding.UTF8.GetString(resultArray);
        }

        #endregion

        #region Helpers

        internal static string ByteArrayToHexString(byte[] inputArray)
        {
            if (inputArray == null)
                return null;
            var o = new StringBuilder("");
            for (var i = 0; i < inputArray.Length; i++)
                o.Append(inputArray[i].ToString("X2"));
            return o.ToString();
        }

        internal static byte[] HexStringToByteArray(string inputString)
        {
            if (inputString == null)
                return null;

            if (inputString.Length == 0)
                return new byte[0];

            if (inputString.Length % 2 != 0)
                throw new Exception("Hex strings have an even number of characters and you have got an odd number of characters!");

            var num = inputString.Length / 2;
            var bytes = new byte[num];
            for (var i = 0; i < num; i++)
            {
                var x = inputString.Substring(i * 2, 2);
                try
                {
                    bytes[i] = Convert.ToByte(x, 16);
                }
                catch (Exception ex)
                {
                    throw new Exception("Part of your \"hex\" string contains a non-hex value.", ex);
                }
            }
            return bytes;
        }

        #endregion

        #region 國泰虛擬帳號即時入金通知解密

        public static byte[] HexToByte(string hexString)
        {
            //運算後的位元組長度:16進位數字字串長/2
            byte[] byteOUT = new byte[hexString.Length / 2];
            for (int i = 0; i < hexString.Length; i = i + 2)
            {
                //每2位16進位數字轉換為一個10進位整數
                byteOUT[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
            }
            return byteOUT;
        }


        public static string AES_Decrypt(string strData, string strKey)
        {
            Encoding encoding = Encoding.GetEncoding("utf-8");
            byte[] keyArray = encoding.GetBytes(strKey);
            byte[] toEncryptArray = Convert.FromBase64String(strData);
            RijndaelManaged rDel = new RijndaelManaged();
            rDel.Key = keyArray;
            rDel.Mode = CipherMode.ECB;
            rDel.Padding = PaddingMode.PKCS7;
            ICryptoTransform cTransform = rDel.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0,
            toEncryptArray.Length);
            //strResult = UTF8Encoding.UTF8.GetString(resultArray);
            strData = Encoding.GetEncoding("big5").GetString(resultArray);


            return strData;
        }

        #endregion
    }
}
