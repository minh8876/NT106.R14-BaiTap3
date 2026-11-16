using SharedLibrary;
using System.Net.Sockets;
using System.Windows.Forms;

namespace Client
{
    public partial class FormDangNhap : Form
    {
        private readonly KhuVucKetNoi khuVucKetNoi = new();
        public string DiaChiMayChu { get; private set; } = "127.0.0.1";
        public int CongMayChu { get; private set; } = 5000;

        public FormDangNhap()
        {
            InitializeComponent();
            GoiYTextBox.Dat(txtTenDangNhap, "nguyenvana");
            ClientSize = new Size(ClientSize.Width, ClientSize.Height + khuVucKetNoi.Height);
            Controls.Add(khuVucKetNoi);
        }

        private bool DocCauHinhKetNoi()
        {
            if (!khuVucKetNoi.LayDiaChiVaCong(out string diaChi, out int cong, out string loi))
            {
                lblLoi.Text = loi;
                return false;
            }

            DiaChiMayChu = diaChi;
            CongMayChu = cong;
            return true;
        }

        private async void btnDangNhap_Click_1(object sender, EventArgs e)
        {
            if (!DocCauHinhKetNoi()) return;

            string username = txtTenDangNhap.Text.Trim();
            string password = txtMatKhau.Text;
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vui lòng nhập tài khoản và mật khẩu.");
                return;
            }

            btnDangNhap.Enabled = false;
            txtTenDangNhap.Enabled = false;
            txtMatKhau.Enabled = false;
            TcpClientService? connection = null;

            try
            {
                connection = new TcpClientService();
                await connection.ConnectAsync(DiaChiMayChu, CongMayChu);
                ServerResponse response = await connection.SendAsync(new ClientRequest
                {
                    Operation = ProtocolOperations.Login,
                    Username = username,
                    Password = password
                });

                if (!response.Success)
                {
                    MessageBox.Show(response.ErrorMessage, "Lỗi đăng nhập", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                await using (connection)
                {
                    FormChinh mainForm = new(response.UserInfo, connection);
                    connection = null;
                    Hide();
                    mainForm.ShowDialog();
                    Show();
                }

                txtMatKhau.Clear();
            }
            catch (SocketException)
            {
                MessageBox.Show("Không thể kết nối đến máy chủ.", "Lỗi mạng", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (IOException)
            {
                MessageBox.Show("Kết nối đến máy chủ đã bị gián đoạn.", "Lỗi mạng", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (connection is not null) await connection.DisposeAsync();
                btnDangNhap.Enabled = true;
                txtTenDangNhap.Enabled = true;
                txtMatKhau.Enabled = true;
            }
        }

        private void btnThoat_Click_1(object sender, EventArgs e) => Application.Exit();

        private void lnkDangKy_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (!DocCauHinhKetNoi()) return;

            using var frmDangKy = new FormDangKy(DiaChiMayChu, CongMayChu);
            Hide();
            DialogResult ketQua = frmDangKy.ShowDialog();
            Show();
            lblLoi.Text = string.Empty;
            if (ketQua == DialogResult.OK)
            {
                txtTenDangNhap.Text = frmDangKy.TenDangNhapVuaTao;
                txtMatKhau.Clear();
                txtMatKhau.Focus();
            }
            else
            {
                txtTenDangNhap.Focus();
            }
        }

        private void txtTenDangNhap_TextChanged(object sender, EventArgs e) => lblLoi.Text = string.Empty;
        private void txtMatKhau_TextChanged(object sender, EventArgs e) => lblLoi.Text = string.Empty;
    }
}
