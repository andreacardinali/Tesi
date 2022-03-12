﻿using System;
using System.IO;
using System.Security.Cryptography;
using System.Xml.Linq;
using static FakePatch.Globals;
using static FakePatch.Install;
using static FakePatch.LogHelper;

namespace FakePatch
{
    public enum ExportKeyType
    {
        None, Public, PublicAndPrivate, PublicAndPrivateSeparate
    }
    public class Crypto
    {
        //public static RSACryptoServiceProvider _rsa;

        public FileInfo GetEncryptedFilePath(FileInfo DecryptedFilePath, string WorkingDir = null)
        {
            if (WorkingDir == null)
            {
                WorkingDir = DecryptedFilePath.DirectoryName;
            }
            FileInfo EncryptedFilePath = new FileInfo(Path.Combine(WorkingDir, Path.ChangeExtension(DecryptedFilePath.Name, Path.GetExtension(DecryptedFilePath.Name) + ".enc")));
            Log("Decrypted path: " + DecryptedFilePath.FullName + " - Encrypted path: " + EncryptedFilePath.FullName);
            return EncryptedFilePath;
        }
        public void GenerateAsimKeys(string KeyName, ExportKeyType ExportKey = ExportKeyType.None)
        {
            CspParameters _cspp = new CspParameters
            {
                KeyContainerName = KeyName,
                Flags = CspProviderFlags.NoFlags
            };
            gRSA = new RSACryptoServiceProvider(_cspp)
            {
                PersistKeyInCsp = gPersistKey,
                KeySize = gKeySize
            };
            Log($"Key created in container {KeyName}: \n {gRSA.ToXmlString(true)}");

            switch (ExportKey)
            {
                case ExportKeyType.None:
                    break;
                case ExportKeyType.Public:
                    //Log($"Exporting Public Key to {gPubKeyFile.FullName}");
                    this.ExportAsimKeys(gKeyName, gPubKeyFile, false);
                    break;
                case ExportKeyType.PublicAndPrivate:
                    //Log($"Exporting Private and Public Key to {gKeyFile.FullName}");
                    this.ExportAsimKeys(gKeyName, gKeyFile, true);
                    break;
                case ExportKeyType.PublicAndPrivateSeparate:
                    //Log($"Exporting Public Key to {gPubKeyFile.FullName}");
                    this.ExportAsimKeys(gKeyName, gPubKeyFile, false);
                    this.ExportAsimKeys(gKeyName, gKeyFile, true);
                    break;
            }
        }

        public void GetStoredAsimKeys(string KeyName, bool CreateIfMissing = false)
        {
            CspParameters _cspp = new CspParameters
            {
                KeyContainerName = KeyName,
                //use different flags to retrieve the key if existing
                Flags = CspProviderFlags.UseExistingKey
            };

            try
            {
                gRSA = new RSACryptoServiceProvider(_cspp)
                {
                    PersistKeyInCsp = gPersistKey
                };
                Log($"Key retrieved from container {KeyName}: \n {gRSA.ToXmlString(true)}");
                //Console.WriteLine($"Key retrieved from container {KeyName}: \n {gRSA.ToXmlString(true)}");
                //return _rsa;
            }
            catch (CryptographicException)
            {
                Log($"Key not found in container {KeyName}");
                if (CreateIfMissing)
                {
                    GenerateAsimKeys(KeyName);
                    //_rsa = GenerateAsimKeys(KeyName);
                    //return _rsa;
                }
                else
                {
                    //return null;
                }
            }
            catch (Exception ex)
            {
                string message = String.Format("Exception occurred: \n {0} \n {1} ", ex.Message, ex.ToString());
                Log(message, LogLevel.Error);
            }

        }

        public void ClearStoredAsimKeys(string KeyName)
        {
            //var _rsa = GetStoredAsimKeys(KeyName,false);
            if (gRSA != null)
            {
                gRSA.Clear();
                Log($"Key cleared from container {KeyName}");
                gRSA = null;
            }
            else
            {
                Log($"Key not esisting in container {KeyName}");
            }
            //return _rsa;
        }

