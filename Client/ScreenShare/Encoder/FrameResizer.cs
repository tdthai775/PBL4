using System.Drawing;
using System.Drawing.Drawing2D;

namespace Client.ScreenShare.Encoder
{
    internal class FrameResizer
    {
        private readonly int _targetWidth;
        private readonly int _targetHeight;

        public FrameResizer(int targetWidth, int targetHeight)
        {
            _targetWidth = targetWidth;
            _targetHeight = targetHeight;
        }

        public Bitmap Resize(Bitmap original)
        {
            if (original.Width == _targetHeight && original.Height == _targetHeight)
            {
                return
                new Bitmap(original);
            }

            var resized = new Bitmap(_targetWidth, _targetHeight);
            using (var graphics = Graphics.FromImage(resized))
            {
                graphics.InterpolationMode = InterpolationMode.Bilinear;
                graphics.SmoothingMode = SmoothingMode.HighSpeed;
                graphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;
                graphics.CompositingQuality = CompositingQuality.HighSpeed;

                graphics.DrawImage(original, 0, 0, _targetWidth, _targetHeight);
            }
            return resized;
        }

        public Bitmap ResizeFast(Bitmap original)
        {
            if (original.Width == _targetWidth && original.Height == _targetHeight)
            {
                return new Bitmap(original);
            }

            var resized = new Bitmap(_targetWidth, _targetHeight);
            using (var graphics = Graphics.FromImage(resized))
            {
                graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                graphics.SmoothingMode = SmoothingMode.HighSpeed;
                graphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;
                graphics.CompositingQuality = CompositingQuality.HighSpeed;

                graphics.DrawImage(original, 0, 0, _targetWidth, _targetHeight);
            }
            return resized;
        }
    }
}