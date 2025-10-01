using System.IO.Compression;
using System.Security.Cryptography;

namespace Koalas.Extensions;

public static class ByteExtension
{
    public static byte[] Compress(this byte[] content)
    {
        using MemoryStream msi = new(content);
        using MemoryStream outputStream = new();

        using (GZipStream gzipStream = new(outputStream, CompressionMode.Compress))
        {
            CopyTo(msi, gzipStream);
        } // using must be closed here to flush the stream

        return outputStream.ToArray();
    }

    public static byte[] Decompress(this byte[] content)
    {
        using MemoryStream inputStream = new(content);
        using MemoryStream outputStream = new();
        using (GZipStream gzipStream = new(inputStream, CompressionMode.Decompress))
        {
            CopyTo(gzipStream, outputStream);
        } // using must be closed here to flush the stream

        return outputStream.ToArray();
    }

    public static string Sha256(this byte[] content)
    {
        HashAlgorithm algo =
            (HashAlgorithm)CryptoConfig.CreateFromName("SHA256")
            ?? throw new Exception("Invalid hash algorithm SHA256");
        byte[] hash = algo.ComputeHash(content);

        return BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
    }

    private static void CopyTo(Stream source, Stream destination)
    {
        byte[] bytes = new byte[4096];

        int cnt;

        while ((cnt = source.Read(bytes, 0, bytes.Length)) != 0)
        {
            destination.Write(bytes, 0, cnt);
        }
    }
}
