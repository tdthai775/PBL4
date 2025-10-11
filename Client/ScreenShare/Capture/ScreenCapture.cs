using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using Vortice;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;

namespace Client.ScreenShare.Capture
{
    /// <summary>
    /// Kết quả capture có thông tin dirty rectangles
    /// </summary>
    public class CaptureResult
    {
        public Bitmap Frame { get; set; } = null!;
        public List<Rectangle> DirtyRects { get; set; } = new List<Rectangle>();
        public bool HasChanges => DirtyRects != null && DirtyRects.Count > 0;
    }

    /// <summary>
    /// Capture màn hình với Dirty Rectangles từ Desktop Duplication API
    /// Tối ưu hiệu năng - không dùng GetPixel()
    /// </summary>
    internal class ScreenCapture : IDisposable
    {
        private ID3D11Device? _device;
        private ID3D11DeviceContext? _context;
        private IDXGIOutputDuplication? _duplication;
        private int _screenWidth;
        private int _screenHeight;

        public int ScreenWidth => _screenWidth;
        public int ScreenHeight => _screenHeight;

        public ScreenCapture()
        {
            InitializeDirectX();
        }

        private void InitializeDirectX()
        {
            // Tạo DirectX device
            var result = D3D11.D3D11CreateDevice(
                adapter: null,
                driverType: DriverType.Hardware,
                flags: DeviceCreationFlags.None,
                featureLevels: new[] { FeatureLevel.Level_11_0 },
                device: out _device,
                featureLevel: out _,
                immediateContext: out _context
            );

            if (result.Failure || _device == null || _context == null)
                throw new Exception($"Failed to create D3D11 device: {result}");

            // Lấy adapter và output (màn hình)
            IDXGIDevice? dxgiDevice = null;
            IDXGIAdapter? adapter = null;
            IDXGIOutput? output = null;
            IDXGIOutput1? output1 = null;

            try
            {
                dxgiDevice = _device.QueryInterface<IDXGIDevice>();
                dxgiDevice.GetAdapter(out adapter);
                adapter.EnumOutputs(0, out output);
                output1 = output.QueryInterface<IDXGIOutput1>();

                // Desktop Duplication
                _duplication = output1.DuplicateOutput(_device);

                // Lấy kích thước màn hình
                var desc = output.Description;
                _screenWidth = desc.DesktopCoordinates.Right - desc.DesktopCoordinates.Left;
                _screenHeight = desc.DesktopCoordinates.Bottom - desc.DesktopCoordinates.Top;
            }
            finally
            {
                output1?.Dispose();
                output?.Dispose();
                adapter?.Dispose();
                dxgiDevice?.Dispose();
            }
        }

        /// <summary>
        /// Capture frame ĐƠN GIẢN (không cần dirty rects)
        /// Để tương thích code cũ
        /// </summary>
        public Bitmap? CaptureFrame()
        {
            var result = CaptureFrameWithDirtyRects();
            return result?.Frame;
        }

