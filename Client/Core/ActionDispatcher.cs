using Client.Models;
using Client.RemoteControl;
using Client.SystemManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Client.Core
{
    internal class ActionDispatcher
    {
        public void Dispatch(RemoteAction action)
        {
            try
            {
                switch (action.Type)
                {
                    // System commands
                    case ActionType.Shutdown:
                        Console.WriteLine("[DISPATCHER] Executing: Shutdown");
                        SystemCommander.Shutdown();
                        break;

                    case ActionType.Restart:
                        Console.WriteLine("[DISPATCHER] Executing: Restart");
                        SystemCommander.Restart();
                        break;

                    // Stream commands are handled directly by MainWindow
                    case ActionType.StartStream:
                    case ActionType.StopStream:
                        // These are handled by MainWindow event handler
                        break;

                    // Mouse control
                    case ActionType.MouseMove:
                        if (action.X.HasValue && action.Y.HasValue)
                        {
                            InputSimulator.MoveMouse(action.X.Value, action.Y.Value);
                        }
                        break;

                    case ActionType.MouseLeftDown:
                        InputSimulator.MouseLeftDown();
                        break;

                    case ActionType.MouseLeftUp:
                        InputSimulator.MouseLeftUp();
                        break;

                    case ActionType.MouseRightDown:
                        InputSimulator.MouseRightDown();
                        break;

                    case ActionType.MouseRightUp:
                        InputSimulator.MouseRightUp();
                        break;

                    case ActionType.MouseMiddleDown:
                        InputSimulator.MouseMiddleDown();
                        break;

                    case ActionType.MouseMiddleUp:
                        InputSimulator.MouseMiddleUp();
                        break;

                    case ActionType.MouseScroll:
                        if (action.ScrollDelta.HasValue)
                        {
                            InputSimulator.MouseScroll(action.ScrollDelta.Value);
                        }
                        break;

                    // Keyboard control
                    case ActionType.KeyDown:
                        if (action.KeyCode.HasValue)
                        {
                            InputSimulator.KeyDown(action.KeyCode.Value);
                        }
                        break;

                    case ActionType.KeyUp:
                        if (action.KeyCode.HasValue)
                        {
                            InputSimulator.KeyUp(action.KeyCode.Value);
                        }
                        break;

                    default:
                        Console.WriteLine($"[DISPATCHER] Unknown action type: {action.Type}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DISPATCHER] Error executing {action.Type}: {ex.Message}");
            }
        }
    }
}
