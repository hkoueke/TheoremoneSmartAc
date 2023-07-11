using System.Security.Cryptography;
using System.Text.Json;

namespace SmartAc.Application.Extensions;

public static class ApplicationExtensions
{
    public static bool InRange<T>(this T value, T minValue, T maxValue)
        where T : struct, IComparable, IComparable<T>, IConvertible, IEquatable<T>
    {
        return value.CompareTo(minValue) >= 0 && value.CompareTo(maxValue) <= 0;
    }

    public static string GetHashString<T>(this T value)
    {
        var serialized = JsonSerializer.SerializeToUtf8Bytes(value);
        using var sha1 = SHA1.Create();
        return Convert.ToHexString(sha1.ComputeHash(serialized));
    }
}