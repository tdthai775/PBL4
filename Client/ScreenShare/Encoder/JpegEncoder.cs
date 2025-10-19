using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace Client.ScreenShare.Encoder
{
    internal class JpegEncoder : IDisposable
    {
        private readonly ImageCodecInfo _jpegCodec;
        private readonly EncoderParameters _encoderParams;

        public JpegEncoder(int quality = 50)
        {
            _jpegCodec = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == ImageFormat.Jpeg.Guid);
            _encoderParams = new EncoderParameters(1);
            _encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)quality);
        }

        public byte[] Encode(Bitmap bitmap)
        {
            using (var ms = new MemoryStream())
            {
                bitmap.Save(ms, _jpegCodec, _encoderParams);
                return ms.ToArray();
            }
        }

        public void Dispose()
        {
            _encoderParams?.Dispose();
        }
    }
}