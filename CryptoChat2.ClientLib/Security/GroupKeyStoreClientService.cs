using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CryptoChat2.ClientLib.Models.DTOs;

namespace CryptoChat2.ClientLib.Security
{
    public class GroupKeyStoreClientService
    {
        private readonly ConcurrentDictionary<(int groupId, int userId), ECDiffieHellman> _privateKeys = new();
        private readonly ConcurrentDictionary<(int groupId, int userId), ECDiffieHellmanPublicKey> _publicKeys = new();

        public void GetOrCreatePrivateKey(int groupId, int userId)
        {
            _privateKeys.TryAdd((groupId, userId), ECDiffieHellman.Create());
        }

        public byte[] GetPublicKey(int groupId, int userId)
        {
            var key = _privateKeys.GetOrAdd((groupId, userId), _ => ECDiffieHellman.Create());
            return key.PublicKey.ExportSubjectPublicKeyInfo();
        }

        public void SetPublicKey(int groupId, int userId, byte[] publicKey)
        {
            var key = ECDiffieHellman.Create();
            key.ImportSubjectPublicKeyInfo(publicKey, out _);
            _publicKeys[(groupId, userId)] = key.PublicKey;
            Console.WriteLine($"Imported public key for group={groupId}, user={userId}");
        }

        public async Task UploadPublicKeyAsync(HttpClient httpClient, int groupId, int userId)
        {
            var pubKey = GetPublicKey(groupId, userId);
            var base64 = Convert.ToBase64String(pubKey);
            var content = new StringContent($"\"{base64}\"", Encoding.UTF8, "application/json");
            await httpClient.PostAsync($"api/groupkeys/{groupId}/upload", content);
        }

        public async Task FetchAndCachePublicKeysAsync(HttpClient httpClient, int groupId, int userId)
        {
            await UploadPublicKeyAsync(httpClient, groupId, userId);

            var response = await httpClient.GetFromJsonAsync<List<GroupMemberPublicKeyDto>>(
                $"api/groupkeys/{groupId}/others");

            if (response == null) return;

            foreach (var member in response)
            {
                var pubKeyBytes = Convert.FromBase64String(member.PublicKey);
                SetPublicKey(groupId, member.UserId, pubKeyBytes);
            }
        }

        public byte[] DeriveSharedKey(int groupId, int senderId, int receiverId)
        {
            if (!_privateKeys.TryGetValue((groupId, senderId), out var senderKey))
                throw new InvalidOperationException($"Missing private key for sender {senderId}");

            if (!_publicKeys.TryGetValue((groupId, receiverId), out var receiverPubKey))
                throw new InvalidOperationException($"Missing public key for receiver {receiverId}");

            return senderKey.DeriveKeyMaterial(receiverPubKey);
        }

        public bool HasPrivateKey(int groupId, int userId) =>
            _privateKeys.ContainsKey((groupId, userId));

        public bool HasPublicKey(int groupId, int userId) =>
            _publicKeys.ContainsKey((groupId, userId));
    }
}
