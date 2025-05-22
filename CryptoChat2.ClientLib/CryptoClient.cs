using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CryptoChat2.ClientLib
{
    public class CryptoClient
    {
        readonly ECDiffieHellman _ecdh;
        readonly HttpClient      _http;

        public CryptoClient(HttpClient http)
        {
            _ecdh = ECDiffieHellman.Create();
            _http = http;
        }

        // Export own public key (DER)
        public byte[] GetPublicKey() 
            => _ecdh.PublicKey.ExportSubjectPublicKeyInfo();

        // Fetch others' public keys from server
        public async Task<Dictionary<int, byte[]>> FetchGroupPublicKeysAsync(int groupId)
        {
            var list = await _http.GetFromJsonAsync<List<MemberKeyDto>>(
                $"api/groups/{groupId}/publickeys");
            return list.ToDictionary(m => m.UserId,
                                     m => Convert.FromBase64String(m.PublicKey));
        }

        // Derive shared 32-byte key with another user's public key
        public byte[] DeriveSharedKey(byte[] otherPubKey)
        {
            using var other = ECDiffieHellman.Create();
            other.ImportSubjectPublicKeyInfo(otherPubKey, out _);
            return _ecdh.DeriveKeyMaterial(other.PublicKey);
        }

        // Encrypt plaintext for a given sharedKey
        public string Encrypt(byte[] sharedKey, string plaintext)
        {
            var plain = Encoding.UTF8.GetBytes(plaintext);
            int nonceSize = 12, tagSize = 16;
            var nonce = RandomNumberGenerator.GetBytes(nonceSize);
            var cipher = new byte[plain.Length];
            var tag    = new byte[tagSize];
            using var aes = new AesGcm(sharedKey, tagSize);
            aes.Encrypt(nonce, plain, cipher, tag);

            var all = new byte[nonceSize + cipher.Length + tagSize];
            Buffer.BlockCopy(nonce,  0, all, 0,               nonceSize);
            Buffer.BlockCopy(cipher, 0, all, nonceSize,       cipher.Length);
            Buffer.BlockCopy(tag,    0, all, nonceSize+cipher.Length, tagSize);
            return Convert.ToBase64String(all);
        }

        // Decrypt base64(nonce|cipher|tag)
        public string Decrypt(byte[] sharedKey, string payload)
        {
            var all = Convert.FromBase64String(payload);
            int nonceSize = 12, tagSize = 16;
            int cipherLen = all.Length - nonceSize - tagSize;

            var nonce  = all[..nonceSize];
            var cipher = all[nonceSize..(nonceSize+cipherLen)];
            var tag    = all[(nonceSize+cipherLen)..];

            var plain = new byte[cipherLen];
            using var aes = new AesGcm(sharedKey, tagSize);
            aes.Decrypt(nonce, cipher, tag, plain);
            return Encoding.UTF8.GetString(plain);
        }

        // DTO for publickeys endpoint
        class MemberKeyDto { public int UserId { get; set; } = 0; public string PublicKey { get; set; } = ""; }
    }
}
