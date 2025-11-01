using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Server.UI
{
    public class ClientViewModel : INotifyPropertyChanged
    {
        public string ClientId { get; }
        public string Name { get; set; }
        public string IpAddress { get; set; }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public ClientViewModel(string clientId, string computerName)
        {
            ClientId = clientId;
            IpAddress = clientId.Split(':')[0];
            Name = string.IsNullOrEmpty(computerName) ? "Unknown" : computerName;
        }
    }
}
