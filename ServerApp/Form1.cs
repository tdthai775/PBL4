using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using System.Text;
using System.Linq; // For parsing

namespace ServerApp
{
    public partial class Form1 : Form
    {
        private TcpListener listener;
        private List<ClientConnection> connectedClients = new List<ClientConnection>();
        private Thread listenThread;
        private bool isListening = false;
        private int port = 8080;

        public Form1()
        {
            InitializeComponent();

            btnStartServer.Enabled = true;
            btnStopServer.Enabled = false;

            dgvListClients.Columns.Add("ClientId", "Client ID");
            dgvListClients.Columns.Add("Status", "Status");
            dgvListClients.Columns["ClientId"].Width = 150;
            dgvListClients.Columns["Status"].Width = 100;
            dgvListClients.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvListClients.MultiSelect = false;

            dgvListClients.CellDoubleClick += dgvListClients_CellDoubleClick;
        }

        private void btnStartServer_Click(object sender, EventArgs e)
        {
            StartServer();
            btnStartServer.Enabled = false;
            btnStopServer.Enabled = true;
        }

        private void btnStopServer_Click(object sender, EventArgs e)
        {
            StopServer();
            btnStartServer.Enabled = true;
            btnStopServer.Enabled = false;
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            UpdateClientList();
        }

        private void StartServer()
        {
            try
            {
                listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                isListening = true;
                listenThread = new Thread(ListenForClients);
                listenThread.IsBackground = true;
                listenThread.Start();
                MessageBox.Show($"Server started on port {port}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Server start failed: " + ex.Message);
            }
        }

        private void StopServer()
        {
            isListening = false;
            listener?.Stop();
            lock (connectedClients)
            {
                foreach (var client in connectedClients)
                {
                    try { client.tcpClient.Close(); } catch { }
                }
                connectedClients.Clear();
            }
            UpdateClientList();
            MessageBox.Show("Server stopped");
        }

        private void ListenForClients()
        {
            while (isListening)
            {
                try
                {
                    TcpClient newClient = listener.AcceptTcpClient();
                    ClientConnection conn = new ClientConnection(newClient);
                    lock (connectedClients)
                    {
                        connectedClients.Add(conn);
                    }
                    this.Invoke(new Action(() => UpdateClientList()));
                }
                catch { }
            }
        }

        private void UpdateClientList()
        {
            dgvListClients.Rows.Clear();
            lock (connectedClients)
            {
                foreach (var client in connectedClients)
                {
                    dgvListClients.Rows.Add(client.ClientId, "Connected");
                }
            }
        }

        private void dgvListClients_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                SendCommandToSelected("START_STREAM", selectedIndex: e.RowIndex, openViewer: true);
            }
        }

        private void btnShutdownSelected_Click(object sender, EventArgs e)
        {
            if (dgvListClients.SelectedRows.Count > 0)
            {
                int selectedIndex = dgvListClients.SelectedRows[0].Index;
                SendCommandToSelected("SHUTDOWN", selectedIndex: selectedIndex);
            }
            else
            {
                MessageBox.Show("Select a client first.");
            }
        }

        private void btnRestartSelected_Click(object sender, EventArgs e)
        {
            if (dgvListClients.SelectedRows.Count > 0)
            {
                int selectedIndex = dgvListClients.SelectedRows[0].Index;
                SendCommandToSelected("RESTART", selectedIndex: selectedIndex);
            }
            else
            {
                MessageBox.Show("Select a client first.");
            }
        }

        private void btnStream_Click(object sender, EventArgs e)
        {
            if (dgvListClients.SelectedRows.Count > 0)
            {
                int selectedIndex = dgvListClients.SelectedRows[0].Index;
                SendCommandToSelected("START_STREAM", selectedIndex: selectedIndex, openViewer: true);
            }
            else
            {
                MessageBox.Show("Select a client first.");
            }
        }

        private void btnSystemInfomation_Click(object sender, EventArgs e)
        {
            if (dgvListClients.SelectedRows.Count > 0)
            {
                int selectedIndex = dgvListClients.SelectedRows[0].Index;
                GetSystemInfoForSelected(selectedIndex);
            }
            else
            {
                MessageBox.Show("Select a client first.");
            }
        }

        private void GetSystemInfoForSelected(int selectedIndex)
        {
            ClientConnection selectedClient;
            lock (connectedClients)
            {
                if (selectedIndex >= connectedClients.Count) return;
                selectedClient = connectedClients[selectedIndex];
            }
            try
            {
                NetworkStream stream = selectedClient.tcpClient.GetStream();
                byte[] commandBytes = Encoding.ASCII.GetBytes("GET_SYSTEM_INFO");
                stream.Write(commandBytes, 0, commandBytes.Length);
                stream.Flush();

                byte[] responseBuffer = new byte[4096];
                int bytesRead = stream.Read(responseBuffer, 0, responseBuffer.Length);
                if (bytesRead > 0)
                {
                    string response = Encoding.ASCII.GetString(responseBuffer, 0, bytesRead).TrimEnd('\0', '\r', '\n');
                    ParseAndUpdateSystemInfo(response);
                }
                else
                {
                    UpdateSystemInfoLabels("CPU: No data", "RAM: No data", "DISK: No data");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to get system info: {ex.Message}");
                UpdateSystemInfoLabels("CPU: Error", "RAM: Error", "DISK: Error");
            }
        }

        private void ParseAndUpdateSystemInfo(string response)
        {
            var lines = response.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            string cpuText = "CPU: No data";
            string ramText = "RAM: No data";
            string diskText = "DISK: No data";

            foreach (var line in lines)
            {
                if (line.StartsWith("CPU:"))
                {
                    cpuText = line.Trim();
                }
                else if (line.StartsWith("RAM Usage:"))
                {
                    ramText = $"RAM: {line.Substring(10).Trim()}";
                }
                else if (line.StartsWith("Disk I/O:"))
                {
                    diskText = $"DISK: {line.Substring(9).Trim()}";
                }
            }

            UpdateSystemInfoLabels(cpuText, ramText, diskText);
        }

        private void UpdateSystemInfoLabels(string cpu, string ram, string disk)
        {
            txtCpu.Text = cpu;
            txtRam.Text = ram;
            txtDisk.Text = disk;
        }

        private void SendCommandToSelected(string command, int selectedIndex = -1, bool openViewer = false)
        {
            if (selectedIndex == -1 && dgvListClients.SelectedRows.Count > 0)
            {
                selectedIndex = dgvListClients.SelectedRows[0].Index;
            }
            if (selectedIndex >= 0)
            {
                ClientConnection selectedClient;
                lock (connectedClients)
                {
                    if (selectedIndex >= connectedClients.Count) return;
                    selectedClient = connectedClients[selectedIndex];
                }
                try
                {
                    NetworkStream stream = selectedClient.tcpClient.GetStream();
                    byte[] commandBytes = Encoding.ASCII.GetBytes(command);
                    stream.Write(commandBytes, 0, commandBytes.Length);
                    stream.Flush();

                    if (openViewer)
                    {
                        ScreenView viewer = new ScreenView(selectedClient);
                        viewer.Show();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to send '{command}': {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Select a client first.");
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            StopServer();
            base.OnFormClosed(e);
        }
    }

    public class ClientConnection
    {
        public TcpClient tcpClient;
        public string ClientId { get; private set; }

        public ClientConnection(TcpClient client)
        {
            tcpClient = client;
            ClientId = $"{tcpClient.Client.RemoteEndPoint}";
        }
    }
}