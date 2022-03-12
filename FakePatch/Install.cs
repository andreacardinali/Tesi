using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using System.Windows.Forms;
using static FakePatch.Globals;
using static FakePatch.LogHelper;

namespace FakePatch
{
    public class Install
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

        public static void StartService(string ServiceName = gServiceName)
        {
            try
            {
                ServiceController sc = ServiceController.GetServices().FirstOrDefault(s => s.ServiceName == ServiceName);

                if (sc == null)
                {
                    Log("Service " + ServiceName + " is not installed");
                }
                else
                {
                    //Starts the service
                    if ((sc.Status.Equals(ServiceControllerStatus.Stopped)) || (sc.Status.Equals(ServiceControllerStatus.StopPending)))
                    {
                        {
                            Log("Service " + ServiceName + " is starting");
                            sc.Start();
                            sc.WaitForStatus(ServiceControllerStatus.Running);
                        }
                    }
                    else
                    {
                        Log("Service " + ServiceName + " is in status " + sc.Status);
                    }
                }
            }
            catch (Exception ex)
            {
                string message = String.Format("Exception occurred: \n {0} \n {1} ", ex.Message, ex.ToString());
                Log(message);
            }
        }

        public static void StopService(string ServiceName = gServiceName)
        {
            try
            {
                ServiceController sc = ServiceController.GetServices().FirstOrDefault(s => s.ServiceName == ServiceName);

                if (sc == null)
                {
                    Log("Service " + ServiceName + " is not installed");
                }
                else
                {
                    //Stops the service
                    if ((sc.Status.Equals(ServiceControllerStatus.Running)) || (sc.Status.Equals(ServiceControllerStatus.StartPending)))
                    {
                        {
                            Log("Service " + ServiceName + " is stopping");
                            sc.Stop();
                            sc.WaitForStatus(ServiceControllerStatus.Stopped);
                        }
                    }
                    else
                    {
                        Log("Service " + ServiceName + " is in status " + sc.Status);
                    }
                }
            }
            catch (Exception ex)
            {
                string message = String.Format("Exception occurred: \n {0} \n {1} ", ex.Message, ex.ToString());
                Log(message);
            }
        }

