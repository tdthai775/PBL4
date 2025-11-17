using System;

namespace Server.Models
{
    public enum ActionType
    {
        // Giai đoạn 2: Lệnh hệ thống
        Shutdown,
        Restart,

        // Giai đoạn 3: Lệnh điều khiển stream
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
        // Các thuộc tính khác sẽ được thêm sau
    }
}