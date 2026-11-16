using System;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharedProtocol;

namespace Client
{
    public partial class FormMain : Form
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private string _username;
        private Label lblWelcome;
        private Label lblInfo;
        private Button btnLogout;

        public FormMain(TcpClient client, NetworkStream stream, string username, string fullName, string email, string birthday)
        {
            _client = client;
            _stream = stream;
            _username = username;
            InitializeComponentCustom(fullName, email, birthday);
        }

        private void InitializeComponentCustom(string fullName, string email, string birthday)
        {
            this.Text = "Trang Chủ - Quản Lý Người Dùng";
            this.Size = new Size(450, 350);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            lblWelcome = new Label
            {
                Text = $"Xin chào, {fullName}!",
                Font = new Font("Arial", 14, FontStyle.Bold),
                Location = new Point(30, 30),
                AutoSize = true
            };
            this.Controls.Add(lblWelcome);

            string infoText = $"Tên đăng nhập: {_username}\n" +
                              $"Họ và tên: {fullName}\n" +
                              $"Email: {email}\n" +
                              $"Ngày sinh: {birthday}";

            lblInfo = new Label
            {
                Text = infoText,
                Font = new Font("Arial", 11, FontStyle.Regular),
                Location = new Point(30, 80),
                Size = new Size(380, 120)
            };
            this.Controls.Add(lblInfo);

            btnLogout = new Button
            {
                Text = "Đăng Xuất",
                Location = new Point(30, 220),
                Width = 120,
                Height = 35,
                BackColor = Color.LightCoral
            };
            btnLogout.Click += BtnLogout_Click;
            this.Controls.Add(btnLogout);
        }

        private async void BtnLogout_Click(object sender, EventArgs e)
        {
            await PerformLogoutAsync();
        }

        private async Task PerformLogoutAsync()
        {
            try
            {
                string message = $"{ProtocolCommands.DANGXUAT}|{_username}";
                await MessageFraming.SendMessageAsync(_stream, message);
            }
            catch
            {
                // Ignore network errors on logout
            }
            finally
            {
                CloseConnectionAndReturnToLogin();
            }
        }

        private void CloseConnectionAndReturnToLogin()
        {
            try
            {
                _stream?.Close();
                _client?.Close();
            }
            catch { }

            this.Hide();
            new FormDangNhap().Show();
            this.Close();
        }

        protected override async void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (e.CloseReason == CloseReason.UserClosing)
            {
                try
                {
                    string message = $"{ProtocolCommands.DANGXUAT}|{_username}";
                    await MessageFraming.SendMessageAsync(_stream, message);
                }
                catch { }
                finally
                {
                    try
                    {
                        _stream?.Close();
                        _client?.Close();
                    }
                    catch { }
                }
            }
        }
    }
}
