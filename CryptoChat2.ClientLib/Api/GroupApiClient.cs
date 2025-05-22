using System.Collections.Generic;
using System.Threading.Tasks;
using CryptoChat2.ClientLib.Models.DTOs;

namespace CryptoChat2.ClientLib.Api
{
    public class GroupApiClient
    {
        private readonly ApiClient _api;

        public GroupApiClient(ApiClient api)
        {
            _api = api;
        }

        public async Task<List<EncryptedGroupMessageResultDto>> GetEncryptedMessagesAsync(int groupId)
        {
            return await _api.GetAsync<List<EncryptedGroupMessageResultDto>>($"api/groups/{groupId}/encrypted-messages");
        }

        public async Task<List<GroupMessageDto>> GetPlainMessagesAsync(int groupId)
        {
            return await _api.GetAsync<List<GroupMessageDto>>($"api/groups/{groupId}/messages");
        }

        public async Task<List<GroupMemberPublicKeyDto>> GetGroupMemberPublicKeysAsync(int groupId)
        {
            return await _api.GetAsync<List<GroupMemberPublicKeyDto>>($"api/groups/{groupId}/publickeys");
        }

        public async Task<GroupDto> CreateGroupAsync(string name)
        {
            var dto = new GroupCreateDto { Name = name };
            return await _api.PostAsync<GroupDto>("api/groups/createGroup", dto);
        }

        public async Task AddUserToGroupAsync(int groupId, int userId)
        {
            var dto = new AddUserToGroupDto { UserId = userId };
            await _api.PostAsync<object>($"api/groups/{groupId}/add-user-to-group", dto);
        }

        public async Task SendEncryptedGroupMessagesAsync(int groupId, List<EncryptedPayloadDto> payloads)
        {
            var dto = new EncryptedGroupMessageDto { Payloads = payloads };
            await _api.PostAsync<object>($"api/groups/{groupId}/send-encrypted-group-message", dto);
        }

        public async Task LeaveGroupAsync(int groupId)
        {
            await _api.DeleteAsync<object>($"api/groups/{groupId}/leave");
        }

        public async Task DeleteGroupAsync(int groupId)
        {
            await _api.DeleteAsync<object>($"api/groups/{groupId}");
        }

        public async Task KickMemberAsync(int groupId, int userId)
        {
            await _api.DeleteAsync<object>($"api/groups/{groupId}/users/{userId}");
        }
    }
}
