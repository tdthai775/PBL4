using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Models
{
    public enum ActionType
    {
        // System commands
        Shutdown,
        Restart,

        // Stream control
        StartStream,
        StopStream,

        // Remote control - Mouse
        MouseMove,
        MouseLeftDown,
        MouseLeftUp,
        MouseRightDown,
        MouseRightUp,
        MouseMiddleDown,
        MouseMiddleUp,
        MouseScroll,

        // Remote control - Keyboard
        KeyDown,
        KeyUp,

        // Process management (placeholder)
        RequestProcessList,
        KillProcess,
        ResponseProcessList,
        StopSendingProcessList
    }

    [Serializable]
    public class RemoteAction
    {
        public ActionType Type { get; set; }


        // Mouse coordinates (for MouseMove)
        public int? X { get; set; }
        public int? Y { get; set; }

        // Keyboard key code (for KeyDown/KeyUp)
        public int? KeyCode { get; set; }

        // Mouse scroll delta (for MouseScroll)
        public int? ScrollDelta { get; set; }

        // Screen dimensions for coordinate scaling
        public int? ScreenWidth { get; set; }
        public int? ScreenHeight { get; set; }

        // Additional data for future use
        public string? Data { get; set; }


    }
}
