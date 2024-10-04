using System.Security.Cryptography;
using System.Text;

namespace pingu.Helpers;

public static class HashHelper
{
    public static string GenerateSHA256Hash(string input)
    {
        if (input == null) throw new ArgumentNullException(nameof(input));

        using (var sha256 = SHA256.Create())
        {
            var inputBytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = sha256.ComputeHash(inputBytes);

            var hashBuilder = new StringBuilder();
            for (var i = 0; i < hashBytes.Length; i++)
            {
                hashBuilder.Append(hashBytes[i].ToString("x2"));
            }

            return hashBuilder.ToString();
        }
    }

    public static string GenerateMD5Hash(string input)
    {
        if (input == null) throw new ArgumentNullException(nameof(input));

        using (var md5 = MD5.Create())
        {
            var inputBytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = md5.ComputeHash(inputBytes);

            var sb = new StringBuilder();
            for (var i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("x2")); // Convert byte to hexadecimal
            }

            return sb.ToString();
        }
    }
}