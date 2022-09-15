using System;
using System.Security.Cryptography;
using System.Text;
namespace DTO_PremierDucts.Utils
{
    public class StringUtils
    {

        public static string MD5Hash(string input)
        {
            StringBuilder hash = new StringBuilder();
            MD5CryptoServiceProvider md5provider = new MD5CryptoServiceProvider();
            byte[] bytes = md5provider.ComputeHash(new UTF8Encoding().GetBytes(input));

            for (int i = 0; i < bytes.Length; i++)
            {
                hash.Append(bytes[i].ToString("x2"));
            }
            return hash.ToString();
        }


        public static bool CheckNullAndEmpty(string str)
        {
            if(str != null && !str.Equals(""))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static string[] SubArray(string[] data, int index, int length)
        {
            string[] result = new string[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }
    }
}

