using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace Client.ScreenShare.Encoder
{
    /// <summary>
    /// Nén bitmap thành JPEG với chất lượng có thể điều chỉnh
    /// </summary>
    internal class JpegEncoder
    {
        private readonly int _quality;
        private readonly ImageCodecInfo _jpegCodec;
        private readonly EncoderParameters _encoderParams;

        public JpegEncoder(int quality = 50)
        {
            // Quality từ 0-100 (50 = cân bằng giữa chất lượng và kích thước)
            _quality = Math.Max(0, Math.Min(100, quality));

            // Lấy JPEG codec
            _jpegCodec = GetJpegCodec();

            // Thiết lập encoder parameters
            _encoderParams = new EncoderParameters(1);
            _encoderParams.Param[0] = new EncoderParameter(
                System.Drawing.Imaging.Encoder.Quality,
                (long)_quality
            );
        }

        /// <summary>
        /// Encode bitmap thành byte array JPEG
        /// </summary>
        public byte[] Encode(Bitmap bitmap)
        {
            using (var ms = new MemoryStream())
            {
                bitmap.Save(ms, _jpegCodec, _encoderParams);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Encode và ghi ra stream (tiết kiệm memory hơn)
        /// </summary>
        public void EncodeToStream(Bitmap bitmap, Stream outputStream)
        {
            bitmap.Save(outputStream, _jpegCodec, _encoderParams);
        }

        /// <summary>
        /// Lấy kích thước ước lượng sau khi nén
        /// </summary>
        public int EstimateSize(Bitmap bitmap)
        {
            using (var ms = new MemoryStream())
            {
                bitmap.Save(ms, _jpegCodec, _encoderParams);
                return (int)ms.Length;
            }
        }

        /// <summary>
        /// Tìm JPEG codec
        /// </summary>
        private static ImageCodecInfo GetJpegCodec()
        {
            var codecs = ImageCodecInfo.GetImageEncoders();
            var codec = codecs.FirstOrDefault(codec => codec.FormatID == ImageFormat.Jpeg.Guid);

            if (codec == null)
            {
                throw new NotSupportedException("JPEG codec not found on this system.");
            }

            return codec;
        }

        public void Dispose()
        {
            _encoderParams?.Dispose();
        }
    }
}