using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using CryptoChat2.ClientLib.Models.DTOs;

namespace CryptoChat2.ClientLib.Api;

public class MessageApiClient
{
    private readonly ApiClient _api;
    public HttpClient Http => _api.HttpClient;

    public MessageApiClient(ApiClient api)
    {
        _api = api;
    }

    public async Task SendPrivateMessageAsync(MessageDto message)
    {
        await _api.PostAsync<object>("api/messages/send", message);
    }

    public async Task<List<MessageDto>> GetPrivateMessagesAsync(int userId)
    {
        return await _api.GetAsync<List<MessageDto>>($"api/messages/{userId}");
    }
}