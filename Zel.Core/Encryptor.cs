// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Zel.Classes;

namespace Zel
{
    /// <summary>
    ///     Encryption helper class
    /// </summary>
    public class Encryptor : IEncryptor
    {
        private readonly ILogger _logger;

        public Encryptor(ILogger logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }
            _logger = logger;
        }

        #region Internals

        /// <summary>
        ///     Default salt to use if salt is not specified
        /// </summary>
        private const string DEFAULT_SALT = "&$90jf#($7";

        /// <summary>
        ///     Vector to use
        /// </summary>
        private const string VECTOR = "S5A1m4w2S3cOre12";

        #endregion

        #region AES Methods

        /// <summary>
        ///     Encrypts the specified string into a string
        /// </summary>
        /// <param name="stringToEncrypt">String to encrypt</param>
        /// <param name="password">Password to use</param>
        /// <param name="salt">Salt to use</param>
        /// <param name="iterationCount">Iterations to perform</param>
        /// <param name="keySize">Key size to use</param>
        /// <returns>Encrypted string, or null if encryption fails</returns>
        public string AesEncryptString(string stringToEncrypt, string password,
            string salt = null, int iterationCount = 1, AesKeySize keySize = AesKeySize.OneTwentyEight)
        {
            salt = salt ?? DEFAULT_SALT;
            var byteArrayToEncrypt = Encoding.UTF8.GetBytes(stringToEncrypt);

            var cipherTextBytes = AesEncryptByteArrayIntoByteArray(byteArrayToEncrypt, password, salt,
                iterationCount,
                keySize);
            if (cipherTextBytes != null)
            {
                return Convert.ToBase64String(cipherTextBytes);
            }

            return null;
        }

        /// <summary>
        ///     Encrypts the specified byte array into a byte array
        /// </summary>
        /// <param name="byteArrayToEncrypt">Byte array to encrypt</param>
        /// <param name="password">Password to use</param>
        /// <param name="salt">Salt to use</param>
        /// <param name="iterationCount">Iterations to perform</param>
        /// <param name="keySize">Key size to use</param>
        /// <returns>Encrypted byte array, or null if encryption fails</returns>
        public byte[] AesEncryptByteArrayIntoByteArray(byte[] byteArrayToEncrypt, string password,
            string salt = null, int iterationCount = 1, AesKeySize keySize = AesKeySize.OneTwentyEight)
        {
            salt = salt ?? DEFAULT_SALT;
            var encryptedStream = AesEncryptByteArrayIntoStream(byteArrayToEncrypt, password, salt, iterationCount,
                keySize);
            if (encryptedStream != null)
            {
                return encryptedStream.ToArray();
            }

            return null;
        }

