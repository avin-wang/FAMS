using System.Text;
using System.Security.Cryptography;
using System.IO;
using System;

namespace FAMS.Services
{
    public class CEncryptor
    {
        private const int BUFFER_SIZE = 128 * 1024;
        private const string PASSWORD = "fams.pwd.0334.057";
        private RandomNumberGenerator _rand = new RNGCryptoServiceProvider(); // Encrypting file random number generator.

        /// <summary>
        /// Encrypt file.
        /// </summary>
        /// <param name="metadata">metadata string</param>
        /// <param name="fileName">path of output file encrypted</param>
        public void EncryptFile(string fileName, string metadata, string pwd = PASSWORD, int size = BUFFER_SIZE)
        {
            pwd = (pwd == null) ? PASSWORD : pwd;
            size = (size == 0) ? BUFFER_SIZE : size;

            using (FileStream fout = File.OpenWrite(fileName))
            {
                // Clear the file before write new data.
                fout.Seek(0, SeekOrigin.Begin);
                fout.SetLength(0);

                byte[] buffer = new byte[size]; // buffer

                // Get IV and salt.
                byte[] IV = GenRandBytes(16);
                byte[] salt = GenRandBytes(16);

                // Create encrypt object.
                SymmetricAlgorithm sa = CreateRijndael(pwd, salt);
                sa.IV = IV;

                // Write IV and salt into the begin part of the output file.
                fout.Write(IV, 0, IV.Length);
                fout.Write(salt, 0, salt.Length);

                HashAlgorithm hasher = SHA256.Create();
                using (CryptoStream cout = new CryptoStream(fout, sa.CreateEncryptor(), CryptoStreamMode.Write),
                    chash = new CryptoStream(Stream.Null, hasher, CryptoStreamMode.Write))
                {
                    // Encode the data string to the buffer.
                    buffer = UnicodeEncoding.Unicode.GetBytes(metadata);
                    cout.Write(buffer, 0, buffer.Length);
                    chash.Write(buffer, 0, buffer.Length);

                    // Close encrypting stream.
                    chash.Flush();
                    chash.Close();

                    // Get hash.
                    byte[] hash = hasher.Hash;

                    // Write hash to the file.
                    cout.Write(hash, 0, hash.Length);

                    // Close file stream.
                    cout.Flush();
                    cout.Close();
                }
            }
        }

        /// <summary>
        /// Decrypt file.
        /// </summary>
        /// <param name="fileName">path of file to be decrypted</param>
        /// <returns>metadata of the file decrypted</returns>
        public string DecryptFile(string fileName, string pwd = PASSWORD, int size = BUFFER_SIZE)
        {
            pwd = (pwd == null) ? PASSWORD : pwd;
            size = (size == 0) ? BUFFER_SIZE : size;

            string metadata = string.Empty;
            using (FileStream fin = File.OpenRead(fileName))
            {
                byte[] bytes = new byte[size];

                byte[] IV = new byte[16];
                fin.Read(IV, 0, 16);

                byte[] salt = new byte[16];
                fin.Read(salt, 0, 16);

                SymmetricAlgorithm sa = CreateRijndael(pwd, salt);
                sa.IV = IV;

                using (CryptoStream cin = new CryptoStream(fin, sa.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    cin.Read(bytes, 0, bytes.Length);
                    cin.Close();
                    metadata = UnicodeEncoding.Unicode.GetString(bytes);
                }
            }

            return metadata;
        }

        /// <summary>
        /// Create Rijndael SymmetricAlgorithm object.
        /// </summary>
        /// <param name="pwd">password</param>
        /// <param name="salt"></param>
        /// <returns>SymmetricAlgorithm object</returns>
        private SymmetricAlgorithm CreateRijndael(string pwd, byte[] salt)
        {
            PasswordDeriveBytes pdb = new PasswordDeriveBytes(pwd, salt, "SHA256", 1000);
            SymmetricAlgorithm sa = Rijndael.Create();
            sa.KeySize = 256;
            sa.Key = pdb.GetBytes(32);
            sa.Padding = PaddingMode.PKCS7;

            return sa;
        }

        /// <summary>
        /// Generate random byte array.
        /// </summary>
        /// <param name="size">size of the byte array</param>
        /// <returns>random byte array generated</returns>
        private byte[] GenRandBytes(int size)
        {
            byte[] bytes = new byte[size];
            _rand.GetBytes(bytes);

            return bytes;
        }
    }
}