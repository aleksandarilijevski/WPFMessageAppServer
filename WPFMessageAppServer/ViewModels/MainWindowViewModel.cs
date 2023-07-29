using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WPFMessageAppServer.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _messageToSend;
        private string _chat;
        private Socket _socket;
        private Socket _client;
        private DelegateCommand _sendMessageCommand;
        private DelegateCommand _openServerCommand;

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

            _client = await _socket.AcceptAsync();

            await ReceiveMessages(_client);
        }

        private async void SendMessages()
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(_messageToSend);
            int _ = await _client.SendAsync(messageBytes, SocketFlags.None);
            Chat += "You said : " + _messageToSend + "\n";
            MessageToSend = string.Empty;   
        }

        private async Task ReceiveMessages(Socket connectedClient)
        {
            while (true)
            {
                byte[] buffer = new byte[1024];
                int received = await connectedClient.ReceiveAsync(buffer, SocketFlags.None);
                string response = Encoding.UTF8.GetString(buffer, 0, received);
                Chat += "Client said : " + response + "\n";
                RaisePropertyChanged(nameof(Chat));
            }
        }
    }
}
