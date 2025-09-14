using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Management; 
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using System.Text;
using System.Runtime.InteropServices; 

namespace ClientApp
{
    public partial class Form1 : Form
    {
        private TcpClient client;
        private NetworkStream stream;
        private Thread streamThread;
        private Thread commandThread;
        private bool isStreaming = false;
        private string serverIp = "127.0.0.1"; 
        private int serverPort = 8080;

        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        public Form1()
        {
            try { SetProcessDPIAware(); } catch { }

            InitializeComponent(); 
            ConnectToServer();
        }

        private void ConnectToServer()
        {
            try
            {
                serverIp = Microsoft.VisualBasic.Interaction.InputBox("Server IP:", "Connect", serverIp);
                client = new TcpClient(serverIp, serverPort);
                stream = client.GetStream();
                lblStatus.Text = "Connected to server";
                StartCommandListener();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connection failed: " + ex.Message);
                lblStatus.Text = "Connection failed";
            }
        }

        private void StartCommandListener()
        {
            commandThread = new Thread(ListenForCommands);
            commandThread.IsBackground = true;
            commandThread.Start();
        }

        private void ListenForCommands()
        {
            byte[] buffer = new byte[1024];
            while (client.Connected)
            {
                try
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        string command = Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim();
                        HandleCommand(command);
                    }
                }
                catch { break; }
            }
        }

        private void HandleCommand(string command)
        {
            switch (command)
            {
                case "START_STREAM":
                    if (!isStreaming)
                    {
                        isStreaming = true;
                        streamThread = new Thread(StreamScreen);
                        streamThread.IsBackground = true;
                        streamThread.Start();
                    }
                    break;
                case "STOP_STREAM":
                    isStreaming = false;
                    streamThread?.Join(1000);
                    break;
                case "SHUTDOWN":
                    Process.Start("shutdown", "/s /t 0"); 
                    break;
                case "RESTART":
                    Process.Start("shutdown", "/r /t 0"); 
                    break;
                case "GET_SYSTEM_INFO":
                    string info = GetSystemInfo();
                    byte[] infoBytes = Encoding.ASCII.GetBytes(info);
                    stream.Write(infoBytes, 0, infoBytes.Length);
                    stream.Flush();
                    break;
                default:
                    MessageBox.Show($"Received command: {command}"); 
                    break;
            }
        }

        private string GetSystemInfo()
        {
            StringBuilder sb = new StringBuilder();

            // RAM Usage (Used / Total)
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize, FreePhysicalMemory FROM Win32_OperatingSystem");
                ManagementObjectCollection collection = searcher.Get();
                foreach (ManagementObject obj in collection)
                {
                    // WMI returns KB values
                    double totalRamKB = Convert.ToDouble(obj["TotalVisibleMemorySize"]);
                    double freeRamKB = Convert.ToDouble(obj["FreePhysicalMemory"]);
                    double usedRamKB = totalRamKB - freeRamKB;
                    double usedRamGB = usedRamKB / (1024 * 1024);
                    double totalRamGB = totalRamKB / (1024 * 1024);
                    sb.AppendLine($"RAM Usage: {usedRamGB:F1} GB / {totalRamGB:F1} GB");
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"RAM Usage: Error retrieving - {ex.Message}");
            }

            
            try
            {
                PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                cpuCounter.NextValue(); 
                Thread.Sleep(1000); 
                float cpuUsage = cpuCounter.NextValue();
                ManagementObjectSearcher cpuSearcher = new ManagementObjectSearcher("SELECT Name FROM Win32_Processor");
                ManagementObjectCollection cpuCollection = cpuSearcher.Get();
                string cpuName = "Unknown";
                foreach (ManagementObject obj in cpuCollection)
                {
                    cpuName = obj["Name"].ToString();
                    break;
                }
                sb.AppendLine($"CPU: {cpuName} ({cpuUsage:F1}% usage)");
            }
            catch (Exception ex)
            {
                sb.AppendLine($"CPU: Error retrieving - {ex.Message}");
            }


            try
            {
                PerformanceCounter diskReadCounter = new PerformanceCounter("PhysicalDisk", "Disk Read Bytes/sec", "_Total");
                PerformanceCounter diskWriteCounter = new PerformanceCounter("PhysicalDisk", "Disk Write Bytes/sec", "_Total");
                diskReadCounter.NextValue(); 
                diskWriteCounter.NextValue();
                Thread.Sleep(1000);
                float readBytesPerSec = diskReadCounter.NextValue();
                float writeBytesPerSec = diskWriteCounter.NextValue();
                double readMBS = readBytesPerSec / (1024 * 1024);
                double writeMBS = writeBytesPerSec / (1024 * 1024);
                sb.AppendLine($"Disk I/O: Read {readMBS:F2} MB/s, Write {writeMBS:F2} MB/s");
            }
            catch (Exception ex)
            {
                sb.AppendLine($"Disk I/O: Error retrieving - {ex.Message}");
            }

            return sb.ToString();
        }

        private void StreamScreen()
        {
            while (isStreaming && client.Connected)
            {
                try
                {
                    Rectangle bounds = Screen.PrimaryScreen.Bounds;
                    using (Bitmap screenshot = new Bitmap(bounds.Width, bounds.Height))
                    {
                        using (Graphics g = Graphics.FromImage(screenshot))
                        {
                            g.CopyFromScreen(bounds.X, bounds.Y, 0, 0, screenshot.Size);
                        }
                        using (MemoryStream ms = new MemoryStream())
                        {
                            screenshot.Save(ms, ImageFormat.Jpeg); 
                            byte[] imageBytes = ms.ToArray();
                            byte[] sizeBytes = BitConverter.GetBytes(imageBytes.Length);
                            stream.Write(sizeBytes, 0, 4);
                            stream.Write(imageBytes, 0, imageBytes.Length);
                            stream.Flush();
                        }
                    }
                }
                catch { isStreaming = false; }
                Thread.Sleep(100); 
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            isStreaming = false;
            client?.Close();
            base.OnFormClosed(e);
        }
    }
}
