using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace EiSorteei.Helpers
{
    public class HashPassword
    {
        public static string PasswordHash(string password)
        {
            var sb = new StringBuilder();
            using (var sha = new SHA256Managed())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                Byte[] result = sha.ComputeHash(bytes);

                foreach (var b in result)
                {
                    sb.Append(b.ToString("x2"));
                }
            }
            return sb.ToString();
        }

    }
}