using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
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

        public static void ElevateProcess(string[] args = null)
        {
            //Public domain; no attribution required.
            const int ERROR_CANCELLED = 1223; //The operation was canceled by the user.

            var info = new System.Diagnostics.ProcessStartInfo
            {
                UseShellExecute = true,
                WorkingDirectory = Environment.CurrentDirectory,
                FileName = Application.ExecutablePath,
                Verb = "runas",
            };

            if (args != null) info.Arguments = string.Join(" ", args);

            try
            {
                Process.Start(info);
                System.Windows.Forms.Application.ExitThread();
            }
            catch (Win32Exception ex)
            {
                if (ex.NativeErrorCode == ERROR_CANCELLED)
                    MessageBox.Show("Why you no select Yes?");
                else
                    throw;
            }
        }
        public static Boolean WaitForFile(String filePath)
        {
            Int32 tries = 0;
            int MaxAttempts = 10;

            while (true)
            {
                ++tries;
                Boolean wait = false;
                FileStream stream = null;

                try
                {
                    stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
                    break;
                }
                catch (Exception ex)
                {
                    Log("Failed to get an exclusive lock on " + filePath + ": " + ex.ToString());

                    if (tries > MaxAttempts)
                    {
                        Log("Skipped the file " + filePath + " after " + MaxAttempts.ToString() + " tries.");
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

            Log("Got an exclusive lock on " + filePath + " after " + tries.ToString() + " tries.");

            return true;
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
                string message = string.Format("Exception while accessing key {0}: {1} {2} {3}", _AppPath, AppPathStr, ex.ToString(), ex.Message);
                Log(message);
                throw;
            }

            return AppPath;
        }
    }
}
