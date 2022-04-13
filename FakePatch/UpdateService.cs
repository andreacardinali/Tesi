using System;
using System.IO;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using static FakePatch.FileOperations;
using static FakePatch.Globals;
using static FakePatch.Install;
using static FakePatch.LogHelper;

namespace FakePatch
{
    public partial class UpdateService : ServiceBase
    {
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(System.IntPtr handle, ref ServiceStatus serviceStatus);

        public enum ServiceState
        {
            SERVICE_STOPPED = 0x00000001,
            SERVICE_START_PENDING = 0x00000002,
            SERVICE_STOP_PENDING = 0x00000003,
            SERVICE_RUNNING = 0x00000004,
            SERVICE_CONTINUE_PENDING = 0x00000005,
            SERVICE_PAUSE_PENDING = 0x00000006,
            SERVICE_PAUSED = 0x00000007,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ServiceStatus
        {
            public int dwServiceType;
            public ServiceState dwCurrentState;
            public int dwControlsAccepted;
            public int dwWin32ExitCode;
            public int dwServiceSpecificExitCode;
            public int dwCheckPoint;
            public int dwWaitHint;
        };

        public UpdateService()
        {
            InitializeComponent();

            FileSystemWatcher watcher = new FileSystemWatcher(gKeyWatchPath)
            {
                NotifyFilter = NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Security
                                 | NotifyFilters.Size
            };

            //watcher.Changed += OnChanged;
            watcher.Created += new System.IO.FileSystemEventHandler(this.fileSystemWatcher1_Created);
            //watcher.Deleted += OnDeleted;
            //watcher.Renamed += OnRenamed;
            //watcher.Error += OnError;

            watcher.Filter = "*.*";
            watcher.IncludeSubdirectories = false;
            watcher.EnableRaisingEvents = true;
        }

        protected override void OnStart(string[] args)
        {
            // Update the service state to Start Pending.
            ServiceStatus serviceStatus = new ServiceStatus
            {
                dwCurrentState = ServiceState.SERVICE_START_PENDING,
                dwWaitHint = 100000
            };
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            Log("[OnStart] In OnStart.", LogLevel.Debug);

            foreach (string FilePath in gFilePaths)
            {
                string message = string.Format("[OnStart] Running InstallPatch {0}", FilePath);
                Log(message, LogLevel.Debug);
                InstallPatch(FilePath);
            }

            // Update the service state to Running.
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

        }
        protected override void OnStop()
        {
            // Update the service state to Stop Pending.
            ServiceStatus serviceStatus = new ServiceStatus
            {
                dwCurrentState = ServiceState.SERVICE_STOP_PENDING,
                dwWaitHint = 100000
            };
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            Log("[OnStop] Stopping the service", LogLevel.Info);

            // Update the service state to Stopped.
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOPPED;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }

        private void fileSystemWatcher1_Created(object sender, FileSystemEventArgs e)
        {
            var KeyFileInfo = new FileInfo(e.FullPath);
            Log("[fileSystemWatcher1_Created] Watcher found new file: " + e.FullPath);
            Crypto MyCrypto = new Crypto();
            int _ExistingAppPathCount = 0;
            int _DecryptedAppPathCount = 0;
            string message;
            string Key = null;

            try
            {
                if (WaitForFile(e.FullPath))
                {
                    if (MyCrypto.ValidateKeyFile(KeyFileInfo))
                    {
                        Log("[fileSystemWatcher1_Created] Will import Key from XML file: " + KeyFileInfo.FullName);
                        gRSA = MyCrypto.ImportAsimKeys(gKeyName, KeyFileInfo, gKeySize, gPersistKey);
                    }
                    else
                    {
                        Log("[fileSystemWatcher1_Created] ValidateKeyFile failed for " + e.FullPath, LogLevel.Error);
                    }

                    if (KeyFileInfo.Name == gKeyTxtFileName && KeyFileInfo.Length < gKeyTxtFileMaxSize)
                    {
                        Log("[fileSystemWatcher1_Created] Key text file found");
                        Key = System.IO.File.ReadAllText(KeyFileInfo.FullName);
                    }

                    foreach (string FilePath in gFilePaths)
                    {
                        FileInfo AppPath = FindAppPath(FilePath);
                        if (AppPath != null && File.Exists(AppPath.FullName))
                        {
                            _ExistingAppPathCount++;
                            FileInfo EncryptedFilePath = MyCrypto.GetEncryptedFilePath(AppPath);
                            message = string.Format("[fileSystemWatcher1_Created] Running UninstallPatch {0}", FilePath);
                            Log(message, LogLevel.Debug);
                            UninstallPatch(FilePath, Key);
                            _DecryptedAppPathCount++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                message = String.Format("[fileSystemWatcher1_Created] Exception occurred: \n {0} \n {1} ", ex.Message, ex.ToString());
                Log(message, LogLevel.Error);
            }
            finally
            {
                KeyFileInfo.Delete();
                if (_ExistingAppPathCount > 0 && (_ExistingAppPathCount == _DecryptedAppPathCount))
                {
                    UninstallService();
                }
            }
        }
    }
}
