﻿using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Onova.Updater.Internal;

namespace Onova.Updater
{
    public class Updater : IDisposable
    {
        private readonly string _updateeFilePath;
        private readonly string _packageContentDirPath;
        private readonly bool _restartUpdatee;
        private readonly string _routedArgs;

        private static readonly TextWriter _log = File.CreateText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log.txt"));

        public Updater(string updateeFilePath, string packageContentDirPath, bool restartUpdatee, string routedArgs)
        {
            _updateeFilePath = updateeFilePath;
            _packageContentDirPath = packageContentDirPath;
            _restartUpdatee = restartUpdatee;
            _routedArgs = routedArgs;
        }

        public static void WriteLog(string content)
        {
            var date = DateTimeOffset.Now;
            _log.WriteLine($"{date:dd-MMM-yyyy HH:mm:ss.fff}> {content}");
            _log.Flush();
        }

        private void RunCore()
        {
            var updateeDirPath = Path.GetDirectoryName(_updateeFilePath);

            // Wait until updatee is writable to ensure all running instances have exited
            WriteLog("Waiting for all running updatee instances to exit...");
            while (!FileEx.CheckWriteAccess(_updateeFilePath))
                Thread.Sleep(100);

            // Copy over the package contents
            WriteLog("Copying package contents from storage to updatee's directory...");
            DirectoryEx.Copy(_packageContentDirPath, updateeDirPath);

            // Restart updatee if requested
            if (_restartUpdatee)
            {
                if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    var startInfo = new ProcessStartInfo
                    {
                        WorkingDirectory = updateeDirPath,
                        Arguments = _routedArgs,
                        UseShellExecute = true // avoid sharing console window with updatee
                    };

                    // If updatee is an .exe file - start it directly
                    if (string.Equals(Path.GetExtension(_updateeFilePath), ".exe", StringComparison.OrdinalIgnoreCase))
                    {
                        startInfo.FileName = _updateeFilePath;
                    }
                    // If not - figure out what to do with it
                    else
                    {
                        // If there's an .exe file with same name - start it instead
                        // Security vulnerability?
                        if (File.Exists(Path.ChangeExtension(_updateeFilePath, ".exe")))
                        {
                            startInfo.FileName = Path.ChangeExtension(_updateeFilePath, ".exe");
                        }
                        // Otherwise - start the updatee using dotnet SDK
                        else
                        {
                            startInfo.FileName = "dotnet";
                            startInfo.Arguments = $"{_updateeFilePath} {_routedArgs}";
                        }
                    }

                    WriteLog($"Restarting updatee [{startInfo.FileName} {startInfo.Arguments}]...");

                    using var restartedUpdateeProcess = Process.Start(startInfo);
                    WriteLog($"Restarted as pid:{restartedUpdateeProcess?.Id}.");
                }
                else if(RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    var startInfo = new ProcessStartInfo
                    {
                        WorkingDirectory = updateeDirPath,
                        Arguments = _routedArgs,
                        UseShellExecute = true // avoid sharing console window with updatee
                    };
                    
                    startInfo.FileName = "dotnet";
                    startInfo.Arguments = $"{_updateeFilePath} {_routedArgs}";
                    WriteLog($"Restarting updatee [{startInfo.FileName} {startInfo.Arguments}]...");

                    using var restartedUpdateeProcess = Process.Start(startInfo);
                    WriteLog($"Restarted as pid:{restartedUpdateeProcess?.Id}.");

                }
                else
                {
                    throw new Exception("platform unknown");
                }
               
            }

            // Delete package content directory
            WriteLog("Deleting package contents from storage...");
            Directory.Delete(_packageContentDirPath, true);
        }

        public void Run()
        {
            var updaterVersion = Assembly.GetExecutingAssembly().GetName().Version;
            WriteLog(
                $"Onova Updater v{updaterVersion} started with the following arguments:" + Environment.NewLine +
                $"  UpdateeFilePath = {_updateeFilePath}" + Environment.NewLine +
                $"  PackageContentDirPath = {_packageContentDirPath}" + Environment.NewLine +
                $"  RestartUpdatee = {_restartUpdatee}" + Environment.NewLine +
                $"  RoutedArgs = {_routedArgs}");

            try
            {
                RunCore();
            }
            catch (Exception ex)
            {
                WriteLog(ex.ToString());
            }
        }

        public void Dispose() => _log.Dispose();
    }
}