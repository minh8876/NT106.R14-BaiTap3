using SharedLibrary;
using System;
using System.Data.Common;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Client
{
    public partial class FormDangNhap : Form
    {
     
        private readonly KhuVucKetNoi khuVucKetNoi = new KhuVucKetNoi();


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
            if (!DocCauHinhKetNoi())
            {
                return; 
            }

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

            try
            {
                var request = new LoginRequest { Username = username, Password = password };

                LoginResponse response = await SimulateSendRequestAsync(request);

                if (response.IsSuccess)
                {
                    FormChinh mainForm = new FormChinh(response.UserInfo);
                    this.Hide();
                    mainForm.ShowDialog(); 

                    this.Show();
                    txtMatKhau.Clear();
                }
                else
                {
                    MessageBox.Show(response.ErrorMessage, "Lỗi đăng nhập", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtMatKhau.Clear();
                    txtMatKhau.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể kết nối đến máy chủ. Vui lòng thử lại sau.", "Lỗi mạng", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnDangNhap.Enabled = true;
                txtTenDangNhap.Enabled = true;
                txtMatKhau.Enabled = true;
            }
        }

        private void btnThoat_Click_1(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void lnkDangKy_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
        {
            
            if (!DocCauHinhKetNoi())
            {
                return;
            }

            using (var frmDangKy = new FormDangKy())
            {
                frmDangKy.DiaChiMayChu = DiaChiMayChu;
                frmDangKy.CongMayChu = CongMayChu;

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
        }

        private Task<LoginResponse> SimulateSendRequestAsync(LoginRequest request)
        {
            return Task.Delay(1000).ContinueWith(t => new LoginResponse { IsSuccess = false, ErrorMessage = "Đây chỉ là mô phỏng." });
        }

        private void txtTenDangNhap_TextChanged(object sender, EventArgs e)
        {
            lblLoi.Text = string.Empty;
        }

        private void txtMatKhau_TextChanged(object sender, EventArgs e)
        {
            lblLoi.Text = string.Empty;
        }
    }
}
