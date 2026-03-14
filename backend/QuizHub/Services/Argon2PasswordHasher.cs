using Konscious.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using QuizHub.Models.Entities;
using System.Security.Cryptography;
using System.Text;

namespace QuizHub.Services
{
    public class Argon2PasswordHasher : IPasswordHasher<NguoiDung>
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 4;
        private const int MemorySize = 65536; // 64 MB
        private const int DegreeOfParallelism = 8;

        public string HashPassword(NguoiDung user, string password)
        {
            // Tạo salt ngẫu nhiên
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

            // Hash password với Argon2id
            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                DegreeOfParallelism = DegreeOfParallelism,
                Iterations = Iterations,
                MemorySize = MemorySize
            };

            byte[] hash = argon2.GetBytes(HashSize);

            // Kết hợp salt + hash
            byte[] hashBytes = new byte[SaltSize + HashSize];
            Array.Copy(salt, 0, hashBytes, 0, SaltSize);
            Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

            return Convert.ToBase64String(hashBytes);
        }

        public PasswordVerificationResult VerifyHashedPassword(
            NguoiDung user,
            string hashedPassword,
            string providedPassword)
        {
            try
            {
                // Decode stored hash
                byte[] hashBytes = Convert.FromBase64String(hashedPassword);

                // Extract salt
                byte[] salt = new byte[SaltSize];
                Array.Copy(hashBytes, 0, salt, 0, SaltSize);

                // Hash provided password với cùng salt
                var argon2 = new Argon2id(Encoding.UTF8.GetBytes(providedPassword))
                {
                    Salt = salt,
                    DegreeOfParallelism = DegreeOfParallelism,
                    Iterations = Iterations,
                    MemorySize = MemorySize
                };

                byte[] providedHash = argon2.GetBytes(HashSize);

                // So sánh hash
                for (int i = 0; i < HashSize; i++)
                {
                    if (hashBytes[i + SaltSize] != providedHash[i])
                    {
                        return PasswordVerificationResult.Failed;
                    }
                }

                return PasswordVerificationResult.Success;
            }
            catch
            {
                return PasswordVerificationResult.Failed;
            }
        }
    }
}