using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CryptoChat2.ClientLib.Api;
using CryptoChat2.ClientLib.Models.DTOs;
using CryptoChat2.ClientLib.Security;

namespace CryptoChat2.ClientLib.Services
{
    public class PrivateMessageClient
    {
        private readonly MessageApiClient _api;
        private readonly CryptoService _crypto;
        private readonly GroupKeyStoreClientService _keyStoreClient;

        public PrivateMessageClient(MessageApiClient api, CryptoService crypto, GroupKeyStoreClientService keyStoreClient)
        {
            _api = api;
            _crypto = crypto;
            _keyStoreClient = keyStoreClient;
        }

        public async Task SendEncryptedMessageAsync(int senderId, int receiverId, string messageText)
        {
            int groupId = 0;

            if (!_keyStoreClient.HasPrivateKey(groupId, senderId))
                _keyStoreClient.GetOrCreatePrivateKey(groupId, senderId);

            var pubKey = _keyStoreClient.GetPublicKey(groupId, senderId);
            await UploadPublicKeyAsync(pubKey, groupId, senderId);

            if (!_keyStoreClient.HasPublicKey(groupId, receiverId))
                await _keyStoreClient.FetchAndCachePublicKeysAsync(_api.Http, groupId, senderId);

            string encrypted = await _crypto.EncryptForRecipientAsync(groupId, senderId, receiverId, messageText);

            var dto = new MessageDto
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = encrypted,
                Timestamp = DateTime.UtcNow
            };

            await _api.SendPrivateMessageAsync(dto);
        }

        private async Task UploadPublicKeyAsync(byte[] publicKey, int groupId, int userId)
        {
            var base64 = Convert.ToBase64String(publicKey);
            var content = new StringContent($"\"{base64}\"", Encoding.UTF8, "application/json");
            await _api.Http.PostAsync($"api/groupkeys/{groupId}/upload", content);
        }


        public async Task<List<string>> GetDecryptedMessagesAsync(int userId, int otherPartyId)
        {
            var messages = await _api.GetPrivateMessagesAsync(otherPartyId);

            var result = new List<string>();

            foreach (var m in messages)
            {
                int senderId = m.SenderId;
                int receiverId = m.ReceiverId;
                string plain = await _crypto.DecryptForRecipientAsync(0, senderId, receiverId, m.Content);
                result.Add($"{senderId}: {plain}");
            }

            return result;
        }
    }
}