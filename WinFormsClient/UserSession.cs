// WinFormsClient/UserSession.cs

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CryptoChat2.ClientLib.Api;
using CryptoChat2.ClientLib.Security;
using CryptoChat2.ClientLib.Services;

namespace WinFormsClient;

public class UserSession
{
    public int UserId { get; }
    public string Token { get; }
    public HttpClient Http { get; }
    public GroupKeyStoreClientService KeyStore { get; }
    public CryptoService Crypto { get; }
    public GroupChatClient ChatClient { get; }

    private const int GroupId = 0;

    public UserSession(int userId, string token)
    {
        UserId = userId;
        Token = token;
        Http = new HttpClient { BaseAddress = new Uri("https://localhost:7169/") };
        Http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        KeyStore = new GroupKeyStoreClientService();
        Crypto = new CryptoService(KeyStore);

        InitKeyStoreAsync().GetAwaiter().GetResult();

        ChatClient = new GroupChatClient(Http, Crypto, KeyStore, GroupId, UserId);
    }

    private async Task InitKeyStoreAsync()
    {
        if (!KeyStore.HasPrivateKey(GroupId, UserId))
        {
            KeyStore.GetOrCreatePrivateKey(GroupId, UserId);
        }

        if (!KeyStore.HasPublicKey(GroupId, UserId))
        {
            await KeyStore.UploadPublicKeyAsync(Http, GroupId, UserId);
        }

        await KeyStore.FetchAndCachePublicKeysAsync(Http, GroupId, UserId);
    }
}