using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Core.Services
{
    public class AesEncryptionService : IEncryptionService
    {
        private readonly byte[] _key;
        private readonly byte[] _iv;
        
        // 🔐 Ключ и вектор инициализации — должны быть секретными!
        // В продакшене: хранить в PlayerPrefs, загружать с сервера, или генерировать уникально на устройстве
        public AesEncryptionService(string password)
        {
            // Простой способ получить ключ из пароля (не идеально, но работает)
            using var sha256 = SHA256.Create();
            _key = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "_key_salt"));
            _iv = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "_iv_salt"))[..16]; // AES требует 16 байт для IV
        }
        
        // 🔐 Альтернатива: ключи из настроек (более безопасно)
        public AesEncryptionService(byte[] key, byte[] iv)
        {
            if (key?.Length != 32) throw new ArgumentException("Key must be 32 bytes (256-bit)");
            if (iv?.Length != 16) throw new ArgumentException("IV must be 16 bytes (128-bit)");
            
            _key = key;
            _iv = iv;
        }

        public byte[] Encrypt(byte[] data)
        {
            if (data == null || data.Length == 0)
                return data; // Нечего шифровать
            
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            
            using var encryptor = aes.CreateEncryptor();
            using var ms = new MemoryStream();
            
            // Записываем IV в начало файла (нужен для дешифровки)
            ms.Write(_iv, 0, _iv.Length);
            
            using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
            cs.Write(data, 0, data.Length);
            cs.FlushFinalBlock();
            
            return ms.ToArray();
        }

        public byte[] Decrypt(byte[] encryptedData)
        {
            if (encryptedData == null || encryptedData.Length == 0)
                return encryptedData;
            
            using var aes = Aes.Create();
            aes.Key = _key;
            
            // Извлекаем IV из начала файла
            var iv = new byte[16];
            Array.Copy(encryptedData, 0, iv, 0, 16);
            aes.IV = iv;
            
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            
            using var decryptor = aes.CreateDecryptor();
            using var ms = new MemoryStream(encryptedData, 16, encryptedData.Length - 16);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var result = new MemoryStream();
            
            cs.CopyTo(result);
            return result.ToArray();
        }

        public string ComputeHash(byte[] data)
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(data);
            return Convert.ToBase64String(hash);
        }

        public bool VerifyHash(byte[] data, string expectedHash)
        {
            var actualHash = ComputeHash(data);
            return actualHash == expectedHash;
        }
    }
}