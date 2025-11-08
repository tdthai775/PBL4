using Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Server.RemoteControl
{
    internal class InputCapture
    {
        public event EventHandler<RemoteAction>? InputActionReceived;

        private readonly System.Windows.Controls.Image _imageControl;
        private readonly int _clientScreenWidth;
        private readonly int _clientScreenHeight;
        private bool _isEnabled = false;

        public InputCapture(System.Windows.Controls.Image imageControl, int clientScreenWidth = 1920, int clientScreenHeight = 1080)
        {
            _imageControl = imageControl;
            _clientScreenWidth = clientScreenWidth;
            _clientScreenHeight = clientScreenHeight;
        }

        public void Enable()
        {
            if (_isEnabled) return;

            // Mouse events
            _imageControl.MouseMove += OnMouseMove;
            _imageControl.MouseLeftButtonDown += OnMouseLeftButtonDown;
            _imageControl.MouseLeftButtonUp += OnMouseLeftButtonUp;
            _imageControl.MouseRightButtonDown += OnMouseRightButtonDown;
            _imageControl.MouseRightButtonUp += OnMouseRightButtonUp;
            _imageControl.MouseWheel += OnMouseWheel;

            // Keyboard events
            _imageControl.KeyDown += OnKeyDown;
            _imageControl.KeyUp += OnKeyUp;

            // Make image focusable to receive keyboard events
            _imageControl.Focusable = true;
            _imageControl.Focus();

            _isEnabled = true;
            Console.WriteLine("[INPUT-CAPTURE] Enabled");
        }

        public void Disable()
        {
            if (!_isEnabled) return;

            // Unsubscribe all events
            _imageControl.MouseMove -= OnMouseMove;
            _imageControl.MouseLeftButtonDown -= OnMouseLeftButtonDown;
            _imageControl.MouseLeftButtonUp -= OnMouseLeftButtonUp;
            _imageControl.MouseRightButtonDown -= OnMouseRightButtonDown;
            _imageControl.MouseRightButtonUp -= OnMouseRightButtonUp;
            _imageControl.MouseWheel -= OnMouseWheel;
            _imageControl.KeyDown -= OnKeyDown;
            _imageControl.KeyUp -= OnKeyUp;

            _isEnabled = false;
            Console.WriteLine("[INPUT-CAPTURE] Disabled");
        }

        #region Mouse Event Handlers

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            var position = e.GetPosition(_imageControl);
            var (scaledX, scaledY) = ScaleCoordinates(position.X, position.Y);

            var action = new RemoteAction
            {
                Type = ActionType.MouseMove,
                X = scaledX,
                Y = scaledY
            };

            InputActionReceived?.Invoke(this, action);
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var action = new RemoteAction { Type = ActionType.MouseLeftDown };
            InputActionReceived?.Invoke(this, action);
            Console.WriteLine("[INPUT-CAPTURE] Mouse left down");
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var action = new RemoteAction { Type = ActionType.MouseLeftUp };
            InputActionReceived?.Invoke(this, action);
            Console.WriteLine("[INPUT-CAPTURE] Mouse left up");
        }

        private void OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var action = new RemoteAction { Type = ActionType.MouseRightDown };
            InputActionReceived?.Invoke(this, action);
            Console.WriteLine("[INPUT-CAPTURE] Mouse right down");
        }

        private void OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            var action = new RemoteAction { Type = ActionType.MouseRightUp };
            InputActionReceived?.Invoke(this, action);
            Console.WriteLine("[INPUT-CAPTURE] Mouse right up");
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var action = new RemoteAction
            {
                Type = ActionType.MouseScroll,
                ScrollDelta = e.Delta
            };
            InputActionReceived?.Invoke(this, action);
            Console.WriteLine($"[INPUT-CAPTURE] Mouse scroll: {e.Delta}");
        }

        #endregion

        #region Keyboard Event Handlers

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            int vkCode = KeyInterop.VirtualKeyFromKey(e.Key);
            var action = new RemoteAction
            {
                Type = ActionType.KeyDown,
                KeyCode = vkCode
            };
            InputActionReceived?.Invoke(this, action);
            Console.WriteLine($"[INPUT-CAPTURE] Key down: {e.Key} (VK: 0x{vkCode:X2})");
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            int vkCode = KeyInterop.VirtualKeyFromKey(e.Key);
            var action = new RemoteAction
            {
                Type = ActionType.KeyUp,
                KeyCode = vkCode
            };
            InputActionReceived?.Invoke(this, action);
            Console.WriteLine($"[INPUT-CAPTURE] Key up: {e.Key} (VK: 0x{vkCode:X2})");
        }

        #endregion

        #region Coordinate Scaling

        /// <summary>
        /// Scale coordinates from viewer image size to actual client screen size
        /// </summary>
        private (int x, int y) ScaleCoordinates(double viewerX, double viewerY)
        {
            double imageWidth = _imageControl.ActualWidth;
            double imageHeight = _imageControl.ActualHeight;

            if (imageWidth <= 0 || imageHeight <= 0)
                return (0, 0);

            // Calculate scale factors
            double scaleX = _clientScreenWidth / imageWidth;
            double scaleY = _clientScreenHeight / imageHeight;

            // Scale coordinates
            int scaledX = (int)(viewerX * scaleX);
            int scaledY = (int)(viewerY * scaleY);

            // Clamp to screen bounds
            scaledX = Math.Clamp(scaledX, 0, _clientScreenWidth - 1);
            scaledY = Math.Clamp(scaledY, 0, _clientScreenHeight - 1);

            return (scaledX, scaledY);
        }

        #endregion
    }
}
