using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Models
{
    public enum InputType
    {
        MouseMove,
        MouseLeftDown,
        MouseLeftUp,
        MouseRightDown,
        MouseRightUp,
        MouseMiddleDown,
        MouseMiddleUp,
        MouseScroll,
        KeyDown,
        KeyUp
    }

    public class InputAction
    {
        public InputType Type { get; set; }

        // Mouse coordinates
        public int? X { get; set; }
        public int? Y { get; set; }

        // Keyboard
        public int? KeyCode { get; set; }

        // Mouse scroll
        public int? ScrollDelta { get; set; }

        // Screen dimensions for coordinate scaling
        public int? ScreenWidth { get; set; }
        public int? ScreenHeight { get; set; }
    }
}
