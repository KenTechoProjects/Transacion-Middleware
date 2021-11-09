using System;
using System.Security.Cryptography;
using System.Text;

namespace Middleware.Service.Utilities
{
    public class CodeGenerator : ICodeGenerator
    {
        public string Generate(int length)
        {
            var provider = new RNGCryptoServiceProvider();
          
            var container = new byte[4];
            var builder = new StringBuilder(length);
            for (int i = 1; i <= length; i++)
            {
                provider.GetBytes(container);
                var t = BitConverter.ToUInt16(container, 0);
                var result = (byte)(10 * t / (double)(UInt16.MaxValue + 1));
                builder.Append(result);
            }
            return builder.ToString();
        }

        public   string ReferralCode(int length)
        {

            string numbers = "abcdefghijklmnopqurtuvwxyz1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            string characters = numbers;
            int lent = length;
            string otp = string.Empty;
            for (int i = 0; i < lent; i++)
            {
                string character = string.Empty;
                do
                {
                    int index = new Random().Next(0, characters.Length);
                    character = characters.ToCharArray()[index].ToString();

                } while (otp.IndexOf(character) != -1);
                otp += character;


            }
            return otp;

        }
    }
}
