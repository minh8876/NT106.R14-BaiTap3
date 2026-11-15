namespace QuanLyNguoiDung
{
    /// <summary>
    /// Các câu báo lỗi hiển thị cho người dùng. Người dùng chỉ thấy câu tiếng Việt,
    /// không bao giờ thấy tên ngoại lệ (yêu cầu câu 9).
    /// </summary>
    public static class ThongBaoLoiMang
    {
        public const string KhongKetNoiDuoc =
            "Không thể kết nối tới máy chủ. Hãy kiểm tra địa chỉ IP, cổng và mạng.";

        public const string MatKetNoi =
            "Kết nối tới máy chủ đã bị ngắt. Vui lòng thử lại.";

        // Dùng tạm cho code cũ nối thẳng CSDL; xoá khi chuyển hết sang server
        public const string LoiCoSoDuLieu =
            "Lỗi kết nối cơ sở dữ liệu. Vui lòng thử lại sau.";
    }
}
