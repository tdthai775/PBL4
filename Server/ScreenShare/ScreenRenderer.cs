using System.Drawing;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Server.ScreenShare.Renderer
{
    internal class ScreenRenderer
    {
        private readonly System.Windows.Controls.Image _wpfImageControl;
        private readonly Dispatcher _dispatcher;

        public ScreenRenderer(System.Windows.Controls.Image wpfImageControl)
        {
            _wpfImageControl = wpfImageControl;
            _dispatcher = wpfImageControl.Dispatcher;
        }

        public void Render(Bitmap frame)
        {
            
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