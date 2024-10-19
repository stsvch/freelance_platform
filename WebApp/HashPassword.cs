using System.Security.Cryptography;
using System.Text;

namespace WebApp
{
    public class HashPassword
    {
        public static string Hash(string password)
        {
            string fixedSalt = "FixedSaltValue"; // Фиксированная соль
            string saltedPassword = password + fixedSalt;

            // Хеширование с использованием SHA256
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
                return Convert.ToBase64String(hashBytes);
            }
        }
    }
}
