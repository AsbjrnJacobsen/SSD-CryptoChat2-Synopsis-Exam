using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using CryptoChat2.ClientLib.Api;
using CryptoChat2.ClientLib.Security;
using CryptoChat2.ClientLib.Services;

namespace WinFormsClient
{
    public class Form1 : Form
    {
        TextBox txtUsername, txtPassword, txtJwt;
        Button btnLogin, btnForgotPassword, btnCreateAccount;

        TextBox txtGroup, txtReceiver, txtMessage;
        Button btnSend, btnRefresh;
        ListView lvMessages;

        readonly HttpClient _http = new() { BaseAddress = new Uri("https://localhost:7169/") };
        GroupChatClient? _groupClient;
        GroupKeyStoreClientService? _keyStore;
        CryptoService? _crypto;
        int _userId = 0;
        int _groupId = 0;

        public Form1()
        {
            Text = "CryptoChat2 Client";
            ClientSize = new Size(800, 700);

            txtUsername = new TextBox { PlaceholderText = "Username", Location = new Point(10, 10), Size = new Size(180, 25) };
            txtPassword = new TextBox { PlaceholderText = "Password", UseSystemPasswordChar = true, Location = new Point(200, 10), Size = new Size(180, 25) };
            btnLogin = new Button { Text = "Login", Location = new Point(390, 10), Size = new Size(80, 25) };
            btnLogin.Click += BtnClickLogin;
            btnForgotPassword = new Button { Text = "Forgot Password", Location = new Point(480, 10), Size = new Size(120, 25), Enabled = false };
            btnCreateAccount = new Button { Text = "Create Account", Location = new Point(610, 10), Size = new Size(120, 25), Enabled = false };
            txtJwt = new TextBox { PlaceholderText = "JWT Token", Location = new Point(10, 45), Size = new Size(760, 25), ReadOnly = true };

            txtGroup = new TextBox { PlaceholderText = "GroupId", Location = new Point(10, 80), Size = new Size(100, 25), Enabled = false };
            txtReceiver = new TextBox { PlaceholderText = "ReceiverId", Location = new Point(120, 80), Size = new Size(100, 25), Enabled = false };
            txtMessage = new TextBox { Multiline = true, Location = new Point(10, 115), Size = new Size(650, 80), Enabled = false };
            btnSend = new Button { Text = "Send", Location = new Point(670, 115), Size = new Size(100, 30), Enabled = false };
            btnSend.Click += BtnClickSend;
            btnRefresh = new Button { Text = "Refresh", Location = new Point(670, 155), Size = new Size(100, 30), Enabled = false };
            btnRefresh.Click += BtnClickRefresh;

            lvMessages = new ListView
            {
                Location = new Point(10, 210),
                Size = new Size(780, 480),
                View = View.Details,
                FullRowSelect = true,
                Enabled = false
            };
            lvMessages.Columns.Add("SenderId", 80);
            lvMessages.Columns.Add("DecryptedText", 540);
            lvMessages.Columns.Add("Timestamp", 140);

            Controls.AddRange(new Control[]
            {
                txtUsername, txtPassword, btnLogin,
                btnForgotPassword, btnCreateAccount, txtJwt,
                txtGroup, txtReceiver, txtMessage,
                btnSend, btnRefresh, lvMessages
            });
        }

        async void BtnClickLogin(object? sender, EventArgs e)
        {
            try
            {
                var loginReq = new
                {
                    username = txtUsername.Text,
                    password = txtPassword.Text
                };

                var resp = await _http.PostAsync("api/Auth/login",
                    new StringContent(JsonSerializer.Serialize(loginReq), Encoding.UTF8, "application/json"));
                resp.EnsureSuccessStatusCode();

                using var doc = await JsonDocument.ParseAsync(await resp.Content.ReadAsStreamAsync());
                var root = doc.RootElement;
                var token = root.GetProperty("token").GetString()!;
                _userId = int.Parse(root.GetProperty("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").GetString()!);

                txtJwt.Text = token;
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                txtGroup.Enabled = txtReceiver.Enabled = txtMessage.Enabled =
                btnSend.Enabled = btnRefresh.Enabled = lvMessages.Enabled = true;

                _keyStore = new GroupKeyStoreClientService();
                _crypto = new CryptoService(_keyStore);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Login failed: " + ex.Message);
            }
        }

        async void BtnClickSend(object? sender, EventArgs e)
        {
            try
            {
                _groupId = int.Parse(txtGroup.Text);
                int receiverId = int.Parse(txtReceiver.Text);

                await EnsureGroupClientReadyAsync();

                var messageDict = new Dictionary<int, string> { [receiverId] = txtMessage.Text };
                await _groupClient!.SendAsync(_userId, messageDict);
                await RefreshMessages();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Send failed: " + ex.Message);
            }
        }

        async void BtnClickRefresh(object? sender, EventArgs e)
        {
            try
            {
                await RefreshMessages();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Refresh failed: " + ex.Message);
            }
        }

        async Task EnsureGroupClientReadyAsync()
        {
            if (_keyStore == null || _crypto == null)
                throw new InvalidOperationException("Crypto not initialized");

            if (!_keyStore.HasPrivateKey(_groupId, _userId))
                _keyStore.GetOrCreatePrivateKey(_groupId, _userId);

            if (!_keyStore.HasPublicKey(_groupId, _userId))
                await _keyStore.UploadPublicKeyAsync(_http, _groupId, _userId);

            await _keyStore.FetchAndCachePublicKeysAsync(_http, _groupId, _userId);

            _groupClient ??= new GroupChatClient(_http, _crypto, _keyStore, _groupId, _userId);
        }

        async Task RefreshMessages()
        {
            _groupId = int.Parse(txtGroup.Text);
            await EnsureGroupClientReadyAsync();

            var messages = await _groupClient!.FetchAndDecryptAsync();
            lvMessages.Items.Clear();

            foreach (var msg in messages)
            {
                var parts = msg.Split(": ", 2);
                lvMessages.Items.Add(new ListViewItem(new[]
                {
                    parts[0], parts[1], DateTime.UtcNow.ToString("u")
                }));
            }
        }
    }
}
