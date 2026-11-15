using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.Common;
using System.IO;
using System.Net.Sockets;


namespace QuanLyNguoiDung
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

        public void XoaLoi()
        {
            lblLoi.Text = string.Empty;
        }

       
        private void DatTrangThaiBan(bool dangBan)
        {
            btnDangNhap.Enabled = !dangBan;
            btnThoat.Enabled = !dangBan;
            lnkDangKy.Enabled = !dangBan;
            khuVucKetNoi.Khoa(dangBan);
            btnDangNhap.Text = dangBan ? "Đang xử lý..." : "Đăng nhập";
            UseWaitCursor = dangBan;
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
            if (string.IsNullOrWhiteSpace(txtTenDangNhap.Text) || string.IsNullOrWhiteSpace(txtMatKhau.Text))
            {
                lblLoi.Text = "Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.";
                return;
            }

            if (!DocCauHinhKetNoi())
            {
                return;
            }

            lblLoi.Text = "";

            DatTrangThaiBan(true);
            khuVucKetNoi.DatTrangThai(TrangThaiKetNoi.DangKetNoi);

            try
            {
                // TODO (thành viên làm phần mạng): mở kết nối TCP tới DiaChiMayChu:CongMayChu tại đây,
                // rồi thay các lệnh truy vấn CSDL bên dưới bằng lệnh DANGNHAP gửi tới server.

                var db = new TruyCapDuLieu();

                // 1. Fetch login credentials for password verification
                var userRecord = await db.LayThongTinDangNhapAsync(txtTenDangNhap.Text);

                // Security check: Same error message for invalid user or wrong password
                if (userRecord == null || !MatKhau.KiemTra(txtMatKhau.Text, userRecord.Salt, userRecord.MatKhauBam))
                {
                    khuVucKetNoi.DatTrangThai(TrangThaiKetNoi.DaKetNoi);
                    lblLoi.Text = "Tên đăng nhập hoặc mật khẩu không đúng";
                    txtMatKhau.Clear();
                    txtMatKhau.Focus();
                    return;
                }

                // 2. Update last login timestamp in database
                await db.CapNhatLanDangNhapCuoiAsync(userRecord.UserId);

                // 3. Fetch form display data (contains Email, NgayTao, LanDangNhapCuoi)
                var formInfo = await db.LayThongTinFormChinhAsync(txtTenDangNhap.Text);

                khuVucKetNoi.DatTrangThai(TrangThaiKetNoi.DaKetNoi);

                if (formInfo != null)
                {
                    using (FormChinh frmChinh = new FormChinh(
                        formInfo.HoTen,
                        formInfo.TenDangNhap,
                        formInfo.Email,
                        formInfo.NgayTao,
                        formInfo.LanDangNhapCuoi
                    ))
                    {
                        this.Hide();
                        frmChinh.ShowDialog();   // đóng lại khi bấm Đăng xuất hoặc nút đỏ
                    }

                    // Đăng xuất xong thì quay lại form Đăng nhập (trước đây là this.Close())
                    // TODO (thành viên làm phần mạng): đóng kết nối TCP tại đây
                    txtMatKhau.Clear();
                    khuVucKetNoi.DatTrangThai(TrangThaiKetNoi.ChuaKetNoi);
                    this.Show();
                }
            }
            catch (SocketException)
            {
                khuVucKetNoi.DatTrangThai(TrangThaiKetNoi.ChuaKetNoi);
                MessageBox.Show(ThongBaoLoiMang.KhongKetNoiDuoc, "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (IOException)
            {
                khuVucKetNoi.DatTrangThai(TrangThaiKetNoi.ChuaKetNoi);
                MessageBox.Show(ThongBaoLoiMang.MatKetNoi, "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (DbException)
            {
                // Tạm giữ cho code cũ nối thẳng SQL Server; xoá khi chuyển hết truy vấn sang server
                khuVucKetNoi.DatTrangThai(TrangThaiKetNoi.ChuaKetNoi);
                MessageBox.Show(ThongBaoLoiMang.LoiCoSoDuLieu, "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                DatTrangThaiBan(false);
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

                XoaLoi();
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

        private void txtTenDangNhap_TextChanged(object sender, EventArgs e)
        {
            XoaLoi();
        }

        private void txtMatKhau_TextChanged(object sender, EventArgs e)
        {
            XoaLoi();
        }
    }
}
