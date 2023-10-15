using System.Security.Cryptography;
using System.Text;
using Imager.Services.Interfaces;

namespace Imager.Services.Implementation;

public class HashService : IHashService
{
    public string HashString(string stringToHash)
    {
        using HashAlgorithm algorithm = SHA256.Create();
        var bytes = algorithm.ComputeHash(Encoding.UTF8.GetBytes(stringToHash));
        return BitConverter.ToString(bytes).Replace("-", String.Empty);
    }
}