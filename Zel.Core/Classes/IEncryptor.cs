// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Zel.Classes
{
    public interface IEncryptor
    {
        /// <summary>
        ///     Encrypts the specified string into a string
        /// </summary>
        /// <param name="stringToEncrypt">String to encrypt</param>
        /// <param name="password">Password to use</param>
        /// <param name="salt">Salt to use</param>
        /// <param name="iterationCount">Iterations to perform</param>
        /// <param name="keySize">Key size to use</param>
        /// <returns>Encrypted string, or null if encryption fails</returns>
        string AesEncryptString(string stringToEncrypt, string password,
            string salt = null, int iterationCount = 1, AesKeySize keySize = AesKeySize.OneTwentyEight);

        /// <summary>
        ///     Encrypts the specified byte array into a byte array
        /// </summary>
        /// <param name="byteArrayToEncrypt">Byte array to encrypt</param>
        /// <param name="password">Password to use</param>
        /// <param name="salt">Salt to use</param>
        /// <param name="iterationCount">Iterations to perform</param>
        /// <param name="keySize">Key size to use</param>
        /// <returns>Encrypted byte array, or null if encryption fails</returns>
        byte[] AesEncryptByteArrayIntoByteArray(byte[] byteArrayToEncrypt, string password,
            string salt = null, int iterationCount = 1, AesKeySize keySize = AesKeySize.OneTwentyEight);

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
        MemoryStream AesEncryptByteArrayIntoStream(byte[] byteArrayToEncrypt, string password,
            string salt = null, int iterationCount = 1, AesKeySize keySize = AesKeySize.OneTwentyEight);

        /// <summary>
        ///     Decrypts the specified encrypted string into a string
        /// </summary>
        /// <param name="encryptedString">Encrypted string to decrypt</param>
        /// <param name="password">Password to use</param>
        /// <param name="salt">Salt to use</param>
        /// <param name="iterationCount">Iterations to perform</param>
        /// <param name="keySize">Key size to use</param>
        /// <returns>Decrypted string, or null if decryption fails</returns>
        string AesDecryptStringIntoString(string encryptedString, string password,
            string salt = null, int iterationCount = 1, AesKeySize keySize = AesKeySize.OneTwentyEight);

        /// <summary>
        ///     Decrypts the specified encrypted stream into a byte array
        /// </summary>
        /// <param name="encryptedStream">Encrypted string to decrypt</param>
        /// <param name="password">Password to use</param>
        /// <param name="salt">Salt to use</param>
        /// <param name="iterationCount">Iterations to perform</param>
        /// <param name="keySize">Key size to use</param>
        /// <returns>Decrypted stream as byte array, or null if decryption fails</returns>
        byte[] AesDecryptStreamIntoByteArray(Stream encryptedStream, string password,
            string salt = null, int iterationCount = 1, AesKeySize keySize = AesKeySize.OneTwentyEight);

        /// <summary>
        ///     Decrypts the specified encrypted byte array into a byte array
        /// </summary>
        /// <param name="cipherBytes">Encrypted byte array to decrypt</param>
        /// <param name="password">Password to use</param>
        /// <param name="salt">Salt to use</param>
        /// <param name="iterationCount">Iterations to perform</param>
        /// <param name="keySize">Key size to use</param>
        /// <returns>Decrypted byte array as byte array, or null if decryption fails</returns>
        byte[] AesDecryptByteArrayIntoByteArray(byte[] cipherBytes, string password,
            string salt = null, int iterationCount = 1, AesKeySize keySize = AesKeySize.OneTwentyEight);

        /// <summary>
        ///     Sign the specified string using the specified private key
        /// </summary>
        /// <param name="stringToSign">String to sign</param>
        /// <param name="privateKey">Private key to use</param>
        /// <returns>Signature</returns>
        string GetSignature(string stringToSign, string privateKey);

        /// <summary>
        ///     Checks if the specified signature is valid for the specified string using the specified public key
        /// </summary>
        /// <param name="stringToVerify">String to verify</param>
        /// <param name="signature">Signature to use</param>
        /// <param name="publicKey">Public key to use</param>
        /// <returns>True if signature is valid, else false</returns>
        bool IsValidSignature(string stringToVerify, string signature, string publicKey);
    }
}