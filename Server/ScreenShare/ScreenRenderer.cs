using System.Drawing;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Server.ScreenShare.Renderer
{
    internal class ScreenRenderer
    {
        // Chỉ định rõ đây là control Image của WPF để tránh xung đột
        private readonly System.Windows.Controls.Image _wpfImageControl;
        private readonly Dispatcher _dispatcher;

        // Constructor cũng chỉ định rõ tham số là control Image của WPF
        public ScreenRenderer(System.Windows.Controls.Image wpfImageControl)
        {
            _wpfImageControl = wpfImageControl;
            _dispatcher = wpfImageControl.Dispatcher;
        }

        /// <summary>
        /// Hiển thị một khung hình ảnh Bitmap lên control Image đã được chỉ định.
        /// Hàm này an toàn để gọi từ bất kỳ luồng nào.
        /// </summary>
        public void Render(Bitmap frame)
        {
            // Dùng Dispatcher.Invoke để yêu cầu luồng UI thực hiện việc cập nhật.
            // Điều này tránh lỗi "cross-thread access".
            _dispatcher.Invoke(() =>
            {
                using (var ms = new MemoryStream())
                {
                    // 1. Lưu ảnh Bitmap (định dạng System.Drawing) vào một dòng bộ nhớ
                    frame.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                    ms.Position = 0;

                    // 2. Tạo đối tượng BitmapImage (định dạng của WPF)
                    var bmpImage = new BitmapImage();
                    bmpImage.BeginInit();
                    bmpImage.CacheOption = BitmapCacheOption.OnLoad; // Tải ảnh ngay lập tức
                    bmpImage.StreamSource = ms; // Đọc dữ liệu từ dòng bộ nhớ
                    bmpImage.EndInit();

                    // 3. Freeze() ảnh để tối ưu hóa hiệu năng, đặc biệt quan trọng khi cập nhật liên tục
                    bmpImage.Freeze();

                    // 4. Gán ảnh mới làm nguồn cho control Image trên giao diện
                    _wpfImageControl.Source = bmpImage;
                }
            });
        }
    }
}