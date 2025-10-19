using System.Drawing;

namespace Server.ScreenShare.Renderer
{
    internal class FrameManager : IDisposable
    {
        private Bitmap? _currentFrame;
        private readonly object _lock = new object();

        public void UpdateFrame(Bitmap newImageData, Rectangle? patchLocation)
        {
            lock (_lock)
            {
                if (patchLocation == null || _currentFrame == null) 
                {
                    _currentFrame?.Dispose();
                    _currentFrame = new Bitmap(newImageData);
                }
                else 
                {
                    using (var g = Graphics.FromImage(_currentFrame))
                    {
                        g.DrawImage(newImageData, patchLocation.Value);
                    }
                }
            }
        }

        public Bitmap? GetCurrentFrame()
        {
            lock (_lock)
            {
                return _currentFrame != null ? (Bitmap)_currentFrame.Clone() : null;
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                _currentFrame?.Dispose();
            }
        }
    }
}