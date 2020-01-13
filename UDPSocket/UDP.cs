using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace UDPSocket
{
    class UDP
    {
        private const int SizeOfBuffer = 8 * 1024;
        private EndPoint endpoint = new IPEndPoint(IPAddress.Any, 0);
        private AsyncCallback _asyncCallback = null;
        private State state = new State();

        public UDP()
        {
            Server("127.0.0.1", 27000);
            Client("127.0.0.1", 27000);
            SendMessage("Nazarii Svyryd 45919");
        }
        private class State
        {
            public byte[] buffer = new byte[SizeOfBuffer];
        }
        private Socket mySocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private void Server(string address, int port)
        {
            mySocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
            mySocket.Bind(new IPEndPoint(IPAddress.Parse(address), port));
        }

        private void Client(string address, int port)
        {
            mySocket.Connect(IPAddress.Parse(address), port);
            ReceiveMessage();
        }

        private void SendMessage(string text)
        {
            byte[] message = Encoding.ASCII.GetBytes(text);
            mySocket.BeginSend(message, 0, message.Length, SocketFlags.None, (ar) =>
            {
                int bytes = mySocket.EndSend(ar);
                State so = (State)ar.AsyncState;
                Console.WriteLine("Wysłano: " + text);
            }, state);
        }

        private void ReceiveMessage()
        {
            mySocket.BeginReceiveFrom(state.buffer, 0, SizeOfBuffer, SocketFlags.None, ref endpoint, _asyncCallback = (ar) =>
            {
                State so = (State)ar.AsyncState;
                int bytes = mySocket.EndReceiveFrom(ar, ref endpoint);
                mySocket.BeginReceiveFrom(so.buffer, 0, SizeOfBuffer, SocketFlags.None, ref endpoint, _asyncCallback, so);
                Console.WriteLine("Otrzymano: " + Encoding.ASCII.GetString(so.buffer, 0, bytes));
            }, state);
        }
    }
}
