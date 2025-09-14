using System;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace ServerApp
{
    public partial class ScreenView : Form
    {
        private ClientConnection selectedClient;
        private Thread receiveThread;

        public ScreenView(ClientConnection client)
        {
            InitializeComponent();
            selectedClient = client;
            this.Text = $"Viewing: {client.ClientId}";
            StartReceiving();
        }

        private void StartReceiving()
        {
            receiveThread = new Thread(ReceiveStream);
            receiveThread.Start();
        }

        private void ReceiveStream()
        {
            NetworkStream stream = selectedClient.tcpClient.GetStream();
            while (!this.IsDisposed)
            {
                try
                {
                    byte[] sizeBytes = new byte[4];
                    int bytesRead = stream.Read(sizeBytes, 0, 4);
                    if (bytesRead == 0) break;
                    int size = BitConverter.ToInt32(sizeBytes, 0);
                    byte[] imageBytes = new byte[size];
                    int totalRead = 0;
                    while (totalRead < size)
                    {
                        int read = stream.Read(imageBytes, totalRead, size - totalRead);
                        if (read == 0) break;
                        totalRead += read;
                    }
                    if (totalRead == size)
                    {
                        using (MemoryStream ms = new MemoryStream(imageBytes))
                        {
                            Image img = Image.FromStream(ms);
                            this.Invoke(new Action(() => {
                                picScreen.Image?.Dispose(); 
                                picScreen.Image = new Bitmap(img);
                            }));
                        }
                    }
                }
                catch { break; }
                Thread.Sleep(50); 
            }
        }

        private void btnStopView_Click(object sender, EventArgs e)
        {
            try
            {
                NetworkStream stream = selectedClient.tcpClient.GetStream();
                byte[] command = System.Text.Encoding.ASCII.GetBytes("STOP_STREAM");
                stream.Write(command, 0, command.Length);
            }
            catch { }
            receiveThread?.Abort();
            picScreen.Image?.Dispose();
            picScreen.Image = null;
            this.Close();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            try
            {
                NetworkStream stream = selectedClient.tcpClient.GetStream();
                byte[] command = System.Text.Encoding.ASCII.GetBytes("STOP_STREAM");
                stream.Write(command, 0, command.Length);
            }
            catch { }
            receiveThread?.Abort();
            picScreen.Image?.Dispose();
            base.OnFormClosed(e);
        }
    }
}