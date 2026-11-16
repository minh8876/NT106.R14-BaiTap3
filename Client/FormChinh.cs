using SharedLibrary;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net.Sockets;
using System.Text;


namespace Client
{
    public partial class FormChinh : Form
    {
        private const string DINH_DANG_NGAY_GIO = "dd/MM/yyyy HH:mm";
        private readonly TcpClient _tcpClient;
        private readonly StreamWriter _writer;
        private bool _daDongKetNoi = false;

        public FormChinh()
        {
            InitializeComponent();
        }

        public FormChinh(UserDto userInfo, TcpClient tcpClient = null) : this()
        {
            this.FormClosing += FormChinh_FormClosing;
            _tcpClient = tcpClient;

            if (_tcpClient != null && _tcpClient.Connected)
            {
                _writer = new StreamWriter(_tcpClient.GetStream(), new UTF8Encoding(false)) { AutoFlush = true };
            }

            if (userInfo != null)
            {
                lblXinChao.Text = $"Xin chào, {(string.IsNullOrWhiteSpace(userInfo.FullName) ? userInfo.Username : userInfo.FullName)}";
                lblTenDangNhapGiaTri.Text = userInfo.Username;
                lblEmailGiaTri.Text = string.IsNullOrWhiteSpace(userInfo.Email) ? "(chưa cập nhật)" : userInfo.Email;
                lblNgayTaoGiaTri.Text = "(chưa có dữ liệu)";
                lblLanDangNhapGanNhatGiaTri.Text = "(đây là lần đăng nhập đầu tiên)";
            }
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

        private async Task GuiLenhDangXuatVaDongKetNoiAsync()
        {
            if (_daDongKetNoi) return;
            _daDongKetNoi = true;

            try
            {
                if (_tcpClient != null && _tcpClient.Connected && _writer != null)
                {
                    await _writer.WriteLineAsync("DANGXUAT");
                }
            }
            catch (SocketException) { }
            catch (IOException) { }
            finally
            {
                try { _writer?.Dispose(); } catch { }
                try { _tcpClient?.Close(); } catch { }
            }
        }

        private async void btnDangXuat_Click_1(object sender, EventArgs e)
        {
            btnDangXuat.Enabled = false;
            await GuiLenhDangXuatVaDongKetNoiAsync();
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private async void FormChinh_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_daDongKetNoi)
            {
                await GuiLenhDangXuatVaDongKetNoiAsync();
            }
        }

    }
}
