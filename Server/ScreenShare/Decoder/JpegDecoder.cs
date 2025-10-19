using System;
using System.Drawing;
using System.IO;

namespace Server.ScreenShare.Decoder
{
    internal class JpegDecoder
    {
        public Bitmap? Decode(byte[] jpegData)
        {
            if (jpegData == null || jpegData.Length == 0) return null;
            try
            {
                using (var ms = new MemoryStream(jpegData))
                {
                    return new Bitmap(ms);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"JPEG decode error: {ex.Message}");
                return null;
            }
        }
    }
}