using SharedLibrary;
using System.ComponentModel;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Client
{
    public partial class FormDangKy : Form
    {
        private const int TEN_DANG_NHAP_MIN = 3;
        private const int TEN_DANG_NHAP_MAX = 20;
        private const int MAT_KHAU_MIN = 8;
        private const int MAT_KHAU_MAX = 64;
        private const int HO_TEN_MAX = 50;
        private const int EMAIL_MAX = 100;
        private static readonly Regex MauEmail = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

        public string TenDangNhapVuaTao { get; private set; } = string.Empty;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string DiaChiMayChu { get; set; } = "127.0.0.1";

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int CongMayChu { get; set; } = 5000;

        public FormDangKy() : this("127.0.0.1", 5000) { }

        public FormDangKy(string diaChiMayChu, int congMayChu)
        {
            DiaChiMayChu = diaChiMayChu;
            CongMayChu = congMayChu;
            InitializeComponent();
            GanSuKien();
            GoiYTextBox.Dat(txtTenDangNhap, "3-20 ký tự");
            GoiYTextBox.Dat(txtHoTen, "Nguyễn Văn A");
            GoiYTextBox.Dat(txtMatKhau, "tối thiểu 8 ký tự");
            GoiYTextBox.Dat(txtXacNhanMatKhau, "nhập lại mật khẩu");
            GoiYTextBox.Dat(txtEmail, "ten@mien.com");
        }

        private void GanSuKien()
        {
            txtTenDangNhap.TextChanged += txtTenDangNhap_TextChanged;
            txtHoTen.TextChanged += txtHoTen_TextChanged;
            txtMatKhau.TextChanged += txtMatKhau_TextChanged;
            txtXacNhanMatKhau.TextChanged += txtXacNhanMatKhau_TextChanged;
            txtEmail.TextChanged += txtEmail_TextChanged;
        }

        private void txtTenDangNhap_TextChanged(object sender, EventArgs e) => XoaLoi(lblLoiTenDangNhap);
        private void txtHoTen_TextChanged(object sender, EventArgs e) => XoaLoi(lblLoiHoTen);
        private void txtMatKhau_TextChanged(object sender, EventArgs e) => XoaLoi(lblLoiMatKhau);
        private void txtXacNhanMatKhau_TextChanged(object sender, EventArgs e) => XoaLoi(lblLoiXacNhanMatKhau);
        private void txtEmail_TextChanged(object sender, EventArgs e) => XoaLoi(lblLoiEmail);

        private static void XoaLoi(Label label) => label.Text = string.Empty;

        private void DatTrangThaiBan(bool dangBan)
        {
            btnDangKy.Enabled = !dangBan;
            btnHuy.Enabled = !dangBan;
            btnDangKy.Text = dangBan ? "Đang xử lý..." : "Đăng ký";
            UseWaitCursor = dangBan;
        }

        private async void btnDangKy_Click_1(object sender, EventArgs e)
        {
            if (!KiemTraDuLieu()) return;
            DatTrangThaiBan(true);
            TcpClientService? connection = null;

            try
            {
                connection = new TcpClientService();
                await connection.ConnectAsync(DiaChiMayChu, CongMayChu);
                ServerResponse response = await connection.SendAsync(new ClientRequest
                {
                    Operation = ProtocolOperations.Register,
                    Username = txtTenDangNhap.Text.Trim(),
                    Password = txtMatKhau.Text,
                    FullName = txtHoTen.Text.Trim(),
                    Email = txtEmail.Text.Trim()
                });

                if (!response.Success)
                {
                    lblLoiChung.Text = response.ErrorMessage;
                    return;
                }

                TenDangNhapVuaTao = txtTenDangNhap.Text.Trim();
                MessageBox.Show("Đăng ký thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
            }
            catch (SocketException)
            {
                lblLoiChung.Text = "Không thể kết nối đến máy chủ.";
            }
            catch (IOException)
            {
                lblLoiChung.Text = "Kết nối đến máy chủ đã bị gián đoạn.";
            }
            finally
            {
                if (connection is not null) await connection.DisposeAsync();
                DatTrangThaiBan(false);
            }
        }

        private bool KiemTraDuLieu()
        {
            string username = txtTenDangNhap.Text.Trim();
            string password = txtMatKhau.Text;
            string fullName = txtHoTen.Text.Trim();
            string email = txtEmail.Text.Trim();

            if (username.Length is < TEN_DANG_NHAP_MIN or > TEN_DANG_NHAP_MAX)
            {
                lblLoiTenDangNhap.Text = $"Tên đăng nhập phải dài {TEN_DANG_NHAP_MIN}-{TEN_DANG_NHAP_MAX} ký tự.";
                return false;
            }
            if (password.Length is < MAT_KHAU_MIN or > MAT_KHAU_MAX || password != txtXacNhanMatKhau.Text)
            {
                lblLoiMatKhau.Text = "Mật khẩu phải dài 8-64 ký tự và khớp xác nhận.";
                return false;
            }
            if (fullName.Length > HO_TEN_MAX)
            {
                lblLoiHoTen.Text = $"Họ tên không được vượt quá {HO_TEN_MAX} ký tự.";
                return false;
            }
            if (email.Length > EMAIL_MAX || (email.Length > 0 && !MauEmail.IsMatch(email)))
            {
                lblLoiEmail.Text = "Email không hợp lệ.";
                return false;
            }

            lblLoiChung.Text = string.Empty;
            return true;
        }

        private void btnHuy_Click(object sender, EventArgs e) => DialogResult = DialogResult.Cancel;
    }
}
