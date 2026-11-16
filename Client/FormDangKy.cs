using System;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharedProtocol;

namespace Client
{
    public partial class FormDangKy : Form
    {
        private TextBox txtServerIp;
        private TextBox txtServerPort;
        private TextBox txtUsername;
        private TextBox txtPassword;
        private TextBox txtConfirmPassword;
        private TextBox txtEmail;
        private TextBox txtFullName;
        private DateTimePicker dtpBirthday;
        private Button btnRegister;
        private Label lblStatus;

        public FormDangKy()
        {
            InitializeComponentCustom();
        }

        private void InitializeComponentCustom()
        {
            this.Text = "Đăng Ký Tài Khoản Mới";
            this.Size = new Size(450, 520);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            int y = 20;
            AddLabel("IP Server:", 30, ref y);
            txtServerIp = new TextBox { Location = new Point(150, y), Width = 240, Text = "127.0.0.1" };
            this.Controls.Add(txtServerIp);
            y += 40;

            AddLabel("Cổng Server:", 30, ref y);
            txtServerPort = new TextBox { Location = new Point(150, y), Width = 240, Text = "8888" };
            this.Controls.Add(txtServerPort);
            y += 40;

            AddLabel("Tên đăng nhập:", 30, ref y);
            txtUsername = new TextBox { Location = new Point(150, y), Width = 240 };
            this.Controls.Add(txtUsername);
            y += 40;

            AddLabel("Mật khẩu:", 30, ref y);
            txtPassword = new TextBox { Location = new Point(150, y), Width = 240, PasswordChar = '*' };
            this.Controls.Add(txtPassword);
            y += 40;

            AddLabel("Xác nhận MK:", 30, ref y);
            txtConfirmPassword = new TextBox { Location = new Point(150, y), Width = 240, PasswordChar = '*' };
            this.Controls.Add(txtConfirmPassword);
            y += 40;

            AddLabel("Email:", 30, ref y);
            txtEmail = new TextBox { Location = new Point(150, y), Width = 240 };
            this.Controls.Add(txtEmail);
            y += 40;

            AddLabel("Họ và tên:", 30, ref y);
            txtFullName = new TextBox { Location = new Point(150, y), Width = 240 };
            this.Controls.Add(txtFullName);
            y += 40;

            AddLabel("Ngày sinh:", 30, ref y);
            dtpBirthday = new DateTimePicker { Location = new Point(150, y), Width = 240, Format = DateTimePickerFormat.Short };
            this.Controls.Add(dtpBirthday);
            y += 50;

            btnRegister = new Button { Text = "Đăng Ký", Location = new Point(150, y), Width = 110, Height = 30, BackColor = Color.LightGreen };
            btnRegister.Click += BtnRegister_Click;
            this.Controls.Add(btnRegister);
            y += 40;

            lblStatus = new Label { Location = new Point(30, y), Width = 360, Text = "Trạng thái: Sẵn sàng" };
            this.Controls.Add(lblStatus);
        }

        private void AddLabel(string text, int x, ref int y)
        {
            Label lbl = new Label { Text = text, Location = new Point(x, y + 3), Width = 110 };
            this.Controls.Add(lbl);
        }

        private async void BtnRegister_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;
            string confirmPassword = txtConfirmPassword.Text;
            string email = txtEmail.Text.Trim();
            string fullname = txtFullName.Text.Trim();
            string birthday = dtpBirthday.Value.ToString("yyyy-MM-dd");

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(fullname))
            {
                MessageBox.Show("Vui lòng điền đầy đủ tất cả các trường bắt buộc!", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Mật khẩu và xác nhận mật khẩu không khớp nhau!", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (!Regex.IsMatch(email, emailPattern))
            {
                MessageBox.Show("Định dạng email không hợp lệ!", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string ip = txtServerIp.Text.Trim();
            if (!int.TryParse(txtServerPort.Text.Trim(), out int port))
            {
                MessageBox.Show("Cổng server không hợp lệ!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            btnRegister.Enabled = false;
            lblStatus.Text = "Trạng thái: Đang kết nối server...";

            try
            {
                using (TcpClient client = new TcpClient())
                {
                    await client.ConnectAsync(ip, port);
                    using (NetworkStream stream = client.GetStream())
                    {
                        string message = $"{ProtocolCommands.DANGKY}|{username}|{password}|{email}|{fullname}|{birthday}";
                        await MessageFraming.SendMessageAsync(stream, message);

                        lblStatus.Text = "Trạng thái: Đang chờ phản hồi...";
                        string response = await MessageFraming.ReceiveMessageAsync(stream);

                        if (response == ProtocolResponses.SUCCESS)
                        {
                            MessageBox.Show("Đăng ký tài khoản thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            this.Close();
                        }
                        else if (response == ProtocolResponses.ERR_USER_EXISTS)
                        {
                            MessageBox.Show("Tên đăng nhập đã tồn tại trong hệ thống. Vui lòng chọn tên khác.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        else
                        {
                            MessageBox.Show("Đăng ký thất bại do lỗi hệ thống.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (SocketException)
            {
                MessageBox.Show("Không thể kết nối đến server. Vui lòng kiểm tra lại IP/Cổng hoặc trạng thái Server.", "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception)
            {
                MessageBox.Show("Đã xảy ra lỗi đường truyền mạng.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnRegister.Enabled = true;
                lblStatus.Text = "Trạng thái: Sẵn sàng";
            }
        }
    }
}