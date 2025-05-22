using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsClient;

public class LoginPanel : Panel
{
    private readonly TextBox _txtUsername = new() { PlaceholderText = "Username", Width = 180 };
    private readonly TextBox _txtPassword = new() { PlaceholderText = "Password", UseSystemPasswordChar = true, Width = 180 };
    private readonly Button _btnLogin = new() { Text = "Login", Width = 80 };
    private readonly TextBox _txtStatus = new() { ReadOnly = true, Width = 400 };

    public Func<UserSession, Task>? OnLoginSuccess;

    public LoginPanel()
    {
        _txtUsername.Location = new(10, 10);
        _txtPassword.Location = new(200, 10);
        _btnLogin.Location = new(400, 10);
        _btnLogin.Click += BtnClickLogin;

        _txtStatus.Location = new(10, 45);

        Controls.AddRange([_txtUsername, _txtPassword, _btnLogin, _txtStatus]);
    }

    private async void BtnClickLogin(object? sender, EventArgs e)
    {
        var loginReq = new
        {
            username = _txtUsername.Text,
            password = _txtPassword.Text
        };

        using var http = new HttpClient { BaseAddress = new Uri("https://localhost:7169/") };

        var response = await http.PostAsync("api/Auth/login",
            new StringContent(JsonSerializer.Serialize(loginReq), Encoding.UTF8, "application/json"));

        if (!response.IsSuccessStatusCode)
        {
            _txtStatus.Text = $"Login failed: {response.StatusCode}";
            return;
        }

        using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var token = doc.RootElement.GetProperty("token").GetString()!;
        var userId = int.Parse(doc.RootElement.GetProperty("userId").GetRawText());

        var session = new UserSession(userId, token);
        await (OnLoginSuccess?.Invoke(session) ?? Task.CompletedTask);
    }
}