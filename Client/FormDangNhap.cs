using System;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharedProtocol;

namespace Client
{
    public partial class FormDangNhap : Form
    {
        private TextBox txtServerIp;
        private TextBox txtServerPort;
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private Button btnOpenRegister;
        private Label lblStatus;

        public FormDangNhap()
        {
            InitializeComponentCustom();
        }

        private void InitializeComponentCustom()
        {
            this.Text = "Đăng Nhập - Quản Lý Người Dùng";
            this.Size = new Size(420, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            int y = 20;
            AddLabel("IP Server:", 30, ref y);
            txtServerIp = new TextBox { Location = new Point(140, y), Width = 220, Text = "127.0.0.1" };
            this.Controls.Add(txtServerIp);
            y += 40;

            AddLabel("Cổng Server:", 30, ref y);
            txtServerPort = new TextBox { Location = new Point(140, y), Width = 220, Text = "8888" };
            this.Controls.Add(txtServerPort);
            y += 40;

            AddLabel("Tên đăng nhập:", 30, ref y);
            txtUsername = new TextBox { Location = new Point(140, y), Width = 220 };
            this.Controls.Add(txtUsername);
            y += 40;

            AddLabel("Mật khẩu:", 30, ref y);
            txtPassword = new TextBox { Location = new Point(140, y), Width = 220, PasswordChar = '*' };
            this.Controls.Add(txtPassword);
            y += 50;

            btnLogin = new Button { Text = "Đăng Nhập", Location = new Point(140, y), Width = 100, Height = 30, BackColor = Color.LightSkyBlue };
            btnLogin.Click += BtnLogin_Click;
            this.Controls.Add(btnLogin);

            btnOpenRegister = new Button { Text = "Đăng Ký", Location = new Point(250, y), Width = 110, Height = 30 };
            btnOpenRegister.Click += (s, e) => { new FormDangKy().ShowDialog(); };
            this.Controls.Add(btnOpenRegister);
            y += 50;

            lblStatus = new Label { Location = new Point(30, y), Width = 330, Text = "Trạng thái: Sẵn sàng", ForeColor = Color.DarkSlateGray };
            this.Controls.Add(lblStatus);
        }

        private void AddLabel(string text, int x, ref int y)
        {
            Label lbl = new Label { Text = text, Location = new Point(x, y + 3), Width = 100 };
            this.Controls.Add(lbl);
        }

        private async void BtnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu!", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string ip = txtServerIp.Text.Trim();
            if (!int.TryParse(txtServerPort.Text.Trim(), out int port))
            {
                MessageBox.Show("Cổng server không hợp lệ!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            btnLogin.Enabled = false;
            btnOpenRegister.Enabled = false;
            lblStatus.Text = "Trạng thái: Đang kết nối tới server...";

            TcpClient client = null;
            NetworkStream stream = null;

            try
            {
                client = new TcpClient();
                await client.ConnectAsync(ip, port);
                stream = client.GetStream();

                string request = $"{ProtocolCommands.DANGNHAP}|{username}|{password}";
                await MessageFraming.SendMessageAsync(stream, request);

                lblStatus.Text = "Trạng thái: Đang chờ phản hồi...";
                string response = await MessageFraming.ReceiveMessageAsync(stream);

                string[] parts = response.Split('|');
                if (parts[0] == ProtocolResponses.SUCCESS && parts.Length >= 4)
                {
                    string fullName = parts[1];
                    string email = parts[2];
                    string birthday = parts[3];

                    MessageBox.Show("Đăng nhập thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    this.Hide();
                    FormMain mainForm = new FormMain(client, stream, username, fullName, email, birthday);
                    mainForm.Closed += (s, args) => this.Close();
                    mainForm.Show();
                }
                else
                {
                    MessageBox.Show("Tên đăng nhập hoặc mật khẩu không chính xác!", "Đăng nhập thất bại", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtPassword.Clear();
                    txtPassword.Focus();
                    stream?.Close();
                    client?.Close();
                }
            }
            catch (SocketException)
            {
                MessageBox.Show("Không thể kết nối đến server. Vui lòng kiểm tra lại IP/Cổng hoặc trạng thái Server.", "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
                stream?.Close();
                client?.Close();
            }
            catch (Exception)
            {
                MessageBox.Show("Đã xảy ra lỗi đường truyền mạng.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                stream?.Close();
                client?.Close();
            }
            finally
            {
                btnLogin.Enabled = true;
                btnOpenRegister.Enabled = true;
                lblStatus.Text = "Trạng thái: Sẵn sàng";
            }
        }
    }
}