        /// <summary>
        /// Capture frame VỚI dirty rectangles từ API (tối ưu)
        /// </summary>
        public CaptureResult? CaptureFrameWithDirtyRects()
        {
            if (_device == null || _context == null || _duplication == null)
                return null;

            try
            {
                // Lấy frame mới từ desktop
                var result = _duplication.AcquireNextFrame(
                    100,
                    out var frameInfo,
                    out IDXGIResource resource
                );

                if (result.Failure || resource == null)
                {
                    resource?.Dispose();
                    return null;
                }

                // Kiểm tra có thay đổi không
                if (frameInfo.AccumulatedFrames == 0)
                {
                    _duplication.ReleaseFrame();
                    resource?.Dispose();
                    return null; // Không có thay đổi
                }

                using (resource)
                using (var texture = resource.QueryInterface<ID3D11Texture2D>())
                {
                    // Lấy dirty rectangles TỪ API (siêu nhanh!)
                    var dirtyRects = GetDirtyRectanglesFromAPI(frameInfo);

                    // Chuyển texture thành Bitmap
                    var bitmap = TextureToBitmap(texture);

                    // Release frame
                    _duplication.ReleaseFrame();

                    return new CaptureResult
                    {
                        Frame = bitmap,
                        DirtyRects = dirtyRects
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error capturing frame: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Lấy dirty rectangles từ Desktop Duplication API
        /// NHANH hơn GetPixel() 10,000 lần!
        /// </summary>
        private List<Rectangle> GetDirtyRectanglesFromAPI(OutduplFrameInfo frameInfo)
        {
            var rects = new List<Rectangle>();

            if (_duplication == null)
            {
                rects.Add(new Rectangle(0, 0, _screenWidth, _screenHeight));
                return rects;
            }

            try
            {
                // Tính số lượng dirty rects tối đa
                int maxRects = frameInfo.TotalMetadataBufferSize > 0
                    ? (int)(frameInfo.TotalMetadataBufferSize / sizeof(int) / 4)
                    :0;

                if (maxRects == 0)
                {
                    // Không có metadata, coi như toàn màn hình thay đổi
                    rects.Add(new Rectangle(0, 0, _screenWidth, _screenHeight));
                    return rects;
                }

                // Lấy dirty rects từ API
                var dirtyRectsBuffer = new RawRect[maxRects];
                _duplication.GetFrameDirtyRects((uint)dirtyRectsBuffer.Length, dirtyRectsBuffer, out uint dirtyCount);

                // Chuyển đổi sang System.Drawing.Rectangle
                for (int i = 0; i < dirtyCount; i++)
                {
                    var rect = dirtyRectsBuffer[i];
                    int width = rect.Right - rect.Left;
                    int height = rect.Bottom - rect.Top;

                    if (width > 0 && height > 0)
                    {
                        rects.Add(new Rectangle(rect.Left, rect.Top, width, height));
                    }
                }

                // Tối ưu: Nếu dirty area > 40% màn hình → capture full
                int totalDirtyArea = 0;
                foreach (var r in rects)
                    totalDirtyArea += r.Width * r.Height;

                int screenArea = _screenWidth * _screenHeight;
                if (totalDirtyArea > screenArea * 0.4)
                {
                    // Quá nhiều thay đổi, gửi full frame
                    rects.Clear();
                    rects.Add(new Rectangle(0, 0, _screenWidth, _screenHeight));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetDirtyRects error: {ex.Message}");
                // Fallback: toàn màn hình
                rects.Add(new Rectangle(0, 0, _screenWidth, _screenHeight));
            }

            return rects;
        }

        /// <summary>
        /// Chuyển DirectX texture thành Bitmap
        /// </summary>
        private Bitmap TextureToBitmap(ID3D11Texture2D texture)
        {
            if (_device == null || _context == null)
                throw new InvalidOperationException("Device not initialized");

            // Tạo staging texture để đọc dữ liệu
            var desc = texture.Description;
            desc.Usage = ResourceUsage.Staging;
            desc.CPUAccessFlags = CpuAccessFlags.Read;
            desc.BindFlags = BindFlags.None;
            desc.MiscFlags = ResourceOptionFlags.None;

            using (var staging = _device.CreateTexture2D(desc))
            {
                // Copy texture vào staging
                _context.CopyResource(staging, texture);

                // Map để đọc dữ liệu
                var dataBox = _context.Map(staging, 0, MapMode.Read, Vortice.Direct3D11.MapFlags.None);

                // Tạo Bitmap từ dữ liệu
                var bitmap = new Bitmap(_screenWidth, _screenHeight, PixelFormat.Format32bppArgb);
                var bmpData = bitmap.LockBits(
                    new Rectangle(0, 0, _screenWidth, _screenHeight),
                    ImageLockMode.WriteOnly,
                    PixelFormat.Format32bppArgb
                );

                unsafe
                {
                    byte* src = (byte*)dataBox.DataPointer;
                    byte* dst = (byte*)bmpData.Scan0;

                    for (int y = 0; y < _screenHeight; y++)
                    {
                        // Sửa: copy đúng số bytes (_screenWidth * 4 thay vì Stride)
                        Buffer.MemoryCopy(src, dst, _screenWidth * 4, _screenWidth * 4);
                        src += dataBox.RowPitch;
                        dst += bmpData.Stride;
                    }
                }

                bitmap.UnlockBits(bmpData);
                _context.Unmap(staging, 0);

                return bitmap;
            }
        }

        public void Dispose()
        {
            _duplication?.Dispose();
            _context?.Dispose();
            _device?.Dispose();
        }
    }
}