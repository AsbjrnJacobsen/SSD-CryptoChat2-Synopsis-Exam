// File: CryptoChat2.ClientLib/Services/GroupChatClient.cs

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using CryptoChat2.ClientLib.Models.DTOs;
using CryptoChat2.ClientLib.Security;

namespace CryptoChat2.ClientLib.Services
{
    public class GroupChatClient
    {
        private readonly HttpClient _http;
        private readonly CryptoService _crypto;
        private readonly GroupKeyStoreClientService _keyStore;
        private readonly int _groupId;
        private readonly int _userId;

        public GroupChatClient(HttpClient http, CryptoService crypto, GroupKeyStoreClientService keyStore, int groupId, int userId)
        {
            _http = http;
            _crypto = crypto;
            _keyStore = keyStore;
            _groupId = groupId;
            _userId = userId;
        }

        public async Task SendAsync(int senderId, Dictionary<int, string> messagesPerRecipient)
        {
            var payloads = new List<EncryptedPayloadDto>();

            foreach (var (receiverId, plaintext) in messagesPerRecipient)
            {
                if (!_keyStore.HasPublicKey(_groupId, receiverId))
                {
                    await _keyStore.FetchAndCachePublicKeysAsync(_http, _groupId, senderId);
                }

                string encrypted = await _crypto.EncryptForRecipientAsync(_groupId, senderId, receiverId, plaintext);
                payloads.Add(new EncryptedPayloadDto
                {
                    ReceiverId = receiverId,
                    Content = encrypted
                });
            }

            var dto = new EncryptedGroupMessageDto { Payloads = payloads };
            await _http.PostAsJsonAsync($"api/groups/{_groupId}/send-encrypted-group-message", dto);
        }

        public async Task<List<string>> FetchAndDecryptAsync()
        {
            var messages = await _http.GetFromJsonAsync<List<EncryptedGroupMessageResultDto>>(
                $"api/groups/{_groupId}/encrypted-messages");

            var results = new List<string>();
            if (messages == null) return results;

            foreach (var msg in messages)
            {
                try
                {
                    string plain = await _crypto.DecryptForRecipientAsync(_groupId, msg.SenderId, _userId, msg.Content);
                    results.Add($"{msg.SenderId}: {plain}");
                }
                catch (Exception ex)
                {
                    results.Add($"{msg.SenderId}: [Failed to decrypt] ({ex.Message})");
                }
            }

            return results;
        }
    }
}
