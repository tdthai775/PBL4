using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Models
{
    public enum ActionType
    {
        Shutdown,
        Restart,

        StartStream,
        StopStream,

        // Placeholder cho các giai đoạn sau
        RequestProcessList,
        KillProcess,
        ResponseProcessList,
        StopSendingProcessList
    }

    [Serializable]
    public class RemoteAction
    {
        public ActionType Type { get; set; }

    }
}
