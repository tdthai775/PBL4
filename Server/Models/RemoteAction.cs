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

        // Placeholder cho các giai đoạn sau
        RequestProcessList
    }

    [Serializable]
    public class RemoteAction
    {
        public ActionType Type { get; set; }
        // Các thuộc tính khác sẽ được thêm sau
    }
}