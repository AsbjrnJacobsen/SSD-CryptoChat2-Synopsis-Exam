using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CryptoChat2.ClientLib.Security
{
    public class CryptoService
    {
        private readonly GroupKeyStoreClientService _keyStoreClient;
        private const int NonceSize = 12;
        private const int TagSize = 16;

        public CryptoService(GroupKeyStoreClientService keyStoreClient)
        {
            _keyStoreClient = keyStoreClient;
        }

        public string Encrypt(byte[] sharedKey, string plaintext)
        {
            var plainBytes = Encoding.UTF8.GetBytes(plaintext);
            var nonce = RandomNumberGenerator.GetBytes(NonceSize);
            var cipher = new byte[plainBytes.Length];
            var tag = new byte[TagSize];

            using var aes = new AesGcm(sharedKey, TagSize);
            aes.Encrypt(nonce, plainBytes, cipher, tag);

            var result = new byte[NonceSize + cipher.Length + tag.Length];
            Buffer.BlockCopy(nonce, 0, result, 0, NonceSize);
            Buffer.BlockCopy(cipher, 0, result, NonceSize, cipher.Length);
            Buffer.BlockCopy(tag, 0, result, NonceSize + cipher.Length, tag.Length);

            return Convert.ToBase64String(result);
        }

        public string Decrypt(byte[] sharedKey, string payload)
        {
            var data = Convert.FromBase64String(payload);
            var nonce = data[..NonceSize];
            var cipherLen = data.Length - NonceSize - TagSize;
            var cipher = data[NonceSize..(NonceSize + cipherLen)];
            var tag = data[(NonceSize + cipherLen)..];

            var plain = new byte[cipherLen];
            using var aes = new AesGcm(sharedKey, TagSize);
            aes.Decrypt(nonce, cipher, tag, plain);

            return Encoding.UTF8.GetString(plain);
        }

        public async Task<string> EncryptForRecipientAsync(int groupId, int senderId, int receiverId, string plaintext)
        {
            var key = _keyStoreClient.DeriveSharedKey(groupId, senderId, receiverId);
            return Encrypt(key, plaintext);
        }

        public async Task<string> DecryptForRecipientAsync(int groupId, int senderId, int receiverId, string payload)
        {
            var key = _keyStoreClient.DeriveSharedKey(groupId, receiverId, senderId);
            return Decrypt(key, payload);
        }
    }
}
