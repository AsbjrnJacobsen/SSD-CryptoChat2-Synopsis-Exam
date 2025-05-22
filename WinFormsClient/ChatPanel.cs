using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsClient;

public class ChatPanel : Panel
{
    private readonly UserSession _session;
    private readonly TextBox _txtReceiverId = new() { PlaceholderText = "ReceiverId", Width = 100 };
    private readonly TextBox _txtMessage = new() { Multiline = true, Width = 500, Height = 80 };
    private readonly Button _btnSend = new() { Text = "Send", Width = 80 };
    private readonly Button _btnRefresh = new() { Text = "Refresh", Width = 80 };
    private readonly ListView _lvMessages = new() { Width = 700, Height = 400 };

    public ChatPanel(UserSession session)
    {
        _session = session;

        _txtReceiverId.Location = new Point(10, 10);
        _txtMessage.Location = new Point(120, 10);
        _btnSend.Location = new Point(630, 10);
        _btnRefresh.Location = new Point(630, 50);
        _btnSend.Click += BtnClickSend;
        _btnRefresh.Click += BtnClickRefresh;

        _lvMessages.Location = new Point(10, 100);
        _lvMessages.View = View.Details;
        _lvMessages.Columns.Add("SenderId", 80);
        _lvMessages.Columns.Add("DecryptedText", 500);
        _lvMessages.Columns.Add("Timestamp", 120);
        _lvMessages.FullRowSelect = true;

        Controls.AddRange([_txtReceiverId, _txtMessage, _btnSend, _btnRefresh, _lvMessages]);
    }

    private async void BtnClickSend(object? sender, EventArgs e)
    {
        int receiverId = int.Parse(_txtReceiverId.Text);
        var message = _txtMessage.Text;
        var msgPerRecipient = new Dictionary<int, string> { [receiverId] = message };

        await _session.ChatClient.SendAsync(_session.UserId, msgPerRecipient);
        await RefreshMessagesAsync();
    }

    private async void BtnClickRefresh(object? sender, EventArgs e)
    {
        await RefreshMessagesAsync();
    }

    private async Task RefreshMessagesAsync()
    {
        var messages = await _session.ChatClient.FetchAndDecryptAsync();
        _lvMessages.Items.Clear();

        foreach (var msg in messages)
        {
            var parts = msg.Split(": ", 2);
            _lvMessages.Items.Add(new ListViewItem(new[] { parts[0], parts[1], DateTime.UtcNow.ToString("u") }));
        }
    }
}