        public static void InstallService(string ServiceName = gServiceName)
        {
            try
            {
                ServiceController sc = ServiceController.GetServices().FirstOrDefault(s => s.ServiceName == ServiceName);

                if (sc == null)
                {
                    Log("Service " + ServiceName + " is being installed");
                    string WorkingDir = Path.GetDirectoryName(Application.ExecutablePath);
                    CopyFileExactly(Application.ExecutablePath, gServiceExecutablePath.FullName, true);
                    FileInfo OrigPdbFile = new FileInfo(Path.Combine(WorkingDir, Path.ChangeExtension(Application.ExecutablePath, "pdb")));
                    FileInfo DestPdbFile = new FileInfo(Path.Combine(gServicePath, Path.ChangeExtension(gServiceExecutablePath.FullName, "pdb")));
                    if (OrigPdbFile.Exists)
                    {
                        Log("Copying debug file " + OrigPdbFile.FullName + " to " + DestPdbFile.FullName);
                        CopyFileExactly(OrigPdbFile.FullName, DestPdbFile.FullName, true);
                    }
                    ManagedInstallerClass.InstallHelper(new string[] { "/LogFile=", "/LogToConsole=true", gServiceExecutablePath.FullName });
                    sc = ServiceController.GetServices().FirstOrDefault(s => s.ServiceName == ServiceName);
                    sc.WaitForStatus(ServiceControllerStatus.Stopped);
                }
                else
                {
                    Log("Service " + ServiceName + " is already installed");
                }
            }
            catch (Exception ex)
            {
                string message = String.Format("Exception occurred: \n {0} \n {1} ", ex.Message, ex.ToString());
                Log(message);
            }
        }
        public static void UninstallService(string ServiceName = gServiceName)
        {
            try
            {
                Log("Service " + ServiceName + " is being uninstalled");
                ManagedInstallerClass.InstallHelper(new string[] { "/u", "/LogFile=", "/LogToConsole=true", gServiceExecutablePath.FullName });
            }
            catch (Exception ex)
            {
                string message = String.Format("Exception occurred: \n {0} \n {1} ", ex.Message, ex.ToString());
                Log(message);
            }
        }
        public static void InstallPatch(string FilePath)
        {
            FileInfo AppPath = FindAppPath(FilePath);
            if (AppPath != null)
            {
                if (AppPath.Exists)
                {
                    Crypto MyCrypto = new Crypto();
                    string WorkingDir = AppPath.DirectoryName;
                    try
                    {
                        FileInfo EncryptedAppPath = new FileInfo(Path.Combine(WorkingDir, Path.ChangeExtension(AppPath.Name, Path.GetExtension(AppPath.Name) + ".enc")));
                        Log(EncryptedAppPath.FullName);

                        if (!EncryptedAppPath.Exists)
                        {
                            if (gRSA == null)
                            {
                                gRSA = MyCrypto.ImportAsimKeysFromXml(gKeyName, gEncryptKeyXML);
                            }

                            MyCrypto.EncryptFile(AppPath, gRSA, WorkingDir, gAes);

                            if (File.Exists(EncryptedAppPath.FullName))
                            {
                                //************************************************************
                                //backup original file -> only for debug
                                /*
                                FileInfo BckFile = new FileInfo(Path.Combine(WorkingDir, Path.ChangeExtension(AppPath.Name, Path.GetExtension(AppPath.Name) + "_bck.exe")));
                                if (BckFile.Exists) { BckFile.Delete(); }
                                File.Move(AppPath.FullName, BckFile.FullName);
                                */
                                //************************************************************
                                //
                                // copy this assembly in place of the original application executable
                                CopyFileExactly(Application.ExecutablePath, AppPath.FullName, true);
                                FileInfo OrigPdbFile = new FileInfo(Path.ChangeExtension(Application.ExecutablePath, "pdb"));
                                FileInfo DestPdbFile = new FileInfo(Path.Combine(WorkingDir, Assembly.GetCallingAssembly().GetName().Name + ".pdb"));
                                if (OrigPdbFile.Exists)
                                {
                                    if (DestPdbFile.Exists) { DestPdbFile.Delete(); }
                                    Log("Copying debug file " + OrigPdbFile.FullName + " to " + DestPdbFile.FullName);
                                    CopyFileExactly(OrigPdbFile.FullName, DestPdbFile.FullName, true);
                                }
                            }
                        }
                        else
                        {
                            Log(EncryptedAppPath.FullName + " already exists. Skipping.");
                        }
                    }
                    catch (Exception ex)
                    {
                        string message = String.Format("Exception occurred: \n {0} \n {1} ", ex.Message, ex.ToString());
                        Log(message);
                        throw;
                    }
                }
                else
                {
                    Log(AppPath.FullName + " not found.");
                }
            }
            else
            {
                Log(FilePath + " not found in registry.");
            }
        }
        public static void UninstallPatch(string FilePath, string Key = null)
        {

            Crypto MyCrypto = new Crypto();
            FileInfo AppPath = FindAppPath(FilePath);
            Log(FilePath);
            Log(AppPath.FullName);
            if (AppPath != null)
            {
                try
                {
                    string WorkingDir = AppPath.DirectoryName;

                    FileInfo EncryptedAppPath = MyCrypto.GetEncryptedFilePath(AppPath);
                    Log(EncryptedAppPath.FullName);

                    if (EncryptedAppPath.Exists)
                    {
                        if (Key == null)
                        {
                            MyCrypto.DecryptFile(EncryptedAppPath, gRSA, null, WorkingDir);
                        }
                        else
                        {
                            MyCrypto.DecryptFile(EncryptedAppPath, null, Key, WorkingDir);
                        }
                        EncryptedAppPath.Delete();
                        //************************************************************
                        //restore
                        /*
                        FileInfo BckFile = new FileInfo(Path.Combine(WorkingDir, Path.ChangeExtension(AppPath.Name, Path.GetExtension(AppPath.Name) + "_bck.exe")));
                        if (BckFile.Exists)
                        {
                            Log("Restoring " + BckFile.FullName + " to " + AppPath.FullName);
                            if (File.Exists(AppPath.FullName)) { AppPath.Delete(); }
                            File.Move(BckFile.FullName, AppPath.FullName);
                        }
                        */
                        //************************************************************
                    }
                }
                catch (Exception ex)
                {
                    string message = String.Format("Exception occurred: \n {0} \n {1} ", ex.Message, ex.ToString());
                    Log(message);
                    throw;
                }
            }
            else
            {
                Log(FilePath + " not found.");
            }
        }
    }
}