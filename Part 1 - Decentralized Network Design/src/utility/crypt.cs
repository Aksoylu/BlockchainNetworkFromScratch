using System;
using System.Text;
using System.Security.Cryptography;

namespace BlockchainNetwork;

public class Crypt
{
    public static String HashSha256(String input)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            // Convert the input string to bytes
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);

            // Compute the hash
            byte[] hashBytes = sha256.ComputeHash(inputBytes);

            // Convert the hash bytes to a hexadecimal string
            string hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            return hashString;
        }
    }
}