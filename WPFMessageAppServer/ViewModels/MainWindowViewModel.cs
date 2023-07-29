using Prism.Commands;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WPFMessageAppServer.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _messageToSend;
        private string _chat;
        private Socket _socket;
        private DelegateCommand _sendMessageCommand;
        private DelegateCommand _openServerCommand;
        private List<Socket> _clients = new List<Socket>();

        public string MessageToSend
        {
            get
            {
                return _messageToSend;
            }

            set
            {
                _messageToSend = value;
                RaisePropertyChanged();
            }
        }

        public string Chat
        {
            get
            {
                return _chat;
            }

            set
            {
                _chat = value;
                RaisePropertyChanged();
            }
        }

        public DelegateCommand OpenServerCommand
        {
            get
            {
                _openServerCommand = new DelegateCommand(OpenServer);
                return _openServerCommand;
            }
        }

        public DelegateCommand SendMessageCommand
        {
            get
            {
                _sendMessageCommand = new DelegateCommand(SendMessages);
                return _sendMessageCommand;
            }
        }

        private async void OpenServer()
        {
            IPAddress ipAddress = IPAddress.Parse("192.168.1.2");
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 4321);

            _socket = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _socket.Bind(ipEndPoint);
            _socket.Listen(100);

            _ = Task.Run(async () => { await ListenForNewClients(); });
        }

        private async Task ListenForNewClients()
        {
            while (true)
            {
                Socket newClient = await _socket.AcceptAsync();
                _clients.Add(newClient);
                _ = Task.Run(async () => { await ReceiveMessages(newClient); });
            }
        }

        private async void SendMessages()
        {
            foreach (Socket client in _clients)
            {
                if (client.Connected)
                {
                    byte[] messageBytes = Encoding.UTF8.GetBytes(_messageToSend);
                    int _ = await client.SendAsync(messageBytes, SocketFlags.None);
                }
            }

            Chat += "You said : " + _messageToSend + "\n";
            MessageToSend = string.Empty;
        }

        private async void SpreadMessage(string message, Socket client)
        {
            foreach (Socket socket in _clients)
            {
                if (socket.Handle != client.Handle)
                {
                    byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                    int _ = await socket.SendAsync(messageBytes, SocketFlags.None);
                }
            }
        }

        private async Task ReceiveMessages(Socket socket)
        {
            while (true)
            {
                byte[] buffer = new byte[1024];
                int received = await socket.ReceiveAsync(buffer, SocketFlags.None);
                string response = Encoding.UTF8.GetString(buffer, 0, received);
                Chat += "Client said : " + response + "\n";
                RaisePropertyChanged(nameof(Chat));
                SpreadMessage(response, socket);
            }
        }
    }
}
