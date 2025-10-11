using System.Drawing;
using System.Drawing.Drawing2D;

namespace Client.ScreenShare.Encoder
{
    /// <summary>
    /// Resize bitmap về kích thước mong muốn
    /// </summary>
    internal class FrameResizer
    {
        private readonly int _targetWidth;
        private readonly int _targetHeight;

        public FrameResizer(int targetWidth, int targetHeight)
        {
            _targetWidth = targetWidth;
            _targetHeight = targetHeight;
        }

        /// <summary>
        /// Resize bitmap với chất lượng tốt (bilinear interpolation)
        /// </summary>
        public Bitmap Resize(Bitmap original)
        {
            // Nếu kích thước giống nhau thì return luôn
            if (original.Width == _targetWidth && original.Height == _targetHeight)
            {
                return new Bitmap(original);
            }

            var resized = new Bitmap(_targetWidth, _targetHeight);

            using (var graphics = Graphics.FromImage(resized))
            {
                // Thiết lập chất lượng resize
                graphics.InterpolationMode = InterpolationMode.Bilinear;
                graphics.SmoothingMode = SmoothingMode.HighSpeed;
                graphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;
                graphics.CompositingQuality = CompositingQuality.HighSpeed;

                // Vẽ image đã resize
                graphics.DrawImage(original, 0, 0, _targetWidth, _targetHeight);
            }

            return resized;
        }

        /// <summary>
        /// Resize nhanh hơn nhưng chất lượng thấp hơn
        /// </summary>
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