        /// <summary>
        ///     Encrypts the specified byte array into a memorystream
        /// </summary>
        /// <param name="byteArrayToEncrypt">Byte array to encrypt</param>
        /// <param name="password">Password to use</param>
        /// <param name="salt">Salt to use</param>
        /// <param name="iterationCount">Iterations to perform</param>
        /// <param name="keySize">Key size to use</param>
        /// <returns>Encrypted byte array, or null if encryption fails</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public MemoryStream AesEncryptByteArrayIntoStream(byte[] byteArrayToEncrypt, string password,
            string salt = null, int iterationCount = 1, AesKeySize keySize = AesKeySize.OneTwentyEight)
        {
            try
            {
                salt = salt ?? DEFAULT_SALT;

                var initialVectorBytes = Encoding.ASCII.GetBytes(VECTOR);
                var saltValueBytes = Encoding.ASCII.GetBytes(salt);

                using (var derivedPassword = new Rfc2898DeriveBytes(password, saltValueBytes, iterationCount))
                {
                    var keyBytes = derivedPassword.GetBytes((int) keySize/8);

                    using (var symmetricKey = new RijndaelManaged())
                    {
                        symmetricKey.Mode = CipherMode.CBC;
                        var encryptor = symmetricKey.CreateEncryptor(keyBytes, initialVectorBytes);
                        var memStream = new MemoryStream();
                        var cryptoStream = new CryptoStream(memStream, encryptor, CryptoStreamMode.Write);
                        cryptoStream.Write(byteArrayToEncrypt, 0, byteArrayToEncrypt.Length);
                        cryptoStream.FlushFinalBlock();

                        return memStream;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogException(e);
                return null;
            }
        }


        /// <summary>
        ///     Decrypts the specified encrypted string into a string
        /// </summary>
        /// <param name="encryptedString">Encrypted string to decrypt</param>
        /// <param name="password">Password to use</param>
        /// <param name="salt">Salt to use</param>
        /// <param name="iterationCount">Iterations to perform</param>
        /// <param name="keySize">Key size to use</param>
        /// <returns>Decrypted string, or null if decryption fails</returns>
        public string AesDecryptStringIntoString(string encryptedString, string password,
            string salt = null, int iterationCount = 1, AesKeySize keySize = AesKeySize.OneTwentyEight)
        {
            salt = salt ?? DEFAULT_SALT;

            byte[] cipherBytes;
            try
            {
                cipherBytes = Convert.FromBase64String(encryptedString);
            }
            catch (Exception e)
            {
                _logger.LogException(e);
                return null;
            }

            var bytes = AesDecryptByteArrayIntoByteArray(cipherBytes, password, salt, iterationCount, keySize);
            if (bytes != null)
            {
                return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            }
            return null;
        }

        /// <summary>
        ///     Decrypts the specified encrypted stream into a byte array
        /// </summary>
        /// <param name="encryptedStream">Encrypted string to decrypt</param>
        /// <param name="password">Password to use</param>
        /// <param name="salt">Salt to use</param>
        /// <param name="iterationCount">Iterations to perform</param>
        /// <param name="keySize">Key size to use</param>
        /// <returns>Decrypted stream as byte array, or null if decryption fails</returns>
        public byte[] AesDecryptStreamIntoByteArray(Stream encryptedStream, string password,
            string salt = null, int iterationCount = 1, AesKeySize keySize = AesKeySize.OneTwentyEight)
        {
            salt = salt ?? DEFAULT_SALT;

            var cipherBytes = encryptedStream.ToByteArray();

            return AesDecryptByteArrayIntoByteArray(cipherBytes, password, salt, iterationCount, keySize);
        }

        /// <summary>
        ///     Decrypts the specified encrypted byte array into a byte array
        /// </summary>
        /// <param name="cipherBytes">Encrypted byte array to decrypt</param>
        /// <param name="password">Password to use</param>
        /// <param name="salt">Salt to use</param>
        /// <param name="iterationCount">Iterations to perform</param>
        /// <param name="keySize">Key size to use</param>
        /// <returns>Decrypted byte array as byte array, or null if decryption fails</returns>
        public byte[] AesDecryptByteArrayIntoByteArray(byte[] cipherBytes, string password,
            string salt = null, int iterationCount = 1, AesKeySize keySize = AesKeySize.OneTwentyEight)
        {
            try
            {
                salt = salt ?? DEFAULT_SALT;

                var initialVectorBytes = Encoding.ASCII.GetBytes(VECTOR);
                var saltValueBytes = Encoding.ASCII.GetBytes(salt);

                using (var derivedPassword = new Rfc2898DeriveBytes(password, saltValueBytes, iterationCount))
                {
                    var keyBytes = derivedPassword.GetBytes((int) keySize/8);

                    using (var symmetricKey = new RijndaelManaged())
                    {
                        symmetricKey.Mode = CipherMode.CBC;
                        var decryptor = symmetricKey.CreateDecryptor(keyBytes, initialVectorBytes);

                        using (var memStream = new MemoryStream(cipherBytes))
                        {
                            var cryptoStream = new CryptoStream(memStream, decryptor, CryptoStreamMode.Read);
                            var decryptedBytesRaw = new byte[cipherBytes.Length];
                            var byteCount = cryptoStream.Read(decryptedBytesRaw, 0, decryptedBytesRaw.Length);
                            var decryptedBytes = new byte[byteCount];
                            Array.Copy(decryptedBytesRaw, decryptedBytes, byteCount);
                            return decryptedBytes;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogException(e);
                return null;
            }
        }

        #endregion

        #region RSA Methods

        /// <summary>
        ///     Sign the specified string using the specified private key
        /// </summary>
        /// <param name="stringToSign">String to sign</param>
        /// <param name="privateKey">Private key to use</param>
        /// <returns>Signature</returns>
        public string GetSignature(string stringToSign, string privateKey)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(privateKey);
                var rsaFormatter = new RSAPKCS1SignatureFormatter(rsa);
                rsaFormatter.SetHashAlgorithm("SHA1");
                using (var sHhash = new SHA1Managed())
                {
                    var signedHashValue =
                        rsaFormatter.CreateSignature(sHhash.ComputeHash(new UnicodeEncoding().GetBytes(stringToSign)));
                    return Convert.ToBase64String(signedHashValue);
                }
            }
        }

        /// <summary>
        ///     Checks if the specified signature is valid for the specified string using the specified public key
        /// </summary>
        /// <param name="stringToVerify">String to verify</param>
        /// <param name="signature">Signature to use</param>
        /// <param name="publicKey">Public key to use</param>
        /// <returns>True if signature is valid, else false</returns>
        public bool IsValidSignature(string stringToVerify, string signature, string publicKey)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(publicKey);
                var rsaDeformatter = new RSAPKCS1SignatureDeformatter(rsa);
                rsaDeformatter.SetHashAlgorithm("SHA1");
                using (var sHhash = new SHA1Managed())
                {
                    if (
                        rsaDeformatter.VerifySignature(
                            sHhash.ComputeHash(new UnicodeEncoding().GetBytes(stringToVerify)),
                            Convert.FromBase64String(signature)))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        #endregion
    }
}