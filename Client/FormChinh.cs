using SharedLibrary;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class FormChinh : Form
    {
        public FormChinh()
        {
            InitializeComponent();
        }

        public FormChinh(UserDto userInfo)
        {
            InitializeComponent();
        }
        /*
        private const string DINH_DANG_NGAY_GIO = "dd/MM/yyyy HH:mm";

        public FormChinh(string hoTen, string tenDangNhap, string email,
                         DateTime ngayTao, DateTime? lanDangNhapGanNhat) : this()
        {

            lblXinChao.Text = $"Xin chào, {hoTen}";
            lblTenDangNhapGiaTri.Text = tenDangNhap;
            lblEmailGiaTri.Text = string.IsNullOrWhiteSpace(email) ? "(chưa cập nhật)" : email;
            lblNgayTaoGiaTri.Text = ngayTao.ToString(DINH_DANG_NGAY_GIO);
            lblLanDangNhapGanNhatGiaTri.Text = lanDangNhapGanNhat.HasValue
                ? lanDangNhapGanNhat.Value.ToString(DINH_DANG_NGAY_GIO)
                : "(đây là lần đăng nhập đầu tiên)";
        }
        */

        private void btnDangXuat_Click_1(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }
    }
}
