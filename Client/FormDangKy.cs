using System;
using System.Data.Common;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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

        private static readonly Regex MauEmail =
            new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

        public string TenDangNhapVuaTao { get; private set; } = string.Empty;

        // FormDangNhap gán hai giá trị này trước khi mở form, để thành viên làm phần mạng dùng khi ConnectAsync
        public string DiaChiMayChu { get; set; } = "127.0.0.1";
        public int CongMayChu { get; set; } = 5000;

        public FormDangKy()
        {
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

        private void txtTenDangNhap_TextChanged(object sender, EventArgs e)
        {
            lblLoiTenDangNhap.Text = string.Empty;
            lblLoiChung.Text = string.Empty;
        }

        private void txtHoTen_TextChanged(object sender, EventArgs e)
        {
            lblLoiHoTen.Text = string.Empty;
            lblLoiChung.Text = string.Empty;
        }

        private void txtMatKhau_TextChanged(object sender, EventArgs e)
        {
            lblLoiMatKhau.Text = string.Empty;
            lblLoiChung.Text = string.Empty;
        }

        private void txtXacNhanMatKhau_TextChanged(object sender, EventArgs e)
        {
            lblLoiXacNhanMatKhau.Text = string.Empty;
            lblLoiChung.Text = string.Empty;
        }

        private void txtEmail_TextChanged(object sender, EventArgs e)
        {
            lblLoiEmail.Text = string.Empty;
            lblLoiChung.Text = string.Empty;
        }

        // Khoá/mở các nút trong lúc chờ server
        private void DatTrangThaiBan(bool dangBan)
        {
            btnDangKy.Enabled = !dangBan;
            btnHuy.Enabled = !dangBan;
            btnDangKy.Text = dangBan ? "Đang xử lý..." : "Đăng ký";
            UseWaitCursor = dangBan;
        }

        private async void btnDangKy_Click_1(object sender, EventArgs e)
        {// 1. Kiểm tra dữ liệu nhập tại Form[cite: 26]
            if (string.IsNullOrWhiteSpace(txtTenDangNhap.Text))
            {
                lblLoiTenDangNhap.Text = "Tên đăng nhập không được để trống.";  
                txtTenDangNhap.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(txtMatKhau.Text) || txtMatKhau.Text != txtXacNhanMatKhau.Text)
            {
                lblLoiMatKhau.Text = "Mật khẩu không được để trống và phải khớp nhau.";
                txtMatKhau.Focus();
                return;
            }

            lblLoiTenDangNhap.Text = "";
            lblLoiMatKhau.Text = "";

            // 2. Khóa nút bấm tránh người dùng spam click
            DatTrangThaiBan(true);
            bool thanhCong = false;

            try
            {
                // TODO (thành viên làm phần mạng): mở kết nối TCP tới DiaChiMayChu:CongMayChu tại đây,
                // rồi thay các lệnh truy vấn CSDL và băm mật khẩu bên dưới bằng lệnh DANGKY gửi tới server.

                var db = new TruyCapDuLieu();

                // 3. Kiểm tra tên đăng nhập đã tồn tại chưa[cite: 26]
                bool tonTai = await db.KiemTraTenDangNhapTonTaiAsync(txtTenDangNhap.Text);
                if (tonTai)
                {
                    lblLoiTenDangNhap.Text = "Tên này đã có người dùng"; // trước đây hiện nhầm ở lblLoiHoTen
                    return;
                }

                // 4. Băm mật khẩu ra Chuỗi Băm và Salt[cite: 28, 29]
                var (salt, bam) = MatKhau.Tao(txtMatKhau.Text);

                string hoTen = string.IsNullOrWhiteSpace(txtHoTen.Text) ? "Người dùng mới" : txtHoTen.Text.Trim();
                string email = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text;

                // 5. Thêm tài khoản vào cơ sở dữ liệu[cite: 26, 28]
                await db.ThemTaiKhoanAsync(txtTenDangNhap.Text, bam, salt, hoTen, email);

                thanhCong = true;
            }
            catch (SocketException)
            {
                MessageBox.Show(ThongBaoLoiMang.KhongKetNoiDuoc, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (IOException)
            {
                MessageBox.Show(ThongBaoLoiMang.MatKetNoi, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (DbException)
            {
                // Tạm giữ cho code cũ nối thẳng SQL Server; xoá khi chuyển hết truy vấn sang server
                MessageBox.Show(ThongBaoLoiMang.LoiCoSoDuLieu, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                DatTrangThaiBan(false);
            }

            if (thanhCong)
            {
                MessageBox.Show("Đăng ký thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                TenDangNhapVuaTao = txtTenDangNhap.Text;
                DialogResult = DialogResult.OK;   // đóng form Đăng ký, FormDangNhap điền sẵn tên đăng nhập
            }
        }

        private void btnHuy_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
