using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsClient;

public class MainForm : Form
{
    public MainForm()
    {
        Text = "CryptoChat2";
        ClientSize = new Size(750, 600);
        LoadLoginPanel();
    }

    private void LoadLoginPanel()
    {
        var login = new LoginPanel { Dock = DockStyle.Fill };
        login.OnLoginSuccess = async session => await LoadChatPanel(session);
        Controls.Clear();
        Controls.Add(login);
    }

    private async Task LoadChatPanel(UserSession session)
    {
        Controls.Clear();
        var chat = new ChatPanel(session) { Dock = DockStyle.Fill };
        Controls.Add(chat);
    }
}