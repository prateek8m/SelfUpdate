﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Onova.Internal
{
    internal static class Extensions
    {
        public static string SubstringUntil(this string s, string sub,
            StringComparison comparison = StringComparison.Ordinal)
        {
            var index = s.IndexOf(sub, comparison);
            return index < 0 ? s : s.Substring(0, index);
        }

        public static string SubstringAfter(this string s, string sub,
            StringComparison comparison = StringComparison.Ordinal)
        {
            var index = s.IndexOf(sub, comparison);
            return index < 0 ? string.Empty : s.Substring(index + sub.Length, s.Length - index - sub.Length);
        }

        public static byte[] GetBytes(this string input, Encoding encoding) => encoding.GetBytes(input);

        public static byte[] GetBytes(this string input) => input.GetBytes(Encoding.UTF8);

        public static string ToBase64(this byte[] data) => Convert.ToBase64String(data);

        public static int AddRange<T>(this HashSet<T> hashSet, IEnumerable<T> sequence) => sequence.Count(hashSet.Add);

        public static async Task<int> CopyBufferedToAsync(this Stream source, Stream destination, byte[] buffer,
            CancellationToken cancellationToken = default)
        {
            var bytesCopied = await source.ReadAsync(buffer, cancellationToken);
            await destination.WriteAsync(buffer, 0, bytesCopied, cancellationToken);

            return bytesCopied;
        }

        public static async Task CopyToAsync(this Stream source, Stream destination,
            IProgress<double>? progress = null, CancellationToken cancellationToken = default)
        {
            var buffer = new byte[81920];
            var totalBytesCopied = 0L;
            int bytesCopied;
            do
            {
                // Copy
                bytesCopied = await source.CopyBufferedToAsync(destination, buffer, cancellationToken);

                // Report progress
                totalBytesCopied += bytesCopied;
                progress?.Report(1.0 * totalBytesCopied / source.Length);
            } while (bytesCopied > 0);
        }

        public static async Task ExtractManifestResourceAsync(this Assembly assembly, string resourceName,
            string destFilePath)
        {
            var inputs = assembly.GetManifestResourceNames();
            foreach (var item in inputs)
            {
                var input = assembly.GetManifestResourceStream(item) ??
                        throw new MissingManifestResourceException($"Could not find resource [{item}].");
                string temp=item.Replace("Onova.Onova", "");
                temp=temp.Replace("Onova", "");
                using var output = File.Create(destFilePath+temp);
                await input.CopyToAsync(output);
            }
            //var input = assembly.GetManifestResourceStream(resourceName) ??
            //            throw new MissingManifestResourceException($"Could not find resource [{resourceName}].");

            //using var output = File.Create(destFilePath);
            //await input.CopyToAsync(output);
        }
    }
}