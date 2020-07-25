﻿using System;
using System.Runtime.InteropServices;

namespace Onova.Internal
{
    internal static class Platform
    {
        public static void EnsureWindows()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new PlatformNotSupportedException("Onova only supports Windows.");
        }
    }
}