using System;
using System.Security.Cryptography;
using System.Text;

namespace domain.Ultils
{
    public class StringUltils
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
        //public static void Main(string[] args)
        //{
        //    Console.WriteLine( MD5Hash("testing"));
        //}
    }
}
