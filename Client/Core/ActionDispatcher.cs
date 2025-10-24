using Client.SystemManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Models;
namespace Client.Core
{
    internal class ActionDispatcher
    {
        public void Dispatch(RemoteAction action)
        {
            switch (action.Type)
            {
                case ActionType.Shutdown:
                    SystemCommander.Shutdown();
                    break;
                case ActionType.Restart:
                    SystemCommander.Restart();
                    break;
                    // Các lệnh StartStream/StopStream sẽ được xử lý trực tiếp bởi MainWindow
            }
        }
    }
}
