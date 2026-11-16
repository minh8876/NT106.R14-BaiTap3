using SharedLibrary;
using SharedLibrary;
using System.IO;
using System.Net.Sockets;
using System.Windows.Forms;

namespace Client
{
    public partial class FormChinh : Form
    {
        private const string DINH_DANG_NGAY_GIO = "dd/MM/yyyy HH:mm";
        private readonly TcpClientService? _connection;
        private bool _daDongKetNoi = false;
        private bool _daLayThongTin;

        public FormChinh()
        {
            InitializeComponent();
        }

        public FormChinh(UserDto userInfo, TcpClientService connection) : this()
        {
            FormClosing += FormChinh_FormClosing;
            Shown += FormChinh_Shown;
            _connection = connection;

            if (userInfo != null)
            {
                lblXinChao.Text = $"Xin chào, {(string.IsNullOrWhiteSpace(userInfo.FullName) ? userInfo.Username : userInfo.FullName)}";
                lblTenDangNhapGiaTri.Text = userInfo.Username;
                lblEmailGiaTri.Text = string.IsNullOrWhiteSpace(userInfo.Email) ? "(chưa cập nhật)" : userInfo.Email;
                lblNgayTaoGiaTri.Text = "(chưa có dữ liệu)";
                lblLanDangNhapGanNhatGiaTri.Text = "(đây là lần đăng nhập đầu tiên)";
            }
        }

        private async void FormChinh_Shown(object? sender, EventArgs e)
        {
            if (_daLayThongTin || _connection is null) return;
            _daLayThongTin = true;

            try
            {
                ServerResponse response = await _connection.SendAsync(new ClientRequest
                {
                    Operation = ProtocolOperations.GetUserInfo
                });
                if (response.Success && response.UserInfo is not null)
                {
                    lblTenDangNhapGiaTri.Text = response.UserInfo.Username;
                    lblEmailGiaTri.Text = string.IsNullOrWhiteSpace(response.UserInfo.Email) ? "(chưa cập nhật)" : response.UserInfo.Email;
                    lblXinChao.Text = $"Xin chào, {(string.IsNullOrWhiteSpace(response.UserInfo.FullName) ? response.UserInfo.Username : response.UserInfo.FullName)}";
                }
            }
            catch (SocketException) { }
            catch (IOException) { }
        }

        private async Task GuiLenhDangXuatVaDongKetNoiAsync()
        {
            if (_daDongKetNoi) return;
            _daDongKetNoi = true;

            try
            {
                if (_connection is not null)
                {
                    await _connection.SendAsync(new ClientRequest { Operation = ProtocolOperations.Logout });
                }
            }
            catch (SocketException) { }
            catch (IOException) { }
            finally
            {
                if (_connection is not null) await _connection.DisposeAsync();
            }
        }

        private async void btnDangXuat_Click_1(object sender, EventArgs e)
        {
            btnDangXuat.Enabled = false;
            await GuiLenhDangXuatVaDongKetNoiAsync();
            DialogResult = DialogResult.OK;
            Close();
        }

        private async void FormChinh_FormClosing(object sender, FormClosingEventArgs e)
        {
            await GuiLenhDangXuatVaDongKetNoiAsync();
        }
    }
}
