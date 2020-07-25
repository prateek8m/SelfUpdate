﻿using System.IO;
using System.IO.Compression;

namespace Onova.Tests.Internal
{
    internal static class Extensions
    {
        public static void WriteAllBytes(this ZipArchiveEntry entry, byte[] content)
        {
            using var stream = entry.Open();
            stream.Write(content, 0, content.Length);
        }

        public static void WriteAllText(this ZipArchiveEntry entry, string contents)
        {
            using var stream = entry.Open();
            using var writer = new StreamWriter(stream);

            writer.Write(contents);
        }

        public static string ReadAllText(this ZipArchiveEntry entry)
        {
            using var stream = entry.Open();
            using var reader = new StreamReader(stream);

            return reader.ReadToEnd();
        }
    }
}