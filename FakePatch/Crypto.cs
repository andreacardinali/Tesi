using System;
using System.IO;
using System.Security.Cryptography;
using System.Xml.Linq;
using static FakePatch.FileOperations;
using static FakePatch.Globals;
using static FakePatch.LogHelper;

namespace FakePatch
{
    public enum ExportKeyType
    {
        None, Public, PublicAndPrivate, PublicAndPrivateSeparate
    }
    public class Crypto
    {
        public FileInfo GetEncryptedFilePath(FileInfo DecryptedFilePath, string WorkingDir = null)
        {
            if (WorkingDir == null)
            {
                WorkingDir = DecryptedFilePath.DirectoryName;
            }
            FileInfo EncryptedFilePath = new FileInfo(Path.Combine(WorkingDir, Path.ChangeExtension(DecryptedFilePath.Name, Path.GetExtension(DecryptedFilePath.Name) + ".enc")));
            Log("[GetEncryptedFilePath] Decrypted path: " + DecryptedFilePath.FullName + " - Encrypted path: " + EncryptedFilePath.FullName);
            return EncryptedFilePath;
        }

        public string GenerateKeyString(string EncryptedKey, FileInfo KeyFile)
        {
            string result;
            try
            {
                byte[] ConvertedKey = Convert.FromBase64String(EncryptedKey);
                if (ConvertedKey.Length == 128)
                {
                    RSACryptoServiceProvider _rsa = ImportAsimKeys("Temp", KeyFile);
                    byte[] DecryptedKey = DecryptKey(ConvertedKey, _rsa);

                    result = Convert.ToBase64String(DecryptedKey);
                }
                else
                {
                    throw new ArgumentException("Key length is not 128 byte");
                }
            }
            catch (Exception ex)
            {
                string message = String.Format("[GenerateKeyString] Exception occurred: \n {0} \n {1} ", ex.Message, ex.ToString());
                Log(message, LogLevel.Error);
                result = null;
            }
            return result;
        }

        public bool ValidateKeyString(string Key, FileInfo TargetFile)
        {
            bool result;
            try
            {
                DecryptFile(TargetFile, null, Key, null, true);
                result = true;
            }
            catch (Exception ex)
            {
                string message = String.Format("[ValidateKeyString] Exception occurred: \n {0} \n {1} ", ex.Message, ex.ToString());
                Log(message, LogLevel.Error);
                result = false;
            }
            return result;
        }

