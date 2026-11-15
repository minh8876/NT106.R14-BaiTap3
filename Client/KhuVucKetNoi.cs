using System;
using System.Drawing;
using System.Windows.Forms;

namespace QuanLyNguoiDung
{
    public enum TrangThaiKetNoi
    {
        ChuaKetNoi,
        DangKetNoi,
        DaKetNoi
    }

    public class KhuVucKetNoi : Panel
    {
        private readonly Label lblMayChu = new Label();
        private readonly TextBox txtDiaChiIp = new TextBox();
        private readonly Label lblCong = new Label();
        private readonly NumericUpDown nudCong = new NumericUpDown();
        private readonly Label lblTrangThaiKetNoi = new Label();

        public KhuVucKetNoi()
        {
            Dock = DockStyle.Bottom;
            Height = 62;

            lblMayChu.Text = "Máy chủ";
            lblMayChu.AutoSize = true;
            lblMayChu.Location = new Point(12, 10);

            txtDiaChiIp.Text = "127.0.0.1";
            txtDiaChiIp.Location = new Point(75, 7);
            txtDiaChiIp.Width = 115;
            txtDiaChiIp.MaxLength = 100;

            lblCong.Text = "Cổng";
            lblCong.AutoSize = true;
            lblCong.Location = new Point(205, 10);

            nudCong.Minimum = 1024;
            nudCong.Maximum = 49151;
            nudCong.Value = 5000;   
            nudCong.Location = new Point(245, 7);
            nudCong.Width = 70;

            lblTrangThaiKetNoi.AutoSize = true;
            lblTrangThaiKetNoi.Location = new Point(12, 38);

            Controls.AddRange(new Control[]
            {
                lblMayChu, txtDiaChiIp, lblCong, nudCong, lblTrangThaiKetNoi
            });

            DatTrangThai(TrangThaiKetNoi.ChuaKetNoi);
        }

        public bool LayDiaChiVaCong(out string diaChi, out int cong, out string loi)
        {
            diaChi = txtDiaChiIp.Text.Trim();
            cong = (int)nudCong.Value;
            loi = null;

            if (string.IsNullOrWhiteSpace(diaChi))
            {
                loi = "Vui lòng nhập địa chỉ IP của máy chủ.";
                txtDiaChiIp.Focus();
                return false;
            }
            return true;
        }

        public void DatTrangThai(TrangThaiKetNoi trangThai)
        {
            switch (trangThai)
            {
                case TrangThaiKetNoi.DangKetNoi:
                    lblTrangThaiKetNoi.Text = "Trạng thái: đang kết nối...";
                    lblTrangThaiKetNoi.ForeColor = Color.DarkOrange;
                    break;
                case TrangThaiKetNoi.DaKetNoi:
                    lblTrangThaiKetNoi.Text = "Trạng thái: đã kết nối";
                    lblTrangThaiKetNoi.ForeColor = Color.Green;
                    break;
                default:
                    lblTrangThaiKetNoi.Text = "Trạng thái: chưa kết nối";
                    lblTrangThaiKetNoi.ForeColor = Color.Firebrick;
                    break;
            }
        }

        public void Khoa(bool khoa)
        {
            txtDiaChiIp.Enabled = !khoa;
            nudCong.Enabled = !khoa;
        }
    }
}