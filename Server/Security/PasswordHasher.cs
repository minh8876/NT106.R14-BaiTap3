using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Server.Security
{
    public class PasswordHasher
    {
        private const int SaltSize = 16; 
        private const int KeySize = 32;  
        private const int Iterations = 100000; 

        public static void HashPassword(string password, out string hash, out string salt)
        {
            using (var algorithm = new Rfc2898DeriveBytes(password, SaltSize, Iterations, HashAlgorithmName.SHA256))
            {
                salt = Convert.ToBase64String(algorithm.Salt);
                hash = Convert.ToBase64String(algorithm.GetBytes(KeySize));
            }
        }

        public static bool VerifyPassword(string password, string savedHash, string savedSalt)
        {
            byte[] saltBytes = Convert.FromBase64String(savedSalt);
            using (var algorithm = new Rfc2898DeriveBytes(password, saltBytes, Iterations, HashAlgorithmName.SHA256))
            {
                byte[] keyToCheck = algorithm.GetBytes(KeySize);
                byte[] originalKey = Convert.FromBase64String(savedHash);

                return CryptographicOperations.FixedTimeEquals(keyToCheck, originalKey);
            }
        }
    }
}
