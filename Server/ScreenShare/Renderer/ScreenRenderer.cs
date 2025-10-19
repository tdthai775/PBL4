using System;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Image = System.Windows.Controls.Image;

namespace Server.ScreenShare.Renderer
{
    internal class ScreenRenderer
    {
        private readonly Image _wpfImageControl;
        private readonly Dispatcher _dispatcher;

        public ScreenRenderer(Image wpfImageControl)
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
                    frame.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                    ms.Position = 0;
                    var bmpImage = new BitmapImage();
                    bmpImage.BeginInit();
                    bmpImage.CacheOption = BitmapCacheOption.OnLoad;
                    bmpImage.StreamSource = ms;
                    bmpImage.EndInit();
                    bmpImage.Freeze(); 

                    _wpfImageControl.Source = bmpImage;
                }
            });
        }
    }
}