using System;
using System.IO;
using System.Text;


namespace Server
{
    /// <summary>
    /// Ghi nhật ký của server ra màn hình console (và ra tệp nếu bật), mỗi dòng kèm mốc thời gian.
    /// Dùng được từ nhiều tác vụ (mỗi client một tác vụ) cùng lúc nhờ khoá lock.
    /// Lớp này chỉ ghi log, không đụng tới TCP hay SQL.
    /// </summary>
    public static class NhatKy
    {
        private static readonly object khoa = new object();
        private static string duongDanTep;

        /// <summary>
        /// Gọi một lần khi server khởi động.
        /// duongDanTep = null: chỉ ghi ra console; có giá trị (ví dụ "server.log"): ghi thêm ra tệp.
        /// </summary>
        public static void KhoiTao(string duongDanTep = null)
        {
        
            Console.InputEncoding = new UTF8Encoding(false);
            Console.OutputEncoding = new UTF8Encoding(false);
            NhatKy.duongDanTep = duongDanTep;
        }

        /// <summary>Ghi một dòng: [2026-11-15 15:32:05] nội dung</summary>
        public static void Ghi(string noiDung)
        {
            string dong = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {noiDung}";

            lock (khoa)
            {
                Console.WriteLine(dong);

                if (duongDanTep != null)
                {
                    try
                    {
                        File.AppendAllText(duongDanTep, dong + Environment.NewLine, Encoding.UTF8);
                    }
                    catch (IOException)
                    {
                        // Lỗi ghi tệp không được làm sập server; console vẫn có dòng log
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Không có quyền ghi vào đường dẫn đã chọn
                    }
                }
            }
        }

        /// <summary>Ghi một dòng lỗi (thêm chữ LỖI ở đầu cho dễ tìm).</summary>
        public static void Loi(string noiDung)
        {
            Ghi("LỖI: " + noiDung);
        }
    }
}
