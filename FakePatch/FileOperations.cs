using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using static FakePatch.LogHelper;

namespace FakePatch
{
    public class FileOperations
    {
        public static void CopyFileExactly(string copyFromPath, string copyToPath, bool overwrite = true)
        {
            var origin = new FileInfo(copyFromPath);
            var destination = new FileInfo(copyToPath);

            if (!Directory.Exists(destination.DirectoryName))
            {
                Directory.CreateDirectory(destination.DirectoryName);
            }

            origin.CopyTo(copyToPath, overwrite);

            destination.CreationTime = origin.CreationTime;
            destination.LastWriteTime = origin.LastWriteTime;
            destination.LastAccessTime = origin.LastAccessTime;
        }

        public static bool WaitForFile(string filePath)
        {
            int tries = 0;
            int MaxAttempts = 10;

            while (true)
            {
                ++tries;
                bool wait = false;
                FileStream stream = null;

                try
                {
                    stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
                    break;
                }
                catch (Exception ex)
                {
                    Log("[WaitForFile] Failed to get an exclusive lock on " + filePath + ": " + ex.ToString());

                    if (tries > MaxAttempts)
                    {
                        Log("[WaitForFile] Skipped the file " + filePath + " after " + MaxAttempts.ToString() + " tries.");
                        return false;
                    }

                    wait = true;
                }
                finally
                {
                    if (stream != null)
                        stream.Close();
                }

                if (wait)
                    Thread.Sleep(250);
            }
            Log("[WaitForFile] Got an exclusive lock on " + filePath + " after " + tries.ToString() + " tries.");
            return true;
        }

        public static bool KillProcess(string ProcessToKill)
        {
            try
            {
                Log("[KillProcess] Looking for running instances of " + ProcessToKill + "...", LogLevel.Debug);
                Process[] Processes = Process.GetProcesses();

                var ProcessesToKill = new List<Process>();
                for (int i = 0; i < Processes.Length; i++)
                {
                    try
                    {
                        if (Processes[i].MainModule.FileName.ToString() == ProcessToKill)
                        {
                            Log("[KillProcess] Found PID " + Processes[i].Id + " running: " + Processes[i].MainModule.FileName.ToString(), LogLevel.Debug);
                            ProcessesToKill.Add(Processes[i]);
                        }
                    }
                    catch { }
                }

                foreach (var process in ProcessesToKill)
                {
                    int ProcessId = process.Id;
                    int tries = 0;
                    int MaxAttempts = 10;
                    Log("[KillProcess] Trying to kill PID " + ProcessId.ToString() + "...", LogLevel.Debug);
                    try
                    {
                        process.Kill();
                    }
                    catch { }
                    while (true)
                    {
                        ++tries;
                        bool wait = false;
                        // Makes sure the process has terminated
                        if (!process.HasExited)
                        {
                            wait = true;
                            if (tries > MaxAttempts)
                            {
                                Log("[KillProcess] Cannot kill PID " + ProcessId.ToString() + " after " + MaxAttempts.ToString() + " tries.");
                                return false;
                            }
                        }
                        else
                        {
                            Log("[KillProcess] Process PID " + ProcessId.ToString() + " was killed successfully.", LogLevel.Debug);
                            break;
                        }
                        if (wait)
                            Thread.Sleep(250);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                string message = String.Format("[KillProcess] Exception occurred: \n {0} \n {1} ", ex.Message, ex.ToString());
                Log(message, LogLevel.Error);
                return false;
            }
        }


        public static FileInfo FindAppPath(string ExeName)
        {
            FileInfo AppPath = null;
            string _AppPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + ExeName;
            string AppPathStr = null;
            try
            {
                RegistryKey key = Registry.LocalMachine;
                RegistryKey AppPathKey = key.OpenSubKey(_AppPath);

                if (AppPathKey != null)
                {
                    AppPathStr = ((string)AppPathKey.GetValue(null, "")).Replace("\"", "");
                    AppPath = new FileInfo(AppPathStr);
                }
            }
            catch (Exception ex)
            {
                string message = string.Format("[FindAppPath] Exception while accessing key {0}: {1} {2} {3}", _AppPath, AppPathStr, ex.ToString(), ex.Message);
                Log(message, LogLevel.Error);
                throw;
            }

            return AppPath;
        }
    }
}
