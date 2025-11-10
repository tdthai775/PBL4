using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using Server.Core;
using Server.Network;


namespace Server.UI
{
    public partial class TaskManagerUI : Window
    {
        private TcpListener listener;
        private TcpClient selectedClient;
        private ServerManager serverManager;
        Thread loadProcessThread;
        private int selectedProcessId = -1;
        public TaskManagerUI(TcpListener tcpListener, TcpClient selectedClient)
        {
            InitializeComponent();
            listener = tcpListener;
            this.selectedClient = selectedClient;
            loadProcessThread = new Thread(LoadProcessesLoop);
            loadProcessThread.IsBackground = true;
            loadProcessThread.Start();
        }

        private void LoadProcessesLoop()
        {
            while (true)
            {
                LoadProcesses();
                Thread.Sleep(1000);
            }
        }

        // Lấy danh sách process từ client
        private List<ProcessInfo> GetProcessesFromClient()
        {
            NetworkStream stream = selectedClient.GetStream();
            byte[] buffer = new byte[4096];
            StringBuilder sb = new StringBuilder();

            while (true)
            {
                int bytes = stream.Read(buffer, 0, buffer.Length);
                if (bytes <= 0) return null;

                sb.Append(Encoding.UTF8.GetString(buffer, 0, bytes));

                string data = sb.ToString();
                int newlineIndex = data.IndexOf('\n');

                if (newlineIndex >= 0)
                {
                    string json = data.Substring(0, newlineIndex).Trim();
                    string remaining = data[(newlineIndex + 1)..];

                    sb.Clear();
                    sb.Append(remaining);

                    if (!string.IsNullOrEmpty(json))
                    {
                        return JsonSerializer.Deserialize<List<ProcessInfo>>(json);
                    }
                }
            }
        }


        // Hiển thị danh sách tiến trình
        private void LoadProcesses()
        {
            try
            {
                var list = GetProcessesFromClient();
                var found = list.FirstOrDefault(p => p.Id == selectedProcessId);

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    dgvProcesses.ItemsSource = list
                        .OrderByDescending(p => ParseRam(p.Memory))
                        .ToList();
                    dgvProcesses.SelectedItem = found;
                }));
            }
            catch { }
        }

        // Chuyển chuỗi RAM thành MB
        private double ParseRam(string ramString)
        {
            if (string.IsNullOrWhiteSpace(ramString)) return 0;
            ramString = ramString.Trim().ToUpper();

            double value = 0;
            if (ramString.EndsWith("GB"))
            {
                double.TryParse(ramString.Replace("GB", "").Trim(), out value);
                value *= 1024;
            }
            else if (ramString.EndsWith("KB"))
            {
                double.TryParse(ramString.Replace("KB", "").Trim(), out value);
                value /= 1024;
            }
            else if (ramString.EndsWith("MB"))
            {
                double.TryParse(ramString.Replace("MB", "").Trim(), out value);
            }
            else
            {
                double.TryParse(ramString, out value);
            }
            return value;
        }

        // Gửi lệnh kill process đến client
        private void SendKillCommand(int pid)
        {
            try
            {
                if (selectedClient == null) return;
                var stream = selectedClient.GetStream();
                if (!stream.CanWrite) return;

                // Gửi theo format: KillProcess|123\n
                string cmd = $"KillProcess|{pid}\n";
                byte[] data = Encoding.UTF8.GetBytes(cmd);

                lock (stream) // tránh ghi đồng thời nếu có nơi khác ghi chung stream
                {
                    stream.Write(data, 0, data.Length);
                    stream.Flush();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi kill tiến trình: " + ex.Message);
            }
        }

        // Sự kiện nhấn nút Kill Process
        private void btnKillProcess_Click(object sender, RoutedEventArgs e)
        {
            if (dgvProcesses.SelectedItem is ProcessInfo selectedProcess)
            {
                SendKillCommand(selectedProcess.Id);
            }
            else
            {
                MessageBox.Show("Vui lòng chọn tiến trình để tắt.");
            }
        }

        // Class chứa thông tin tiến trình
        public class ProcessInfo
        {
            public int Id { get; set; }
            public string ProcessName { get; set; }
            public string Memory { get; set; }
        }

        private void dgvProcesses_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedProcessId = dgvProcesses.SelectedItem is ProcessInfo process ? process.Id : -1;
        }
    }
}
