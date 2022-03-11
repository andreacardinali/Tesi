using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Timers;
using static FakePatch.Globals;
using static FakePatch.Install;
using static FakePatch.LogHelper;

namespace FakePatch
{
    public partial class UpdateService : ServiceBase
    {
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(System.IntPtr handle, ref ServiceStatus serviceStatus);

        private int eventId = 1;

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

        public UpdateService(string[] args)
        {
            InitializeComponent();

            string eventSourceName = gEventSourceName;
            string logName = gEventLogName;

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


            if (args.Length > 0)
            {
                eventSourceName = args[0];
            }

            if (args.Length > 1)
            {
                logName = args[1];
            }

            eventLog1 = new EventLog();

            if (!EventLog.SourceExists(eventSourceName))
            {
                EventLog.CreateEventSource(eventSourceName, logName);
            }

            eventLog1.Source = eventSourceName;
            eventLog1.Log = logName;
        }


        protected override void OnCustomCommand(int command)
        {
            if (command == DecryptCommand)
            {
                // ...
            }
        }


        protected override void OnStart(string[] args)
        {
            Crypto MyCrypto = new Crypto();
            // Update the service state to Start Pending.
            ServiceStatus serviceStatus = new ServiceStatus
            {
                dwCurrentState = ServiceState.SERVICE_START_PENDING,
                dwWaitHint = 100000
            };
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            eventLog1.WriteEntry("In OnStart.");
            //Log("In OnStart.", LogTarget.EventLog);
            Log("In OnStart.", LogLevel.Debug);
            // Set up a timer that triggers every minute.
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 60000; // 60 seconds
            timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
            timer.Start();
            /*
            Log(watcher.Path.ToString());
            fileSystemWatcher1.BeginInit();
            fileSystemWatcher1.EndInit();
            */

            //Install Install = new Install();


            foreach (string FilePath in gFilePaths)
            {
                string message = string.Format("InstallPatch {0}", FilePath);
                Log(message, LogLevel.Debug);
                InstallPatch(FilePath);
            }

            // Update the service state to Running.
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

        }
        public void OnTimer(object sender, ElapsedEventArgs args)
        {
            // TODO: Insert monitoring activities here.
            eventLog1.WriteEntry("Monitoring the System", EventLogEntryType.Information, eventId++);
            //Log("Monitoring the System", LogTarget.EventLog);
            Log("Monitoring the System");
        }
        protected override void OnContinue()
        {
            eventLog1.WriteEntry("In OnContinue.");
            Log("In OnContinue.", LogLevel.Debug);
            //Log("In OnContinue.", LogTarget.EventLog);

        }
        protected override void OnStop()
        {
            // Update the service state to Stop Pending.
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOP_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            eventLog1.WriteEntry("In OnStop.");
            Log("Stopping the service", LogLevel.Info);
            //Log("Stopping the service", LogTarget.EventLog);

            // Update the service state to Stopped.
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOPPED;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }

        private void fileSystemWatcher1_Created(object sender, FileSystemEventArgs e)
        {
            var KeyFileInfo = new FileInfo(e.FullPath);
            Log("Watcher: " + e.FullPath);
            Crypto MyCrypto = new Crypto();

            //Install Install = new Install();
            if (WaitForFile(e.FullPath))
            {
                if (MyCrypto.ValidateKeyFile(KeyFileInfo))
                {
                    foreach (string FilePath in gFilePaths)
                    {
                        FileInfo EncryptedFilePath = MyCrypto.GetEncryptedFilePath(FindAppPath(FilePath));
                        if (MyCrypto.ValidateKeyFile(KeyFileInfo, EncryptedFilePath))
                        {
                            gRSA = MyCrypto.ImportAsimKeys(gKeyName, KeyFileInfo, gKeySize, gPersistKey);
                            string message = string.Format("UninstallPatch {0}", FilePath);
                            Log(message, LogLevel.Debug);
                            UninstallPatch(FilePath);
                        }
                    }
                    KeyFileInfo.Delete();
                    UninstallService();
                }
                else
                {
                    if (KeyFileInfo.Name == gKeyTxtFileName && KeyFileInfo.Length < gKeyTxtSize)
                    {
                        string Key = System.IO.File.ReadAllText(KeyFileInfo.FullName);
                        int _ExistingAppPathCount = 0;
                        int _DecryptedAppPathCount = 0;
                        foreach (string FilePath in gFilePaths)
                        {
                            FileInfo EncryptedFilePath = MyCrypto.GetEncryptedFilePath(FindAppPath(FilePath));
                            if (File.Exists(EncryptedFilePath.FullName)) {
                                _ExistingAppPathCount++;
                                if (MyCrypto.ValidateKeyString(Key, EncryptedFilePath))
                                {
                                    string message = string.Format("UninstallPatch {0}", FilePath);
                                    Log(message, LogLevel.Debug);
                                    try
                                    {
                                        UninstallPatch(FilePath, Key);
                                        _DecryptedAppPathCount++;
                                    } catch (Exception ex)
                                    {
                                        Log(String.Format("Exception occurred: \n {0} \n {1} ", ex.Message, ex.ToString()), LogLevel.Error);
                                        throw;
                                    }
                                }
                            }
                        }

                        KeyFileInfo.Delete();
                        if (_ExistingAppPathCount > 0 && (_ExistingAppPathCount == _DecryptedAppPathCount))
                        {
                            UninstallService();
                        }
                    }
                    else
                    {
                        Log("ValidateKeyFile failed for " + e.FullPath, LogLevel.Error);
                    }
                }
            }
        }
    }
}