        public string GenerateKeyString(string EncryptedKey, FileInfo KeyFile)
        {
            string result;
            try
            {
                RSACryptoServiceProvider _rsa = ImportAsimKeys("Temp", KeyFile);
                byte[] ConvertedKey = Convert.FromBase64String(EncryptedKey);
                if (ConvertedKey.Length == 128)
                {
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
                string message = String.Format("Exception occurred: \n {0} \n {1} ", ex.Message, ex.ToString());
                Log(message, LogLevel.Error);
                result = null;
            }
            return result;
        }


        public bool ValidateKeyString(string Key, FileInfo TargetFile)
        {
            bool result = false;
            try
            {
                try
                {
                    DecryptFile(TargetFile, null, Key, null, true);
                    result = true;
                }
                catch
                {
                    //throw new ArgumentException("Key length is not 128 byte");
                }
            }
            catch (Exception ex)
            {
                string message = String.Format("Exception occurred: \n {0} \n {1} ", ex.Message, ex.ToString());
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
                    Log(xd1.Root.Name.ToString());
                    //checks if the XML root element is named RSAKeyValue
                    if (xd1.Root.Name.ToString() == "RSAKeyValue")
                    {
                        if (TargetFile != null)
                        {
                            if (!File.Exists(TargetFile.FullName))
                            {
                                throw new FileNotFoundException(TargetFile.FullName);
                            }
                            Log(TargetFile.FullName);
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
                                string message = String.Format("Exception occurred: \n {0} \n {1} ", ex.Message, ex.ToString());
                                Log(message);
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
                string message = String.Format("Exception occurred: \n {0} \n {1} ", ex.Message, ex.ToString());
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
            Log(keytxt);
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

        public void ExportAsimKeys(string KeyName, FileInfo KeyFile, bool includePrivateParameters = false)
        {
            if (gRSA is null)
            {
                Log("Key not set.");
            }
            else
            {
                string message = "";
                switch (includePrivateParameters)
                {
                    case true:
                        message = string.Format("Exporting Public and Private key from container {0} to {1}", KeyName, KeyFile.FullName);
                        break;
                    case false:
                        message = string.Format("Exporting Public key from container {0} to {1}", KeyName, KeyFile.FullName);
                        break;
                }

                Log(message);
                if (!Directory.Exists(KeyFile.DirectoryName))
                {
                    Directory.CreateDirectory(KeyFile.DirectoryName);
                }
                using (var sw = new StreamWriter(KeyFile.FullName, false))
                {
                    sw.Write(gRSA.ToXmlString(includePrivateParameters));
                }
            }
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
                string message = String.Format("Exception occurred: \n {0} \n {1} ", ex.Message, ex.ToString());
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
                    throw new Exception("File not found");
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

                Log("AES Key: " + Convert.ToBase64String(aes.Key));
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
                //string outFile = Path.Combine(DestFolder, Path.ChangeExtension(file.Name, Path.GetExtension(file.Name) + ".enc"));

                Log("Encrypting " + file.FullName + " to " + outFile);

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
                string message = String.Format("Exception occurred: \n {0} \n {1} ", ex.Message, ex.ToString());
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
                    Log(Path.GetExtension(file.FullName).ToLower());
                    outFile = Path.Combine(DestFolder, file.Name);

                    if (Path.GetExtension(file.FullName).ToLower() == ".enc")
                    {
                        //removes .enc extension
                        outFile = Path.ChangeExtension(outFile, null);
                    }
                }
                Log("Trying to decrypt " + file.FullName);

                var KeyData = GetEncryptedKey(file);
                byte[] KeyEncrypted = KeyData.Item1;
                byte[] IV = KeyData.Item2;
                int startC = KeyData.Item3;



                /**************************************************************************************************************
                // Create byte arrays to get the length of
                // the encrypted key and IV.
                // These values were stored as 4 bytes each
                // at the beginning of the encrypted package.
                byte[] LenK = new byte[4];
                byte[] LenIV = new byte[4];
                
                

                // Use FileStream objects to read the encrypted
                // file (inFs) and save the decrypted file (outFs).
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
                    Log(lenK.ToString());
                    byte[] KeyEncrypted = new byte[lenK];
                    byte[] IV = new byte[lenIV];

                    // Extract the key and IV
                    // starting from index 8
                    // after the length values.
                    inFs.Seek(8, SeekOrigin.Begin);
                    inFs.Read(KeyEncrypted, 0, lenK);
                    inFs.Seek(8 + lenK, SeekOrigin.Begin);
                    inFs.Read(IV, 0, lenIV);
                    *********************************************************************************************************/

                using (var inFs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    // Use RSACryptoServiceProvider
                    // to decrypt the AES key.
                    byte[] KeyDecrypted = null;

                    if (DecryptedAESKey == null)
                    {
                        Log(Convert.ToBase64String(KeyEncrypted));
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

                    Log("Beginning Decryption with supplied key...");

                    // Decrypt the cipher text from
                    // from the FileSteam of the encrypted
                    // file (inFs) into the FileStream
                    // for the decrypted file (outFs).
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
                            Log("Decryption in memory was successful.");

                            if (!TestOnly)
                            {
                                if (WaitForFile(outFile))
                                {
                                    using (var outFs = new FileStream(outFile, FileMode.Create))
                                    {
                                        //writes the file
                                        Log("Writing to " + outFile);

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
                string message = String.Format("Exception occurred: \n {0} \n {1} ", ex.Message, ex.ToString());
                Log(message, LogLevel.Error);
                throw;
            }
        }
    }
}