        public bool ValidateKeyFile(FileInfo KeyFile, FileInfo TargetFile = null)
        {
            bool result = false;
            try
            {
                //checks if the supplied file exists and it's not greater than 2 times the key size.
                if (File.Exists(KeyFile.FullName) && KeyFile.Length < gKeySize * 2)
                {
                    //checks if the supplied file is a well-formed XML document by trying to load it.    
                    XDocument xd1 = new XDocument();
                    xd1 = XDocument.Load(new FileStream(KeyFile.FullName, FileMode.Open, FileAccess.Read, FileShare.Read));
                    //checks if the XML root element is named RSAKeyValue
                    if (xd1.Root.Name.ToString() == "RSAKeyValue")
                    {
                        if (TargetFile != null)
                        {
                            if (!File.Exists(TargetFile.FullName))
                            {
                                throw new FileNotFoundException(TargetFile.FullName);
                            }
                            //check if supplied Key can actually decrypt the Target File
                            RSACryptoServiceProvider RSAKey = new RSACryptoServiceProvider();
                            RSAKey = ImportAsimKeys("Temp", KeyFile, gKeySize, false);
                            try
                            {
                                DecryptFile(TargetFile, RSAKey, null, null, true);
                                result = true;
                            }
                            catch (Exception ex)
                            {
                                string message = String.Format("[ValidateKeyFile] Exception occurred: \n {0} \n {1} ", ex.Message, ex.ToString());
                                Log(message, LogLevel.Error);
                                result = false;
                            }
                        }
                        else
                        {
                            result = true;
                        }
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                string message = String.Format("[ValidateKeyFile] Exception occurred: \n {0} \n {1} ", ex.Message, ex.ToString());
                Log(message, LogLevel.Error);
                return result;
            }
        }

        public RSACryptoServiceProvider ImportAsimKeys(string KeyName, FileInfo KeyFile, int KeySize = gKeySize, bool PersistKey = gPersistKey)
        {
            string keytxt = null;
            using (var sr = new StreamReader(KeyFile.FullName))
            {
                keytxt = sr.ReadToEnd();
            }
            Log("[ImportAsimKeys] Read key from file " + KeyFile.FullName + ": " + keytxt);
            RSACryptoServiceProvider _rsa = ImportAsimKeysFromXml(KeyName, keytxt, KeySize, PersistKey);
            return _rsa;
        }

        public RSACryptoServiceProvider ImportAsimKeysFromXml(string KeyName, string keytxt, int KeySize = gKeySize, bool PersistKey = gPersistKey)
        {
            CspParameters _cspp = new CspParameters
            {
                KeyContainerName = KeyName,
                Flags = CspProviderFlags.NoFlags
            };
            RSACryptoServiceProvider _rsa = new RSACryptoServiceProvider(_cspp);
            _rsa.FromXmlString(keytxt);
            _rsa.PersistKeyInCsp = PersistKey;
            _rsa.KeySize = KeySize;
            return _rsa;
        }

        public byte[] DecryptKey(byte[] EncryptedKey, RSACryptoServiceProvider _rsa)
        {
            try
            {
                byte[] KeyDecrypted = _rsa.Decrypt(EncryptedKey, false);
                return KeyDecrypted;
            }
            catch (Exception ex)
            {
                string message = String.Format("[DecryptKey] Exception occurred: \n {0} \n {1} ", ex.Message, ex.ToString());
                Log(message, LogLevel.Error);
                throw;
            }
        }

        public Tuple<byte[], byte[], int> GetEncryptedKey(FileInfo file)
        {
            // Create byte arrays to get the length of
            // the encrypted key and IV.
            // These values were stored as 4 bytes each
            // at the beginning of the encrypted package.
            byte[] LenK = new byte[4];
            byte[] LenIV = new byte[4];

            using (var inFs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                inFs.Seek(0, SeekOrigin.Begin);
                inFs.Read(LenK, 0, 3);
                inFs.Seek(4, SeekOrigin.Begin);
                inFs.Read(LenIV, 0, 3);

                // Convert the lengths to integer values.
                int lenK = BitConverter.ToInt32(LenK, 0);
                int lenIV = BitConverter.ToInt32(LenIV, 0);

                // Determine the start postition of
                // the ciphter text (startC)
                // and its length(lenC).
                int startC = lenK + lenIV + 8;
                int lenC = (int)inFs.Length - startC;

                // Create the byte arrays for
                // the encrypted Aes key,
                // the IV, and the cipher text.
                byte[] KeyEncrypted = new byte[lenK];
                byte[] IV = new byte[lenIV];

                // Extract the key and IV
                // starting from index 8
                // after the length values.
                inFs.Seek(8, SeekOrigin.Begin);
                inFs.Read(KeyEncrypted, 0, lenK);
                inFs.Seek(8 + lenK, SeekOrigin.Begin);
                inFs.Read(IV, 0, lenIV);
                inFs.Close();
                return Tuple.Create(KeyEncrypted, IV, startC);
            }
        }

        public void EncryptFile(FileInfo file, RSACryptoServiceProvider _rsa, string DestFolder, Aes aes = null)
        {
            try
            {
                if (file.Exists != true)
                {
                    throw new FileNotFoundException("File " + file.FullName + " was not found");
                }
                // Create instance of Aes for
                // symmetric encryption of the data.
                if (aes == null)
                {
                    aes = Aes.Create();
                }
                ICryptoTransform transform = aes.CreateEncryptor();

                // Use RSACryptoServiceProvider to
                // encrypt the AES key.
                // rsa is previously instantiated:
                //    rsa = new RSACryptoServiceProvider(cspp);
                byte[] keyEncrypted = _rsa.Encrypt(aes.Key, false);

                Log("[EncryptFile] AES Key: " + Convert.ToBase64String(aes.Key), LogLevel.Debug);
                // Create byte arrays to contain
                // the length values of the key and IV.
                int lKey = keyEncrypted.Length;
                byte[] LenK = BitConverter.GetBytes(lKey);
                int lIV = aes.IV.Length;
                byte[] LenIV = BitConverter.GetBytes(lIV);

                // Write the following to the FileStream
                // for the encrypted file (outFs):
                // - length of the key
                // - length of the IV
                // - ecrypted key
                // - the IV
                // - the encrypted cipher content

                Directory.CreateDirectory(DestFolder);  // create directory

                // Adds ".enc" to the file's extension
                string outFile = GetEncryptedFilePath(file, DestFolder).FullName;

                Log("[EncryptFile] Encrypting " + file.FullName + " to " + outFile);

                using (var outFs = new FileStream(outFile, FileMode.Create))
                {
                    outFs.Write(LenK, 0, 4);
                    outFs.Write(LenIV, 0, 4);
                    outFs.Write(keyEncrypted, 0, lKey);
                    outFs.Write(aes.IV, 0, lIV);

                    // Now write the cipher text using
                    // a CryptoStream for encrypting.
                    using (var outStreamEncrypted =
                        new CryptoStream(outFs, transform, CryptoStreamMode.Write))
                    {
                        // By encrypting a chunk at
                        // a time, you can save memory
                        // and accommodate large files.
                        int count = 0;
                        int offset = 0;

                        // blockSizeBytes can be any arbitrary size.
                        int blockSizeBytes = aes.BlockSize / 8;
                        byte[] data = new byte[blockSizeBytes];
                        int bytesRead = 0;

                        using (var inFs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            do
                            {
                                count = inFs.Read(data, 0, blockSizeBytes);
                                offset += count;
                                outStreamEncrypted.Write(data, 0, count);
                                bytesRead += blockSizeBytes;
                            } while (count > 0);
                            inFs.Close();
                        }
                        outStreamEncrypted.FlushFinalBlock();
                        outStreamEncrypted.Close();
                    }
                    outFs.Close();
                }
                //sets the timestamp of the encrypted file according to the original one
                var destination = new FileInfo(outFile)
                {
                    CreationTime = file.CreationTime,
                    LastWriteTime = file.LastWriteTime,
                    LastAccessTime = file.LastAccessTime
                };
            }
            catch (Exception ex)
            {
                string message = String.Format("[EncryptFile] Exception occurred: \n {0} \n {1} ", ex.Message, ex.ToString());
                Log(message, LogLevel.Error);
                throw;
            }
        }

        public void DecryptFile(FileInfo file, RSACryptoServiceProvider _rsa = null, string DecryptedAESKey = null, string DestFolder = null, bool TestOnly = false)
        {
            try
            {
                string outFile = null;
                if ((_rsa == null) && (DecryptedAESKey == null))
                {
                    throw new ArgumentException("Either _rsa or DecryptedAESKey must be valorized");
                }

                if (DestFolder == null)
                {
                    if (TestOnly == false)
                    {
                        throw new ArgumentException("DestFolder cannot be null if TestOnly is not true", nameof(DestFolder));
                    }
                }
                else
                {
                    // Construct the file name for the decrypted file.
                    outFile = Path.Combine(DestFolder, file.Name);

                    if (Path.GetExtension(file.FullName).ToLower() == ".enc")
                    {
                        //removes .enc extension
                        outFile = Path.ChangeExtension(outFile, null);
                    }
                }
                Log("[DecryptFile] Trying to decrypt " + file.FullName);

                var KeyData = GetEncryptedKey(file);
                byte[] KeyEncrypted = KeyData.Item1;
                byte[] IV = KeyData.Item2;
                int startC = KeyData.Item3;

                using (var inFs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    // Use RSACryptoServiceProvider
                    // to decrypt the AES key.
                    byte[] KeyDecrypted = null;

                    if (DecryptedAESKey == null)
                    {
                        KeyDecrypted = _rsa.Decrypt(KeyEncrypted, false);
                    }
                    else
                    {
                        byte[] ConvertedKey = Convert.FromBase64String(DecryptedAESKey);
                        KeyDecrypted = ConvertedKey;
                    }

                    // Create instance of Aes for
                    // symmetric decryption of the data.
                    Aes aes = Aes.Create();

                    // Decrypt the key.
                    ICryptoTransform transform = aes.CreateDecryptor(KeyDecrypted, IV);

                    Log("[DecryptFile] Beginning Decryption with supplied key...");

                    // Decrypt the cipher text from
                    // from the FileSteam of the encrypted
                    // file (inFs) into the FileStream
                    // for the decrypted file (outFs).
                    // Will use an intermediate MemoryStream
                    // in order to be able to test any supplied key
                    // without need to write the file to disk
                    using (var outMs = new MemoryStream())
                    {
                        int count = 0;
                        int offset = 0;

                        // blockSizeBytes can be any arbitrary size.
                        int blockSizeBytes = aes.BlockSize / 8;
                        byte[] data = new byte[blockSizeBytes];

                        // By decrypting a chunk a time,
                        // you can save memory and
                        // accommodate large files.

                        // Start at the beginning
                        // of the cipher text.
                        inFs.Seek(startC, SeekOrigin.Begin);
                        using (var outStreamDecrypted = new CryptoStream(outMs, transform, CryptoStreamMode.Write))
                        {
                            do
                            {
                                count = inFs.Read(data, 0, blockSizeBytes);
                                offset += count;
                                outStreamDecrypted.Write(data, 0, count);
                            } while (count > 0);

                            outStreamDecrypted.FlushFinalBlock();
                            Log("[DecryptFile] Decryption in memory was successful.");

                            if (!TestOnly)
                            {
                                if (WaitForFile(outFile))
                                {
                                    using (var outFs = new FileStream(outFile, FileMode.Create))
                                    {
                                        //writes the file
                                        Log("[DecryptFile] Writing to " + outFile);

                                        Directory.CreateDirectory(DestFolder);
                                        outMs.Seek(0, SeekOrigin.Begin);
                                        outMs.CopyTo(outFs);
                                        outFs.Close();
                                        //sets the timestamp of the decrypted file according to the original one
                                        var destination = new FileInfo(outFile)
                                        {
                                            CreationTime = file.CreationTime,
                                            LastWriteTime = file.LastWriteTime,
                                            LastAccessTime = file.LastAccessTime
                                        };
                                    }
                                }
                            }
                            outStreamDecrypted.Close();
                        }
                        outMs.Close();
                    }
                    inFs.Close();
                }
            }
            catch (Exception ex)
            {
                string message = String.Format("[DecryptFile] Exception occurred: \n {0} \n {1} ", ex.Message, ex.ToString());
                Log(message, LogLevel.Error);
                throw;
            }
        }
    }
}
