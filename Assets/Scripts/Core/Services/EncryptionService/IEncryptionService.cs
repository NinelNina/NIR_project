namespace Core.Services
{
    public interface IEncryptionService
    {
        byte[] Encrypt(byte[] data);
        
        byte[] Decrypt(byte[] encryptedData);
        
        string ComputeHash(byte[] data);
        bool VerifyHash(byte[] data, string expectedHash);
    }
}