using System;
using System.Runtime.InteropServices;

namespace Onova.Internal
{
    internal static class Platform
    {
        public static void EnsureWindowsOrMac()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                throw new PlatformNotSupportedException("Onova only supports Windows and Mac.");
        }
    }
}