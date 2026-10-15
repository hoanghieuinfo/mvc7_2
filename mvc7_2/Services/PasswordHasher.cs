using System.Security.Cryptography;

namespace mvc7_2.Services;

/// <summary>
/// Băm mật khẩu một chiều bằng PBKDF2 (HMAC-SHA256) với salt ngẫu nhiên.
/// Chuỗi lưu trong CSDL có dạng: {số vòng lặp}.{salt Base64}.{hash Base64}
/// </summary>
public static class PasswordHasher
{
    private const int SaltSize = 16;      // 128 bit
    private const int HashSize = 32;      // 256 bit
    private const int Iterations = 100_000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public static string Hash(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSize);
        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public static bool Verify(string password, string passwordHashed)
    {
        var parts = passwordHashed.Split('.', 3);
        if (parts.Length != 3 || !int.TryParse(parts[0], out int iterations))
        {
            return false;
        }

        byte[] salt = Convert.FromBase64String(parts[1]);
        byte[] expected = Convert.FromBase64String(parts[2]);
        byte[] actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, Algorithm, expected.Length);

        // So sánh thời gian cố định để tránh timing attack
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
