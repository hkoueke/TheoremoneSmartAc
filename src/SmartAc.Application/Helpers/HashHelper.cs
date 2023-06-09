using System.Security.Cryptography;
using System.Text.Json;
using System.Text;

namespace SmartAc.Application.Helpers;

internal static class HashHelper
{
    public static string GetHexString<T>(this T value)
    {
        var serialized = JsonSerializer.Serialize(value);
        var bytes = Encoding.UTF8.GetBytes(serialized);
        using var sha256 = SHA256.Create();
        return Convert.ToHexString(sha256.ComputeHash(bytes));
    }
}