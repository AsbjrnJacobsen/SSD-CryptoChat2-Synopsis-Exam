using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CryptoChat2.ClientLib.Security;

namespace e2eetest;

class Program
{
    static async Task Main()
    {
        const int groupId = 1;


        Console.WriteLine("Step 1: Logger ind som Test1...");
        var http = new HttpClient { BaseAddress = new Uri("https://localhost:7169/") };
        var test1Token = await LoginAsync(http, "Test1", "Test1pw!");
        Console.WriteLine("Test1 JWT Token:\n" + test1Token);
        var test1Id = GetUserIdFromToken(test1Token);
        Console.WriteLine("Test1 ID: " + test1Id);

        Console.WriteLine("Step 2: Logger ind som Test2...");
        var test2Token = await LoginAsync(http, "Test2", "Test2pw!");
        Console.WriteLine("Test2 JWT Token:\n" + test2Token);
        var test2Id = GetUserIdFromToken(test2Token);
        Console.WriteLine("Test2 ID: " + test2Id);
        var httpTest1 = NewHttpClientWithToken(test1Token);
        var httpTest2 = NewHttpClientWithToken(test2Token);
        Console.WriteLine("Step 2.5: Sikrer at begge brugere er i gruppe 1");
        await EnsureGroupMembershipAsync(httpTest1, groupId, test2Id);
        await EnsureGroupMembershipAsync(httpTest2, groupId, test1Id);
        Console.WriteLine("Begge brugere er i gruppe.");


        Console.WriteLine("Step 3: Initialiserer keystores og crypto...");
        var test1KeyStore = new GroupKeyStoreClientService();
        var test2KeyStore = new GroupKeyStoreClientService();
        var test1Crypto = new CryptoService(test1KeyStore);
        var test2Crypto = new CryptoService(test2KeyStore);

        Console.WriteLine("Step 4: Genererer lokale private nøgler...");
        test1KeyStore.GetOrCreatePrivateKey(groupId, test1Id);
        test2KeyStore.GetOrCreatePrivateKey(groupId, test2Id);
        Console.WriteLine("Private keys oprettet.");

        Console.WriteLine("Step 5: Synkroniserer public keys med server...");
        Console.WriteLine("Step 5: Uploader egne public keys...");
        await UploadPublicKeyAsync(httpTest1, groupId, test1KeyStore.GetPublicKey(groupId, test1Id));
        await UploadPublicKeyAsync(httpTest2, groupId, test2KeyStore.GetPublicKey(groupId, test2Id));
        Console.WriteLine("Public keys uploaded.");

        Console.WriteLine("Step 6: Henter andres public keys...");
        await test1KeyStore.FetchAndCachePublicKeysAsync(httpTest1, groupId, test1Id);
        await test2KeyStore.FetchAndCachePublicKeysAsync(httpTest2, groupId, test2Id);
        Console.WriteLine("Public keys synkroniseret.");

        Console.WriteLine("Public keys synkroniseret.");

        Console.WriteLine("Step 6: Test1 sender krypteret besked til Test2");
        var encrypted1 = await test1Crypto.EncryptForRecipientAsync(groupId, test1Id, test2Id, "Hej fra Test1");
        Console.WriteLine("Krypteret besked:\n" + encrypted1);
        var decrypted1 = await test2Crypto.DecryptForRecipientAsync(groupId, test1Id, test2Id, encrypted1);
        Console.WriteLine("Dekrypteret af Test2:\n" + decrypted1);

        Console.WriteLine("Step 7: Test2 sender krypteret besked til Test1");
        var encrypted2 = await test2Crypto.EncryptForRecipientAsync(groupId, test2Id, test1Id, "Hej fra Test2");
        Console.WriteLine("Krypteret besked:\n" + encrypted2);
        var decrypted2 = await test1Crypto.DecryptForRecipientAsync(groupId, test2Id, test1Id, encrypted2);
        Console.WriteLine("Dekrypteret af Test1:\n" + decrypted2);
    }

    static async Task<string> LoginAsync(HttpClient http, string username, string password)
    {
        var loginReq = new { username, password };
        var resp = await http.PostAsync("api/Auth/login",
            new StringContent(JsonSerializer.Serialize(loginReq), Encoding.UTF8, "application/json"));
        resp.EnsureSuccessStatusCode();

        using var doc = await JsonDocument.ParseAsync(await resp.Content.ReadAsStreamAsync());
        return doc.RootElement.GetProperty("token").GetString()!;
    }

    static int GetUserIdFromToken(string token)
    {
        var payload = token.Split('.')[1];
        var padded = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
        var json = Encoding.UTF8.GetString(Convert.FromBase64String(padded));
        var doc = JsonDocument.Parse(json);

        return int.Parse(doc.RootElement
            .GetProperty("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")
            .GetString()!);
    }

    static HttpClient NewHttpClientWithToken(string token)
    {
        var http = new HttpClient { BaseAddress = new Uri("https://localhost:7169/") };
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return http;
    }
    static async Task EnsureGroupMembershipAsync(HttpClient http, int groupId, int userIdToAdd)
    {
        var dto = new { userId = userIdToAdd };
        var json = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
        var response = await http.PostAsync($"api/groups/{groupId}/add-user-to-group", json);
    
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            if (!content.Contains("already in group", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception($"Kunne ikke tilføje bruger {userIdToAdd} til gruppe {groupId}:\n{content}");
            }
        }
    }
    static async Task UploadPublicKeyAsync(HttpClient http, int groupId, byte[] pubKey)
    {
        var base64 = Convert.ToBase64String(pubKey);
        var content = new StringContent($"\"{base64}\"", Encoding.UTF8, "application/json");
        var response = await http.PostAsync($"api/groupkeys/{groupId}/upload", content);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Fejl ved upload af public key:\n{error}");
        }
    }


}
